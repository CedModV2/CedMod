// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableCharFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace Utf8Json.Formatters
{
  public sealed class NullableCharFormatter : IJsonFormatter<char?>, IJsonFormatter
  {
    public static readonly NullableCharFormatter Default = new NullableCharFormatter();

    public void Serialize(
      ref JsonWriter writer,
      char? value,
      IJsonFormatterResolver formatterResolver)
    {
      if (!value.HasValue)
        writer.WriteNull();
      else
        CharFormatter.Default.Serialize(ref writer, value.Value, formatterResolver);
    }

    public char? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadIsNull() ? new char?() : new char?(CharFormatter.Default.Deserialize(ref reader, formatterResolver));
    }
  }
}
