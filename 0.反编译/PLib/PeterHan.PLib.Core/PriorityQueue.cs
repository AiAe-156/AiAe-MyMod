using System;
using System.Collections.Generic;

namespace PeterHan.PLib.Core;

/// <summary>
/// A class similar to Queue<typeparamref name="T" /> that allows efficient access to its
/// items in ascending order.
/// </summary>
public class PriorityQueue<T> where T : IComparable<T>
{
	/// <summary>
	/// The heap where the items are stored.
	/// </summary>
	private readonly IList<T> heap;

	/// <summary>
	/// The number of elements in this queue.
	/// </summary>
	public int Count => heap.Count;

	/// <summary>
	/// Returns the index of the specified item's first child. Its second child index is
	/// that index plus one.
	/// </summary>
	/// <param name="index">The item index.</param>
	/// <returns>The index of its first child.</returns>
	private static int ChildIndex(int index)
	{
		return 2 * index + 1;
	}

	/// <summary>
	/// Returns the index of the specified item's parent.
	/// </summary>
	/// <param name="index">The item index.</param>
	/// <returns>The index of its parent.</returns>
	private static int ParentIndex(int index)
	{
		return (index - 1) / 2;
	}

	/// <summary>
	/// Creates a new PriorityQueue&lt;<typeparamref name="T" />&gt; with the default
	/// initial capacity.
	/// </summary>
	public PriorityQueue()
		: this(32)
	{
	}

	/// <summary>
	/// Creates a new PriorityQueue&lt;<typeparamref name="T" />&gt; with the specified
	/// initial capacity.
	/// </summary>
	/// <param name="capacity">The initial capacity of this queue.</param>
	public PriorityQueue(int capacity)
	{
		if (capacity < 1)
		{
			throw new ArgumentException("capacity > 0");
		}
		heap = new List<T>(Math.Max(capacity, 8));
	}

	/// <summary>
	/// Removes all objects from this PriorityQueue&lt;<typeparamref name="T" />&gt;.
	/// </summary>
	public void Clear()
	{
		heap.Clear();
	}

	/// <summary>
	/// Returns whether the specified key is present in this priority queue. This operation
	/// is fairly slow, use with caution.
	/// </summary>
	/// <param name="key">The key to check.</param>
	/// <returns>true if it exists in this priority queue, or false otherwise.</returns>
	public bool Contains(T key)
	{
		return heap.Contains(key);
	}

	/// <summary>
	/// Removes and returns the smallest object in the
	/// PriorityQueue&lt;<typeparamref name="T" />&gt;.
	///
	/// If multiple objects are the smallest object, an unspecified one is returned.
	/// </summary>
	/// <returns>The object that is removed from this PriorityQueue.</returns>
	/// <exception cref="T:System.InvalidOperationException">If this queue is empty.</exception>
	public T Dequeue()
	{
		int index = 0;
		int count = heap.Count;
		if (count == 0)
		{
			throw new InvalidOperationException("Queue is empty");
		}
		T result = heap[0];
		heap[0] = heap[--count];
		heap.RemoveAt(count);
		int num;
		while ((num = ChildIndex(index)) < count)
		{
			T value = heap[index];
			T val = heap[num];
			if (num < count - 1)
			{
				T val2 = heap[num + 1];
				if (val.CompareTo(val2) > 0)
				{
					num++;
					val = val2;
				}
			}
			if (value.CompareTo(val) < 0)
			{
				break;
			}
			heap[num] = value;
			heap[index] = val;
			index = num;
		}
		return result;
	}

	/// <summary>
	/// Adds an object to the PriorityQueue&lt;<typeparamref name="T" />&gt;.
	/// </summary>
	/// <param name="item">The object to add to this PriorityQueue.</param>
	/// <exception cref="T:System.ArgumentNullException">If item is null.</exception>
	public void Enqueue(T item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		int num = heap.Count;
		heap.Add(item);
		while (num > 0)
		{
			int num2 = ParentIndex(num);
			T value = heap[num];
			T val = heap[num2];
			if (value.CompareTo(val) <= 0)
			{
				heap[num2] = value;
				heap[num] = val;
				num = num2;
				continue;
			}
			break;
		}
	}

	/// <summary>
	/// Returns the smallest object in the PriorityQueue&lt;<typeparamref name="T" />&gt;
	/// without removing it.
	///
	/// If multiple objects are the smallest object, an unspecified one is returned.
	/// </summary>
	/// <returns>The smallest object in this PriorityQueue.</returns>
	/// <exception cref="T:System.InvalidOperationException">If this queue is empty.</exception>
	public T Peek()
	{
		if (Count == 0)
		{
			throw new InvalidOperationException("Queue is empty");
		}
		return heap[0];
	}

	public override string ToString()
	{
		return heap.ToString();
	}
}
