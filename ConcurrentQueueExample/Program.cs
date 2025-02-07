using System.Collections.Concurrent;

namespace ConcurrentQueueExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== ConcurrentQueue<T> Demo ===");

            // Shared concurrent queue
            var queue = new ConcurrentQueue<int>();

            // Number of producers/consumers
            int producerCount = 3;
            int consumerCount = 2;
            int itemsPerProducer = 20;

            // Start producer tasks
            Task[] producers = new Task[producerCount];
            for (int p = 0; p < producerCount; p++)
            {
                int producerId = p;
                producers[p] = Task.Run(() =>
                {
                    for (int i = 0; i < itemsPerProducer; i++)
                    {
                        int item = (producerId * 1000) + i;
                        queue.Enqueue(item);
                        Console.WriteLine($"Producer {producerId} enqueued {item}");
                        Thread.Sleep(new Random().Next(10, 30));
                    }
                });
            }

            // Start consumer tasks
            Task[] consumers = new Task[consumerCount];
            int consumedCount = 0;
            object lockObj = new object();

            for (int c = 0; c < consumerCount; c++)
            {
                int consumerId = c;
                consumers[c] = Task.Run(() =>
                {
                    while (true)
                    {
                        // Attempt to dequeue
                        if (queue.TryDequeue(out int value))
                        {
                            lock (lockObj) consumedCount++;
                            Console.WriteLine($"Consumer {consumerId} dequeued {value}");
                            Thread.Sleep(new Random().Next(15, 40));
                        }
                        else
                        {
                            // If producers are done and queue is empty, exit
                            bool allProducersDone = Task.WaitAll(producers, 50);
                            if (allProducersDone && queue.IsEmpty)
                            {
                                break;
                            }
                        }
                    }
                });
            }

            // Wait for all tasks
            await Task.WhenAll(producers);
            await Task.WhenAll(consumers);

            Console.WriteLine($"Total items consumed: {consumedCount}");
            Console.WriteLine("Queue final Count: " + queue.Count);

            // Final snapshot
            var leftover = queue.ToArray();
            Console.WriteLine($"Leftover items in queue (snapshot): {leftover.Length}");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
