// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableInt16Formatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class NullableInt16Formatter : IJsonFormatter<short?>, IJsonFormatter, IObjectPropertyNameFormatter<short?>
  {
    public static readonly NullableInt16Formatter Default = new NullableInt16Formatter();

    public void Serialize(
      ref JsonWriter writer,
      short? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
        writer.WriteNull();
      else
        writer.WriteInt16(value.Value);
    }

    public short? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? new short?() : new short?(reader.ReadInt16());
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      short? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
      {
        writer.WriteNull();
      }
      else
      {
        writer.WriteQuotation();
        writer.WriteInt16(value.Value);
        writer.WriteQuotation();
      }
    }

    public short? DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return new short?();
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return new short?(NumberConverter.ReadInt16(arraySegment.Array, arraySegment.Offset, out int _));
    }
  }
}
