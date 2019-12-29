// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.DoubleFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class DoubleFormatter : IJsonFormatter<double>, IJsonFormatter, IObjectPropertyNameFormatter<double>
  {
    public static readonly DoubleFormatter Default = new DoubleFormatter();

    public void Serialize(
      ref JsonWriter writer,
      double value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteDouble(value);
    }

    public double Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadDouble();
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      double value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteQuotation();
      writer.WriteDouble(value);
      writer.WriteQuotation();
    }

    public double DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return NumberConverter.ReadDouble(arraySegment.Array, arraySegment.Offset, out int _);
    }
  }
}
