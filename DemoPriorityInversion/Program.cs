namespace PriorityInversionDemo
{
    class Program
    {
        // Shared lock object
        private static object _sharedLock = new object();

        static void Main(string[] args)
        {
            DemoPriorityInversion();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        public static void DemoPriorityInversion()
        {
            // Create a low-priority thread
            Thread lowPriority = new Thread(() =>
            {
                lock (_sharedLock)
                {
                    // Simulate a long CPU-bound operation
                    double sum = 0;
                    for (int i = 0; i < 1000000000; i++)
                    {
                        sum += Math.Sqrt(i);
                    }
                    Console.WriteLine($"Low-priority done. Sum={sum}");
                }
            })
            {
                Name = "LowPriority",
                Priority = ThreadPriority.BelowNormal
            };

            // Create a high-priority thread
            Thread highPriority = new Thread(() =>
            {
                Console.WriteLine("High-priority wants the lock...");
                lock (_sharedLock)
                {
                    Console.WriteLine("High-priority got the lock!");
                }
            })
            {
                Name = "HighPriority",
                Priority = ThreadPriority.Highest
            };

            // Start the low-priority thread
            lowPriority.Start();

            // Give the low-priority thread a head-start
            // so it can acquire the lock first
            Thread.Sleep(200);

            // Now start the high-priority thread
            highPriority.Start();

            // Wait for both threads to finish
            lowPriority.Join();
            highPriority.Join();
        }
    }
}
