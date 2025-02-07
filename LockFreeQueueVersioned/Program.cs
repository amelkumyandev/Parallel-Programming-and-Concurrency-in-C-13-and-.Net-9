namespace LockFreeQueueFinalDemo
{
    /// <summary>
    /// A lock-free queue based on the Michael-Scott algorithm (multi-producer multi-consumer).
    /// It relies on .NET's GC to mitigate ABA and doesn't use version counters.
    /// </summary>
    public class LockFreeQueue<T>
    {
        /// <summary>
        /// Internal node representation.
        /// Each node holds a value and a reference to the next node.
        /// </summary>
        private class Node
        {
            public T Value;
            public Node? Next;
            public Node(T value) => Value = value;
        }

        // We maintain a dummy sentinel node, so _head and _tail are never null.
        private Node _head;
        private Node _tail;

        /// <summary>
        /// Initializes the queue with a dummy sentinel node.
        /// </summary>
        public LockFreeQueue()
        {
            var sentinel = new Node(default!);
            _head = sentinel;
            _tail = sentinel;
        }

        /// <summary>
        /// Enqueues an item at the tail of the queue using lock-free CAS logic.
        /// </summary>
        public void Enqueue(T value)
        {
            var newNode = new Node(value);
            while (true)
            {
                Node tail = _tail;
                Node? next = tail.Next;

                // If tail.Next == null, tail points to the actual last node
                if (next == null)
                {
                    // Attempt to link our new node at tail.Next
                    if (Interlocked.CompareExchange(ref tail.Next, newNode, null) == null)
                    {
                        // Successfully appended the new node
                        Interlocked.CompareExchange(ref _tail, newNode, tail);
                        return;
                    }
                }
                else
                {
                    // Help move tail forward if tail is lagging
                    Interlocked.CompareExchange(ref _tail, next, tail);
                }
            }
        }

        /// <summary>
        /// Attempts to dequeue an item from the head of the queue.
        /// Returns false if the queue is empty.
        /// </summary>
        public bool TryDequeue(out T? value)
        {
            while (true)
            {
                Node head = _head;
                Node tail = _tail;
                Node? next = head.Next;

                // If head == tail and there's no next, the queue is empty
                if (head == tail)
                {
                    if (next == null)
                    {
                        value = default;
                        return false; // empty
                    }
                    // If next != null, tail is outdated, help move it forward
                    Interlocked.CompareExchange(ref _tail, next, tail);
                }
                else
                {
                    // We have something to dequeue
                    if (Interlocked.CompareExchange(ref _head, next!, head) == head)
                    {
                        // The item is in next.Value
                        value = next!.Value;
                        return true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Demo program to stress test the LockFreeQueue with multiple producers and consumers.
    /// </summary>
    internal class Program
    {
        private static async Task Main()
        {
            Console.WriteLine("=== Lock-Free Michael-Scott Queue (No Versioning) Demo ===");

            var queue = new LockFreeQueue<int>();

            // Number of producer and consumer tasks
            int producerCount = 4;
            int consumerCount = 4;
            // Each producer enqueues 50k items
            int itemsPerProducer = 50_000;

            // 1) Start producer tasks
            Task[] producers = new Task[producerCount];
            for (int p = 0; p < producerCount; p++)
            {
                int localProducer = p;
                producers[p] = Task.Run(() =>
                {
                    for (int i = 0; i < itemsPerProducer; i++)
                    {
                        int item = localProducer * itemsPerProducer + i;
                        queue.Enqueue(item);
                    }
                });
            }

            // 2) Start consumer tasks
            int totalItems = producerCount * itemsPerProducer;
            int consumedCount = 0;
            object lockObj = new object();

            Task[] consumers = new Task[consumerCount];
            for (int c = 0; c < consumerCount; c++)
            {
                consumers[c] = Task.Run(() =>
                {
                    while (true)
                    {
                        if (queue.TryDequeue(out int val))
                        {
                            // threadsafe increment
                            lock (lockObj)
                            {
                                consumedCount++;
                            }
                        }
                        else
                        {
                            // If we've already consumed everything, break
                            if (Volatile.Read(ref consumedCount) >= totalItems)
                            {
                                break;
                            }
                            // Otherwise yield, to avoid busy-wait
                            Thread.Yield();
                        }
                    }
                });
            }

            // Wait for all producers and consumers to finish
            await Task.WhenAll(producers);
            await Task.WhenAll(consumers);

            // 3) Validate results
            Console.WriteLine($"Total items enqueued: {totalItems}");
            Console.WriteLine($"Total items dequeued: {consumedCount}");

            // Final check: queue should be empty
            bool anyLeft = queue.TryDequeue(out _);
            Console.WriteLine($"Queue empty after consumption? {!anyLeft}");

            Console.WriteLine("Demo complete. Press any key to exit...");
            Console.ReadKey();
        }
    }
}
