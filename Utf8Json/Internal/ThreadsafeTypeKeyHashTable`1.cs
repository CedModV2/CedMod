// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.ThreadsafeTypeKeyHashTable`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Threading;

namespace Utf8Json.Internal
{
  internal class ThreadsafeTypeKeyHashTable<TValue>
  {
    private readonly object writerLock = new object();
    private ThreadsafeTypeKeyHashTable<TValue>.Entry[] buckets;
    private int size;
    private readonly float loadFactor;

    public ThreadsafeTypeKeyHashTable(int capacity = 4, float loadFactor = 0.75f)
    {
      this.buckets = new ThreadsafeTypeKeyHashTable<TValue>.Entry[ThreadsafeTypeKeyHashTable<TValue>.CalculateCapacity(capacity, loadFactor)];
      this.loadFactor = loadFactor;
    }

    public bool TryAdd(Type key, TValue value)
    {
      return this.TryAdd(key, (Func<Type, TValue>) (_ => value));
    }

    public bool TryAdd(Type key, Func<Type, TValue> valueFactory)
    {
      return this.TryAddInternal(key, valueFactory, out TValue _);
    }

    private bool TryAddInternal(
      Type key,
      Func<Type, TValue> valueFactory,
      out TValue resultingValue)
    {
      lock (this.writerLock)
      {
        int capacity = ThreadsafeTypeKeyHashTable<TValue>.CalculateCapacity(this.size + 1, this.loadFactor);
        if (this.buckets.Length < capacity)
        {
          ThreadsafeTypeKeyHashTable<TValue>.Entry[] buckets = new ThreadsafeTypeKeyHashTable<TValue>.Entry[capacity];
          for (int index = 0; index < this.buckets.Length; ++index)
          {
            for (ThreadsafeTypeKeyHashTable<TValue>.Entry entry = this.buckets[index]; entry != null; entry = entry.Next)
            {
              ThreadsafeTypeKeyHashTable<TValue>.Entry newEntryOrNull = new ThreadsafeTypeKeyHashTable<TValue>.Entry()
              {
                Key = entry.Key,
                Value = entry.Value,
                Hash = entry.Hash
              };
              this.AddToBuckets(buckets, key, newEntryOrNull, (Func<Type, TValue>) null, out resultingValue);
            }
          }
          int num = this.AddToBuckets(buckets, key, (ThreadsafeTypeKeyHashTable<TValue>.Entry) null, valueFactory, out resultingValue) ? 1 : 0;
          ThreadsafeTypeKeyHashTable<TValue>.VolatileWrite(ref this.buckets, buckets);
          if (num != 0)
            ++this.size;
          return num != 0;
        }
        int num1 = this.AddToBuckets(this.buckets, key, (ThreadsafeTypeKeyHashTable<TValue>.Entry) null, valueFactory, out resultingValue) ? 1 : 0;
        if (num1 != 0)
          ++this.size;
        return num1 != 0;
      }
    }

    private bool AddToBuckets(
      ThreadsafeTypeKeyHashTable<TValue>.Entry[] buckets,
      Type newKey,
      ThreadsafeTypeKeyHashTable<TValue>.Entry newEntryOrNull,
      Func<Type, TValue> valueFactory,
      out TValue resultingValue)
    {
      int num = newEntryOrNull != null ? newEntryOrNull.Hash : newKey.GetHashCode();
      if (buckets[num & buckets.Length - 1] == null)
      {
        if (newEntryOrNull != null)
        {
          resultingValue = newEntryOrNull.Value;
          ThreadsafeTypeKeyHashTable<TValue>.VolatileWrite(ref buckets[num & buckets.Length - 1], newEntryOrNull);
        }
        else
        {
          resultingValue = valueFactory(newKey);
          ThreadsafeTypeKeyHashTable<TValue>.VolatileWrite(ref buckets[num & buckets.Length - 1], new ThreadsafeTypeKeyHashTable<TValue>.Entry()
          {
            Key = newKey,
            Value = resultingValue,
            Hash = num
          });
        }
      }
      else
      {
        ThreadsafeTypeKeyHashTable<TValue>.Entry entry;
        for (entry = buckets[num & buckets.Length - 1]; !(entry.Key == newKey); entry = entry.Next)
        {
          if (entry.Next == null)
          {
            if (newEntryOrNull != null)
            {
              resultingValue = newEntryOrNull.Value;
              ThreadsafeTypeKeyHashTable<TValue>.VolatileWrite(ref entry.Next, newEntryOrNull);
              goto label_12;
            }
            else
            {
              resultingValue = valueFactory(newKey);
              ThreadsafeTypeKeyHashTable<TValue>.VolatileWrite(ref entry.Next, new ThreadsafeTypeKeyHashTable<TValue>.Entry()
              {
                Key = newKey,
                Value = resultingValue,
                Hash = num
              });
              goto label_12;
            }
          }
        }
        resultingValue = entry.Value;
        return false;
      }
label_12:
      return true;
    }

    public bool TryGetValue(Type key, out TValue value)
    {
      ThreadsafeTypeKeyHashTable<TValue>.Entry[] buckets = this.buckets;
      int hashCode = key.GetHashCode();
      ThreadsafeTypeKeyHashTable<TValue>.Entry entry = buckets[hashCode & buckets.Length - 1];
      if (entry != null)
      {
        if (entry.Key == key)
        {
          value = entry.Value;
          return true;
        }
        for (ThreadsafeTypeKeyHashTable<TValue>.Entry next = entry.Next; next != null; next = next.Next)
        {
          if (next.Key == key)
          {
            value = next.Value;
            return true;
          }
        }
      }
      value = default (TValue);
      return false;
    }

    public TValue GetOrAdd(Type key, Func<Type, TValue> valueFactory)
    {
      TValue resultingValue;
      if (this.TryGetValue(key, out resultingValue))
        return resultingValue;
      this.TryAddInternal(key, valueFactory, out resultingValue);
      return resultingValue;
    }

    private static int CalculateCapacity(int collectionSize, float loadFactor)
    {
      int num1 = (int) ((double) collectionSize / (double) loadFactor);
      int num2 = 1;
      while (num2 < num1)
        num2 <<= 1;
      return num2 < 8 ? 8 : num2;
    }

    private static void VolatileWrite(
      ref ThreadsafeTypeKeyHashTable<TValue>.Entry location,
      ThreadsafeTypeKeyHashTable<TValue>.Entry value)
    {
      Thread.MemoryBarrier();
      location = value;
    }

    private static void VolatileWrite(
      ref ThreadsafeTypeKeyHashTable<TValue>.Entry[] location,
      ThreadsafeTypeKeyHashTable<TValue>.Entry[] value)
    {
      Thread.MemoryBarrier();
      location = value;
    }

    private class Entry
    {
      public Type Key;
      public TValue Value;
      public int Hash;
      public ThreadsafeTypeKeyHashTable<TValue>.Entry Next;

      public override string ToString()
      {
        return this.Key.ToString() + "(" + (object) this.Count() + ")";
      }

      private int Count()
      {
        int num = 1;
        for (ThreadsafeTypeKeyHashTable<TValue>.Entry entry = this; entry.Next != null; entry = entry.Next)
          ++num;
        return num;
      }
    }
  }
}
