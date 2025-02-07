using System.Collections.Concurrent;

public static class CountdownEventExtensions
{
    // Implementation from Solution 1
    public static Task WaitAsync(this CountdownEvent countdownEvent, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<bool>();
        var registration = cancellationToken.Register(() => tcs.TrySetCanceled());

        ThreadPool.RegisterWaitForSingleObject(
            countdownEvent.WaitHandle,
            (state, timedOut) =>
            {
                registration.Dispose();
                if (countdownEvent.IsSet) tcs.TrySetResult(true);
                else tcs.TrySetCanceled();
            },
            null,
            Timeout.Infinite,
            true
        );

        return tcs.Task;
    }
}

class Program
{
    static async Task Main()
    {
        var countdown = new CountdownEvent(3);
        var results = new ConcurrentBag<string>();

        // Worker tasks
        var tasks = new[]
        {
            Task.Run(() => ProcessWorkItem(1, countdown, results)),
            Task.Run(() => ProcessWorkItem(2, countdown, results)),
            Task.Run(() => ProcessWorkItem(3, countdown, results))
        };

        // Async wait with timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            await countdown.WaitAsync(cts.Token);
            Console.WriteLine("All tasks completed!");
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"Timeout! Completed: {results.Count}/3");
        }

        Console.WriteLine("Results: " + string.Join(", ", results));
    }

    static void ProcessWorkItem(int id, CountdownEvent countdown, ConcurrentBag<string> results)
    {
        try
        {
            // Simulate work
            Thread.Sleep(Random.Shared.Next(1000, 3000));
            results.Add($"Task {id} result");
        }
        finally
        {
            countdown.Signal();
        }
    }
}