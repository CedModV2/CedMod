// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NullableStringArrayFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class NullableStringArrayFormatter : IJsonFormatter<string[]>, IJsonFormatter
  {
    public static readonly NullableStringArrayFormatter Default = new NullableStringArrayFormatter();

    public void Serialize(
      ref JsonWriter writer,
      string[] value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value == null)
      {
        writer.WriteNull();
      }
      else
      {
        writer.WriteBeginArray();
        if (value.Length != 0)
          writer.WriteString(value[0]);
        for (int index = 1; index < value.Length; ++index)
        {
          writer.WriteValueSeparator();
          writer.WriteString(value[index]);
        }
        writer.WriteEndArray();
      }
    }

    public string[] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (string[]) null;
      reader.ReadIsBeginArrayWithVerify();
      string[] array = new string[4];
      int count = 0;
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
      {
        if (array.Length < count)
          Array.Resize<string>(ref array, count * 2);
        array[count - 1] = reader.ReadString();
      }
      Array.Resize<string>(ref array, count);
      return array;
    }
  }
}
