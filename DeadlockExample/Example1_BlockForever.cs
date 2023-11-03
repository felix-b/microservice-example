
using System;
using System.Threading;

public static class Example1_BlockForever
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

    private static void RunThread1()
    {
        Console.WriteLine("THRD1: needs ResourceA");
        lock (ResourceA) 
        {
            Console.WriteLine("THRD1: got ResourceA");
            Thread.Sleep(1000); 

            Console.WriteLine("THRD1: needs ResourceB");
            lock (ResourceB) // DEADLOCK: blocks here forever
            {
                Console.WriteLine("THRD1: got ResourceB - this cannot happen!");
                Thread.Sleep(1000); 
            }
        }
    }

    private static void RunThread2()
    { 
        Console.WriteLine("THRD2: needs ResourceB");
        lock (ResourceB)
        {
            Console.WriteLine("THRD2: got ResourceB");
            Thread.Sleep(1000);

            Console.WriteLine("THRD2: needs ResourceA");
            lock (ResourceA) // DEADLOCK: blocks here forever
            {
                Console.WriteLine("THRD2: got ResourceA - this cannot happen!");
                Thread.Sleep(1000);
            }
        }
    }
}

