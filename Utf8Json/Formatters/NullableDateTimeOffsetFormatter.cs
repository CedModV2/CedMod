// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableDateTimeOffsetFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class NullableDateTimeOffsetFormatter : IJsonFormatter<DateTimeOffset?>, IJsonFormatter
  {
    private readonly DateTimeOffsetFormatter innerFormatter;

    public NullableDateTimeOffsetFormatter()
    {
      this.innerFormatter = new DateTimeOffsetFormatter();
    }

    public NullableDateTimeOffsetFormatter(string formatString)
    {
      this.innerFormatter = new DateTimeOffsetFormatter(formatString);
    }

    public void Serialize(
      ref JsonWriter writer,
      DateTimeOffset? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
        writer.WriteNull();
      else
        this.innerFormatter.Serialize(ref writer, value.Value, formatterResolver);
    }

    public DateTimeOffset? Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? new DateTimeOffset?() : new DateTimeOffset?(this.innerFormatter.Deserialize(ref reader, formatterResolver));
    }
  }
}
