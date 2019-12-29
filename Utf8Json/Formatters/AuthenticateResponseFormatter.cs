// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.AuthenticateResponseFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class AuthenticateResponseFormatter : IJsonFormatter<AuthenticateResponse>, IJsonFormatter
  {
    private readonly AutomataDictionary ____keyMapping;
    private readonly byte[][] ____stringByteKeys;

    public AuthenticateResponseFormatter()
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
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("token"),
          2
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("id"),
          3
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("nonce"),
          4
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("country"),
          5
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("flags"),
          6
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("expiration"),
          7
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("preauth"),
          8
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("globalBan"),
          9
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("lifetime"),
          10
        }
      };
      this.____stringByteKeys = new byte[11][]
      {
        JsonWriter.GetEncodedPropertyNameWithBeginObject("success"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("error"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("token"),
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
      AuthenticateResponse value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(this.____stringByteKeys[0]);
      writer.WriteBoolean(value.success);
      writer.WriteRaw(this.____stringByteKeys[1]);
      writer.WriteString(value.error);
      writer.WriteRaw(this.____stringByteKeys[2]);
      writer.WriteString(value.token);
      writer.WriteRaw(this.____stringByteKeys[3]);
      writer.WriteString(value.id);
      writer.WriteRaw(this.____stringByteKeys[4]);
      writer.WriteString(value.nonce);
      writer.WriteRaw(this.____stringByteKeys[5]);
      writer.WriteString(value.country);
      writer.WriteRaw(this.____stringByteKeys[6]);
      writer.WriteByte(value.flags);
      writer.WriteRaw(this.____stringByteKeys[7]);
      writer.WriteUInt64(value.expiration);
      writer.WriteRaw(this.____stringByteKeys[8]);
      writer.WriteString(value.preauth);
      writer.WriteRaw(this.____stringByteKeys[9]);
      writer.WriteString(value.globalBan);
      writer.WriteRaw(this.____stringByteKeys[10]);
      writer.WriteUInt16(value.lifetime);
      writer.WriteEndObject();
    }

    public AuthenticateResponse Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("typecode is null, struct not supported");
      bool success = false;
      string error = (string) null;
      string token = (string) null;
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
              token = reader.ReadString();
              continue;
            case 3:
              id = reader.ReadString();
              continue;
            case 4:
              nonce = reader.ReadString();
              continue;
            case 5:
              country = reader.ReadString();
              continue;
            case 6:
              flags = reader.ReadByte();
              continue;
            case 7:
              expiration = reader.ReadUInt64();
              continue;
            case 8:
              preauth = reader.ReadString();
              continue;
            case 9:
              globalBan = reader.ReadString();
              continue;
            case 10:
              lifetime = reader.ReadUInt16();
              continue;
            default:
              reader.ReadNextBlock();
              continue;
          }
        }
      }
      return new AuthenticateResponse(success, error, token, id, nonce, country, flags, expiration, preauth, globalBan, lifetime);
    }
  }
}
