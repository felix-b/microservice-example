using System;
using System.Threading;

public static class Example3_SafeLockAndThreadSync
{
    //--- the resources to lock
    public static readonly object ResourceA = new();  // 'new()' is same as 'new Object()'
    public static readonly object ResourceB = new();  // 'new()' is same as 'new Object()'

    //--- synchronization of worker threads
    
    public static readonly ManualResetEvent Ready1 = new(initialState: false); 
    public static readonly CountdownEvent LockedFirstResource = new(initialCount: 2);
    public static readonly ManualResetEvent Ready2 = new(initialState: false); 

    public static void Run()
    {
        var thread1 = new Thread(RunThread1);
        thread1.Start();

        var thread2 = new Thread(RunThread2);
        thread2.Start();

        Ready1.Set(); 
        Console.WriteLine(" MAIN: done Ready1.Set");

        Console.WriteLine(" MAIN: will LockedFirstResource.Wait");
        LockedFirstResource.Wait();
        Console.WriteLine(" MAIN: done LockedFirstResource.Wait");

        Ready2.Set(); 
        Console.WriteLine(" MAIN: done Ready2.Set");
        thread1.Join();
        Console.WriteLine(" MAIN: done thread1.Join");
        thread2.Join();            
        Console.WriteLine(" MAIN: done thread2.Join");
    }

    private static void RunThread1()
    {
        try
        {
            Console.WriteLine("THRD1: started, waiting for Ready1");
            Ready1.WaitOne();
            Console.WriteLine("THRD1: got Ready1");
            
            Console.WriteLine("THRD1: needs ResourceA");
            using (new SafeLock(ResourceA, nameof(ResourceA)))
            {
                Console.WriteLine("THRD1: got ResourceA");
                //Thread.Sleep(1000); //no need to Sleep because we use ManualResetEvent and CountdownEvent to sync between threads
                
                Console.WriteLine("THRD1: signaling LockedFirstResource");
                LockedFirstResource.Signal();

                Console.WriteLine("THRD1: waiting for Ready2");
                Ready2.WaitOne();
                Console.WriteLine("THRD1: got for Ready2");

                Console.WriteLine("THRD1: needs ResourceB");
                using (new SafeLock(ResourceB, nameof(ResourceB))) // SafeLock constructor -> Monitor.Enter()
                {
                    Console.WriteLine("THRD1: got ResourceB - this cannot happen!");
                    //Thread.Sleep(1000); //no need to Sleep because we use ManualResetEvent and CountdownEvent to sync between threads
                } // here the compiler inserts call to SafeLock.Dispose() -> Monitor.Exit()
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"THRD1: ERROR: {e.Message}");
        }
    }

    private static void RunThread2()
    { 
        try
        {
            Console.WriteLine("THRD2: started, waiting for Ready1");
            Ready1.WaitOne();
            Console.WriteLine("THRD2: got Ready1");
            Console.WriteLine("THRD2: needs ResourceB");

            using (new SafeLock(ResourceB, nameof(ResourceB)))
            {
                Console.WriteLine("THRD2: got ResourceB");
                //Thread.Sleep(1000); //no need to Sleep because we use ManualResetEvent and CountdownEvent to sync between threads

                Console.WriteLine("THRD2: signaling LockedFirstResource");
                LockedFirstResource.Signal();

                Console.WriteLine("THRD2: waiting for Ready2");
                Ready2.WaitOne();
                Console.WriteLine("THRD2: got for Ready2");

                Console.WriteLine("THRD2: needs ResourceA");
                using (new SafeLock(ResourceA, nameof(ResourceA)))
                {
                    Console.WriteLine("THRD2: got ResourceA - this cannot happen!");
                    //Thread.Sleep(1000); //no need to Sleep because we use ManualResetEvent and CountdownEvent to sync between threads
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"THRD2: ERROR: {e.Message}");
        }
    }

    // Why? always use 'Monitor.TryEnter(x, timeout)' instead of 'lock(x)'
    // 'lock(x) {...}' is compiled as 'Monitor.Enter(x); try {...} finally { Monitor.Exit(x); }'.
    // In case of deadlock, 'Monitor.Enter(x)' will lock forever, and the only solution will be to kill the process
    // In contrast, 'Monitor.TryEnter()' can return false and allows to handle the situation
    class SafeLock : IDisposable
    {
        private readonly object _syncRoot;
        private bool _disposed = false;
        
        public SafeLock(object syncRoot, string resourceName)
        {
            _syncRoot = syncRoot;
            if (!Monitor.TryEnter(_syncRoot, millisecondsTimeout: 1000)) // throw exception instead of locking forever
            {
                throw new SynchronizationLockException($"Cannot lock {resourceName}!");
            }
        }

        public void Dispose() 
        {
            if (!_disposed) // according to IDisposable spec, Dispose() can be called multiple times, so we need to account for that
            {
                _disposed = true;
                Monitor.Exit(_syncRoot);
            }
        }
    }
}

