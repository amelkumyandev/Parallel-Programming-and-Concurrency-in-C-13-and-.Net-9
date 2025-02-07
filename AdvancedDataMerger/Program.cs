namespace LockingMonitorDemo
{
    class Program
    {
        // A .NET 9 Lock object for local concurrency
        static Lock dataLock = new Lock();

        // A second Lock to illustrate multi-lock ordering
        static Lock loggingLock = new Lock();

        // A simple object for demonstrating Monitor usage
        static readonly object monitorObject = new object();

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Locking & Monitor Demo (.NET 9) ===");

            // Example 1: Using Lock
            var t1 = Task.Run(() => UseNet9Lock("Task1"));
            var t2 = Task.Run(() => UseNet9Lock("Task2"));

            // Example 2: Using Monitor
            var t3 = Task.Run(() => UseMonitor("Task3"));
            var t4 = Task.Run(() => UseMonitor("Task4"));

            // Wait for tasks to complete
            await Task.WhenAll(t1, t2, t3, t4);

            // Example 3: Multi-lock ordering to avoid deadlock
            var multi1 = Task.Run(() => MultiLockOrder("Multi1"));
            var multi2 = Task.Run(() => MultiLockOrder("Multi2"));

            await Task.WhenAll(multi1, multi2);

            Console.WriteLine("All tasks finished. Press any key to exit.");
            Console.ReadKey();
        }

        static void UseNet9Lock(string taskName)
        {
            // Acquire the lock
            dataLock.Enter();
            try
            {
                Console.WriteLine($"[{taskName}] Lock acquired. Doing work...");
                Thread.Sleep(500); // simulate some work
            }
            finally
            {
                dataLock.Exit(); // release the lock
            }
        }

        static void UseMonitor(string taskName)
        {
            Monitor.Enter(monitorObject);
            try
            {
                Console.WriteLine($"[{taskName}] Monitor lock acquired. Doing work...");
                Thread.Sleep(300);
            }
            finally
            {
                Monitor.Exit(monitorObject);
            }
        }

        static void MultiLockOrder(string taskName)
        {
            // Acquire locks in a consistent order: dataLock -> loggingLock
            dataLock.Enter();
            try
            {
                loggingLock.Enter();
                try
                {
                    Console.WriteLine($"[{taskName}] Acquired both locks in consistent order!");
                    Thread.Sleep(200);
                }
                finally
                {
                    loggingLock.Exit();
                }
            }
            finally
            {
                dataLock.Exit();
            }
        }
    }
}
