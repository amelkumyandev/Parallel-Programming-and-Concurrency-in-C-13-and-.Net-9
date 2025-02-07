namespace OrderProcessingSystem
{
    class DatabaseService
    {
        public async Task SaveOrderAsync(int orderId)
        {
            await Task.Delay(500);
            System.Console.WriteLine($"Order {orderId} saved to database.");
        }
    }
}
