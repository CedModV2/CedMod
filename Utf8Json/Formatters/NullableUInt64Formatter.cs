// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableUInt64Formatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class NullableUInt64Formatter : IJsonFormatter<ulong?>, IJsonFormatter, IObjectPropertyNameFormatter<ulong?>
  {
    public static readonly NullableUInt64Formatter Default = new NullableUInt64Formatter();

    public void Serialize(
      ref JsonWriter writer,
      ulong? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
        writer.WriteNull();
      else
        writer.WriteUInt64(value.Value);
    }

    public ulong? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? new ulong?() : new ulong?(reader.ReadUInt64());
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      ulong? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
      {
        writer.WriteNull();
      }
      else
      {
        writer.WriteQuotation();
        writer.WriteUInt64(value.Value);
        writer.WriteQuotation();
      }
    }

    public ulong? DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return new ulong?();
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return new ulong?(NumberConverter.ReadUInt64(arraySegment.Array, arraySegment.Offset, out int _));
    }
  }
}
