using System;
using System.Threading;

public static class Example2_RecoverWithException
{
    //--- the resources to lock
    public static readonly object ResourceA = new();  // 'new()' is same as 'new Object()'
    public static readonly object ResourceB = new();  // 'new()' is same as 'new Object()'

    public static void Run()
    {
        var thread1 = new Thread(RunThread1);
        thread1.Start();

        var thread2 = new Thread(RunThread2);
        thread2.Start();

        thread1.Join();
        Console.WriteLine(" MAIN: done thread1.Join");
        thread2.Join();            
        Console.WriteLine(" MAIN: done thread2.Join");
    }

    public static void RunThread1()
    {
        try
        {
            Console.WriteLine("THRD1: needs ResourceA");
            if (!Monitor.TryEnter(ResourceA, millisecondsTimeout: 1000)) 
            {
                throw new SynchronizationLockException("Cannot lock ResourceA!");
            }
            try
            {
                Console.WriteLine("THRD1: got ResourceA");
                Thread.Sleep(1000);
                
                Console.WriteLine("THRD1: needs ResourceB");
                if (!Monitor.TryEnter(ResourceB, millisecondsTimeout: 1000)) 
                {
                    throw new SynchronizationLockException("Cannot lock ResourceB!");
                }
                try
                {
                    Console.WriteLine("THRD1: got ResourceB - this cannot happen!");
                    Thread.Sleep(1000); 
                } 
                finally
                {
                    Monitor.Exit(ResourceB);
                }
            }
            finally
            {
                Monitor.Exit(ResourceA);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"THRD1: ERROR: {e.Message}");
        }
    }

    public static void RunThread2()
    { 
        try
        {
            Console.WriteLine("THRD2: needs ResourceB");

            if (!Monitor.TryEnter(ResourceB, millisecondsTimeout: 1000)) 
            {
                throw new SynchronizationLockException("Cannot lock ResourceB!");
            }
            try
            {
                Console.WriteLine("THRD2: got ResourceB");
                Thread.Sleep(1000);

                Console.WriteLine("THRD2: needs ResourceA");

                if (!Monitor.TryEnter(ResourceA, millisecondsTimeout: 1000)) 
                {
                    throw new SynchronizationLockException("Cannot lock ResourceA!");
                }
                try
                {
                    Console.WriteLine("THRD2: got ResourceA - this cannot happen!");
                    Thread.Sleep(1000); 
                }
                finally
                {
                    Monitor.Exit(ResourceA);
                }
            }
            finally
            {
                Monitor.Exit(ResourceB);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"THRD2: ERROR: {e.Message}");
        }
    }
}

