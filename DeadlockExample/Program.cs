using System;

public static class Program
{
    public static void Main(string[] args)
    {
        MyMutexTest.Run();
        MyMutexTest.Assert();

        // uncomment exactly one of the 4 options below:

        // Example 1: the simplest example. It uses lock(). 
        //           the bad thing about lock() is that it doesn't accept a timeout
        //           therefore, if the lock cannot be obtained, the thead blocks forever
        //           and the only solution is to kill the process
        //Example1_BlockForever.Run();

        // Example 2: a better example, because it uses Monitor.TryEnter instead of lock(). 
        //           when waiting for a lock (or Mutex/Semaphore/.../...), always allow timeout.
        //           with timeout, you can write logic that handles the situation when a lock could not be obtained
        //           it is better than killing a process like in example 1
        //Example2_RecoverWithException.Run();
        
        
        // Example 3: uses Monitor.TryEnter instead of lock(). 
        //            it uses thread synchronization primitives instead of Thread.Sleep(1000)...
        //            It's faster for unit tests, and also... 
        //            ...constant sleep times might not work when the CPU is loaded, due to starvation (a thread cannot get a to time slice to run, for long time... 
        //            ...- in this case starvation of more than 1 second breaks the example

        //Example3_SafeLockAndThreadSync.Run();

        // Option 4: run one of the examples above by command line argument (1 to 3)
        //
        //RunExampleByCommandLineArg();

        Console.WriteLine("All Done!");

        void RunExampleByCommandLineArg()
        {
            var exampleToRun = args.Length > 0 ? int.Parse(args[0]) : 1;
            Console.WriteLine($"Running example {exampleToRun}");

            switch (exampleToRun)
            {
                case 2: 
                    Example2_RecoverWithException.Run();
                    break;
                case 3: 
                    Example3_SafeLockAndThreadSync.Run();
                    break;
                default: 
                    Example1_BlockForever.Run();
                    break;
            }
        }
    }
}
