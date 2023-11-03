using System;
using System.Threading;
using System.Collections.Generic;

public class MyMutexSolution
{
    private record struct QueueEntry(Thread Thread, object WaitSyncRoot);

    private long _spinAcquireCounter = 0;
    private long _resumeCounter = 0;
    private volatile int _mutexOwnerThreadId = 0;
    private volatile int _queueOwnerThreadId = 0;
    
    private readonly Queue<QueueEntry> _queue = new Queue<QueueEntry>();

    public IDisposable Enter()
    {
        for (int i = 0 ; i < 100 ; i++)
        {
            if (TryLockMutex())
            {
                Interlocked.Increment(ref _spinAcquireCounter);
                return new MyMutexScope(this);
            }
            if (i > 75)
            {
                Thread.Yield();
            }
        }

        LockQueue();
    
        var queueEntry = new QueueEntry(
            Thread.CurrentThread, 
            WaitSyncRoot: new object());

        try
        {
            _queue.Enqueue(queueEntry);
        }
        finally
        {
            UnlockQueue();
        }

        //Thread.CurrentThread.Suspend();
        lock (queueEntry.WaitSyncRoot)
        {
            //Console.WriteLine($"Blocking thread {Thread.CurrentThread.ManagedThreadId}");
            Monitor.Wait(queueEntry.WaitSyncRoot);
            //Console.WriteLine($"Woke up thread {Thread.CurrentThread.ManagedThreadId}");
        }
        Interlocked.Increment(ref _resumeCounter);
        return new MyMutexScope(this);
    }

    public long SpinAcquireCounter => _spinAcquireCounter;
    public long ResumeCounter => _resumeCounter;

    private void Exit()
    {
        QueueEntry? nextQueueEntry = null;

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

        if (nextQueueEntry == null)
        {
            UnlockMutex();
            return;
        }

        var nextOwnerThread = nextQueueEntry.Value.Thread;
        var currentResumeCounter = Interlocked.Read(ref _resumeCounter);
        Interlocked.Exchange(ref _mutexOwnerThreadId, nextOwnerThread.ManagedThreadId);
        while (Interlocked.Read(ref _resumeCounter) == currentResumeCounter)
        {
            //nextOwnerThread.Resume();
            lock (nextQueueEntry.Value.WaitSyncRoot)
            {
                //Console.WriteLine($"Signaling thread {nextOwnerThread.ManagedThreadId}");
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
        var result = Interlocked.CompareExchange(ref _mutexOwnerThreadId, value: 0, comparand: currentThreadId);
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
        private readonly MyMutexSolution _owner;

        public MyMutexScope(MyMutexSolution owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            _owner.Exit();
        }
    }
}

public static class MyMutexSolutionTest
{
    public const int ThreadCount = 100;
    public const int IterationCount = 1000;
    public static readonly MyMutexSolution OutputMutex = new();
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