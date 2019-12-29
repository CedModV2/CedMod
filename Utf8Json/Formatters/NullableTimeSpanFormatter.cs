// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableTimeSpanFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class NullableTimeSpanFormatter : IJsonFormatter<TimeSpan?>, IJsonFormatter
  {
    private readonly TimeSpanFormatter innerFormatter;

    public NullableTimeSpanFormatter()
    {
      this.innerFormatter = new TimeSpanFormatter();
    }

    public void Serialize(
      ref JsonWriter writer,
      TimeSpan? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
        writer.WriteNull();
      else
        this.innerFormatter.Serialize(ref writer, value.Value, formatterResolver);
    }

    public TimeSpan? Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? new TimeSpan?() : new TimeSpan?(this.innerFormatter.Deserialize(ref reader, formatterResolver));
    }
  }
}
