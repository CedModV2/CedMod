// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.AuthenticatorResponseFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class AuthenticatorResponseFormatter : IJsonFormatter<AuthenticatorResponse>, IJsonFormatter
  {
    private readonly AutomataDictionary ____keyMapping;
    private readonly byte[][] ____stringByteKeys;

    public AuthenticatorResponseFormatter()
    {
      this.____keyMapping = new AutomataDictionary()
      {
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("success"),
          0
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("verified"),
          1
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("error"),
          2
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("token"),
          3
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("messages"),
          4
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("actions"),
          5
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("authAccepted"),
          6
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("authRejected"),
          7
        }
      };
      this.____stringByteKeys = new byte[8][]
      {
        JsonWriter.GetEncodedPropertyNameWithBeginObject("success"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("verified"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("error"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("token"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("messages"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("actions"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("authAccepted"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("authRejected")
      };
    }

    public void Serialize(
      ref JsonWriter writer,
      AuthenticatorResponse value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(this.____stringByteKeys[0]);
      writer.WriteBoolean(value.success);
      writer.WriteRaw(this.____stringByteKeys[1]);
      writer.WriteBoolean(value.verified);
      writer.WriteRaw(this.____stringByteKeys[2]);
      writer.WriteString(value.error);
      writer.WriteRaw(this.____stringByteKeys[3]);
      writer.WriteString(value.token);
      writer.WriteRaw(this.____stringByteKeys[4]);
      formatterResolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.messages, formatterResolver);
      writer.WriteRaw(this.____stringByteKeys[5]);
      formatterResolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.actions, formatterResolver);
      writer.WriteRaw(this.____stringByteKeys[6]);
      formatterResolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.authAccepted, formatterResolver);
      writer.WriteRaw(this.____stringByteKeys[7]);
      formatterResolver.GetFormatterWithVerify<AuthenticatiorAuthReject[]>().Serialize(ref writer, value.authRejected, formatterResolver);
      writer.WriteEndObject();
    }

    public AuthenticatorResponse Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("typecode is null, struct not supported");
      bool success = false;
      bool verified = false;
      string error = (string) null;
      string token = (string) null;
      string[] messages = (string[]) null;
      string[] actions = (string[]) null;
      string[] authAccepted = (string[]) null;
      AuthenticatiorAuthReject[] authRejected = (AuthenticatiorAuthReject[]) null;
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
              verified = reader.ReadBoolean();
              continue;
            case 2:
              error = reader.ReadString();
              continue;
            case 3:
              token = reader.ReadString();
              continue;
            case 4:
              messages = formatterResolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, formatterResolver);
              continue;
            case 5:
              actions = formatterResolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, formatterResolver);
              continue;
            case 6:
              authAccepted = formatterResolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, formatterResolver);
              continue;
            case 7:
              authRejected = formatterResolver.GetFormatterWithVerify<AuthenticatiorAuthReject[]>().Deserialize(ref reader, formatterResolver);
              continue;
            default:
              reader.ReadNextBlock();
              continue;
          }
        }
      }
      return new AuthenticatorResponse(success, verified, error, token, messages, actions, authAccepted, authRejected);
    }
  }
}
