namespace TplDeepDiveDemo
{
    // 5. Attached Child Tasks Example
    public static class AttachedChildTasksExample
    {
        public static async Task RunParentChildExample()
        {
            var parentTask = ParentTaskWithChildren();
            await parentTask;
            Console.WriteLine("[AttachedChildTasksExample] Parent task completed after all children.");
        }

        private static Task ParentTaskWithChildren()
        {
            return Task.Factory.StartNew(() =>
            {
                Console.WriteLine("[AttachedChildTasksExample] Parent task starting...");
                for (int i = 0; i < 3; i++)
                {
                    // create attached child tasks
                    Task.Factory.StartNew(
                        index => ChildWork((int)index),
                        i,
                        CancellationToken.None,
                        TaskCreationOptions.AttachedToParent,
                        TaskScheduler.Default);
                }
            });
        }

        private static void ChildWork(int index)
        {
            Console.WriteLine($"[AttachedChildTasksExample] Child {index} started.");
            Thread.Sleep(500);
            Console.WriteLine($"[AttachedChildTasksExample] Child {index} completed.");
        }
    }
}
