public class WhenEachDemo
{
    public static async Task Main()
    {
        // 1. Create a list of tasks.
        var tasks = new List<Task<int>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(SimulateAsyncOperation(i));
        }

        // 2. Use Task.WhenEach to process them as they complete.
        await foreach (var completedTask in Task.WhenEach(tasks))
        {
            try
            {
                // 3. Await the completed task's result.
                int result = await completedTask;
                Console.WriteLine($"Task completed with result: {result}");
            }
            catch (Exception ex)
            {
                // Handle any exceptions on a per-task basis.
                Console.WriteLine($"Task threw an exception: {ex.Message}");
            }
        }
    }

    // Simulate an asynchronous operation that returns an integer.
    private static async Task<int> SimulateAsyncOperation(int value)
    {
        // Simulate work with a random delay.
        await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(200, 1000)));
        return value * 2;
    }
}
