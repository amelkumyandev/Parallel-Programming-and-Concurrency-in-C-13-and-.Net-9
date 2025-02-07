using System.Threading.Tasks.Dataflow;

namespace HybridPipeline
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Hybrid I/O + CPU Pipeline Demo (.NET 9 / C# 13) ===\n");

            // Generate a sample data file if it doesn't exist
            string filePath = "floatsample.bin";
            if (!File.Exists(filePath))
            {
                Console.WriteLine("[Main] Generating data file...");
                GenerateSampleBinaryFile(filePath, numFloats: 5_000_000); // ~20 MB
            }

            try
            {
                using var cts = new CancellationTokenSource();
                // We'll read in 64KB chunks, then transform
                Console.WriteLine("[Main] Starting pipeline...");
                float result = await RunDataFlowPipeline(filePath, cts.Token);
                Console.WriteLine($"[Main] Final result: {result:F2}");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[Main] Pipeline canceled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Main] Exception: {ex}");
            }

            Console.WriteLine("\nAll done. Press any key to exit.");
            Console.ReadKey();
        }

        // Stage A: Asynchronously read from binary file in chunks -> outputs float[] blocks
        // Stage B: Transform block with parallel CPU usage -> partial sum of each chunk
        // Stage C: Aggregate partial sums into a final float
        private static async Task<float> RunDataFlowPipeline(string filePath, CancellationToken ct)
        {
            const int ChunkSize = 64 * 1024;
            var buffer = new byte[ChunkSize];

            // Step B: Transform block (parallel)
            // Each float[] chunk -> partial sum (float)
            var transformBlock = new TransformBlock<float[], float>(chunk =>
            {
                // CPU-bound partial sum
                return ParallelPartialSum(chunk);
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1)
            });

            // Step C: ActionBlock or final aggregator
            float globalSum = 0;
            var aggregatorBlock = new ActionBlock<float>(partial =>
            {
                // Interlocked is for double/long, not float
                // but we can do a lock or local aggregator:
                lock (transformBlock)
                {
                    globalSum += partial;
                }
            });

            // Link transform -> aggregator
            transformBlock.LinkTo(aggregatorBlock, new DataflowLinkOptions { PropagateCompletion = true });

            // Step A: Producer reads file in chunks, posts to transformBlock
            await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                                               bufferSize: ChunkSize, useAsync: true);

            int bytesRead;
            while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                // Convert to float array
                int floatCount = bytesRead / sizeof(float);
                float[] chunk = new float[floatCount];
                Buffer.BlockCopy(buffer, 0, chunk, 0, floatCount * sizeof(float));

                // Post chunk to the transform block
                // We can use SendAsync to handle backpressure
                await transformBlock.SendAsync(chunk, ct);
            }

            // Once done reading, signal no more data
            transformBlock.Complete();
            // Wait for aggregator to finish
            await aggregatorBlock.Completion;

            return globalSum;
        }

        private static float ParallelPartialSum(float[] chunk)
        {
            // We sum in parallel to simulate heavy CPU usage
            // For large arrays, this can help, but watch overhead
            object lockObj = new object();
            float localSum = 0;

            Parallel.ForEach(chunk,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1)
                },
                value =>
                {
                    // Some CPU-bound transformation, e.g.:
                    float transformed = (float)Math.Sqrt(value) + 1.0f;
                    lock (lockObj)
                    {
                        localSum += transformed;
                    }
                }
            );

            return localSum;
        }

        // Helper to generate a random binary file of floats
        private static void GenerateSampleBinaryFile(string path, int numFloats)
        {
            var rnd = new Random();
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            using var bw = new BinaryWriter(fs);
            for (int i = 0; i < numFloats; i++)
            {
                float val = (float)(rnd.NextDouble() * 1000.0);
                bw.Write(val);
            }
        }
    }
}
