// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.CharFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Globalization;

namespace Utf8Json.Formatters
{
  public sealed class CharFormatter : IJsonFormatter<char>, IJsonFormatter
  {
    public static readonly CharFormatter Default = new CharFormatter();

    public void Serialize(
      ref JsonWriter writer,
      char value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteString(value.ToString((IFormatProvider) CultureInfo.InvariantCulture));
    }

    public char Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadString()[0];
    }
  }
}
