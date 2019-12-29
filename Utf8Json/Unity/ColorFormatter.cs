// Decompiled with JetBrains decompiler
// Type: Utf8Json.Unity.ColorFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using Utf8Json.Internal;

namespace Utf8Json.Unity
{
  public sealed class ColorFormatter : IJsonFormatter<Color>, IJsonFormatter
  {
    private readonly AutomataDictionary ____keyMapping;
    private readonly byte[][] ____stringByteKeys;

    public ColorFormatter()
    {
      this.____keyMapping = new AutomataDictionary()
      {
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("r"),
          0
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("g"),
          1
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("b"),
          2
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("a"),
          3
        }
      };
      this.____stringByteKeys = new byte[4][]
      {
        JsonWriter.GetEncodedPropertyNameWithBeginObject("r"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("g"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("b"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("a")
      };
    }

    public void Serialize(
      ref JsonWriter writer,
      Color value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(this.____stringByteKeys[0]);
      writer.WriteSingle(value.r);
      writer.WriteRaw(this.____stringByteKeys[1]);
      writer.WriteSingle(value.g);
      writer.WriteRaw(this.____stringByteKeys[2]);
      writer.WriteSingle(value.b);
      writer.WriteRaw(this.____stringByteKeys[3]);
      writer.WriteSingle(value.a);
      writer.WriteEndObject();
    }

    public Color Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("typecode is null, struct not supported");
      float r = 0.0f;
      float g = 0.0f;
      float b = 0.0f;
      float a = 0.0f;
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
              r = reader.ReadSingle();
              continue;
            case 1:
              g = reader.ReadSingle();
              continue;
            case 2:
              b = reader.ReadSingle();
              continue;
            case 3:
              a = reader.ReadSingle();
              continue;
            default:
              reader.ReadNextBlock();
              continue;
          }
        }
      }
      return new Color(r, g, b, a)
      {
        r = r,
        g = g,
        b = b,
        a = a
      };
    }
  }
}
