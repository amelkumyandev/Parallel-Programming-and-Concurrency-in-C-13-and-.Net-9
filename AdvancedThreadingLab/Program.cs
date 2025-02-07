
Console.WriteLine("=== Advanced Threading Lab ===");
Console.WriteLine($"Runtime Version: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"OS: {System.Runtime.InteropServices.RuntimeInformation.OSDescription}");
Console.WriteLine();

// Optionally set the minimum worker and I/O threads to see how the runtime scales up.
// ThreadPool.SetMinThreads(4, 4);

// Step A: Print initial state
PrintThreadPoolStats("Initial");

// Step B: Launch CPU-bound tasks
Console.WriteLine("Launching CPU-bound tasks...");
Task[] cpuTasks = new Task[8];
for (int i = 0; i < cpuTasks.Length; i++)
{
    int capture = i; // local copy for closure
    cpuTasks[i] = Task.Run(() => DoCpuBoundWork(capture));
}

// Step C: Launch I/O-bound tasks
Console.WriteLine("Launching I/O-bound tasks...");
Task[] ioTasks = new Task[8];
for (int i = 0; i < ioTasks.Length; i++)
{
    int capture = i;
    ioTasks[i] = Task.Run(async () => await DoIoBoundWorkAsync(capture));
}

// Step D: Periodically monitor thread pool usage while tasks are running
for (int i = 1; i <= 5; i++)
{
    PrintThreadPoolStats($"Monitoring iteration {i}");
    await Task.Delay(6500);
}

// Step E: Wait for all tasks to complete
Console.WriteLine("Waiting for tasks to complete...");
await Task.WhenAll(cpuTasks);
await Task.WhenAll(ioTasks);

// Step F: Print final stats
PrintThreadPoolStats("Final");
Console.WriteLine("All tasks completed. Press any key to exit.");
Console.ReadKey();


static void DoCpuBoundWork(int taskId)
{
    // Simulate CPU-heavy work
    double result = 0;
    for (int i = 0; i < 2_000_000; i++)
    {
        result += Math.Sqrt(i);
    }
    Console.WriteLine($"[CPU Task {taskId}] completed. Result={result:F2}");
}

static async Task DoIoBoundWorkAsync(int taskId)
{
    // Simulate I/O wait (e.g., a network call, file read, etc.)
    Console.WriteLine($"[I/O Task {taskId}] started.");
    await Task.Delay(3000);  // 3-second simulated I/O
    Console.WriteLine($"[I/O Task {taskId}] completed.");
}

static void PrintThreadPoolStats(string label)
{
    ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);
    ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);

    Console.WriteLine(
        $"[{label}] " +
        $"WorkerThreads: Available={workerThreads}, Max={maxWorkerThreads} | " +
        $"IOThreads: Available={completionPortThreads}, Max={maxCompletionPortThreads} | " +
        $"Time: {DateTime.Now:HH:mm:ss.fff}"
    );
}

