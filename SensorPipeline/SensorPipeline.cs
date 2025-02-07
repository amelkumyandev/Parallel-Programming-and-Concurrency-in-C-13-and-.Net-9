using System.Text.Json;
using System.Threading.Tasks.Dataflow;

public class SensorPipeline
{
    private readonly TransformBlock<string, SensorData[]> _readBlock;
    private readonly TransformBlock<SensorData[], SensorData[]> _processBlock;
    private readonly ActionBlock<SensorData[]> _writeBlock;

    public SensorPipeline()
    {
        _readBlock = new TransformBlock<string, SensorData[]>(async file =>
        {
            Console.WriteLine($"Reading data from {file}...");
            var json = await File.ReadAllTextAsync(file);
            return JsonSerializer.Deserialize<SensorData[]>(json);
        });

        _processBlock = new TransformBlock<SensorData[], SensorData[]>(data =>
        {
            Console.WriteLine("Processing sensor data...");
            foreach (var sensor in data)
            {
                sensor.Value = sensor.Value * 1.1; // Simulated processing
            }
            return data;
        });

        _writeBlock = new ActionBlock<SensorData[]>(async data =>
        {
            Console.WriteLine("Writing processed data...");
            await File.WriteAllTextAsync("processed_data.json", JsonSerializer.Serialize(data));
        });

        _readBlock.LinkTo(_processBlock, new DataflowLinkOptions { PropagateCompletion = true });
        _processBlock.LinkTo(_writeBlock, new DataflowLinkOptions { PropagateCompletion = true });
    }

    public async Task ProcessAsync(string inputFile, string outputFile)
    {
        await _readBlock.SendAsync(inputFile);
        _readBlock.Complete();
        await _writeBlock.Completion;
    }
}
