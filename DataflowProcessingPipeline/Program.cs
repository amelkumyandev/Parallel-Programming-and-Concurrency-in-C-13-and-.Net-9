class Program
{
    static async Task Main()
    {
        var logProcessor = new LogProcessor();
        var cts = new CancellationTokenSource();

        Console.WriteLine("Press ENTER to stop log processing...");

        // Start log generation
        var logger = Task.Run(() => Logger.GenerateLogs(logProcessor.InputBlock, cts.Token));

        // Wait for user input to stop
        Console.ReadLine();
        cts.Cancel();

        // Ensure all messages are processed
        await logProcessor.CompleteAsync();
        Console.WriteLine("Log processing completed.");
    }
}
