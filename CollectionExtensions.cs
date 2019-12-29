// Decompiled with JetBrains decompiler
// Type: CollectionExtensions
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public static class CollectionExtensions
{
  public static void ShuffleList<T>(this IList<T> list)
  {
    System.Random random = new System.Random();
    int count = list.Count;
    while (count > 1)
    {
      --count;
      int index = random.Next(count + 1);
      T obj = list[index];
      list[index] = list[count];
      list[count] = obj;
    }
  }

  public static void ShuffleListSecure<T>(this IList<T> list)
  {
    using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
    {
      int count = list.Count;
      while (count > 1)
      {
        byte[] data = new byte[1];
        do
        {
          cryptoServiceProvider.GetBytes(data);
        }
        while ((int) data[0] >= count * ((int) byte.MaxValue / count));
        int index = (int) data[0] % count;
        --count;
        T obj = list[index];
        list[index] = list[count];
        list[count] = obj;
      }
    }
  }

  public static bool IsEmpty(this Array array)
  {
    return array.Length == 0;
  }

  public static bool IsEmpty<T>(this T[] array)
  {
    return array.Length == 0;
  }

  public static bool IsEmpty<T>(this List<T> list)
  {
    return list.Count == 0;
  }

  public static bool IsEmpty<T>(this Queue<T> queue)
  {
    return queue.Count == 0;
  }

  public static bool IsEmpty<T>(this Stack<T> stack)
  {
    return stack.Count == 0;
  }

  public static bool IsEmpty<T>(this HashSet<T> set)
  {
    return set.Count == 0;
  }

  public static bool IsEmpty<T>(this SortedSet<T> set)
  {
    return set.Count == 0;
  }

  public static bool IsEmpty<T>(this SyncList<T> list)
  {
    return list.Count == 0;
  }

  public static bool IsEmpty<T>(this SyncSet<T> set)
  {
    return set.Count == 0;
  }

  public static bool IsEmpty<TKey, TValue>(this SyncDictionary<TKey, TValue> dictionary)
  {
    return dictionary.Count == 0;
  }

  public static bool IsEmpty<T>(this ICollection<T> collection)
  {
    return collection.Count == 0;
  }

  public static bool IsEmpty<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
  {
    return dictionary.Count == 0;
  }

  public static bool IsEmpty<T>(this IEnumerable<T> iEnumerable)
  {
    return !iEnumerable.Any<T>();
  }

  public static void EnsureCapacity<T>(this List<T> list, int capacity)
  {
    if (list.Capacity >= capacity)
      return;
    list.Capacity = capacity;
  }

  public static int IndexOf<T>(this T[] array, T obj)
  {
    for (int index = 0; index < array.Length; ++index)
    {
      if (EqualityComparer<T>.Default.Equals(array[index], obj))
        return index;
    }
    return -1;
  }

  public static int LastIndexOf<T>(this T[] array, T obj)
  {
    for (int index = array.Length - 1; index >= 0; --index)
    {
      if (EqualityComparer<T>.Default.Equals(array[index], obj))
        return index;
    }
    return -1;
  }

  public static bool Contains<T>(this T[] array, T obj)
  {
    for (int index = 0; index < array.Length; ++index)
    {
      if (EqualityComparer<T>.Default.Equals(array[index], obj))
        return true;
    }
    return false;
  }

  public static void Reverse<T>(this T[] array)
  {
    int index = 0;
    for (int length = array.Length; index < length; --length)
    {
      T obj = array[index];
      array[index] = array[length];
      array[length] = obj;
      ++index;
    }
  }

  public static bool Contains(this string[] array, string str, StringComparison comparison = StringComparison.Ordinal)
  {
    for (int index = 0; index < array.Length; ++index)
    {
      if (string.Equals(array[index], str, comparison))
        return true;
    }
    return false;
  }

  public static bool Contains(this List<string> list, string str, StringComparison comparison = StringComparison.Ordinal)
  {
    for (int index = 0; index < list.Count; ++index)
    {
      if (string.Equals(list[index], str, comparison))
        return true;
    }
    return false;
  }

  public static bool TryGet<T>(this T[] array, int index, out T element)
  {
    if (index > -1 && index < array.Length)
    {
      element = array[index];
      return true;
    }
    element = default (T);
    return false;
  }

  public static bool TryGet<T>(this List<T> list, int index, out T element)
  {
    if (index > -1 && index < list.Count)
    {
      element = list[index];
      return true;
    }
    element = default (T);
    return false;
  }

  public static bool TryDequeue<T>(this Queue<T> queue, out T element)
  {
    if (queue.Count > 0)
    {
      element = queue.Dequeue();
      return true;
    }
    element = default (T);
    return false;
  }

  public static T[] ToArray<T>(this Array array)
  {
    T[] objArray = new T[array.Length];
    array.CopyTo((Array) objArray, 0);
    return objArray;
  }

  public static int IndexOf(this GameObject[] array, GameObject obj)
  {
    for (int index = 0; index < array.Length; ++index)
    {
      if ((UnityEngine.Object) array[index] == (UnityEngine.Object) obj)
        return index;
    }
    return -1;
  }

  public static int IndexOf(this List<GameObject> list, GameObject obj)
  {
    for (int index = 0; index < list.Count; ++index)
    {
      if ((UnityEngine.Object) list[index] == (UnityEngine.Object) obj)
        return index;
    }
    return -1;
  }

  public static bool Contains(this GameObject[] array, GameObject obj)
  {
    for (int index = 0; index < array.Length; ++index)
    {
      if ((UnityEngine.Object) array[index] == (UnityEngine.Object) obj)
        return true;
    }
    return false;
  }

  public static bool Contains(this List<GameObject> list, GameObject obj)
  {
    for (int index = 0; index < list.Count; ++index)
    {
      if ((UnityEngine.Object) list[index] == (UnityEngine.Object) obj)
        return true;
    }
    return false;
  }
}
