namespace TplDeepDiveDemo
{
    // 1. Basic Task Example
    public static class BasicTaskExample
    {
        public static async Task RunBasicTask()
        {
            Console.WriteLine("[BasicTaskExample] Starting basic task...");

            // Offload heavy CPU-bound work
            await Task.Run(() =>
            {
                long sum = 0;
                for (int i = 0; i < 5_000_000; i++)
                {
                    sum += i;
                }
                Console.WriteLine($"[BasicTaskExample] Computed Sum = {sum}");
            });

            Console.WriteLine("[BasicTaskExample] Basic task completed.");
        }
    }
}
