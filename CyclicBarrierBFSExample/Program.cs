using System.Collections.Concurrent;

class ParallelTreeBFS
{
    // Tree structure: node -> children
    static Dictionary<int, List<int>> tree = new Dictionary<int, List<int>>
    {
        {0, new List<int> {1, 2, 3}},         // Level 0
        {1, new List<int> {4, 5}},            // Level 1
        {2, new List<int> {6}},               // Level 1
        {3, new List<int> {7, 8}},            // Level 1
        {4, new List<int> {9, 10}},           // Level 2
        {5, new List<int> {11}},              // Level 2
        {6, new List<int> {12, 13, 14}},      // Level 2
        {7, new List<int> {15}},              // Level 2
        {15, new List<int> {16}}              // Level 3
    };

    static bool[] visited = new bool[17];
    static readonly object queueLock = new object();

    static void Main()
    {
        var currentLevel = new ConcurrentQueue<int>();
        currentLevel.Enqueue(0); // Root node
        visited[0] = true;

        int levelNumber = 0;

        while (!currentLevel.IsEmpty)
        {
            var nextLevel = new ConcurrentQueue<int>();
            int levelSize = currentLevel.Count;
            var levelBarrier = new Barrier(levelSize, _ =>
            {
                Console.WriteLine($"[Barrier] Completed level {levelNumber++}");
                currentLevel = nextLevel;
            });

            Console.WriteLine($"\nProcessing level {levelNumber} with {levelSize} nodes");

            Parallel.ForEach(currentLevel, node =>
            {
                ProcessNode(node, nextLevel);
                levelBarrier.SignalAndWait();
            });

            Thread.Sleep(100); // Allow final messages to flush
        }

        Console.WriteLine("\nBFS traversal completed!");
    }

    static void ProcessNode(int node, ConcurrentQueue<int> nextLevel)
    {
        Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] Visiting node {node}");

        if (tree.TryGetValue(node, out var children))
        {
            foreach (var child in children)
            {
                lock (queueLock)
                {
                    if (!visited[child])
                    {
                        visited[child] = true;
                        nextLevel.Enqueue(child);
                    }
                }
            }
        }
    }
}