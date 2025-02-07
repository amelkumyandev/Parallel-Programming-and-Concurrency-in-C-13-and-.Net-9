namespace ParallelProcessingProject.Services
{
    public class DataProcessor
    {
        private readonly List<int> _data;

        public DataProcessor()
        {
            _data = Enumerable.Range(1, 1000000).ToList();
        }

        public void ProcessSequentially()
        {
            foreach (var item in _data)
            {
                Compute(item);
            }
        }

        public void ProcessInParallel()
        {
            Parallel.ForEach(_data, item =>
            {
                Compute(item);
            });
        }

        private void Compute(int value)
        {
            // Simulatio CPU bound job
            Math.Sqrt(value);
            Math.Abs(value);
            Math.Log2(value);
            Math.Log10(value);
            Math.SinCos(value);
            Math.Tan(value - 5);
        }
    }
}
