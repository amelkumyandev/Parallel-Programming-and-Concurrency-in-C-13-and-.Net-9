namespace AtomicOperationsBasicDemo
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== 1) Lock-Free Atomic Counter Demo ===");
            var counter = new PaddedCounter(); // or use AtomicCounter for simpler approach
            int taskCount = 8;
            int incrementsPerTask = 100_000;

            // Launch multiple tasks to increment the counter
            Task[] tasks = new Task[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < incrementsPerTask; j++)
                    {
                        counter.Increment();
                    }
                });
            }

            // Wait for all tasks to finish
            await Task.WhenAll(tasks);

            long expected = taskCount * (long)incrementsPerTask;
            long actual = counter.GetValue();
            Console.WriteLine($"Expected Count = {expected}, Actual Count = {actual}");

            Console.WriteLine("\n=== 2) Atomic Reference Swap Demo ===");
            var refUpdater = new AtomicReference<string>("Initial Value");
            Console.WriteLine($"Original Reference: {refUpdater.Read()}");

            // We'll spin up tasks that attempt to swap in a new value
            var referenceSwapTasks = new Task[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                var localValue = "New Value from Task " + i;
                referenceSwapTasks[i] = Task.Run(() =>
                {
                    // Attempt to swap 'Initial Value' -> 'New Value from Task i'
                    var original = refUpdater.CompareExchange(localValue, "Initial Value");
                    if (original != "Initial Value")
                    {
                        Console.WriteLine(
                            $"[Task {i}] Swap failed: Expected 'Initial Value' " +
                            $"but found '{original}'.");
                    }
                });
            }

            await Task.WhenAll(referenceSwapTasks);

            // Check the final reference: only one thread should succeed in setting it
            Console.WriteLine($"Final Reference: {refUpdater.Read()}");
        }
    }

    /// <summary>
    /// Lock-free counter with padding to reduce false sharing.
    /// </summary>
    public class PaddedCounter
    {
        // The primary counter field
        private long _count;

        // Padding fields to mitigate false sharing on cache lines
        private long _pad1, _pad2, _pad3, _pad4, _pad5, _pad6;

        public void Increment()
        {
            Interlocked.Increment(ref _count);
        }

        public long GetValue()
        {
            return Interlocked.Read(ref _count);
        }
    }

    /// <summary>
    /// A simple lock-free reference swap using CompareExchange.
    /// </summary>
    public class AtomicReference<T> where T : class
    {
        private T? _value;

        public AtomicReference(T? initialValue)
        {
            _value = initialValue;
        }

        public T? CompareExchange(T? newValue, T? comparand)
        {
            return Interlocked.CompareExchange(ref _value, newValue, comparand);
        }

        public T? Read()
        {
            // Use CompareExchange with no actual change to force a full memory barrier
            return Interlocked.CompareExchange(ref _value, null, null);
        }
    }
}
