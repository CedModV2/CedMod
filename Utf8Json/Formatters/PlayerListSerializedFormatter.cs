// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.PlayerListSerializedFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class PlayerListSerializedFormatter : IJsonFormatter<PlayerListSerialized>, IJsonFormatter
  {
    private readonly AutomataDictionary ____keyMapping;
    private readonly byte[][] ____stringByteKeys;

    public PlayerListSerializedFormatter()
    {
      this.____keyMapping = new AutomataDictionary()
      {
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("objects"),
          0
        }
      };
      this.____stringByteKeys = new byte[1][]
      {
        JsonWriter.GetEncodedPropertyNameWithBeginObject("objects")
      };
    }

    public void Serialize(
      ref JsonWriter writer,
      PlayerListSerialized value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(this.____stringByteKeys[0]);
      formatterResolver.GetFormatterWithVerify<List<string>>().Serialize(ref writer, value.objects, formatterResolver);
      writer.WriteEndObject();
    }

    public PlayerListSerialized Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("typecode is null, struct not supported");
      List<string> objects = (List<string>) null;
      int count = 0;
      reader.ReadIsBeginObjectWithVerify();
      while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
      {
        int num;
        if (!this.____keyMapping.TryGetValueSafe(reader.ReadPropertyNameSegmentRaw(), out num))
          reader.ReadNextBlock();
        else if (num == 0)
          objects = formatterResolver.GetFormatterWithVerify<List<string>>().Deserialize(ref reader, formatterResolver);
        else
          reader.ReadNextBlock();
      }
      return new PlayerListSerialized(objects);
    }
  }
}
