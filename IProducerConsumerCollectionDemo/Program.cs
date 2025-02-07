using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

public class BoundedRingBuffer<T> : IProducerConsumerCollection<T>
{
    private readonly T[] _buffer;
    private int _head;          // Index of the next item to read
    private int _tail;          // Index of the next write position
    private int _count;         // Current number of items

    private readonly object _syncRoot = new object();

    public BoundedRingBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity),
                "Capacity must be greater than 0.");
        _buffer = new T[capacity];
        _head = 0;
        _tail = 0;
        _count = 0;
    }

    public bool TryAdd(T item)
    {
        lock (_syncRoot)
        {
            // If full, we cannot insert
            if (_count == _buffer.Length)
                return false;

            _buffer[_tail] = item;
            _tail = (_tail + 1) % _buffer.Length;
            _count++;
            return true;
        }
    }

    public bool TryTake(out T item)
    {
        lock (_syncRoot)
        {
            // If empty, cannot remove
            if (_count == 0)
            {
                item = default!;
                return false;
            }

            item = _buffer[_head];
            _buffer[_head] = default!; // Clear for GC
            _head = (_head + 1) % _buffer.Length;
            _count--;
            return true;
        }
    }

    // -----------------------------
    // IProducerConsumerCollection<T> / ICollection members
    // -----------------------------

    public void CopyTo(T[] array, int index)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (index < 0 || (index + _count) > array.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        lock (_syncRoot)
        {
            int snapshotCount = _count;
            for (int i = 0; i < snapshotCount; i++)
            {
                int bufferIndex = (_head + i) % _buffer.Length;
                array[index + i] = _buffer[bufferIndex];
            }
        }
    }

    public T[] ToArray()
    {
        lock (_syncRoot)
        {
            var result = new T[_count];
            CopyTo(result, 0);
            return result;
        }
    }

    public int Count
    {
        get
        {
            lock (_syncRoot)
            {
                return _count;
            }
        }
    }

    public bool IsSynchronized => false;     // Typically 'false' in concurrent collections
    public object SyncRoot => _syncRoot;     // Rarely used in .NET concurrency, but required by ICollection

    public IEnumerator<T> GetEnumerator()
    {
        // Return a snapshot enumerator
        T[] snapshot;
        lock (_syncRoot)
        {
            snapshot = ToArray();
        }
        foreach (T item in snapshot)
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
