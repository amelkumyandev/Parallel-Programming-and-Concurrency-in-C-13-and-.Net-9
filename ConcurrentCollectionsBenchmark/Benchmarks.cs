using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
[Config(typeof(BenchmarkConfig))]
public class Benchmarks
{
    private const int NumThreads = 100;
    private const int NumOperations = 1_000_000;

    private readonly ConcurrentDictionary<int, int> _concurrentDictionary = new();
    private readonly ConcurrentQueue<int> _concurrentQueue = new();
    private readonly BlockingCollection<int> _blockingCollection = new();
    private readonly Dictionary<int, int> _lockedDictionary = new();
    private readonly object _lock = new();

    [Benchmark]
    public void ConcurrentDictionaryTest()
    {
        Parallel.For(0, NumOperations, i =>
        {
            if (i % 2 == 0)
                _concurrentDictionary[i] = i;
            else
                _concurrentDictionary.TryGetValue(i, out _);
        });
    }

    [Benchmark]
    public void ConcurrentQueueTest()
    {
        Parallel.For(0, NumOperations, i =>
        {
            if (i % 2 == 0)
                _concurrentQueue.Enqueue(i);
            else
                _concurrentQueue.TryDequeue(out _);
        });
    }

    [Benchmark]
    public void BlockingCollectionTest()
    {
        Parallel.For(0, NumOperations, i =>
        {
            if (i % 2 == 0)
                _blockingCollection.Add(i);
            else
                _blockingCollection.TryTake(out _);
        });
    }

    [Benchmark]
    public void LockedDictionaryTest()
    {
        Parallel.For(0, NumOperations, i =>
        {
            if (i % 2 == 0)
            {
                lock (_lock)
                {
                    _lockedDictionary[i] = i;
                }
            }
            else
            {
                lock (_lock)
                {
                    _lockedDictionary.TryGetValue(i, out _);
                }
            }
        });
    }
}
