using System;
using System.Collections.Concurrent;
using System.Threading;

public class CustomSingleThreadContext : SynchronizationContext, IDisposable
{
    private readonly Thread _workerThread;
    private readonly BlockingCollection<(SendOrPostCallback, object)> _queue
        = new BlockingCollection<(SendOrPostCallback, object)>();
    private bool _disposed = false;

    public CustomSingleThreadContext()
    {
        _workerThread = new Thread(WorkerLoop);
        _workerThread.IsBackground = true;
        _workerThread.Start();
    }

    private void WorkerLoop()
    {
        SetSynchronizationContext(this); // Ensure custom context is set
        foreach (var (callback, state) in _queue.GetConsumingEnumerable())
        {
            callback(state); // Execute callback
        }
    }

    public override void Post(SendOrPostCallback d, object state)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(CustomSingleThreadContext));

        _queue.Add((d, state));
    }

    public override void Send(SendOrPostCallback d, object state)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(CustomSingleThreadContext));

        if (Thread.CurrentThread == _workerThread)
        {
            d(state);
        }
        else
        {
            using (var done = new ManualResetEvent(false))
            {
                _queue.Add((s =>
                {
                    try
                    {
                        d(s);
                    }
                    finally
                    {
                        done.Set();
                    }
                }, state));
                done.WaitOne();
            }
        }
    }

    public void Shutdown()
    {
        if (!_disposed)
        {
            _queue.CompleteAdding();
            _workerThread.Join();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Shutdown();
            _queue.Dispose();
            _disposed = true;
        }
    }
}
