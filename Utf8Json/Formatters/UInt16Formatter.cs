// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.UInt16Formatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class UInt16Formatter : IJsonFormatter<ushort>, IJsonFormatter, IObjectPropertyNameFormatter<ushort>
  {
    public static readonly UInt16Formatter Default = new UInt16Formatter();

    public void Serialize(
      ref JsonWriter writer,
      ushort value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteUInt16(value);
    }

    public ushort Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadUInt16();
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      ushort value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteQuotation();
      writer.WriteUInt16(value);
      writer.WriteQuotation();
    }

    public ushort DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return NumberConverter.ReadUInt16(arraySegment.Array, arraySegment.Offset, out int _);
    }
  }
}
