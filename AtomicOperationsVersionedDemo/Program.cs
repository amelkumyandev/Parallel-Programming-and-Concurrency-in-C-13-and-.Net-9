namespace AtomicOperationsVersionedDemo
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Versioned Reference Demo to Mitigate ABA ===");

            var versionedRef = new VersionedReference<string>("Value-A");
            Console.WriteLine($"Initial Value: {versionedRef.Read().Value}, Version: {versionedRef.Read().Version}");

            // We'll create tasks that attempt transitions: A -> B -> A
            int taskCount = 2;
            Task[] tasks = new Task[taskCount];

            tasks[0] = Task.Run(() =>
            {
                // Simulate a quick A -> B -> A transition
                var currentPair = versionedRef.Read();

                // Step 1: Try swapping from (Value-A, versionX) to (Value-B, versionX+1)
                var nextPair = (Value: "Value-B", Version: currentPair.Version + 1);
                versionedRef.CompareExchange(nextPair, currentPair);

                // Step 2: Now swap back from (Value-B, versionY) to (Value-A, versionY+1)
                var newCurrent = versionedRef.Read();
                var nextBack = (Value: "Value-A", Version: newCurrent.Version + 1);
                versionedRef.CompareExchange(nextBack, newCurrent);
            });

            tasks[1] = Task.Run(() =>
            {
                // Another thread tries to see if the value is still A 
                // and if so, set it to "Value-C"
                Thread.Sleep(100); // small delay to let the first task do A->B->A
                var oldPair = versionedRef.Read();
                var newPair = (Value: "Value-C", Version: oldPair.Version + 1);

                var actualPair = versionedRef.CompareExchange(newPair, oldPair);
                if (actualPair.Value == oldPair.Value && actualPair.Version == oldPair.Version)
                {
                    Console.WriteLine("[Task 1] Successfully swapped from A to C.");
                }
                else
                {
                    Console.WriteLine("[Task 1] Swap failed. " +
                                      $"Expected version {oldPair.Version}, found version {actualPair.Version}.");
                }
            });

            await Task.WhenAll(tasks);

            var finalPair = versionedRef.Read();
            Console.WriteLine($"Final Value: {finalPair.Value}, Version: {finalPair.Version}");
        }
    }

    /// <summary>
    /// A versioned reference to mitigate the ABA problem.
    /// Stores both the value and a version integer, updating version upon each swap.
    /// </summary>
    public class VersionedReference<T> where T : class
    {
        /// <summary>
        /// Immutable holder for (Value, Version). Using a class so we can atomically swap references.
        /// </summary>
        private class ValueWithVersion
        {
            public T? Value { get; }
            public int Version { get; }

            public ValueWithVersion(T? value, int version)
            {
                Value = value;
                Version = version;
            }
        }

        // The current atomic reference to our (value, version) pair
        private ValueWithVersion _current;

        public VersionedReference(T? initialValue)
        {
            _current = new ValueWithVersion(initialValue, 0);
        }

        /// <summary>
        /// Atomically compare (Value, Version). If it matches 'comparand', 
        /// swap in 'newPair'. Returns the old (value, version).
        /// </summary>
        public (T? Value, int Version) CompareExchange(
            (T? Value, int Version) newPair,
            (T? Value, int Version) comparand)
        {
            while (true)
            {
                // Snapshot the current object
                var snapshot = Interlocked.CompareExchange(ref _current, _current, _current);

                // If the current Value & Version match the comparand, attempt a swap
                if (snapshot.Value == comparand.Value && snapshot.Version == comparand.Version)
                {
                    var newObject = new ValueWithVersion(newPair.Value, newPair.Version);

                    // Attempt to CAS the new reference in
                    var original = Interlocked.CompareExchange(
                        ref _current,
                        newObject,    // the value we want to set
                        snapshot      // we only set if still == snapshot
                    );

                    // If 'original' == 'snapshot', the CAS succeeded
                    if (ReferenceEquals(original, snapshot))
                    {
                        return (original.Value, original.Version);
                    }
                    // Otherwise, some other thread changed _current in the meantime; retry
                }
                else
                {
                    // Fields didn't match; return current snapshot, no swap
                    return (snapshot.Value, snapshot.Version);
                }
            }
        }

        /// <summary>
        /// Atomically reads the current (Value, Version) with a full memory barrier,
        /// using a "no-op" Interlocked CAS.
        /// </summary>
        public (T? Value, int Version) Read()
        {
            // A CompareExchange with the same reference as both 'value' and 'comparand'
            // effectively returns the current pointer as a read, with the full memory fence.
            var snapshot = Interlocked.CompareExchange(ref _current, _current, _current);
            return (snapshot.Value, snapshot.Version);
        }
    }
}
