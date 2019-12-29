// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.RequestSignatureResponseFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class RequestSignatureResponseFormatter : IJsonFormatter<RequestSignatureResponse>, IJsonFormatter
  {
    private readonly AutomataDictionary ____keyMapping;
    private readonly byte[][] ____stringByteKeys;

    public RequestSignatureResponseFormatter()
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
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("auth"),
          2
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("badge"),
          3
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("pub"),
          4
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("nonce"),
          5
        }
      };
      this.____stringByteKeys = new byte[6][]
      {
        JsonWriter.GetEncodedPropertyNameWithBeginObject("success"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("error"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("auth"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("badge"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("pub"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("nonce")
      };
    }

    public void Serialize(
      ref JsonWriter writer,
      RequestSignatureResponse value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(this.____stringByteKeys[0]);
      writer.WriteBoolean(value.success);
      writer.WriteRaw(this.____stringByteKeys[1]);
      writer.WriteString(value.error);
      writer.WriteRaw(this.____stringByteKeys[2]);
      writer.WriteString(value.auth);
      writer.WriteRaw(this.____stringByteKeys[3]);
      writer.WriteString(value.badge);
      writer.WriteRaw(this.____stringByteKeys[4]);
      writer.WriteString(value.pub);
      writer.WriteRaw(this.____stringByteKeys[5]);
      writer.WriteString(value.nonce);
      writer.WriteEndObject();
    }

    public RequestSignatureResponse Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("typecode is null, struct not supported");
      bool success = false;
      string error = (string) null;
      string auth = (string) null;
      string badge = (string) null;
      string pub = (string) null;
      string nonce = (string) null;
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
              auth = reader.ReadString();
              continue;
            case 3:
              badge = reader.ReadString();
              continue;
            case 4:
              pub = reader.ReadString();
              continue;
            case 5:
              nonce = reader.ReadString();
              continue;
            default:
              reader.ReadNextBlock();
              continue;
          }
        }
      }
      return new RequestSignatureResponse(success, error, auth, badge, pub, nonce);
    }
  }
}
