using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

class Program
{
    static async Task Main()
    {
        var temperatureSource = new BufferBlock<double>();
        var humiditySource = new BufferBlock<double>();

        var joinBlock = new JoinBlock<double, double>();

        var processingBlock = new ActionBlock<Tuple<double, double>>(data =>
        {
            Console.WriteLine($" Temperature: {data.Item1}°C, Humidity: {data.Item2}%");
        });

        // Link sources to JoinBlock (Tuple-based)
        temperatureSource.LinkTo(joinBlock.Target1);
        humiditySource.LinkTo(joinBlock.Target2);

        // Link JoinBlock to processing with DataflowLinkOptions
        joinBlock.LinkTo(processingBlock, new DataflowLinkOptions { PropagateCompletion = true });

        // Simulate temperature and humidity sensors
        var random = new Random();
        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(random.Next(200, 500));  // Simulate sensor delays
            await temperatureSource.SendAsync(20 + random.NextDouble() * 10); // 20-30°C
            await humiditySource.SendAsync(40 + random.NextDouble() * 20);   // 40-60%
        }

        // Signal completion
        temperatureSource.Complete();
        humiditySource.Complete();
        joinBlock.Complete();

        // Wait for processing to complete
        await processingBlock.Completion;
    }
}
