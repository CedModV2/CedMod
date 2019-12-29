// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.ServerListFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class ServerListFormatter : IJsonFormatter<ServerList>, IJsonFormatter
  {
    private readonly AutomataDictionary ____keyMapping;
    private readonly byte[][] ____stringByteKeys;

    public ServerListFormatter()
    {
      this.____keyMapping = new AutomataDictionary()
      {
        {
          JsonWriter.GetEncodedPropertyNameWithoutQuotation("servers"),
          0
        }
      };
      this.____stringByteKeys = new byte[1][]
      {
        JsonWriter.GetEncodedPropertyNameWithBeginObject("servers")
      };
    }

    public void Serialize(
      ref JsonWriter writer,
      ServerList value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteRaw(this.____stringByteKeys[0]);
      formatterResolver.GetFormatterWithVerify<ServerListItem[]>().Serialize(ref writer, value.servers, formatterResolver);
      writer.WriteEndObject();
    }

    public ServerList Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        throw new InvalidOperationException("typecode is null, struct not supported");
      ServerListItem[] servers = (ServerListItem[]) null;
      int count = 0;
      reader.ReadIsBeginObjectWithVerify();
      while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
      {
        int num;
        if (!this.____keyMapping.TryGetValueSafe(reader.ReadPropertyNameSegmentRaw(), out num))
          reader.ReadNextBlock();
        else if (num == 0)
          servers = formatterResolver.GetFormatterWithVerify<ServerListItem[]>().Deserialize(ref reader, formatterResolver);
        else
          reader.ReadNextBlock();
      }
      return new ServerList(servers);
    }
  }
}
