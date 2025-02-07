using BenchmarkDotNet.Attributes;

namespace PLINQBenchmark
{
    [MemoryDiagnoser]
    public class PLINQBenchmarkExample
    {
        private readonly List<int> _numbers;

        public PLINQBenchmarkExample()
        {
            _numbers = Enumerable.Range(1, 1_000_000).ToList();
        }

        [Benchmark]
        public List<int> SequentialPrimeCalculation()
        {
            return _numbers.Where(IsPrime).ToList();
        }

        [Benchmark]
        public List<int> ParallelPrimeCalculation()
        {
            return _numbers.AsParallel()
                           .WithDegreeOfParallelism(Environment.ProcessorCount)
                           .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                           .WithMergeOptions(ParallelMergeOptions.AutoBuffered)
                           .Where(IsPrime)
                           .ToList();
        }

        private static bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number == 2 || number == 3) return true;
            if (number % 2 == 0 || number % 3 == 0) return false;

            int boundary = (int)Math.Sqrt(number);
            for (int i = 5; i <= boundary; i += 6)
            {
                if (number % i == 0 || number % (i + 2) == 0)
                    return false;
            }
            return true;
        }
    }
}
