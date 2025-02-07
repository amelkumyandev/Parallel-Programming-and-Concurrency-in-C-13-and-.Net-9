
Console.WriteLine("== TaskFactory and Async/Await Deep Dive ==");

// 1) Create a TaskFactory with custom defaults
var cts = new CancellationTokenSource();
var myScheduler = TaskScheduler.Default; // could be a custom one
var factory = new TaskFactory(
    cancellationToken: cts.Token,
    creationOptions: TaskCreationOptions.DenyChildAttach,
    continuationOptions: TaskContinuationOptions.None,
    scheduler: myScheduler
    );

// 2) Demonstrate using TaskFactory to create tasks
var taskA = factory.StartNew(() =>
{
    Console.WriteLine("Task A started on factory with DenyChildAttach.");
    Thread.Sleep(200);
    Console.WriteLine("Task A finished normally.");
});

var taskB = factory.StartNew(() =>
{
    Console.WriteLine("Task B started - will throw exception.");
    throw new InvalidOperationException("Simulated error in Task B.");
});

try
{
    Task.WaitAll(taskA, taskB); // For demonstration, block here
}
catch (AggregateException agex)
{
    Console.WriteLine($"Caught AggregateException: {agex.InnerException?.Message}");
}
Console.WriteLine($"taskA status: {taskA.Status}, taskB status: {taskB.Status}\n");

// 3) Demonstrate cancellation with the factory's token
var cancellableTask = factory.StartNew(() =>
{
    // This task sees cts.Token
    for (int i = 0; i < 5; i++)
    {
        cts.Token.ThrowIfCancellationRequested();
        Console.WriteLine($"Cancellable task running iteration {i}...");
        Thread.Sleep(100);
    }
    Console.WriteLine("Cancellable task completed successfully.");
});

// Cancel after 200ms
Task.Run(async () =>
{
    await Task.Delay(200);
    Console.WriteLine(" -> Triggering cancellation...");
    cts.Cancel();
});

try
{
    cancellableTask.Wait();
}
catch (AggregateException agex) when (agex.InnerExceptions[0] is OperationCanceledException)
{
    Console.WriteLine("Cancellable task recognized OperationCanceledException.");
}
Console.WriteLine($"cancellableTask status: {cancellableTask.Status}\n");

// 4) Demonstrate an async method (showing internal state machine)
try
{
    int sumResult = await ComplexOperationAsync(3, 7, new CancellationToken());
    Console.WriteLine($"ComplexOperationAsync result: {sumResult}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error from ComplexOperationAsync: {ex}");
}

Console.WriteLine("\n== End of Deep Dive ==");
Console.ReadKey();
        
// An async method with multiple awaits, illustrating state machine
static async Task<int> ComplexOperationAsync(int x, int y, CancellationToken token)
{
    Console.WriteLine("Starting ComplexOperationAsync... [State A]");
    // first await
    await Task.Delay(300, token);

    Console.WriteLine("After first await, computing partial result... [State B]");
    int partial = x * y;

    // second await
    await Task.Delay(300, token);

    Console.WriteLine("After second await, finalizing result... [State C]");
    int final = partial + 50;
    return final;
}
   
