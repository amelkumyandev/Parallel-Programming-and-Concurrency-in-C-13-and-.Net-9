using System.Collections.Concurrent;

namespace BlockingCollectionAdvancedDemo
{
    class Program
    {
        private static BlockingCollection<int> _jobQueue
            = new BlockingCollection<int>(boundedCapacity: 5);

        // Optional dead-letter queue for poison messages
        private static ConcurrentQueue<int> _deadLetterQueue
            = new ConcurrentQueue<int>();

        private static CancellationTokenSource _cts = new CancellationTokenSource();

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== BlockingCollection Advanced Demo ===");

            var producers = new List<Task>();
            var consumers = new List<Task>();

            // Create 2 producers
            for (int p = 0; p < 2; p++)
            {
                int prodId = p;
                producers.Add(Task.Run(() => Producer(prodId, _cts.Token)));
            }

            // Create 3 consumers
            for (int c = 0; c < 3; c++)
            {
                int consId = c;
                consumers.Add(Task.Run(() => Consumer(consId, _cts.Token)));
            }

            Console.WriteLine("[Main] Press ENTER to request cancellation, or wait for producers to finish.");
            Console.ReadLine();

            // Request cancellation to demonstrate graceful shutdown
            _cts.Cancel();

            // Wait for producers
            try
            {
                await Task.WhenAll(producers);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[Main] Some or all producers were canceled.");
            }

            // Once producers are done, no more items
            _jobQueue.CompleteAdding();

            // Wait for consumers
            try
            {
                await Task.WhenAll(consumers);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[Main] Some or all consumers were canceled.");
            }

            Console.WriteLine($"[Main] Finished. Dead-letter count: {_deadLetterQueue.Count}");
            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();
        }

        static void Producer(int producerId, CancellationToken token)
        {
            try
            {
                for (int i = 1; i <= 10; i++)
                {
                    token.ThrowIfCancellationRequested();

                    // Blocks if queue is at capacity
                    int job = (producerId * 100) + i;
                    _jobQueue.Add(job, token);

                    Console.WriteLine($"[Producer {producerId}] Added job: {job}. QueueCount = {_jobQueue.Count}");
                    Thread.Sleep(300); // simulate production time
                }
                Console.WriteLine($"[Producer {producerId}] Completed production of items.");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[Producer {producerId}] Canceled while producing.");
                throw; // Rethrow to inform Task.WhenAll
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Producer {producerId}] Error: {ex.Message}");
            }
        }

        static void Consumer(int consumerId, CancellationToken token)
        {
            try
            {
                // Blocks if queue empty, ends when CompleteAdding() and queue is drained
                foreach (var job in _jobQueue.GetConsumingEnumerable(token))
                {
                    token.ThrowIfCancellationRequested();

                    try
                    {
                        // Simulate processing
                        ProcessJob(job);
                        Console.WriteLine($"\t[Consumer {consumerId}] Processed job {job}.");
                    }
                    catch (Exception ex)
                    {
                        // Poison handling: store item in dead-letter queue
                        _deadLetterQueue.Enqueue(job);
                        Console.WriteLine($"\t[Consumer {consumerId}] Job {job} failed: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"\t[Consumer {consumerId}] Canceled.");
                throw;
            }
            finally
            {
                Console.WriteLine($"\t[Consumer {consumerId}] Done consuming.");
            }
        }

        // Example processing method (can throw exception for demonstration)
        static void ProcessJob(int jobId)
        {
            // This can randomly throw an exception to simulate a poison message scenario
            if (jobId % 33 == 0)
            {
                throw new InvalidOperationException("Synthetic processing error!");
            }
            Thread.Sleep(500); // simulate workload
        }
    }
}
