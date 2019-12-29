// Decompiled with JetBrains decompiler
// Type: Utf8Json.Unity.RectFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using Utf8Json.Internal;

namespace Utf8Json.Unity
{
  public sealed class RectFormatter : IJsonFormatter<Rect>, IJsonFormatter
  {
    private readonly AutomataDictionary ____keyMapping;
    private readonly byte[][] ____stringByteKeys;

    public RectFormatter()
    {
      this.____keyMapping = new AutomataDictionary()
      {
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("x"),
          0
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("y"),
          1
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("width"),
          2
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("height"),
          3
        }
      };
      this.____stringByteKeys = new byte[4][]
      {
        JsonWriter.GetEncodedPropertyNameWithBeginObject("x"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("y"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("width"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("height")
      };
    }

    public void Serialize(
      ref JsonWriter writer,
      Rect value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(this.____stringByteKeys[0]);
      writer.WriteSingle(value.x);
      writer.WriteRaw(this.____stringByteKeys[1]);
      writer.WriteSingle(value.y);
      writer.WriteRaw(this.____stringByteKeys[2]);
      writer.WriteSingle(value.width);
      writer.WriteRaw(this.____stringByteKeys[3]);
      writer.WriteSingle(value.height);
      writer.WriteEndObject();
    }

    public Rect Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("typecode is null, struct not supported");
      float x = 0.0f;
      float y = 0.0f;
      float width = 0.0f;
      float height = 0.0f;
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
              x = reader.ReadSingle();
              continue;
            case 1:
              y = reader.ReadSingle();
              continue;
            case 2:
              width = reader.ReadSingle();
              continue;
            case 3:
              height = reader.ReadSingle();
              continue;
            default:
              reader.ReadNextBlock();
              continue;
          }
        }
      }
      return new Rect(x, y, width, height)
      {
        x = x,
        y = y,
        width = width,
        height = height
      };
    }
  }
}
