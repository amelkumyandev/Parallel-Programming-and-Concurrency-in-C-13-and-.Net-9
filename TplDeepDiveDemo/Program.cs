namespace TplDeepDiveDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== TPL Deep Dive Demo ===\n");

            // 1. Basic Task with Task.Run
            Console.WriteLine("1) Running Basic Task using Task.Run:");
            await BasicTaskExample.RunBasicTask();
            Console.WriteLine();

            // 2. Advanced Task Creation with StartNew (LongRunning)
            Console.WriteLine("2) Running LongRunning Task with Task.Factory.StartNew:");
            var longRunning = StartNewExample.LongRunningTask();
            await longRunning;
            Console.WriteLine();

            // 3. Checking Task Properties (Faulted Task)
            Console.WriteLine("3) Checking Task Properties with a Faulted Task:");
            await TaskPropertiesExample.CheckTaskProperties();
            Console.WriteLine();

            // 4. Using TaskCompletionSource to wrap custom/legacy async
            Console.WriteLine("4) Demonstrating TaskCompletionSource:");
            await TaskCompletionSourceExample.UseTcsAsync();
            Console.WriteLine();

            // 5. Parent-Child Tasks (AttachedToParent)
            Console.WriteLine("5) Running Parent-Child Example:");
            await AttachedChildTasksExample.RunParentChildExample();
            Console.WriteLine();

            Console.WriteLine("=== End of Demo ===");
            Console.ReadLine();
        }
    }
}
