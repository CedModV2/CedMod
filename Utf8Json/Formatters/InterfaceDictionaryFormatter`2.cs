// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.InterfaceDictionaryFormatter`2
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

namespace Utf8Json.Formatters
{
  public sealed class InterfaceDictionaryFormatter<TKey, TValue> : DictionaryFormatterBase<TKey, TValue, Dictionary<TKey, TValue>, IDictionary<TKey, TValue>>
  {
    protected override void Add(
      ref Dictionary<TKey, TValue> collection,
      int index,
      TKey key,
      TValue value)
    {
      collection.Add(key, value);
    }

    protected override Dictionary<TKey, TValue> Create()
    {
      return new Dictionary<TKey, TValue>();
    }

    protected override IDictionary<TKey, TValue> Complete(
      ref Dictionary<TKey, TValue> intermediateCollection)
    {
      return (IDictionary<TKey, TValue>) intermediateCollection;
    }
  }
}
