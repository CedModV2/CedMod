// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableByteFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class NullableByteFormatter : IJsonFormatter<byte?>, IJsonFormatter, IObjectPropertyNameFormatter<byte?>
  {
    public static readonly NullableByteFormatter Default = new NullableByteFormatter();

    public void Serialize(
      ref JsonWriter writer,
      byte? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
        writer.WriteNull();
      else
        writer.WriteByte(value.Value);
    }

    public byte? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? new byte?() : new byte?(reader.ReadByte());
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      byte? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
      {
        writer.WriteNull();
      }
      else
      {
        writer.WriteQuotation();
        writer.WriteByte(value.Value);
        writer.WriteQuotation();
      }
    }

    public byte? DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return new byte?();
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return new byte?(NumberConverter.ReadByte(arraySegment.Array, arraySegment.Offset, out int _));
    }
  }
}
