namespace TplDeepDiveDemo
{
    // 2. StartNew with LongRunning
    public static class StartNewExample
    {
        public static Task LongRunningTask()
        {
            return Task.Factory.StartNew(() =>
            {
                Console.WriteLine("[StartNewExample] LongRunning task started...");
                // Simulate a longer job
                Thread.Sleep(1500);
                Console.WriteLine("[StartNewExample] LongRunning task is done.");
            },
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        }
    }
}
