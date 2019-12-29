// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.ServerListItemFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class ServerListItemFormatter : IJsonFormatter<ServerListItem>, IJsonFormatter
  {
    private readonly AutomataDictionary ____keyMapping;
    private readonly byte[][] ____stringByteKeys;

    public ServerListItemFormatter()
    {
      this.____keyMapping = new AutomataDictionary()
      {
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("ip"),
          0
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("port"),
          1
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("players"),
          2
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("info"),
          3
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("pastebin"),
          4
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("version"),
          5
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("friendlyFire"),
          6
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("modded"),
          7
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("whitelist"),
          8
        },
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("official"),
          9
        }
      };
      this.____stringByteKeys = new byte[10][]
      {
        JsonWriter.GetEncodedPropertyNameWithBeginObject("ip"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("port"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("players"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("info"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("pastebin"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("version"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("friendlyFire"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("modded"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("whitelist"),
        JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("official")
      };
    }

    public void Serialize(
      ref JsonWriter writer,
      ServerListItem value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(this.____stringByteKeys[0]);
      writer.WriteString(value.ip);
      writer.WriteRaw(this.____stringByteKeys[1]);
      writer.WriteUInt16(value.port);
      writer.WriteRaw(this.____stringByteKeys[2]);
      writer.WriteString(value.players);
      writer.WriteRaw(this.____stringByteKeys[3]);
      writer.WriteString(value.info);
      writer.WriteRaw(this.____stringByteKeys[4]);
      writer.WriteString(value.pastebin);
      writer.WriteRaw(this.____stringByteKeys[5]);
      writer.WriteString(value.version);
      writer.WriteRaw(this.____stringByteKeys[6]);
      writer.WriteBoolean(value.friendlyFire);
      writer.WriteRaw(this.____stringByteKeys[7]);
      writer.WriteBoolean(value.modded);
      writer.WriteRaw(this.____stringByteKeys[8]);
      writer.WriteBoolean(value.whitelist);
      writer.WriteRaw(this.____stringByteKeys[9]);
      writer.WriteString(value.official);
      writer.WriteEndObject();
    }

    public ServerListItem Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("typecode is null, struct not supported");
      string ip = (string) null;
      ushort port = 0;
      string players = (string) null;
      string info = (string) null;
      string pastebin = (string) null;
      string version = (string) null;
      bool friendlyFire = false;
      bool modded = false;
      bool whitelist = false;
      string official = (string) null;
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
              ip = reader.ReadString();
              continue;
            case 1:
              port = reader.ReadUInt16();
              continue;
            case 2:
              players = reader.ReadString();
              continue;
            case 3:
              info = reader.ReadString();
              continue;
            case 4:
              pastebin = reader.ReadString();
              continue;
            case 5:
              version = reader.ReadString();
              continue;
            case 6:
              friendlyFire = reader.ReadBoolean();
              continue;
            case 7:
              modded = reader.ReadBoolean();
              continue;
            case 8:
              whitelist = reader.ReadBoolean();
              continue;
            case 9:
              official = reader.ReadString();
              continue;
            default:
              reader.ReadNextBlock();
              continue;
          }
        }
      }
      return new ServerListItem(ip, port, players, info, pastebin, version, friendlyFire, modded, whitelist, official);
    }
  }
}
