// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.SortedDictionaryFormatter`2
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

namespace Utf8Json.Formatters
{
  public sealed class SortedDictionaryFormatter<TKey, TValue> : DictionaryFormatterBase<TKey, TValue, SortedDictionary<TKey, TValue>, SortedDictionary<TKey, TValue>.Enumerator, SortedDictionary<TKey, TValue>>
  {
    protected override void Add(
      ref SortedDictionary<TKey, TValue> collection,
      int index,
      TKey key,
      TValue value)
    {
      collection.Add(key, value);
    }

    protected override SortedDictionary<TKey, TValue> Complete(
      ref SortedDictionary<TKey, TValue> intermediateCollection)
    {
      return intermediateCollection;
    }

    protected override SortedDictionary<TKey, TValue> Create()
    {
      return new SortedDictionary<TKey, TValue>();
    }

    protected override SortedDictionary<TKey, TValue>.Enumerator GetSourceEnumerator(
      SortedDictionary<TKey, TValue> source)
    {
      return source.GetEnumerator();
    }
  }
}
