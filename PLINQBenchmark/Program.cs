using BenchmarkDotNet.Running;
using PLINQBenchmark;

var summary = BenchmarkRunner.Run<PLINQBenchmarkExample>();