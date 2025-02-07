using System.Threading.Tasks.Dataflow;

public static class Logger
{
    private static readonly string[] SampleLogs = {
        "User logged in",
        "File uploaded",
        "Database updated",
        "User logged out",
        "Server request received"
    };

    public static async Task GenerateLogs(ITargetBlock<string> target, CancellationToken token)
    {
        Random rand = new Random();
        while (!token.IsCancellationRequested)
        {
            string log = SampleLogs[rand.Next(SampleLogs.Length)];
            if (await target.SendAsync(log)) // Ensures backpressure is respected
            {
                Console.WriteLine($"Produced Log: {log}");
            }
            else
            {
                Console.WriteLine("Log queue is full. Dropping log.");
            }

            await Task.Delay(100); // Simulating log generation rate
        }
    }
}