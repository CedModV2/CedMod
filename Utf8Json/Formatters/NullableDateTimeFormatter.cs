// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableDateTimeFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class NullableDateTimeFormatter : IJsonFormatter<DateTime?>, IJsonFormatter
  {
    private readonly DateTimeFormatter innerFormatter;

    public NullableDateTimeFormatter()
    {
      this.innerFormatter = new DateTimeFormatter();
    }

    public NullableDateTimeFormatter(string formatString)
    {
      this.innerFormatter = new DateTimeFormatter(formatString);
    }

    public void Serialize(
      ref JsonWriter writer,
      DateTime? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
        writer.WriteNull();
      else
        this.innerFormatter.Serialize(ref writer, value.Value, formatterResolver);
    }

    public DateTime? Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? new DateTime?() : new DateTime?(this.innerFormatter.Deserialize(ref reader, formatterResolver));
    }
  }
}
