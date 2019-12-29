// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.PublicKeyResponseFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class PublicKeyResponseFormatter : IJsonFormatter<PublicKeyResponse>, IJsonFormatter
  {
    private readonly AutomataDictionary ____keyMapping;
    private readonly byte[][] ____stringByteKeys;

    public PublicKeyResponseFormatter()
    {
      this.____keyMapping = new AutomataDictionary()
      {
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("key"),
          0
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("signature"),
          1
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("credits"),
          2
        }
      };
      this.____stringByteKeys = new byte[3][]
      {
        JsonWriter.GetEncodedPropertyNameWithBeginObject("key"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("signature"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("credits")
      };
    }

    public void Serialize(
      ref JsonWriter writer,
      PublicKeyResponse value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(this.____stringByteKeys[0]);
      writer.WriteString(value.key);
      writer.WriteRaw(this.____stringByteKeys[1]);
      writer.WriteString(value.signature);
      writer.WriteRaw(this.____stringByteKeys[2]);
      writer.WriteString(value.credits);
      writer.WriteEndObject();
    }

    public PublicKeyResponse Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("typecode is null, struct not supported");
      string key = (string) null;
      string signature = (string) null;
      string credits = (string) null;
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
              key = reader.ReadString();
              continue;
            case 1:
              signature = reader.ReadString();
              continue;
            case 2:
              credits = reader.ReadString();
              continue;
            default:
              reader.ReadNextBlock();
              continue;
          }
        }
      }
      return new PublicKeyResponse(key, signature, credits);
    }
  }
}
