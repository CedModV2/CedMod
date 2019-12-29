// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.ServerListSignedFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class ServerListSignedFormatter : IJsonFormatter<ServerListSigned>, IJsonFormatter
  {
    private readonly AutomataDictionary ____keyMapping;
    private readonly byte[][] ____stringByteKeys;

    public ServerListSignedFormatter()
    {
      this.____keyMapping = new AutomataDictionary()
      {
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("payload"),
          0
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("timestamp"),
          1
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("signature"),
          2
        }
      };
      this.____stringByteKeys = new byte[3][]
      {
        JsonWriter.GetEncodedPropertyNameWithBeginObject("payload"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("timestamp"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("signature")
      };
    }

    public void Serialize(
      ref JsonWriter writer,
      ServerListSigned value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(this.____stringByteKeys[0]);
      writer.WriteString(value.payload);
      writer.WriteRaw(this.____stringByteKeys[1]);
      writer.WriteUInt64(value.timestamp);
      writer.WriteRaw(this.____stringByteKeys[2]);
      writer.WriteString(value.signature);
      writer.WriteEndObject();
    }

    public ServerListSigned Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("typecode is null, struct not supported");
      string payload = (string) null;
      ulong timestamp = 0;
      string signature = (string) null;
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
              payload = reader.ReadString();
              continue;
            case 1:
              timestamp = reader.ReadUInt64();
              continue;
            case 2:
              signature = reader.ReadString();
              continue;
            default:
              reader.ReadNextBlock();
              continue;
          }
        }
      }
      return new ServerListSigned(payload, timestamp, signature);
    }
  }
}
