// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.RenewResponseFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class RenewResponseFormatter : IJsonFormatter<RenewResponse>, IJsonFormatter
  {
    private readonly AutomataDictionary ____keyMapping;
    private readonly byte[][] ____stringByteKeys;

    public RenewResponseFormatter()
    {
      this.____keyMapping = new AutomataDictionary()
      {
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("success"),
          0
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("error"),
          1
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("id"),
          2
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("nonce"),
          3
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("country"),
          4
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("flags"),
          5
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("expiration"),
          6
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("preauth"),
          7
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("globalBan"),
          8
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("lifetime"),
          9
        }
      };
      this.____stringByteKeys = new byte[10][]
      {
        JsonWriter.GetEncodedPropertyNameWithBeginObject("success"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("error"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("id"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("nonce"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("country"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("flags"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("expiration"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("preauth"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("globalBan"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("lifetime")
      };
    }

    public void Serialize(
      ref JsonWriter writer,
      RenewResponse value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(this.____stringByteKeys[0]);
      writer.WriteBoolean(value.success);
      writer.WriteRaw(this.____stringByteKeys[1]);
      writer.WriteString(value.error);
      writer.WriteRaw(this.____stringByteKeys[2]);
      writer.WriteString(value.id);
      writer.WriteRaw(this.____stringByteKeys[3]);
      writer.WriteString(value.nonce);
      writer.WriteRaw(this.____stringByteKeys[4]);
      writer.WriteString(value.country);
      writer.WriteRaw(this.____stringByteKeys[5]);
      writer.WriteByte(value.flags);
      writer.WriteRaw(this.____stringByteKeys[6]);
      writer.WriteUInt64(value.expiration);
      writer.WriteRaw(this.____stringByteKeys[7]);
      writer.WriteString(value.preauth);
      writer.WriteRaw(this.____stringByteKeys[8]);
      writer.WriteString(value.globalBan);
      writer.WriteRaw(this.____stringByteKeys[9]);
      writer.WriteUInt16(value.lifetime);
      writer.WriteEndObject();
    }

    public RenewResponse Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("typecode is null, struct not supported");
      bool success = false;
      string error = (string) null;
      string id = (string) null;
      string nonce = (string) null;
      string country = (string) null;
      byte flags = 0;
      ulong expiration = 0;
      string preauth = (string) null;
      string globalBan = (string) null;
      ushort lifetime = 0;
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
              success = reader.ReadBoolean();
              continue;
            case 1:
              error = reader.ReadString();
              continue;
            case 2:
              id = reader.ReadString();
              continue;
            case 3:
              nonce = reader.ReadString();
              continue;
            case 4:
              country = reader.ReadString();
              continue;
            case 5:
              flags = reader.ReadByte();
              continue;
            case 6:
              expiration = reader.ReadUInt64();
              continue;
            case 7:
              preauth = reader.ReadString();
              continue;
            case 8:
              globalBan = reader.ReadString();
              continue;
            case 9:
              lifetime = reader.ReadUInt16();
              continue;
            default:
              reader.ReadNextBlock();
              continue;
          }
        }
      }
      return new RenewResponse(success, error, id, nonce, country, flags, expiration, preauth, globalBan, lifetime);
    }
  }
}
