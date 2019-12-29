// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.UInt64Formatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class UInt64Formatter : IJsonFormatter<ulong>, IJsonFormatter, IObjectPropertyNameFormatter<ulong>
  {
    public static readonly UInt64Formatter Default = new UInt64Formatter();

    public void Serialize(
      ref JsonWriter writer,
      ulong value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteUInt64(value);
    }

    public ulong Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadUInt64();
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      ulong value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteQuotation();
      writer.WriteUInt64(value);
      writer.WriteQuotation();
    }

    public ulong DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return NumberConverter.ReadUInt64(arraySegment.Array, arraySegment.Offset, out int _);
    }
  }
}
