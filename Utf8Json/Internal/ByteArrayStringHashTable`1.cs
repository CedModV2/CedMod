// Utf8Json.Internal.ByteArrayStringHashTable<T>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Utf8Json.Internal;

internal class ByteArrayStringHashTable<T> : IEnumerable<KeyValuePair<string, T>>, IEnumerable
{
	private struct Entry
	{
		public byte[] Key;

		public T Value;

		public override string ToString()
		{
			return "(" + Encoding.UTF8.GetString(Key) + ", " + Value + ")";
		}
	}

	private readonly Entry[][] buckets;

	private readonly ulong indexFor;

	public ByteArrayStringHashTable(int capacity)
		: this(capacity, 0.42f)
	{
	}

	public ByteArrayStringHashTable(int capacity, float loadFactor)
	{
		int num = CalculateCapacity(capacity, loadFactor);
		buckets = new Entry[num][];
		indexFor = (ulong)((long)buckets.Length - 1L);
	}

	public void Add(string key, T value)
	{
		if (!TryAddInternal(Encoding.UTF8.GetBytes(key), value))
		{
			throw new ArgumentException("Key was already exists. Key:" + key);
		}
	}

	public void Add(byte[] key, T value)
	{
		if (!TryAddInternal(key, value))
		{
			throw new ArgumentException("Key was already exists. Key:" + key);
		}
	}

	private bool TryAddInternal(byte[] key, T value)
	{
		ulong num = ByteArrayGetHashCode(key, 0, key.Length);
		Entry entry = default(Entry);
		entry.Key = key;
		entry.Value = value;
		Entry entry2 = entry;
		Entry[] array = buckets[num & indexFor];
		if (array == null)
		{
			buckets[num & indexFor] = new Entry[1]
			{
				entry2
			};
		}
		else
		{
			for (int i = 0; i < array.Length; i++)
			{
				byte[] key2 = array[i].Key;
				if (ByteArrayComparer.Equals(key, 0, key.Length, key2))
				{
					return false;
				}
			}
			Entry[] array2 = new Entry[array.Length + 1];
			Array.Copy(array, array2, array.Length);
			array = array2;
			array[array.Length - 1] = entry2;
			buckets[num & indexFor] = array;
		}
		return true;
	}

	public bool TryGetValue(ArraySegment<byte> key, out T value)
	{
		Entry[][] array = buckets;
		ulong num = ByteArrayGetHashCode(key.Array, key.Offset, key.Count);
		Entry[] array2 = array[num & indexFor];
		if (array2 != null)
		{
			Entry entry = array2[0];
			if (ByteArrayComparer.Equals(key.Array, key.Offset, key.Count, entry.Key))
			{
				value = entry.Value;
				return true;
			}
			for (int i = 1; i < array2.Length; i++)
			{
				Entry entry2 = array2[i];
				if (ByteArrayComparer.Equals(key.Array, key.Offset, key.Count, entry2.Key))
				{
					value = entry2.Value;
					return true;
				}
			}
		}
		value = default(T);
		return false;
	}

	private static ulong ByteArrayGetHashCode(byte[] x, int offset, int count)
	{
		uint num = 0u;
		if (x != null)
		{
			int num2 = offset + count;
			num = 2166136261u;
			for (int i = offset; i < num2; i++)
			{
				num = (x[i] ^ num) * 16777619;
			}
		}
		return num;
	}

	private static int CalculateCapacity(int collectionSize, float loadFactor)
	{
		int num = (int)((float)collectionSize / loadFactor);
		int num2;
		for (num2 = 1; num2 < num; num2 <<= 1)
		{
		}
		if (num2 < 8)
		{
			return 8;
		}
		return num2;
	}

	public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
	{
		Entry[][] array = buckets;
		Entry[][] array2 = array;
		foreach (Entry[] array3 in array2)
		{
			if (array3 != null)
			{
				Entry[] array4 = array3;
				for (int j = 0; j < array4.Length; j++)
				{
					Entry entry = array4[j];
					yield return new KeyValuePair<string, T>(Encoding.UTF8.GetString(entry.Key), entry.Value);
				}
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
