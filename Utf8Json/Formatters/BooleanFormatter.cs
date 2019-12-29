// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.BooleanFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class BooleanFormatter : IJsonFormatter<bool>, IJsonFormatter, IObjectPropertyNameFormatter<bool>
  {
    public static readonly BooleanFormatter Default = new BooleanFormatter();

    public void Serialize(
      ref JsonWriter writer,
      bool value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteBoolean(value);
    }

    public bool Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadBoolean();
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      bool value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteQuotation();
      writer.WriteBoolean(value);
      writer.WriteQuotation();
    }

    public bool DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return NumberConverter.ReadBoolean(arraySegment.Array, arraySegment.Offset, out int _);
    }
  }
}
