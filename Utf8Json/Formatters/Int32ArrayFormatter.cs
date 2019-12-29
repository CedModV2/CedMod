// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.Int32ArrayFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class Int32ArrayFormatter : IJsonFormatter<int[]>, IJsonFormatter
  {
    public static readonly Int32ArrayFormatter Default = new Int32ArrayFormatter();

    public void Serialize(
      ref JsonWriter writer,
      int[] value,
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
          writer.WriteInt32(value[0]);
        for (int index = 1; index < value.Length; ++index)
        {
          writer.WriteValueSeparator();
          writer.WriteInt32(value[index]);
        }
        writer.WriteEndArray();
      }
    }

    public int[] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (int[]) null;
      reader.ReadIsBeginArrayWithVerify();
      int[] array = new int[4];
      int count = 0;
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
      {
        if (array.Length < count)
          Array.Resize<int>(ref array, count * 2);
        array[count - 1] = reader.ReadInt32();
      }
      Array.Resize<int>(ref array, count);
      return array;
    }
  }
}
