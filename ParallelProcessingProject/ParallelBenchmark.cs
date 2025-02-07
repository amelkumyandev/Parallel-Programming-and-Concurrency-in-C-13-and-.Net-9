using ParallelProcessingProject.Services;
using BenchmarkDotNet.Attributes;

namespace ParallelProcessingProject
{
    public class ParallelBenchmark
    {
        private readonly DataProcessor _processor = new DataProcessor();

        [Benchmark]
        public void SequentialProcessing() => _processor.ProcessSequentially();

        [Benchmark]
        public void ParallelProcessing() => _processor.ProcessInParallel();
    }
}
