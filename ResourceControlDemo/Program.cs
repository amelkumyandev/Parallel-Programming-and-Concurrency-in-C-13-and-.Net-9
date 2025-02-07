namespace ResourceControlDemo
{
    class Program
    {
        // (1) Named Mutex to ensure single-instance app
        static Mutex? _instanceMutex;

        // (2) Semaphore for CPU-bound concurrency (3 slots)
        static Semaphore _cpuSemaphore = new Semaphore(3, 3);

        // (3) SemaphoreSlim for I/O-bound concurrency (5 slots)
        static SemaphoreSlim _ioSemSlim = new SemaphoreSlim(5, 5);

        static async Task Main(string[] args)
        {
            // Try to create a named Mutex for single-instance enforcement
            bool createdNew;
            _instanceMutex = new Mutex(initiallyOwned: false, "Global\\MyServiceRunner", out createdNew);

            if (!createdNew)
            {
                Console.WriteLine("Another instance of ServiceRunner is already running.");
                return;
            }

            // Explicitly acquire the mutex
            try
            {
                if (!_instanceMutex.WaitOne(0)) // Attempt to acquire ownership
                {
                    Console.WriteLine("Failed to acquire Mutex ownership. Exiting...");
                    return;
                }

                Console.WriteLine("ServiceRunner instance started. Mutex acquired.");

                // Launch tasks simulating CPU-bound & I/O-bound concurrently
                var tasks = new Task[10];
                for (int i = 0; i < 10; i++)
                {
                    if (i % 2 == 0)
                    {
                        // CPU-bound
                        int idx = i;
                        tasks[i] = Task.Run(() => CpuTask(idx));
                    }
                    else
                    {
                        // I/O-bound
                        int idx = i;
                        tasks[i] = IoTaskAsync(idx);
                    }
                }

                // Wait for all tasks to complete
                Task.WaitAll(tasks);

                Console.WriteLine("All tasks finished.");
            }
            finally
            {
                // Release the mutex only if it was acquired
                if (_instanceMutex != null)
                {
                    try
                    {
                        _instanceMutex.ReleaseMutex();
                        Console.WriteLine("Releasing single-instance mutex...");
                    }
                    catch (ApplicationException)
                    {
                        Console.WriteLine("Failed to release the Mutex. Ownership may not be held by this thread.");
                    }
                }
            }

            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey();
        }

        static void CpuTask(int index)
        {
            Console.WriteLine($"[CPU {index}] Waiting for CPU slot...");
            _cpuSemaphore.WaitOne(); // Acquire a slot
            try
            {
                Console.WriteLine($"[CPU {index}] Acquired CPU slot. Doing CPU-bound work...");
                Thread.Sleep(800); // Simulate CPU-bound work
            }
            finally
            {
                _cpuSemaphore.Release(); // Free the slot
                Console.WriteLine($"[CPU {index}] Released CPU slot.");
            }
        }

        static async Task IoTaskAsync(int index)
        {
            Console.WriteLine($"[I/O {index}] Waiting for I/O slot...");
            await _ioSemSlim.WaitAsync(); // Acquire a slot asynchronously
            try
            {
                Console.WriteLine($"[I/O {index}] Acquired I/O slot. Simulating download...");
                await Task.Delay(1000); // Simulate async I/O work
            }
            finally
            {
                _ioSemSlim.Release(); // Free the slot
                Console.WriteLine($"[I/O {index}] Released I/O slot.");
            }
        }
    }
}
