#if false
#pragma warning disable CS0618

using System;
using System.Threading;
using System.Collections.Generic;

public class CriticalSection : IDisposable
{
    private volatile int _sectionOwnerThreadId = 0;
    private volatile int _queueOwnerThreadId = 0;
    private volatile bool _disposed = false;
    private readonly Queue<Thread> _queue = new Queue<Thread>();

    public IDisposable Enter()
    {
        while (true)
        {
            for (int i = 0 ; i < 100 ; i++)
            {
                if (TryLockSection())
                {
                    return new CriticalSectionScope(this);
                }
                if (i > 75)
                {
                    Thread.Yield();
                }
            }

            LockQueue();
            
            try
            {
                _queue.Enqueue(Thread.CurrentThread);
            }
            finally
            {
                UnlockQueue();
            }

            Thread.CurrentThread.Suspend();
        }
    }

    private void Exit()
    {


        
    }

    bool TryLockSection()
    {
        var currentThreadId = Thread.CurrentThread.ManagedThreadId;
        var result = Interlocked.CompareExchange(
            ref _sectionOwnerThreadId, 
            value: currentThreadId, 
            comparand: 0);
        
        var sectionWasLocked = (result == 0);
        return sectionWasLocked;
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
        var queueWasUnlocked = (result == 0);

        if (!queueWasUnlocked)
        {
            throw new InvalidOperationException("Queue is not locked");
        }
    }

    private class CriticalSectionScope : IDisposable
    {
        private readonly CriticalSection _owner;

        public CriticalSectionScope(CriticalSection owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            _owner.Exit();
        }
    }
}
#endif
