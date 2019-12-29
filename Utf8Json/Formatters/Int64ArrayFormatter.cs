// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.Int64ArrayFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class Int64ArrayFormatter : IJsonFormatter<long[]>, IJsonFormatter
  {
    public static readonly Int64ArrayFormatter Default = new Int64ArrayFormatter();

    public void Serialize(
      ref JsonWriter writer,
      long[] value,
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
          writer.WriteInt64(value[0]);
        for (int index = 1; index < value.Length; ++index)
        {
          writer.WriteValueSeparator();
          writer.WriteInt64(value[index]);
        }
        writer.WriteEndArray();
      }
    }

    public long[] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (long[]) null;
      reader.ReadIsBeginArrayWithVerify();
      long[] array = new long[4];
      int count = 0;
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
      {
        if (array.Length < count)
          Array.Resize<long>(ref array, count * 2);
        array[count - 1] = reader.ReadInt64();
      }
      Array.Resize<long>(ref array, count);
      return array;
    }
  }
}
