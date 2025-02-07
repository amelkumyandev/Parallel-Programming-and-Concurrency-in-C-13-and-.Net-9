namespace TplDeepDiveDemo
{
    // 3. Task Properties Example

    public static class TaskPropertiesExample
    {
        public static async Task CheckTaskProperties()
        {
            Task faultyTask = null;

            try
            {
                faultyTask = Task.Run(() =>
                {
                    throw new InvalidOperationException("Simulated error from TaskPropertiesExample");
                });
                await faultyTask; // rethrows the exception

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TaskPropertiesExample] Task instance is null?   {faultyTask == null}");
                Console.WriteLine($"[TaskPropertiesExample] Task completed? {faultyTask?.IsCompleted}");
                Console.WriteLine($"[TaskPropertiesExample] Task canceled? {faultyTask?.IsCanceled}");
                Console.WriteLine($"[TaskPropertiesExample] Task faulted? {faultyTask?.IsFaulted}");
                Console.WriteLine($"[TaskPropertiesExample] Exception: {ex.Message}");
            }
        }
    }

}
