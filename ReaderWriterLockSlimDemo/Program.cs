class ReaderWriterLockSlimDemo
{
    static ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
    static List<int> sharedData = new List<int>();
    static Random random = new Random();

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== ReaderWriterLockSlim Demo ===");

        var readerTasks = new List<Task>();
        for (int i = 1; i <= 10; i++)
        {
            int readerId = i;
            readerTasks.Add(Task.Run(() => Reader(readerId)));
        }

        var writerTask = Task.Run(() => Writer());

        await Task.WhenAll(readerTasks);
        await writerTask;

        Console.WriteLine("Demo complete. Press any key to exit.");
        Console.ReadKey();
    }

    static void Reader(int readerId)
    {
        while (true)
        {
            rwLock.EnterReadLock();
            try
            {
                if (sharedData.Count > 0)
                {
                    int value = sharedData[^1]; // Read last value
                    Console.WriteLine($"[Reader {readerId}] Read value: {value}");
                }
                else
                {
                    Console.WriteLine($"[Reader {readerId}] No data to read.");
                }
            }
            finally
            {
                rwLock.ExitReadLock();
            }

            Thread.Sleep(random.Next(200, 500)); // Simulate work
        }
    }

    static void Writer()
    {
        for (int i = 1; i <= 10; i++)
        {
            rwLock.EnterWriteLock();
            try
            {
                sharedData.Add(i);
                Console.WriteLine($"[Writer] Added value: {i}");
            }
            finally
            {
                rwLock.ExitWriteLock();
            }

            Thread.Sleep(1000); // Simulate work
        }
    }
}
