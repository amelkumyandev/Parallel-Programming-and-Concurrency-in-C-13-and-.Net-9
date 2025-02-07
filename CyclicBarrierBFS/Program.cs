using Java.Lang;
using java.util.concurrent;
using DotNetty.Common.Concurrency;


public class BarrierRunnable : Java.Lang.Object, IRunnable
{
    public void Run()
    {
        // This method is called when the barrier is tripped
        Console.WriteLine("[Barrier] Level completed. Moving to next level...");
    }
}
class CyclicBarrierBFS
{
    static int[,] graph = {
        {0, 1, 1, 0},
        {1, 0, 1, 1},
        {1, 1, 0, 1},
        {0, 1, 1, 0}
    };

    static bool[] visited = new bool[4];
    static CyclicBarrier barrier = new CyclicBarrier(3, new BarrierRunnable());


    static void Main()
    {
        Parallel.For(0, 3, i =>
        {
            BFS(i);
        });

        Console.WriteLine("BFS traversal completed.");
    }

    static void BFS(int node)
    {
        if (visited[node]) return;

        lock (visited)
        {
            if (!visited[node])
            {
                visited[node] = true;
                Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] Visiting Node {node}");
            }
        }

        barrier.await(); // Wait for all threads to finish processing this level
    }
}
