class Program
{
    static async Task Main()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        Task task = PerformLongRunningOperationAsync(token);

        await Task.Delay(2000);
        cts.Cancel();

        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
        }
    }

    static async Task PerformLongRunningOperationAsync(CancellationToken token)
    {
        for (int i = 0; i < 10; i++)
        {
            token.ThrowIfCancellationRequested();
            Console.WriteLine($"Processing {i}");
            await Task.Delay(1000, token);
        }
    }
}
