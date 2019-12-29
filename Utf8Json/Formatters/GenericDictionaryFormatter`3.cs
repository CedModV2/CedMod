// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.GenericDictionaryFormatter`3
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

namespace Utf8Json.Formatters
{
  public sealed class GenericDictionaryFormatter<TKey, TValue, TDictionary> : DictionaryFormatterBase<TKey, TValue, TDictionary>
    where TDictionary : class, IDictionary<TKey, TValue>, new()
  {
    protected override void Add(ref TDictionary collection, int index, TKey key, TValue value)
    {
      collection.Add(key, value);
    }

    protected override TDictionary Create()
    {
      return new TDictionary();
    }
  }
}
