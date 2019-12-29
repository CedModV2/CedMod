// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.SingleArrayFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class SingleArrayFormatter : IJsonFormatter<float[]>, IJsonFormatter
  {
    public static readonly SingleArrayFormatter Default = new SingleArrayFormatter();

    public void Serialize(
      ref JsonWriter writer,
      float[] value,
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
          writer.WriteSingle(value[0]);
        for (int index = 1; index < value.Length; ++index)
        {
          writer.WriteValueSeparator();
          writer.WriteSingle(value[index]);
        }
        writer.WriteEndArray();
      }
    }

    public float[] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (float[]) null;
      reader.ReadIsBeginArrayWithVerify();
      float[] array = new float[4];
      int count = 0;
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
      {
        if (array.Length < count)
          Array.Resize<float>(ref array, count * 2);
        array[count - 1] = reader.ReadSingle();
      }
      Array.Resize<float>(ref array, count);
      return array;
    }
  }
}
