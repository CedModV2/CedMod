// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.CharArrayFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class CharArrayFormatter : IJsonFormatter<char[]>, IJsonFormatter
  {
    public static readonly CharArrayFormatter Default = new CharArrayFormatter();

    public void Serialize(
      ref JsonWriter writer,
      char[] value,
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
          CharFormatter.Default.Serialize(ref writer, value[0], formatterResolver);
        for (int index = 1; index < value.Length; ++index)
        {
          writer.WriteValueSeparator();
          CharFormatter.Default.Serialize(ref writer, value[index], formatterResolver);
        }
        writer.WriteEndArray();
      }
    }

    public char[] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (char[]) null;
      reader.ReadIsBeginArrayWithVerify();
      char[] array = new char[4];
      int count = 0;
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
      {
        if (array.Length < count)
          Array.Resize<char>(ref array, count * 2);
        array[count - 1] = CharFormatter.Default.Deserialize(ref reader, formatterResolver);
      }
      Array.Resize<char>(ref array, count);
      return array;
    }
  }
}
