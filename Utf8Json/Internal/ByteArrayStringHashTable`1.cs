// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.ByteArrayStringHashTable`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Utf8Json.Internal
{
  internal class ByteArrayStringHashTable<T> : IEnumerable<KeyValuePair<string, T>>, IEnumerable
  {
    private readonly ByteArrayStringHashTable<T>.Entry[][] buckets;
    private readonly ulong indexFor;

    public ByteArrayStringHashTable(int capacity)
      : this(capacity, 0.42f)
    {
    }

    public ByteArrayStringHashTable(int capacity, float loadFactor)
    {
      this.buckets = new ByteArrayStringHashTable<T>.Entry[ByteArrayStringHashTable<T>.CalculateCapacity(capacity, loadFactor)][];
      this.indexFor = (ulong) this.buckets.Length - 1UL;
    }

    public void Add(string key, T value)
    {
      if (!this.TryAddInternal(Encoding.UTF8.GetBytes(key), value))
        throw new ArgumentException("Key was already exists. Key:" + key);
    }

    public void Add(byte[] key, T value)
    {
      if (!this.TryAddInternal(key, value))
        throw new ArgumentException("Key was already exists. Key:" + (object) key);
    }

    private bool TryAddInternal(byte[] key, T value)
    {
      ulong hashCode = ByteArrayStringHashTable<T>.ByteArrayGetHashCode(key, 0, key.Length);
      ByteArrayStringHashTable<T>.Entry entry = new ByteArrayStringHashTable<T>.Entry()
      {
        Key = key,
        Value = value
      };
      ByteArrayStringHashTable<T>.Entry[] bucket = this.buckets[checked ((ulong) (unchecked ((long) hashCode) & unchecked ((long) this.indexFor)))];
      if (bucket == null)
      {
        this.buckets[(IntPtr) checked ((ulong) (unchecked ((long) hashCode) & unchecked ((long) this.indexFor)))] = new ByteArrayStringHashTable<T>.Entry[1]
        {
          entry
        };
      }
      else
      {
        for (int index = 0; index < bucket.Length; ++index)
        {
          byte[] key1 = bucket[index].Key;
          if (ByteArrayComparer.Equals(key, 0, key.Length, key1))
            return false;
        }
        ByteArrayStringHashTable<T>.Entry[] entryArray1 = new ByteArrayStringHashTable<T>.Entry[bucket.Length + 1];
        Array.Copy((Array) bucket, (Array) entryArray1, bucket.Length);
        ByteArrayStringHashTable<T>.Entry[] entryArray2 = entryArray1;
        entryArray2[entryArray2.Length - 1] = entry;
        this.buckets[checked ((ulong) (unchecked ((long) hashCode) & unchecked ((long) this.indexFor)))] = entryArray2;
      }
      return true;
    }

    public bool TryGetValue(ArraySegment<byte> key, out T value)
    {
      ByteArrayStringHashTable<T>.Entry[] bucket = this.buckets[checked ((ulong) (unchecked ((long) ByteArrayStringHashTable<T>.ByteArrayGetHashCode(key.Array, key.Offset, key.Count)) & unchecked ((long) this.indexFor)))];
      if (bucket != null)
      {
        ByteArrayStringHashTable<T>.Entry entry1 = bucket[0];
        if (ByteArrayComparer.Equals(key.Array, key.Offset, key.Count, entry1.Key))
        {
          value = entry1.Value;
          return true;
        }
        for (int index = 1; index < bucket.Length; ++index)
        {
          ByteArrayStringHashTable<T>.Entry entry2 = bucket[index];
          if (ByteArrayComparer.Equals(key.Array, key.Offset, key.Count, entry2.Key))
          {
            value = entry2.Value;
            return true;
          }
        }
      }
      value = default (T);
      return false;
    }

    private static ulong ByteArrayGetHashCode(byte[] x, int offset, int count)
    {
      uint num1 = 0;
      if (x != null)
      {
        int num2 = offset + count;
        num1 = 2166136261U;
        for (int index = offset; index < num2; ++index)
          num1 = (uint) (((int) x[index] ^ (int) num1) * 16777619);
      }
      return (ulong) num1;
    }

    private static int CalculateCapacity(int collectionSize, float loadFactor)
    {
      int num1 = (int) ((double) collectionSize / (double) loadFactor);
      int num2 = 1;
      while (num2 < num1)
        num2 <<= 1;
      return num2 < 8 ? 8 : num2;
    }

    public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
    {
      ByteArrayStringHashTable<T>.Entry[][] entryArray1 = this.buckets;
      for (int index1 = 0; index1 < entryArray1.Length; ++index1)
      {
        ByteArrayStringHashTable<T>.Entry[] entryArray = entryArray1[index1];
        if (entryArray != null)
        {
          ByteArrayStringHashTable<T>.Entry[] entryArray2 = entryArray;
          for (int index2 = 0; index2 < entryArray2.Length; ++index2)
          {
            ByteArrayStringHashTable<T>.Entry entry = entryArray2[index2];
            yield return new KeyValuePair<string, T>(Encoding.UTF8.GetString(entry.Key), entry.Value);
          }
          entryArray2 = (ByteArrayStringHashTable<T>.Entry[]) null;
        }
      }
      entryArray1 = (ByteArrayStringHashTable<T>.Entry[][]) null;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) this.GetEnumerator();
    }

    private struct Entry
    {
      public byte[] Key;
      public T Value;

      public override string ToString()
      {
        return "(" + Encoding.UTF8.GetString(this.Key) + ", " + (object) this.Value + ")";
      }
    }
  }
}
