using System;

namespace PeterHan.PLib.Core;

/// <summary>
/// A priority queue that includes a paired value.
/// </summary>
/// <typeparam name="K">The type to use for the sorting in the PriorityQueue.</typeparam>
/// <typeparam name="V">The type to include as extra data.</typeparam>
public sealed class PriorityDictionary<K, V> : PriorityQueue<PriorityDictionary<K, V>.PriorityQueuePair> where K : IComparable<K>
{
	/// <summary>
	/// Stores a value with the key that is used for comparison.
	/// </summary>
	public sealed class PriorityQueuePair : IComparable<PriorityQueuePair>
	{
		/// <summary>
		/// Retrieves the key of this QueueItem.
		/// </summary>
		public K Key { get; }

		/// <summary>
		/// Retrieves the value of this QueueItem.
		/// </summary>
		public V Value { get; }

		/// <summary>
		/// Creates a new priority queue pair.
		/// </summary>
		/// <param name="key">The item key.</param>
		/// <param name="value">The item value.</param>
		public PriorityQueuePair(K key, V value)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			Key = key;
			Value = value;
		}

		public int CompareTo(PriorityQueuePair other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("other");
			}
			return Key.CompareTo(other.Key);
		}

		public override bool Equals(object obj)
		{
			if (obj is PriorityQueuePair { Key: var key })
			{
				return key.Equals(Key);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Key.GetHashCode();
		}

		public override string ToString()
		{
			return "PriorityQueueItem[key=" + Key?.ToString() + ",value=" + Value?.ToString() + "]";
		}
	}

	/// <summary>
	/// Creates a new PriorityDictionary&lt;<typeparamref name="K" />,
	/// <typeparamref name="V" />&gt; with the default initial capacity.
	/// </summary>
	public PriorityDictionary()
	{
	}

	/// <summary>
	/// Creates a new PriorityDictionary&lt;<typeparamref name="K" />,
	/// <typeparamref name="V" />&gt; with the specified initial capacity.
	/// </summary>
	/// <param name="capacity">The initial capacity of this dictionary.</param>
	public PriorityDictionary(int capacity)
		: base(capacity)
	{
	}

	/// <summary>
	/// Removes and returns the smallest object in the
	/// PriorityDictionary&lt;<typeparamref name="K" />, <typeparamref name="V" />&gt;.
	///
	/// If multiple objects are the smallest object, an unspecified one is returned.
	/// </summary>
	/// <param name="key">The key of the object removed.</param>
	/// <param name="value">The value of the object removed.</param>
	/// <exception cref="T:System.InvalidOperationException">If this dictionary is empty.</exception>
	public void Dequeue(out K key, out V value)
	{
		PriorityQueuePair priorityQueuePair = Dequeue();
		key = priorityQueuePair.Key;
		value = priorityQueuePair.Value;
	}

	/// <summary>
	/// Adds an object to the PriorityDictionary&lt;<typeparamref name="K" />,
	/// <typeparamref name="V" />&gt;.
	/// </summary>
	/// <param name="key">The key to add to this PriorityDictionary.</param>
	/// <param name="value">The value to add to this PriorityDictionary.</param>
	/// <exception cref="T:System.ArgumentNullException">If item is null.</exception>
	public void Enqueue(K key, V value)
	{
		Enqueue(new PriorityQueuePair(key, value));
	}

	/// <summary>
	/// Returns the smallest object in the PriorityDictionary&lt;<typeparamref name="K" />,
	/// <typeparamref name="V" />&gt; without removing it.
	///
	/// If multiple objects are the smallest object, an unspecified one is returned.
	/// </summary>
	/// <param name="key">The key of the smallest object.</param>
	/// <param name="value">The value of the smallest object.</param>
	/// <exception cref="T:System.InvalidOperationException">If this dictionary is empty.</exception>
	public void Peek(out K key, out V value)
	{
		PriorityQueuePair priorityQueuePair = Peek();
		key = priorityQueuePair.Key;
		value = priorityQueuePair.Value;
	}
}
