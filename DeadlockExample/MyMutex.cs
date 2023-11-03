using System;
using System.Threading;
using System.Collections.Generic;

/// <summary>
/// Example:
/// 
/// var mutex = new MyMutex();
/// 
/// using (mutex.Enter())
/// {
///     // use thr resource
/// }
/// 
/// </summary>
public class MyMutex
{
    private record struct QueueEntry
    (
        Thread Thread,        // the waiting thread
        object WaitSyncRoot   // the object to use as sync root for Monitor.Wait/Pulse
    );

    // thread id that currently owns (has the lock) the mutex
    // 0 means the mutex is unlocked
    private volatile int _mutexOwnerThreadId = 0;

    // thread id that currently owns (has the lock) on the waiting queue
    // 0 means the queue is unlocked
    private volatile int _queueOwnerThreadId = 0;

    // the queue of the waiting threads
    private readonly Queue<QueueEntry> _queue = new Queue<QueueEntry>();

    // this is for statistics - number of times the optimistic strategy worked
    // and we were able to acquire a lock by spinning in a loop, without blocking a thread
    private long _spinAcquireCounter = 0;

    // this is for statistics - number of times the pessimistic strategy worked
    // and we had to block a thread
    private long _resumeCounter = 0;

    /// <summary>
    /// Acquires the mutex and returns a disposable object. 
    /// To release the mutex, call Dispose() on the returned object.
    /// </summary>
    public IDisposable Enter()
    {
        // 1. Optimistic - First try to acquire a lock while spinning in a loop for 100 iterations
        //    ("spin lock") this is much cheaper than using a synchronization primitive

        // 1.1. Inside the loop, use TryLockMutex() to try to acquire the lock, and see if it returns true.
        //      This is a very fast operation (few CPU cycles) than using synchronization primitives (~1k CPU cycles)
        //      because TryLockMutex() uses atomic CPU operations like "test-and-set" (via Interlocked class)

        // 1.1.1. If TryLockMutex succeeds, we have the lock - so we return an IDisposable scope object
        //        which will be used to release the lock with a call to Dispose()
        //        MyMutexScope class should implement such scope object.

        // 2. If we couldn't acquire the lock while spinning, we will have to block the thread and wait.
        //    Before we block the thread, we add it to a queue. This is how we avoid starvation, 
        //    because with the queue, waiting threads receive the lock in the FIFO order. 
        //    Before we can add current thread to a queue, we have to lock the queue.
        //    Since operations on the queue are always short, we use a spin lock for the queue,
        //    combined with test-and-set operations of Interlocked class.
        //    LockQueue() function implements locking the queue.
        //    A call to LockQueue() should be paired with a call to UnlockQueue().
        //    For safety, we put UnlockQueue into a try..finally block.

        // uncomment
        // LockQueue();

        var queueEntry = new QueueEntry(
                Thread.CurrentThread, 
                WaitSyncRoot: new object());

        try
        {
            // uncomment
            //_queue.Enqueue(queueEntry);
        }
        finally
        {
            // uncomment
            //UnlockQueue();
        }

        // 3. Now that we added current thread to the queue, we have to block the thread.
        //    For blocking the thread, we use Monitor.Wait() and Monitor.Pulse().
        //    This is a bit cheating, as we reuse ready synchronization mechanisms, 
        //    but there is no other option without consuming an OS primitive.

        // before Monitor.Wait or Monitor.Pulse can be called, the sync root must be locked
        lock (queueEntry.WaitSyncRoot)
        {
            // the thread will be blocked until Monitor.Pulse will be called on the sync root object
            Monitor.Wait(queueEntry.WaitSyncRoot); 
        }
        
        Interlocked.Increment(ref _resumeCounter); // this is for statistics, we print it in the end of the test
        
        // return MyMutexScope object
        throw new NotImplementedException();
    }

    // statistics property
    public long SpinAcquireCounter => _spinAcquireCounter;
    // statistics property
    public long ResumeCounter => _resumeCounter;

