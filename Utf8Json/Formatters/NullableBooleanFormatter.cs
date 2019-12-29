// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableBooleanFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class NullableBooleanFormatter : IJsonFormatter<bool?>, IJsonFormatter, IObjectPropertyNameFormatter<bool?>
  {
    public static readonly NullableBooleanFormatter Default = new NullableBooleanFormatter();

    public void Serialize(
      ref JsonWriter writer,
      bool? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
        writer.WriteNull();
      else
        writer.WriteBoolean(value.Value);
    }

    public bool? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? new bool?() : new bool?(reader.ReadBoolean());
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      bool? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
      {
        writer.WriteNull();
      }
      else
      {
        writer.WriteQuotation();
        writer.WriteBoolean(value.Value);
        writer.WriteQuotation();
      }
    }

    public bool? DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return new bool?();
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return new bool?(NumberConverter.ReadBoolean(arraySegment.Array, arraySegment.Offset, out int _));
    }
  }
}
