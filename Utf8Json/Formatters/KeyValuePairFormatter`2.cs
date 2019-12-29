// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.KeyValuePairFormatter`2
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using Utf8Json.Formatters.Internal;

namespace Utf8Json.Formatters
{
  public sealed class KeyValuePairFormatter<TKey, TValue> : IJsonFormatter<KeyValuePair<TKey, TValue>>, IJsonFormatter
  {
    public void Serialize(
      ref JsonWriter writer,
      KeyValuePair<TKey, TValue> value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(StandardClassLibraryFormatterHelper.keyValuePairName[0]);
      formatterResolver.GetFormatterWithVerify<TKey>().Serialize(ref writer, value.Key, formatterResolver);
      writer.WriteRaw(StandardClassLibraryFormatterHelper.keyValuePairName[1]);
      formatterResolver.GetFormatterWithVerify<TValue>().Serialize(ref writer, value.Value, formatterResolver);
      writer.WriteEndObject();
    }

    public KeyValuePair<TKey, TValue> Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("Data is Nil, KeyValuePair can not be null.");
      TKey key1 = default (TKey);
      TValue obj = default (TValue);
      reader.ReadIsBeginObjectWithVerify();
      int count = 0;
      while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
      {
        ArraySegment<byte> key2 = reader.ReadPropertyNameSegmentRaw();
        int num;
        StandardClassLibraryFormatterHelper.keyValuePairAutomata.TryGetValueSafe(key2, out num);
        switch (num)
        {
          case 0:
            key1 = formatterResolver.GetFormatterWithVerify<TKey>().Deserialize(ref reader, formatterResolver);
            continue;
          case 1:
            obj = formatterResolver.GetFormatterWithVerify<TValue>().Deserialize(ref reader, formatterResolver);
            continue;
          default:
            reader.ReadNextBlock();
            continue;
        }
      }
      return new KeyValuePair<TKey, TValue>(key1, obj);
    }
  }
}
