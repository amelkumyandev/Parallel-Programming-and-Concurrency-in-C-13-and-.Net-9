using System.Threading.Tasks.Dataflow;

public class StockBroadcaster
{
    private readonly BroadcastBlock<StockData> _broadcastBlock;
    private readonly ActionBlock<StockData> _client1;
    private readonly ActionBlock<StockData> _client2;

    public StockBroadcaster()
    {
        _broadcastBlock = new BroadcastBlock<StockData>(data => data);

        _client1 = new ActionBlock<StockData>(data =>
        {
            Console.WriteLine($"Client 1 received: {data.Symbol} - {data.Price}");
        });

        _client2 = new ActionBlock<StockData>(data =>
        {
            Console.WriteLine($"Client 2 received: {data.Symbol} - {data.Price}");
        });

        _broadcastBlock.LinkTo(_client1);
        _broadcastBlock.LinkTo(_client2);
    }

    public async Task StartBroadcasting()
    {
        var stockData = new StockData { Symbol = "AAPL", Price = 145.67 };
        await _broadcastBlock.SendAsync(stockData);
    }
}
