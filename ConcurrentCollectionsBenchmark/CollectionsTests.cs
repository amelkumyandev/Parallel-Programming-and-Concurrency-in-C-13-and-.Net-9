using System.Collections.Concurrent;
public static class CollectionsTests
{
    public static void RunTests()
    {
        Console.WriteLine("Running Real-World Concurrent Collection Tests...");

        var dictTest = RunConcurrentDictionaryTest();
        var queueTest = RunConcurrentQueueTest();
        var blockingTest = RunBlockingCollectionTest();
        var lockedDictTest = RunLockedDictionaryTest();

        Task.WaitAll(dictTest, queueTest, blockingTest, lockedDictTest);
    }

    private static Task RunConcurrentDictionaryTest()
    {
        var dict = new ConcurrentDictionary<int, int>();
        return Task.Run(() =>
        {
            Parallel.For(0, 1_000_000, i =>
            {
                if (i % 2 == 0)
                    dict[i] = i;
                else
                    dict.TryGetValue(i, out _);
            });
        });
    }

    private static Task RunConcurrentQueueTest()
    {
        var queue = new ConcurrentQueue<int>();
        return Task.Run(() =>
        {
            Parallel.For(0, 1_000_000, i =>
            {
                if (i % 2 == 0)
                    queue.Enqueue(i);
                else
                    queue.TryDequeue(out _);
            });
        });
    }

    private static Task RunBlockingCollectionTest()
    {
        var collection = new BlockingCollection<int>();
        return Task.Run(() =>
        {
            Parallel.For(0, 1_000_000, i =>
            {
                if (i % 2 == 0)
                    collection.Add(i);
                else
                    collection.TryTake(out _);
            });
        });
    }

    private static Task RunLockedDictionaryTest()
    {
        var dict = new Dictionary<int, int>();
        var lockObj = new object();
        return Task.Run(() =>
        {
            Parallel.For(0, 1_000_000, i =>
            {
                if (i % 2 == 0)
                {
                    lock (lockObj)
                    {
                        dict[i] = i;
                    }
                }
                else
                {
                    lock (lockObj)
                    {
                        dict.TryGetValue(i, out _);
                    }
                }
            });
        });
    }
}
