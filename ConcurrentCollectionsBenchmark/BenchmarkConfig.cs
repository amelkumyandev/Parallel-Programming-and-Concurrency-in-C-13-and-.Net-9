using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        AddJob(Job.ShortRun
            .WithLaunchCount(1)
            .WithWarmupCount(1)
            .WithIterationCount(3));
    }
}
