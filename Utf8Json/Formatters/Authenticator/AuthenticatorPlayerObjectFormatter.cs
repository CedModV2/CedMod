// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.Authenticator.AuthenticatorPlayerObjectFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Authenticator;
using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters.Authenticator
{
  public sealed class AuthenticatorPlayerObjectFormatter : IJsonFormatter<AuthenticatorPlayerObject>, IJsonFormatter
  {
    private readonly AutomataDictionary ____keyMapping;
    private readonly byte[][] ____stringByteKeys;

    public AuthenticatorPlayerObjectFormatter()
    {
      this.____keyMapping = new AutomataDictionary()
      {
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("Id"),
          0
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("Ip"),
          1
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("RequestIp"),
          2
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("Asn"),
          3
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("AuthSerial"),
          4
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("VacSession"),
          5
        }
      };
      this.____stringByteKeys = new byte[6][]
      {
        JsonWriter.GetEncodedPropertyNameWithBeginObject("Id"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Ip"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("RequestIp"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Asn"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("AuthSerial"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("VacSession")
      };
    }

    public void Serialize(
      ref JsonWriter writer,
      AuthenticatorPlayerObject value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(this.____stringByteKeys[0]);
      writer.WriteString(value.Id);
      writer.WriteRaw(this.____stringByteKeys[1]);
      writer.WriteString(value.Ip);
      writer.WriteRaw(this.____stringByteKeys[2]);
      writer.WriteString(value.RequestIp);
      writer.WriteRaw(this.____stringByteKeys[3]);
      writer.WriteString(value.Asn);
      writer.WriteRaw(this.____stringByteKeys[4]);
      writer.WriteString(value.AuthSerial);
      writer.WriteRaw(this.____stringByteKeys[5]);
      writer.WriteString(value.VacSession);
      writer.WriteEndObject();
    }

    public AuthenticatorPlayerObject Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("typecode is null, struct not supported");
      string Id = (string) null;
      string Ip = (string) null;
      string RequestIp = (string) null;
      string Asn = (string) null;
      string AuthSerial = (string) null;
      string VacSession = (string) null;
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
              Id = reader.ReadString();
              continue;
            case 1:
              Ip = reader.ReadString();
              continue;
            case 2:
              RequestIp = reader.ReadString();
              continue;
            case 3:
              Asn = reader.ReadString();
              continue;
            case 4:
              AuthSerial = reader.ReadString();
              continue;
            case 5:
              VacSession = reader.ReadString();
              continue;
            default:
              reader.ReadNextBlock();
              continue;
          }
        }
      }
      return new AuthenticatorPlayerObject(Id, Ip, RequestIp, Asn, AuthSerial, VacSession);
    }
  }
}
