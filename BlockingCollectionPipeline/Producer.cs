using System.Collections.Concurrent;
using System.Transactions;

public class Producer
{
    private readonly BlockingCollection<Transaction> _queue;
    private readonly Random _random = new();

    public Producer(BlockingCollection<Transaction> queue) => _queue = queue;

    public void Start(int count)
    {
        Task.Run(() =>
        {
            for (int i = 0; i < count; i++)
            {
                var transaction = new Transaction(i, _random.Next(100, 1000), DateTime.UtcNow);
                _queue.Add(transaction);
                Logger.Log($"Produced: {transaction}");
                Thread.Sleep(_random.Next(200, 500));
            }
            _queue.CompleteAdding();
        });
    }
}
