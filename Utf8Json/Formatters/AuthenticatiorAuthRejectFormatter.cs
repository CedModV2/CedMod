// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.AuthenticatiorAuthRejectFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class AuthenticatiorAuthRejectFormatter : IJsonFormatter<AuthenticatiorAuthReject>, IJsonFormatter
  {
    private readonly AutomataDictionary ____keyMapping;
    private readonly byte[][] ____stringByteKeys;

    public AuthenticatiorAuthRejectFormatter()
    {
      this.____keyMapping = new AutomataDictionary()
      {
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("Id"),
          0
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("Reason"),
          1
        }
      };
      this.____stringByteKeys = new byte[2][]
      {
        JsonWriter.GetEncodedPropertyNameWithBeginObject("Id"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Reason")
      };
    }

    public void Serialize(
      ref JsonWriter writer,
      AuthenticatiorAuthReject value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(this.____stringByteKeys[0]);
      writer.WriteString(value.Id);
      writer.WriteRaw(this.____stringByteKeys[1]);
      writer.WriteString(value.Reason);
      writer.WriteEndObject();
    }

    public AuthenticatiorAuthReject Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("typecode is null, struct not supported");
      string id = (string) null;
      string reason = (string) null;
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
              id = reader.ReadString();
              continue;
            case 1:
              reason = reader.ReadString();
              continue;
            default:
              reader.ReadNextBlock();
              continue;
          }
        }
      }
      return new AuthenticatiorAuthReject(id, reason);
    }
  }
}
