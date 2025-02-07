namespace AdvancedInversionDemo
{
    class Program
    {
        private static Lock _sharedLock = new Lock();

        static void Main()
        {
            Console.WriteLine("Starting Priority Inversion Demo");

            // Low-priority thread that will hold a lock
            Thread lowPriorityThread = new Thread(LowPriorityLockHolder)
            {
                Name = "LowPriorityLockHolder",
                Priority = ThreadPriority.BelowNormal
            };
            // High-priority thread that needs the same lock
            Thread highPriorityThread = new Thread(HighPriorityWorker)
            {
                Name = "HighPriorityWorker",
                Priority = ThreadPriority.Highest
            };

            lowPriorityThread.Start();
            Thread.Sleep(500); // Give low-priority thread time to grab the lock
            highPriorityThread.Start();

            lowPriorityThread.Join();
            highPriorityThread.Join();

            Console.WriteLine("All threads done. Check PerfView or WPA for context switch data.");
        }

        private static void LowPriorityLockHolder()
        {
            lock (_sharedLock)
            {
                Console.WriteLine($">> [{Thread.CurrentThread.Name}] Lock acquired. Doing long work...");
                double sum = 0;
                for (int i = 0; i < 10_000_000; i++)
                    sum += Math.Sqrt(i);

                Console.WriteLine($">> [{Thread.CurrentThread.Name}] Done. sum={sum:F2}");
            }
        }

        private static void HighPriorityWorker()
        {
            Console.WriteLine($">> [{Thread.CurrentThread.Name}] Wants the lock now...");
            lock (_sharedLock)
            {
                Console.WriteLine($">> [{Thread.CurrentThread.Name}] Lock acquired. Doing quick work...");
            }
        }
    }
}
