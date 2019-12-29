// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.InterfaceLookupFormatter`2
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Linq;

namespace Utf8Json.Formatters
{
  public sealed class InterfaceLookupFormatter<TKey, TElement> : IJsonFormatter<ILookup<TKey, TElement>>, IJsonFormatter
  {
    public void Serialize(
      ref JsonWriter writer,
      ILookup<TKey, TElement> value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value == null)
        writer.WriteNull();
      else
        formatterResolver.GetFormatterWithVerify<IEnumerable<IGrouping<TKey, TElement>>>().Serialize(ref writer, value.AsEnumerable<IGrouping<TKey, TElement>>(), formatterResolver);
    }

    public ILookup<TKey, TElement> Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (ILookup<TKey, TElement>) null;
      if (reader.ReadIsNull())
        return (ILookup<TKey, TElement>) null;
      int count = 0;
      IJsonFormatter<IGrouping<TKey, TElement>> formatterWithVerify = formatterResolver.GetFormatterWithVerify<IGrouping<TKey, TElement>>();
      Dictionary<TKey, IGrouping<TKey, TElement>> groupings = new Dictionary<TKey, IGrouping<TKey, TElement>>();
      reader.ReadIsBeginArrayWithVerify();
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
      {
        IGrouping<TKey, TElement> grouping = formatterWithVerify.Deserialize(ref reader, formatterResolver);
        groupings.Add(grouping.Key, grouping);
      }
      return (ILookup<TKey, TElement>) new Lookup<TKey, TElement>(groupings);
    }
  }
}
