// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableUInt32Formatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class NullableUInt32Formatter : IJsonFormatter<uint?>, IJsonFormatter, IObjectPropertyNameFormatter<uint?>
  {
    public static readonly NullableUInt32Formatter Default = new NullableUInt32Formatter();

    public void Serialize(
      ref JsonWriter writer,
      uint? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
        writer.WriteNull();
      else
        writer.WriteUInt32(value.Value);
    }

    public uint? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? new uint?() : new uint?(reader.ReadUInt32());
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      uint? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
      {
        writer.WriteNull();
      }
      else
      {
        writer.WriteQuotation();
        writer.WriteUInt32(value.Value);
        writer.WriteQuotation();
      }
    }

    public uint? DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return new uint?();
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return new uint?(NumberConverter.ReadUInt32(arraySegment.Array, arraySegment.Offset, out int _));
    }
  }
}
