// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.ByteFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class ByteFormatter : IJsonFormatter<byte>, IJsonFormatter, IObjectPropertyNameFormatter<byte>
  {
    public static readonly ByteFormatter Default = new ByteFormatter();

    public void Serialize(
      ref JsonWriter writer,
      byte value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteByte(value);
    }

    public byte Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadByte();
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      byte value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteQuotation();
      writer.WriteByte(value);
      writer.WriteQuotation();
    }

    public byte DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return NumberConverter.ReadByte(arraySegment.Array, arraySegment.Offset, out int _);
    }
  }
}
