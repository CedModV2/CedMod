// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.Int16Formatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class Int16Formatter : IJsonFormatter<short>, IJsonFormatter, IObjectPropertyNameFormatter<short>
  {
    public static readonly Int16Formatter Default = new Int16Formatter();

    public void Serialize(
      ref JsonWriter writer,
      short value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteInt16(value);
    }

    public short Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadInt16();
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      short value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteQuotation();
      writer.WriteInt16(value);
      writer.WriteQuotation();
    }

    public short DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return NumberConverter.ReadInt16(arraySegment.Array, arraySegment.Offset, out int _);
    }
  }
}
