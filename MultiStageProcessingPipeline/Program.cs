using System.Collections.Concurrent;

using var stage1 = new BlockingCollection<string>(10);
using var stage2 = new BlockingCollection<string>(10);

var cts = new CancellationTokenSource();

// Producer (Fetch Transactions)
Task.Run(() =>
{
    for (int i = 1; i <= 20; i++)
    {
        stage1.Add($"Transaction-{i}");
        Console.WriteLine($"Fetched: Transaction-{i}");
        Thread.Sleep(100);
    }
    stage1.CompleteAdding();
});

// Processor (Validate Transactions)
Task.Run(() =>
{
    foreach (var item in stage1.GetConsumingEnumerable())
    {
        var processed = $"Validated-{item}";
        stage2.Add(processed);
        Console.WriteLine($"Processed: {processed}");
        Thread.Sleep(200);
    }
    stage2.CompleteAdding();
});

// Consumer (Save to Database)
Task.Run(() =>
{
    foreach (var item in stage2.GetConsumingEnumerable())
    {
        Console.WriteLine($"Saved to DB: {item}");
        Thread.Sleep(150);
    }
});

Console.ReadLine();
cts.Cancel();
