using System.Diagnostics;
using System.Threading.Channels;

namespace ChannelHighThroughput
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Channel High-Throughput Demo (.NET 9 / C# 13) ===\n");

            // Create a ring-buffer channel with capacity = 1000
            var ringChannel = Channel.CreateBounded<double>(new BoundedChannelOptions(1000)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleWriter = false,
                SingleReader = false
            });

            // We'll run multiple producers, a single transformer, and a single aggregator
            using var cts = new CancellationTokenSource();

            // Start pipeline
            var transformChannel = Channel.CreateUnbounded<double>(); // transform -> aggregator
            var producerTasks = StartProducers(ringChannel.Writer, 3, cts.Token);
            var transformerTask = StartTransformer(ringChannel.Reader, transformChannel.Writer, cts.Token);
            var aggregatorTask = StartAggregator(transformChannel.Reader, cts.Token);

            // Let producers run for 5 seconds
            await Task.Delay(TimeSpan.FromSeconds(5));
            cts.Cancel(); // signal cancellation to all tasks

            // Wait for pipeline to gracefully shut down
            await Task.WhenAll(producerTasks);
            ringChannel.Writer.Complete(); // no more data
            await transformerTask;
            transformChannel.Writer.Complete();
            await aggregatorTask;

            Console.WriteLine("\n=== Pipeline completed. Press any key to exit. ===");
            Console.ReadKey();
        }

        private static Task[] StartProducers(ChannelWriter<double> writer, int producerCount, CancellationToken ct)
        {
            var tasks = new Task[producerCount];
            for (int i = 0; i < producerCount; i++)
            {
                int producerId = i;
                tasks[i] = Task.Run(async () =>
                {
                    var rnd = new Random();
                    var sw = Stopwatch.StartNew();
                    int itemsProduced = 0;

                    try
                    {
                        while (!ct.IsCancellationRequested)
                        {
                            // Random item
                            double item = rnd.NextDouble() * 1000.0;
                            await writer.WriteAsync(item, ct);
                            itemsProduced++;

                            // Simulate variable production rate
                            await Task.Delay(rnd.Next(1, 5), ct);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Normal cancellation
                    }
                    finally
                    {
                        sw.Stop();
                        Console.WriteLine($"[Producer {producerId}] Items produced: {itemsProduced}, Elapsed: {sw.ElapsedMilliseconds} ms");
                    }
                });
            }
            return tasks;
        }

        private static async Task StartTransformer(ChannelReader<double> inputReader,
                                                  ChannelWriter<double> outputWriter,
                                                  CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            long itemsTransformed = 0;

            try
            {
                await foreach (var item in inputReader.ReadAllAsync(ct))
                {
                    // CPU-bound transform
                    double transformed = Math.Sqrt(item) * 2.0;

                    // Simulate a short processing delay
                    await Task.Delay(1, ct);

                    await outputWriter.WriteAsync(transformed, ct);
                    itemsTransformed++;
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            finally
            {
                sw.Stop();
                Console.WriteLine($"[Transformer] Items transformed: {itemsTransformed}, Elapsed: {sw.ElapsedMilliseconds} ms");
            }
        }

        private static async Task StartAggregator(ChannelReader<double> reader, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            long itemsAggregated = 0;
            double sumOfProcessed = 0.0;

            try
            {
                await foreach (var data in reader.ReadAllAsync(ct))
                {
                    // Simple accumulation
                    sumOfProcessed += data;
                    itemsAggregated++;
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            finally
            {
                sw.Stop();
                Console.WriteLine($"[Aggregator] Items aggregated: {itemsAggregated}");
                Console.WriteLine($"[Aggregator] Sum of processed data: {sumOfProcessed:F2}");
                Console.WriteLine($"[Aggregator] Elapsed: {sw.ElapsedMilliseconds} ms");
            }
        }
    }
}
