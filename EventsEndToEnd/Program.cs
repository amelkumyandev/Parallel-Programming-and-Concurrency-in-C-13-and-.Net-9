using System.Collections.Concurrent;

namespace EventsEndToEnd
{
    class Program
    {
        // ManualResetEvent to gate all workers until initialization completes
        private static ManualResetEvent _initializationGate = new ManualResetEvent(false);

        // AutoResetEvent to signal workers one-by-one for sub-tasks
        private static AutoResetEvent _taskSignal = new AutoResetEvent(false);

        // Shared queue for demonstration
        private static ConcurrentQueue<int> _workQueue = new ConcurrentQueue<int>();

        // Flag indicating if we should continue scheduling tasks
        private static volatile bool _keepRunning = true;

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== E2E Demo: ManualResetEvent & AutoResetEvent ===");

            // Start multiple worker threads
            for (int i = 0; i < 3; i++)
            {
                int workerId = i;
                Task.Run(() => WorkerLoop(workerId));
            }

            Console.WriteLine("[Main] Performing initialization...");
            await Task.Delay(2000); // Simulate setup

            // Initialization complete; open the gate
            Console.WriteLine("[Main] Initialization complete, opening gate (MRE.Set).");
            _initializationGate.Set();

            // Enqueue some work items and signal them
            for (int i = 1; i <= 5; i++)
            {
                _workQueue.Enqueue(i);
                Console.WriteLine($"[Main] Enqueued work item {i}.");
                _taskSignal.Set(); // Signal one worker
                await Task.Delay(500);
            }

            Console.WriteLine("[Main] Press ENTER to stop scheduling tasks.");
            Console.ReadLine();
            _keepRunning = false;

            // Final signals to let waiting threads exit gracefully if they time out
            for (int i = 0; i < 3; i++)
            {
                _taskSignal.Set();
            }

            // Wait so we can observe final logs
            await Task.Delay(2000);

            // Dispose events properly
            _initializationGate.Dispose();
            _taskSignal.Dispose();

            Console.WriteLine("[Main] Exiting demo. Press any key to close.");
            Console.ReadKey();
        }

        private static void WorkerLoop(int workerId)
        {
            Console.WriteLine($"[Worker {workerId}] Started. Waiting for init gate...");

            // Wait for overall initialization
            _initializationGate.WaitOne();
            Console.WriteLine($"[Worker {workerId}] Initialization complete, now active.");

            while (true)
            {
                // Wait for a signal that there might be work
                Console.WriteLine($"[Worker {workerId}] Waiting for task signal...");
                bool signaled = _taskSignal.WaitOne(3000);

                if (!signaled)
                {
                    // Timed out waiting; check if we should stop
                    if (!_keepRunning) break;
                    continue;
                }

                // If signaled, attempt to dequeue an item
                if (_workQueue.TryDequeue(out int item))
                {
                    try
                    {
                        Console.WriteLine($"[Worker {workerId}] Processing item {item}...");
                        // Simulate item processing
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Worker {workerId}] Error processing item {item}: {ex.Message}");
                    }
                }
                else
                {
                    // No item found (could be spurious or final signals)
                    if (!_keepRunning) break;
                }
            }

            Console.WriteLine($"[Worker {workerId}] Exiting worker loop.");
        }
    }
}
