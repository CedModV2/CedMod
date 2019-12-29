// Decompiled with JetBrains decompiler
// Type: Utf8Json.Unity.BoundsFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using Utf8Json.Internal;

namespace Utf8Json.Unity
{
  public sealed class BoundsFormatter : IJsonFormatter<Bounds>, IJsonFormatter
  {
    private readonly AutomataDictionary ____keyMapping;
    private readonly byte[][] ____stringByteKeys;

    public BoundsFormatter()
    {
      this.____keyMapping = new AutomataDictionary()
      {
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("center"),
          0
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("size"),
          1
        }
      };
      this.____stringByteKeys = new byte[2][]
      {
        JsonWriter.GetEncodedPropertyNameWithBeginObject("center"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("size")
      };
    }

    public void Serialize(
      ref JsonWriter writer,
      Bounds value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(this.____stringByteKeys[0]);
      formatterResolver.GetFormatterWithVerify<Vector3>().Serialize(ref writer, value.center, formatterResolver);
      writer.WriteRaw(this.____stringByteKeys[1]);
      formatterResolver.GetFormatterWithVerify<Vector3>().Serialize(ref writer, value.size, formatterResolver);
      writer.WriteEndObject();
    }

    public Bounds Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("typecode is null, struct not supported");
      Vector3 center = new Vector3();
      Vector3 size = new Vector3();
      int count = 0;
      reader.ReadIsBeginObjectWithVerify();
      while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
      {
        int num;
        if (!this.____keyMapping.TryGetValueSafe(reader.ReadPropertyNameSegmentRaw(), out num))
        {
          reader.ReadNextBlock();
        }
        else
        {
          switch (num)
          {
            case 0:
              center = formatterResolver.GetFormatterWithVerify<Vector3>().Deserialize(ref reader, formatterResolver);
              continue;
            case 1:
              size = formatterResolver.GetFormatterWithVerify<Vector3>().Deserialize(ref reader, formatterResolver);
              continue;
            default:
              reader.ReadNextBlock();
              continue;
          }
        }
      }
      return new Bounds(center, size)
      {
        center = center,
        size = size
      };
    }
  }
}
