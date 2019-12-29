// Decompiled with JetBrains decompiler
// Type: Utf8Json.Unity.Vector2Formatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using Utf8Json.Internal;

namespace Utf8Json.Unity
{
  public sealed class Vector2Formatter : IJsonFormatter<Vector2>, IJsonFormatter
  {
    private readonly AutomataDictionary ____keyMapping;
    private readonly byte[][] ____stringByteKeys;

    public Vector2Formatter()
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
        }
      };
      this.____stringByteKeys = new byte[2][]
      {
        JsonWriter.GetEncodedPropertyNameWithBeginObject("x"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("y")
      };
    }

    public void Serialize(
      ref JsonWriter writer,
      Vector2 value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(this.____stringByteKeys[0]);
      writer.WriteSingle(value.x);
      writer.WriteRaw(this.____stringByteKeys[1]);
      writer.WriteSingle(value.y);
      writer.WriteEndObject();
    }

    public Vector2 Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("typecode is null, struct not supported");
      float x = 0.0f;
      float y = 0.0f;
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
            default:
              reader.ReadNextBlock();
              continue;
          }
        }
      }
      return new Vector2(x, y) { x = x, y = y };
    }
  }
}
