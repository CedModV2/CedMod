// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableStringFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace Utf8Json.Formatters
{
  public sealed class NullableStringFormatter : IJsonFormatter<string>, IJsonFormatter, IObjectPropertyNameFormatter<string>
  {
    public static readonly IJsonFormatter<string> Default = (IJsonFormatter<string>) new NullableStringFormatter();

    public void Serialize(
      ref JsonWriter writer,
      string value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteString(value);
    }

    public string Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadString();
    }

    public void SerializeToPropertyName(
      ref JsonWriter writer,
      string value,
      IJsonFormatterResolver formatterResolver)
    {
      writer.WriteString(value);
    }

    public string DeserializeFromPropertyName(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      return reader.ReadString();
    }
  }
}
