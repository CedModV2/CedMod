// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.InterfaceGroupingFormatter`2
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using Utf8Json.Formatters.Internal;

namespace Utf8Json.Formatters
{
  public sealed class InterfaceGroupingFormatter<TKey, TElement> : IJsonFormatter<IGrouping<TKey, TElement>>, IJsonFormatter
  {
    public void Serialize(
      ref JsonWriter writer,
      IGrouping<TKey, TElement> value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value == null)
      {
        writer.WriteNull();
      }
      else
      {
        writer.WriteRaw(CollectionFormatterHelper.groupingName[0]);
        formatterResolver.GetFormatterWithVerify<TKey>().Serialize(ref writer, value.Key, formatterResolver);
        writer.WriteRaw(CollectionFormatterHelper.groupingName[1]);
        formatterResolver.GetFormatterWithVerify<IEnumerable<TElement>>().Serialize(ref writer, value.AsEnumerable<TElement>(), formatterResolver);
        writer.WriteEndObject();
      }
    }

    public IGrouping<TKey, TElement> Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (IGrouping<TKey, TElement>) null;
      TKey key1 = default (TKey);
      IEnumerable<TElement> elements = (IEnumerable<TElement>) null;
      reader.ReadIsBeginObjectWithVerify();
      int count = 0;
      while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
      {
        ArraySegment<byte> key2 = reader.ReadPropertyNameSegmentRaw();
        int num;
        CollectionFormatterHelper.groupingAutomata.TryGetValueSafe(key2, out num);
        switch (num)
        {
          case 0:
            key1 = formatterResolver.GetFormatterWithVerify<TKey>().Deserialize(ref reader, formatterResolver);
            continue;
          case 1:
            elements = formatterResolver.GetFormatterWithVerify<IEnumerable<TElement>>().Deserialize(ref reader, formatterResolver);
            continue;
          default:
            reader.ReadNextBlock();
            continue;
        }
      }
      return (IGrouping<TKey, TElement>) new Grouping<TKey, TElement>(key1, elements);
    }
  }
}
