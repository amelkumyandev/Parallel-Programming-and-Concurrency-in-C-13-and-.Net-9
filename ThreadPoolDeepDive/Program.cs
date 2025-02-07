using System.Diagnostics;

namespace ThreadPoolDeepDive
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== .NET 9+ ThreadPool Advanced Deep Dive ===\n");

            // 1. Basic explanation
            Console.WriteLine("The ThreadPool is a global pool of reusable threads. " +
                              "It dynamically grows/shrinks based on workload and schedules " +
                              "tasks efficiently via work-stealing mechanisms.\n");

            // 2. Show default min/max threads
            ThreadPool.GetMinThreads(out int defaultMinWorker, out int defaultMinIO);
            ThreadPool.GetMaxThreads(out int defaultMaxWorker, out int defaultMaxIO);
            Console.WriteLine("Default ThreadPool Settings:");
            Console.WriteLine($"  MinThreads: Worker={defaultMinWorker}, IO={defaultMinIO}");
            Console.WriteLine($"  MaxThreads: Worker={defaultMaxWorker}, IO={defaultMaxIO}\n");

            // 3. Optionally tweak min threads to see effect
            bool setMinSuccess = ThreadPool.SetMinThreads(10, defaultMinIO);
            Console.WriteLine($"SetMinThreads(10, {defaultMinIO}) success={setMinSuccess}\n");

            // 4. Queue short CPU-bound tasks
            int shortTaskCount = 20;
            var sw = Stopwatch.StartNew();
            int completedCount = 0;
            Console.WriteLine($"Queuing {shortTaskCount} short CPU-bound tasks...");
            for (int i = 0; i < shortTaskCount; i++)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    double sum = 0;
                    for (int x = 0; x < 100_000; x++)
                        sum += Math.Sqrt(x);
                    Interlocked.Increment(ref completedCount);
                });
            }

            // Wait for all to complete
            while (completedCount < shortTaskCount)
            {
                await Task.Delay(20);
            }
            sw.Stop();
            Console.WriteLine($"Completed {shortTaskCount} tasks in {sw.ElapsedMilliseconds} ms\n");

            // 5. Demonstrate an async I/O operation
            string filePath = Path.Combine(Path.GetTempPath(), "threadpool_demo.txt");
            string sampleData = "Hello, .NET ThreadPool I/O!";
            await File.WriteAllTextAsync(filePath, sampleData);
            string readBack = await File.ReadAllTextAsync(filePath);
            Console.WriteLine($"Async I/O: Wrote and read file data: '{readBack}' (Thread={Thread.CurrentThread.ManagedThreadId})\n");

            // 6. Demonstrate blocking tasks + short tasks
            Console.WriteLine("Queuing 3 blocking tasks and 3 short tasks...");
            for (int i = 0; i < 3; i++)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Console.WriteLine($"[Blocking Task] Start on Thread={Thread.CurrentThread.ManagedThreadId}");
                    Thread.Sleep(2000);
                    Console.WriteLine($"[Blocking Task] End on Thread={Thread.CurrentThread.ManagedThreadId}");
                });
            }
            for (int i = 0; i < 3; i++)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    Console.WriteLine($"[Short Task {state}] on Thread={Thread.CurrentThread.ManagedThreadId}");
                }, i);
            }

            await Task.Delay(3000);

            // 7. Demonstrate a LongRunning task
            Console.WriteLine("\nQueuing a LongRunning task...");
            Task longRunning = Task.Factory.StartNew(() =>
            {
                Console.WriteLine($"[LongRunning] Start on Thread={Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(3000);
                Console.WriteLine("[LongRunning] End");
            }, TaskCreationOptions.LongRunning);

            // Meanwhile, queue another short worker item
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Console.WriteLine($"[Another short task] on Thread={Thread.CurrentThread.ManagedThreadId}");
            });

            await longRunning;

            Console.WriteLine("\nAll demonstrations completed. Press ENTER to exit.");
            Console.ReadLine();
        }
    }
}
