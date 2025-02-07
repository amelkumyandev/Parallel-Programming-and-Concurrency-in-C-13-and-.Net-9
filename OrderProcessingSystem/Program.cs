using OrderProcessingSystem;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Processing orders...");

        var orderService = new OrderService();
        Task processOrder1 = orderService.ProcessOrderAsync(101);
        Task processOrder2 = orderService.ProcessOrderAsync(102);

        await Task.WhenAll(processOrder1, processOrder2);
        Console.WriteLine("All orders processed successfully.");
    }
}