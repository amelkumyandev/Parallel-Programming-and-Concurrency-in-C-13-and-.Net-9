class GameLoopBarrier
{
    static Barrier barrier = new Barrier(3, (b) =>
    {
        Console.WriteLine($"[Barrier] Frame {b.CurrentPhaseNumber} completed. Moving to the next frame...");
    });

    static void Main()
    {
        Thread physics = new Thread(() => GameTask("Physics"));
        Thread ai = new Thread(() => GameTask("AI"));
        Thread rendering = new Thread(() => GameTask("Rendering"));

        physics.Start();
        ai.Start();
        rendering.Start();

        physics.Join();
        ai.Join();
        rendering.Join();

        Console.WriteLine("Game loop finished.");
    }

    static void GameTask(string taskName)
    {
        for (int i = 0; i < 5; i++) // Simulate 5 game loop iterations
        {
            Console.WriteLine($"[{taskName}] Processing frame {i}...");
            Thread.Sleep(500); // Simulate work

            barrier.SignalAndWait(); // Synchronization point
        }
    }
}
