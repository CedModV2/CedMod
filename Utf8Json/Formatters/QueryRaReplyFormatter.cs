// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.QueryRaReplyFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class QueryRaReplyFormatter : IJsonFormatter<QueryRaReply>, IJsonFormatter
  {
    private readonly AutomataDictionary ____keyMapping;
    private readonly byte[][] ____stringByteKeys;

    public QueryRaReplyFormatter()
    {
      this.____keyMapping = new AutomataDictionary()
      {
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("Text"),
          0
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("Success"),
          1
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("LogToConsole"),
          2
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("OverrideDisplay"),
          3
        }
      };
      this.____stringByteKeys = new byte[4][]
      {
        JsonWriter.GetEncodedPropertyNameWithBeginObject("Text"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Success"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("LogToConsole"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("OverrideDisplay")
      };
    }

    public void Serialize(
      ref JsonWriter writer,
      QueryRaReply value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(this.____stringByteKeys[0]);
      writer.WriteString(value.Text);
      writer.WriteRaw(this.____stringByteKeys[1]);
      writer.WriteBoolean(value.Success);
      writer.WriteRaw(this.____stringByteKeys[2]);
      writer.WriteBoolean(value.LogToConsole);
      writer.WriteRaw(this.____stringByteKeys[3]);
      writer.WriteString(value.OverrideDisplay);
      writer.WriteEndObject();
    }

    public QueryRaReply Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("typecode is null, struct not supported");
      string text = (string) null;
      bool success = false;
      bool logToConsole = false;
      string overrideDisplay = (string) null;
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
              text = reader.ReadString();
              continue;
            case 1:
              success = reader.ReadBoolean();
              continue;
            case 2:
              logToConsole = reader.ReadBoolean();
              continue;
            case 3:
              overrideDisplay = reader.ReadString();
              continue;
            default:
              reader.ReadNextBlock();
              continue;
          }
        }
      }
      return new QueryRaReply(text, success, logToConsole, overrideDisplay);
    }
  }
}
