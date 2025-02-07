using System;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncSensorDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Async Sensor Demo (NET 9 / C# 13) ===");

            var sensorSimulator = new SensorSimulator();
            var sensorReader = new SensorReader();

            // We'll run for 5 seconds, then cancel
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            // Start reading sensor data
            try
            {
                await sensorReader.ReadDataAsync(sensorSimulator, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[Main] Reading canceled.");
            }

            Console.WriteLine("All done. Press any key to exit.");
            Console.ReadKey();
        }
    }

    public class SensorSimulator
    {
        private readonly Random _random = new Random();

        public async IAsyncEnumerable<double> GetReadingsAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested)
            {
                // Simulate random delay between readings
                int delayMs = _random.Next(50, 250);
                await Task.Delay(delayMs, ct);

                // Generate a random sensor value
                double reading = _random.NextDouble() * 100.0;
                yield return reading;
            }
        }
    }

    public class SensorReader
    {
        public async Task ReadDataAsync(SensorSimulator simulator, CancellationToken ct)
        {
            await foreach (var reading in simulator.GetReadingsAsync(ct))
            {
                Console.WriteLine($"[SensorReader] Reading: {reading:F2}");
                // Simulate some processing
                await Task.Delay(100, ct);

                if (reading > 80.0)
                {
                    Console.WriteLine("**Warning**: Sensor reading is high!");
                }
            }
        }
    }
}
