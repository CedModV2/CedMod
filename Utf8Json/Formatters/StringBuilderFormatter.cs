// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.StringBuilderFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Text;

namespace Utf8Json.Formatters
{
  public sealed class StringBuilderFormatter : IJsonFormatter<StringBuilder>, IJsonFormatter
  {
    public static readonly IJsonFormatter<StringBuilder> Default = (IJsonFormatter<StringBuilder>) new StringBuilderFormatter();

    public void Serialize(
      ref JsonWriter writer,
      StringBuilder value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value == null)
        writer.WriteNull();
      else
        writer.WriteString(value.ToString());
    }

    public StringBuilder Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? (StringBuilder) null : new StringBuilder(reader.ReadString());
    }
  }
}
