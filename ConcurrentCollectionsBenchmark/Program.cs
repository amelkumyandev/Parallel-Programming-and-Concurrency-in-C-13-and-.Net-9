using BenchmarkDotNet.Running;

Console.WriteLine("Running Concurrent Collections Benchmark...");
BenchmarkRunner.Run<Benchmarks>();