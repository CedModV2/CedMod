// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableInt32Formatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class NullableInt32Formatter : IJsonFormatter<int?>, IJsonFormatter, IObjectPropertyNameFormatter<int?>
  {
    public static readonly NullableInt32Formatter Default = new NullableInt32Formatter();

    public void Serialize(
      ref JsonWriter writer,
      int? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
        writer.WriteNull();
      else
        writer.WriteInt32(value.Value);
    }

    public int? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? new int?() : new int?(reader.ReadInt32());
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      int? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
      {
        writer.WriteNull();
      }
      else
      {
        writer.WriteQuotation();
        writer.WriteInt32(value.Value);
        writer.WriteQuotation();
      }
    }

    public int? DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return new int?();
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return new int?(NumberConverter.ReadInt32(arraySegment.Array, arraySegment.Offset, out int _));
    }
  }
}
