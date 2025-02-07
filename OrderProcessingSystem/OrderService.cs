namespace OrderProcessingSystem
{
    using System.Threading.Tasks;

    class OrderService
    {
        private readonly DatabaseService _database = new DatabaseService();

        public async Task ProcessOrderAsync(int orderId)
        {
            await Task.Delay(1000);
            await _database.SaveOrderAsync(orderId);
        }
    }
}
