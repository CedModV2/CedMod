// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableInt64Formatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class NullableInt64Formatter : IJsonFormatter<long?>, IJsonFormatter, IObjectPropertyNameFormatter<long?>
  {
    public static readonly NullableInt64Formatter Default = new NullableInt64Formatter();

    public void Serialize(
      ref JsonWriter writer,
      long? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
        writer.WriteNull();
      else
        writer.WriteInt64(value.Value);
    }

    public long? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? new long?() : new long?(reader.ReadInt64());
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      long? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
      {
        writer.WriteNull();
      }
      else
      {
        writer.WriteQuotation();
        writer.WriteInt64(value.Value);
        writer.WriteQuotation();
      }
    }

    public long? DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return new long?();
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return new long?(NumberConverter.ReadInt64(arraySegment.Array, arraySegment.Offset, out int _));
    }
  }
}
