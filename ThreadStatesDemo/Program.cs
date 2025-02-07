Console.WriteLine("=== Thread States and Lifecycle Demo ===");

// 1. Create a new foreground thread
Thread foregroundThread = new Thread(ForegroundThreadMethod)
{
    Name = "ForegroundThread1",
    Priority = ThreadPriority.AboveNormal
};
Console.WriteLine($"Created thread '{foregroundThread.Name}' " +
                  $"with priority {foregroundThread.Priority}. " +
                  $"Initial state: {foregroundThread.ThreadState}");

// 2. Start the thread
foregroundThread.Start();
Console.WriteLine($"After Start(): {foregroundThread.ThreadState}");

// 3. Create a background thread
Thread backgroundThread = new Thread(BackgroundThreadMethod)
{
    Name = "BackgroundThread1",
    IsBackground = true
};
backgroundThread.Start();
Console.WriteLine($"Created background thread '{backgroundThread.Name}'. " +
                  $"State: {backgroundThread.ThreadState}");

// 4. Wait for foreground thread to finish
foregroundThread.Join();
Console.WriteLine($"Foreground thread state after completion: {foregroundThread.ThreadState}");

Console.WriteLine("Main thread is exiting. The background thread will be killed if still running.");
Console.ReadKey();
 
static void ForegroundThreadMethod()
{
    Console.WriteLine($">> [{Thread.CurrentThread.Name}] Starting. State={Thread.CurrentThread.ThreadState}");
    Thread.Sleep(500);
    Console.WriteLine($">> [{Thread.CurrentThread.Name}] Woke up. State={Thread.CurrentThread.ThreadState}");

    double result = 0;
    for (int i = 0; i < 1_000_000; i++)
    {
        result += Math.Sqrt(i);
    }
    Console.WriteLine($">> [{Thread.CurrentThread.Name}] Computation complete, result={result:F2}. State={Thread.CurrentThread.ThreadState}");
}

static void BackgroundThreadMethod()
{
    while (true)
    {
        Console.WriteLine($">> [{Thread.CurrentThread.Name}] Running. State={Thread.CurrentThread.ThreadState}");
        Thread.Sleep(1000);
    }
}