    /// <summary>
    /// Releases the mutex. This function is called from MyMutexScope.Dispose.
    /// Another thing this function does, it dequeues the first waiting thread from the queue 
    /// and makes it the owner of the mutex.
    /// </summary>
    private void Exit()
    {
        QueueEntry? nextQueueEntry = null;

        // try to dequeue the first waiting thread, if any threads are waiting
        
        LockQueue();
        try
        {
            if (_queue.Count > 0)
            {
                nextQueueEntry = _queue.Dequeue();
            }
        }
        finally
        {
            UnlockQueue();
        }

        // if there was no waiting thread, we have nothing else to do - release the mutex
        if (nextQueueEntry == null)
        {
            UnlockMutex();
            return;
        }

        // if there was a waiting thread, we replace the current thread with the first waiting thread
        // and make the first waiting thread the owner of the mutex

        var nextOwnerThread = nextQueueEntry.Value.Thread;
        var currentResumeCounter = Interlocked.Read(ref _resumeCounter);
        Interlocked.Exchange(ref _mutexOwnerThreadId, nextOwnerThread.ManagedThreadId);

        while (Interlocked.Read(ref _resumeCounter) == currentResumeCounter) // here we have a subtle race condition.
        {
            lock (nextQueueEntry.Value.WaitSyncRoot)
            {
                //Console.WriteLine($"Signaling thread {nextOwnerThread.ManagedThreadId}");
                
                // here we 
                Monitor.Pulse(nextQueueEntry.Value.WaitSyncRoot); 
            }
        }
    }

    private bool TryLockMutex()
    {
        var currentThreadId = Thread.CurrentThread.ManagedThreadId;
        var result = Interlocked.CompareExchange(
            ref _mutexOwnerThreadId, 
            value: currentThreadId, 
            comparand: 0);
        
        var sectionWasLocked = (result == 0);
        return sectionWasLocked;
    }

    private void UnlockMutex()
    {
        var currentThreadId = Thread.CurrentThread.ManagedThreadId;
        var result = Interlocked.CompareExchange(
            ref _mutexOwnerThreadId, 
            value: 0, 
            comparand: currentThreadId);
        
        var mutexWasUnlocked = (result == currentThreadId);

        if (!mutexWasUnlocked)
        {
            throw new InvalidOperationException("Mutex is not locked");
        }
    }

    private void LockQueue()
    {
        var currentThreadId = Thread.CurrentThread.ManagedThreadId;
        var queueWasLocked = false;

        while (!queueWasLocked)
        {
            var result = Interlocked.CompareExchange(
                ref _queueOwnerThreadId, 
                value: currentThreadId, 
                comparand: 0);
            
            queueWasLocked = (result == 0);
        }
    }

    private void UnlockQueue()
    {
        var currentThreadId = Thread.CurrentThread.ManagedThreadId;
        var result = Interlocked.CompareExchange(ref _queueOwnerThreadId, value: 0, comparand: currentThreadId);
        var queueWasUnlocked = (result == currentThreadId);

        if (!queueWasUnlocked)
        {
            throw new InvalidOperationException("Queue is not locked");
        }
    }

    private class MyMutexScope : IDisposable
    {
        private readonly MyMutex _owner;

        public MyMutexScope(MyMutex owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            _owner.Exit();
        }
    }
}

public static class MyMutexTest
{
    public const int ThreadCount = 100;
    public const int IterationCount = 1000;
    public static readonly MyMutex OutputMutex = new();
    public static readonly List<string> Output = new();

    public static void Run()
    {
        var threads = new Thread[ThreadCount];
        for (int i = 0 ; i < ThreadCount ; i++)
        {
            threads[i] = new Thread(RunThread);
        }
        for (int i = 0 ; i < ThreadCount ; i++)
        {
            threads[i].Start(parameter: i);
        }
        for (int i = 0 ; i < ThreadCount ; i++)
        {
            threads[i].Join();
        }

        static void RunThread(object? parameter)
        {
            var threadIndex = (int)parameter!;

            for (int i = 0 ; i < IterationCount ; i++)
            {
                if ((i % 100) == 0)
                {
                    Console.WriteLine($"{threadIndex}|{i + 1}");
                }
                using (OutputMutex.Enter())
                {
                    Output.Add($"{threadIndex}|{i + 1}");
                }
            }
        }
    }

    public static void Assert()
    {
        if (Output.Count != ThreadCount * IterationCount)
        {
            throw new Exception($"ASSERTION FAILED! Output.Count - expected {ThreadCount * IterationCount}, found {Output.Count}");
        } 

        var outputSet = new HashSet<string>(Output);
        if (outputSet.Count != ThreadCount * IterationCount)
        {
            throw new Exception($"ASSERTION FAILED! outputSet.Count - expected {ThreadCount * IterationCount}, found {outputSet.Count}");
        } 
        
        var totalAcquires = OutputMutex.SpinAcquireCounter + OutputMutex.ResumeCounter;
        if (totalAcquires != ThreadCount * IterationCount)
        {
            throw new Exception($"ASSERTION FAILED! totalAcquires - expected {ThreadCount * IterationCount}, found {totalAcquires}");
        } 

        Console.WriteLine($"All Correct, SpinAcquireCounter={OutputMutex.SpinAcquireCounter}, ResumeCounter={OutputMutex.ResumeCounter}");
    }
}