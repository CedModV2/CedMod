// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.SByteFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class SByteFormatter : IJsonFormatter<sbyte>, IJsonFormatter, IObjectPropertyNameFormatter<sbyte>
  {
    public static readonly SByteFormatter Default = new SByteFormatter();

    public void Serialize(
      ref JsonWriter writer,
      sbyte value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteSByte(value);
    }

    public sbyte Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadSByte();
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      sbyte value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteQuotation();
      writer.WriteSByte(value);
      writer.WriteQuotation();
    }

    public sbyte DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return NumberConverter.ReadSByte(arraySegment.Array, arraySegment.Offset, out int _);
    }
  }
}
