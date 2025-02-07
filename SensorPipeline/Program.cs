var pipeline = new SensorPipeline();
await pipeline.ProcessAsync("sensors.json", "processed_data.json");
