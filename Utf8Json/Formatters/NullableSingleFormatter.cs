// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableSingleFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class NullableSingleFormatter : IJsonFormatter<float?>, IJsonFormatter, IObjectPropertyNameFormatter<float?>
  {
    public static readonly NullableSingleFormatter Default = new NullableSingleFormatter();

    public void Serialize(
      ref JsonWriter writer,
      float? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
        writer.WriteNull();
      else
        writer.WriteSingle(value.Value);
    }

    public float? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? new float?() : new float?(reader.ReadSingle());
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      float? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
      {
        writer.WriteNull();
      }
      else
      {
        writer.WriteQuotation();
        writer.WriteSingle(value.Value);
        writer.WriteQuotation();
      }
    }

    public float? DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return new float?();
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return new float?(NumberConverter.ReadSingle(arraySegment.Array, arraySegment.Offset, out int _));
    }
  }
}
