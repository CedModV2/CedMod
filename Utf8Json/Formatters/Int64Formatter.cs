// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.Int64Formatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class Int64Formatter : IJsonFormatter<long>, IJsonFormatter, IObjectPropertyNameFormatter<long>
  {
    public static readonly Int64Formatter Default = new Int64Formatter();

    public void Serialize(
      ref JsonWriter writer,
      long value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteInt64(value);
    }

    public long Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadInt64();
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      long value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteQuotation();
      writer.WriteInt64(value);
      writer.WriteQuotation();
    }

    public long DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      ArraySegment<byte> arraySegment = reader.ReadStringSegmentRaw();
      return NumberConverter.ReadInt64(arraySegment.Array, arraySegment.Offset, out int _);
    }
  }
}
