// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableDoubleFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class NullableDoubleFormatter : IJsonFormatter<double?>, IJsonFormatter, IObjectPropertyNameFormatter<double?>
  {
    public static readonly NullableDoubleFormatter Default = new NullableDoubleFormatter();

    public void Serialize(
      ref JsonWriter writer,
      double? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
        writer.WriteNull();
      else
        writer.WriteDouble(value.Value);
    }

    public double? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? new double?() : new double?(reader.ReadDouble());
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      double? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
      {
        writer.WriteNull();
      }
      else
      {
        writer.WriteQuotation();
        writer.WriteDouble(value.Value);
        writer.WriteQuotation();
      }
    }

    public double? DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return new double?();
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return new double?(NumberConverter.ReadDouble(arraySegment.Array, arraySegment.Offset, out int _));
    }
  }
}
