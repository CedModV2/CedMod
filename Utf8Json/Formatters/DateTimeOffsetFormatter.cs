// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.DateTimeOffsetFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Globalization;

namespace Utf8Json.Formatters
{
  public sealed class DateTimeOffsetFormatter : IJsonFormatter<DateTimeOffset>, IJsonFormatter
  {
    private readonly string formatString;

    public DateTimeOffsetFormatter()
    {
      this.formatString = (string) null;
    }

    public DateTimeOffsetFormatter(string formatString)
    {
      this.formatString = formatString;
    }

    public void Serialize(
      ref JsonWriter writer,
      DateTimeOffset value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteString(value.ToString(this.formatString));
    }

    public DateTimeOffset Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      string input = reader.ReadString();
      return this.formatString == null ? DateTimeOffset.Parse(input, (IFormatProvider) CultureInfo.InvariantCulture) : DateTimeOffset.ParseExact(input, this.formatString, (IFormatProvider) CultureInfo.InvariantCulture);
    }
  }
}
