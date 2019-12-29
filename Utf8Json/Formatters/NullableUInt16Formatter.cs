// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableUInt16Formatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class NullableUInt16Formatter : IJsonFormatter<ushort?>, IJsonFormatter, IObjectPropertyNameFormatter<ushort?>
  {
    public static readonly NullableUInt16Formatter Default = new NullableUInt16Formatter();

    public void Serialize(
      ref JsonWriter writer,
      ushort? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
        writer.WriteNull();
      else
        writer.WriteUInt16(value.Value);
    }

    public ushort? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? new ushort?() : new ushort?(reader.ReadUInt16());
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      ushort? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
      {
        writer.WriteNull();
      }
      else
      {
        writer.WriteQuotation();
        writer.WriteUInt16(value.Value);
        writer.WriteQuotation();
      }
    }

    public ushort? DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return new ushort?();
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return new ushort?(NumberConverter.ReadUInt16(arraySegment.Array, arraySegment.Offset, out int _));
    }
  }
}
