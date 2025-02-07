namespace TplDeepDiveDemo
{
    // 4. TaskCompletionSource Example
    public static class TaskCompletionSourceExample
    {
        public static async Task UseTcsAsync()
        {
            string result = await SimulateAsyncOperation();
            Console.WriteLine($"[TaskCompletionSourceExample] Operation Result: {result}");
        }

        private static Task<string> SimulateAsyncOperation()
        {
            var tcs = new TaskCompletionSource<string>();

            // Using ThreadPool to simulate background async operation
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    Thread.Sleep(1000); // simulate latency
                    tcs.SetResult("Hello from TaskCompletionSource!");
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }
    }
}
