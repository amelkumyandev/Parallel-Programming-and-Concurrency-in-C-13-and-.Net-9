using System.Collections.Concurrent;

var queue = new BlockingCollection<Transaction>(boundedCapacity: 5);
var producer = new Producer(queue);
var consumer = new Consumer(queue);

producer.Start(10);
consumer.Start(1);

while (!queue.IsCompleted)
    Thread.Sleep(500);

Console.WriteLine("Processing complete.");
