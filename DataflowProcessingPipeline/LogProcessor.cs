using System.Threading.Tasks.Dataflow;

public class LogProcessor
{
    private readonly BufferBlock<string> _inputBlock;
    private readonly TransformBlock<string, string> _transformBlock;
    private readonly ActionBlock<string> _outputBlock;

    public ITargetBlock<string> InputBlock => _inputBlock;

    public LogProcessor()
    {
        // Buffer Block: Stores incoming log messages with a bounded capacity
        var bufferOptions = new DataflowBlockOptions { BoundedCapacity = 10 };
        _inputBlock = new BufferBlock<string>(bufferOptions);

        // Transform Block: Simulates log parsing and formatting
        var transformOptions = new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = 5,
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            EnsureOrdered = false // Allow parallel out-of-order processing
        };

        _transformBlock = new TransformBlock<string, string>(log =>
        {
            return $"[{DateTime.UtcNow:O}] Processed Log: {log}";
        }, transformOptions);

        // Action Block: Writes logs to a file
        var actionOptions = new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = 5,
            MaxDegreeOfParallelism = 2 // Only allow two concurrent file writes
        };

        _outputBlock = new ActionBlock<string>(async log =>
        {
            using (var writer = new StreamWriter("logs.txt", append: true))
            {
                await writer.WriteLineAsync(log);
            }
        }, actionOptions);

        // Link blocks with backpressure
        _inputBlock.LinkTo(_transformBlock, new DataflowLinkOptions { PropagateCompletion = true });
        _transformBlock.LinkTo(_outputBlock, new DataflowLinkOptions { PropagateCompletion = true });
    }

    public async Task CompleteAsync()
    {
        _inputBlock.Complete();
        await _outputBlock.Completion;
    }
}
