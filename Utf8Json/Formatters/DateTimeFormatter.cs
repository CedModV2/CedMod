// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.DateTimeFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Globalization;

namespace Utf8Json.Formatters
{
  public sealed class DateTimeFormatter : IJsonFormatter<DateTime>, IJsonFormatter
  {
    private readonly string formatString;

    public DateTimeFormatter()
    {
      this.formatString = (string) null;
    }

    public DateTimeFormatter(string formatString)
    {
      this.formatString = formatString;
    }

    public void Serialize(
      ref JsonWriter writer,
      DateTime value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteString(value.ToString(this.formatString));
    }

    public DateTime Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      string s = reader.ReadString();
      return this.formatString == null ? DateTime.Parse(s, (IFormatProvider) CultureInfo.InvariantCulture) : DateTime.ParseExact(s, this.formatString, (IFormatProvider) CultureInfo.InvariantCulture);
    }
  }
}
