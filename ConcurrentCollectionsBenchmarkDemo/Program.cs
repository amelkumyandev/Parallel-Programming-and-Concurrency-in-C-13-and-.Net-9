using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

public record TaskMetadata(Guid Id, string Name, int Priority);

[MemoryDiagnoser]
public class ConcurrentCollectionsBenchmark
{
    private ConcurrentQueue<int> _queue;
    private ConcurrentBag<int> _bag;
    private ConcurrentDictionary<int, int> _dict;

    [GlobalSetup]
    public void Setup()
    {
        _queue = new ConcurrentQueue<int>();
        _bag = new ConcurrentBag<int>();
        _dict = new ConcurrentDictionary<int, int>();

        for (int i = 0; i < 100_000; i++)
        {
            _queue.Enqueue(i);
            _bag.Add(i);
            _dict[i] = i;
        }
    }

    [Benchmark]
    public void ConcurrentQueueTest()
    {
        Parallel.For(0, 100_000, i =>
        {
            _queue.Enqueue(i);
            _queue.TryDequeue(out _);
        });
    }

    [Benchmark]
    public void ConcurrentBagTest()
    {
        Parallel.For(0, 100_000, i =>
        {
            _bag.Add(i);
            _bag.TryTake(out _);
        });
    }

    [Benchmark]
    public void ConcurrentDictionaryTest()
    {
        Parallel.For(0, 100_000, i =>
        {
            _dict[i] = i;
            _dict.TryGetValue(i, out _);
        });
    }
}

class Program
{
    static void Main() => BenchmarkRunner.Run<ConcurrentCollectionsBenchmark>();
}
