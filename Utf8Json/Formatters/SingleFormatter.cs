// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.SingleFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class SingleFormatter : IJsonFormatter<float>, IJsonFormatter, IObjectPropertyNameFormatter<float>
  {
    public static readonly SingleFormatter Default = new SingleFormatter();

    public void Serialize(
      ref JsonWriter writer,
      float value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteSingle(value);
    }

    public float Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadSingle();
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      float value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteQuotation();
      writer.WriteSingle(value);
      writer.WriteQuotation();
    }

    public float DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return NumberConverter.ReadSingle(arraySegment.Array, arraySegment.Offset, out int _);
    }
  }
}
