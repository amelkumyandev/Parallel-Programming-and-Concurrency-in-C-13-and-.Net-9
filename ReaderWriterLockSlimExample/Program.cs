class ReaderWriterLockSlimExample
{
    static ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
    static int sharedResource = 0;

    static void Main()
    {
        var reader1 = new Thread(() => Read("Reader1"));
        var reader2 = new Thread(() => Read("Reader2"));
        var writer = new Thread(() => Write());

        reader1.Start();
        reader2.Start();
        writer.Start();

        reader1.Join();
        reader2.Join();
        writer.Join();
    }

    static void Read(string readerName)
    {
        rwLock.EnterReadLock();
        try
        {
            Console.WriteLine($"{readerName}: Read value {sharedResource}");
            Thread.Sleep(100); // Simulate reading
        }
        finally
        {
            rwLock.ExitReadLock();
        }
    }

    static void Write()
    {
        rwLock.EnterWriteLock();
        try
        {
            sharedResource++;
            Console.WriteLine($"Writer: Updated value to {sharedResource}");
            Thread.Sleep(200); // Simulate writing
        }
        finally
        {
            rwLock.ExitWriteLock();
        }
    }
}
