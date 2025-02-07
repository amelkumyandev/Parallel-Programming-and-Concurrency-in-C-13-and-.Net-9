using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PerformanceDemos
{
    public class Program
    {
        static void Main(string[] args)
        {
            // 1) Demonstrate Amdahl's Law limit with partial parallel code
            AmdahlDemo.Run();

            // 2) Showcase false sharing and how padding helps
            FalseSharingDemo.Run();
            FalseSharingPaddedDemo.Run();

            // 3) Measure overhead using StopWatch
            OverheadMeasurement.CompareSummation();

            // Done
            Console.WriteLine("\nAll demos complete.");
        }
    }

    public static class AmdahlDemo
    {
        public static void Run()
        {
            Console.WriteLine("[AmdahlDemo] Demonstrating partial parallelization scenario...");

            const int totalIterations = 100_000_000;
            // Example: 80% parallel, 20% serial
            const double parallelFraction = 0.8;
            int parallelWork = (int)(totalIterations * parallelFraction);
            int serialWork = totalIterations - parallelWork;

            var stopwatch = Stopwatch.StartNew();

            // Parallelizable portion
            Parallel.For(0, parallelWork, i =>
            {
                // Some dummy parallel work
                _ = Math.Sqrt(i);
            });

            // Serial portion
            for (int i = 0; i < serialWork; i++)
            {
                // Some dummy serial work
                _ = Math.Log(i + 1);
            }

            stopwatch.Stop();

            Console.WriteLine($"[AmdahlDemo] Elapsed ms: {stopwatch.ElapsedMilliseconds}");
        }
    }

    public static class FalseSharingDemo
    {
        // 8 longs = 64 bytes total (on many systems), likely on one or few cache lines
        private static long[] sharedArray = new long[8];

        public static void Run()
        {
            Console.WriteLine("[FalseSharingDemo] Running...");
            const int iterations = 50_000_000;
            var stopwatch = Stopwatch.StartNew();

            // Each element is touched by a different thread, but might share a cache line
            Parallel.For(0, sharedArray.Length, i =>
            {
                for (int j = 0; j < iterations; j++)
                {
                    sharedArray[i]++;
                }
            });

            stopwatch.Stop();
            Console.WriteLine($"[FalseSharingDemo] Time: {stopwatch.ElapsedMilliseconds} ms");
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct CacheLinePaddedLong
    {
        [FieldOffset(0)]
        public long Value;

        // Add padding to fill up to 64 bytes (or more)
        [FieldOffset(64)]
        public long Pad;
    }

    public static class FalseSharingPaddedDemo
    {
        // Each entry is now ~64 bytes in size, so each is on its own cache line
        private static CacheLinePaddedLong[] paddedArray = new CacheLinePaddedLong[8];

        public static void Run()
        {
            Console.WriteLine("[FalseSharingPaddedDemo] Running...");
            const int iterations = 50_000_000;
            var stopwatch = Stopwatch.StartNew();

            Parallel.For(0, paddedArray.Length, i =>
            {
                for (int j = 0; j < iterations; j++)
                {
                    paddedArray[i].Value++;
                }
            });

            stopwatch.Stop();
            Console.WriteLine($"[FalseSharingPaddedDemo] Time: {stopwatch.ElapsedMilliseconds} ms");
        }
    }

    public static class OverheadMeasurement
    {
        public static void CompareSummation()
        {
            Console.WriteLine("[OverheadMeasurement] Comparing sequential vs. parallel summation...");

            const int size = 1_000_000;
            var data = new int[size];
            var rand = new Random();
            for (int i = 0; i < size; i++)
            {
                data[i] = rand.Next(1000);
            }

            // 1) Sequential
            var sw = Stopwatch.StartNew();
            long seqSum = 0;
            for (int i = 0; i < data.Length; i++)
                seqSum += data[i];
            sw.Stop();
            long seqTime = sw.ElapsedMilliseconds;
            Console.WriteLine($"  Sequential sum = {seqSum}, Time = {seqTime} ms");

            // 2) Parallel
            sw.Restart();
            long parSum = 0;
            Parallel.For<long>(
                fromInclusive: 0,
                toExclusive: data.Length,
                localInit: static () => 0L,
                body: (index, loopState, localSum) =>
                {
                    localSum += data[index];
                    return localSum;
                },
                localFinally: localSum =>
                {
                    Interlocked.Add(ref parSum, localSum);
                }
            );
            sw.Stop();
            long parTime = sw.ElapsedMilliseconds;
            Console.WriteLine($"  Parallel sum   = {parSum}, Time = {parTime} ms");

            if (parTime > 0)
            {
                double speedup = (double)seqTime / parTime;
                Console.WriteLine($"  => Speedup: {speedup:F2}x");
            }
        }
    }
}
