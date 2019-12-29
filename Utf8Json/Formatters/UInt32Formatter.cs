// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.UInt32Formatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class UInt32Formatter : IJsonFormatter<uint>, IJsonFormatter, IObjectPropertyNameFormatter<uint>
  {
    public static readonly UInt32Formatter Default = new UInt32Formatter();

    public void Serialize(
      ref JsonWriter writer,
      uint value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteUInt32(value);
    }

    public uint Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadUInt32();
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      uint value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteQuotation();
      writer.WriteUInt32(value);
      writer.WriteQuotation();
    }

    public uint DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return NumberConverter.ReadUInt32(arraySegment.Array, arraySegment.Offset, out int _);
    }
  }
}
