using System.Collections.Concurrent;
using System.Transactions;

public class Consumer
{
    private readonly BlockingCollection<Transaction> _queue;

    public Consumer(BlockingCollection<Transaction> queue) => _queue = queue;

    public void Start(int consumerId)
    {
        Task.Run(() =>
        {
            foreach (var transaction in _queue.GetConsumingEnumerable())
            {
                Logger.Log($"Consumer {consumerId} processed: {transaction}");
                Thread.Sleep(300);
            }
        });
    }
}
