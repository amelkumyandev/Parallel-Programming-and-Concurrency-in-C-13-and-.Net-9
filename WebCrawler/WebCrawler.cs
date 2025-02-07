using System.Threading.Tasks.Dataflow;

class WebCrawler
{
    private readonly HttpClient _httpClient = new HttpClient();
    private readonly BufferBlock<string> _urlQueue;
    private readonly TransformBlock<string, string> _downloadBlock;
    private readonly TransformBlock<string, List<string>> _extractBlock;
    private readonly ActionBlock<List<string>> _processBlock;

    public WebCrawler()
    {
        _urlQueue = new BufferBlock<string>();
        _downloadBlock = new TransformBlock<string, string>(async url => await _httpClient.GetStringAsync(url));
        _extractBlock = new TransformBlock<string, List<string>>(html => new List<string> { "http://example.com/a", "http://example.com/b" });
        _processBlock = new ActionBlock<List<string>>(urls => urls.ForEach(Console.WriteLine));

        _urlQueue.LinkTo(_downloadBlock);
        _downloadBlock.LinkTo(_extractBlock);
        _extractBlock.LinkTo(_processBlock);
    }

    public async Task Run()
    {
        await _urlQueue.SendAsync("http://example.com");
        _urlQueue.Complete();
        await _processBlock.Completion;
    }
}
