using System.Threading.Tasks.Dataflow;

public class SensorMerger
{
    private readonly JoinBlock<int, int> _joinBlock;
    private readonly ActionBlock<Tuple<int, int>> _outputBlock;

    public SensorMerger()
    {
        _joinBlock = new JoinBlock<int, int>();

        _outputBlock = new ActionBlock<Tuple<int, int>>(data =>
        {
            Console.WriteLine($" Merged Data -> Temperature: {data.Item1}, Humidity: {data.Item2}%");
        });

        // Linking JoinBlock to ActionBlock with proper Tuple<T1, T2> handling
        _joinBlock.LinkTo(_outputBlock, new DataflowLinkOptions { PropagateCompletion = true });
    }

    public async Task MergeData()
    {
        await _joinBlock.Target1.SendAsync(25);
        await _joinBlock.Target2.SendAsync(65);
        _joinBlock.Complete();
        await _outputBlock.Completion;
    }
}
