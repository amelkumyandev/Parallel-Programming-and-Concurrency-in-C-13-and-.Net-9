namespace ThreadLocalDemo
{
    public class Program
    {
        static void Main(string[] args)
        {
            // 1) Demonstration: Partial Sum with localInit/localFinally
            DemonstrateParallelForPartialSum();

            // 2) Demonstration: ThreadLocal<T> usage
            DemonstrateThreadLocal();

            // 3) Demonstration: ThreadLocal with resource disposal
            DemonstrateResourceDisposal();
        }

        private static void DemonstrateParallelForPartialSum()
        {
            var random = new Random();
            var data = new int[1_000_000];
            for (int i = 0; i < data.Length; i++)
                data[i] = random.Next(0, 500);

            long globalSum = 0;

            Parallel.For<long>(
                fromInclusive: 0,
                toExclusive: data.Length,
                localInit: static () => 0L,
                body: (index, state, localSum) =>
                {
                    localSum += data[index];
                    return localSum;
                },
                localFinally: partialSum =>
                {
                    // Merge partial sums with atomic operation
                    Interlocked.Add(ref globalSum, partialSum);
                }
            );

            Console.WriteLine($"[DemonstrateParallelForPartialSum] Summation result: {globalSum}");
        }

        private static void DemonstrateThreadLocal()
        {
            var random = new Random();
            var data = new int[500_000];
            for (int i = 0; i < data.Length; i++)
                data[i] = random.Next(0, 500);

            // Each thread gets its own sum
            ThreadLocal<long> threadLocalSum = new ThreadLocal<long>(
                valueFactory: () => 0L,
                trackAllValues: true
            );

            Parallel.For(0, data.Length, i =>
            {
                threadLocalSum.Value += data[i];
            });

            long globalSum = 0;
            foreach (var partialSum in threadLocalSum.Values)
            {
                globalSum += partialSum;
            }

            Console.WriteLine($"[DemonstrateThreadLocal] Summation result: {globalSum}");
        }

        private static void DemonstrateResourceDisposal()
        {
            // Example: each thread gets a local "ResourceSimulator"
            // We'll simulate some heavy operation, then we'll dispose it in localFinally

            int totalIterations = 100;

            Console.WriteLine("[DemonstrateResourceDisposal] Starting...");

            Parallel.For<ResourceSimulator>(
                0,
                totalIterations,
                localInit: static () => new ResourceSimulator(),
                body: (index, state, simulator) =>
                {
                    simulator.DoWork(index);
                    return simulator;
                },
                localFinally: simulator =>
                {
                    simulator.Dispose();
                }
            );

            Console.WriteLine("[DemonstrateResourceDisposal] All work done, resources disposed.");
        }
    }

    // A simple disposable resource simulator
    public class ResourceSimulator : IDisposable
    {
        private bool _disposed;
        private readonly string _resourceName;

        public ResourceSimulator()
        {
            _resourceName = "Resource_" + Guid.NewGuid();
            Console.WriteLine($"{_resourceName} created on Thread {Thread.CurrentThread.ManagedThreadId}.");
        }

        public void DoWork(int iteration)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ResourceSimulator));

            // Simulated work
            if (iteration % 10 == 0)
            {
                Console.WriteLine($"{_resourceName} working on iteration {iteration} " +
                                  $"(Thread {Thread.CurrentThread.ManagedThreadId}).");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Console.WriteLine($"{_resourceName} disposed on Thread {Thread.CurrentThread.ManagedThreadId}.");
                _disposed = true;
            }
        }
    }
}
