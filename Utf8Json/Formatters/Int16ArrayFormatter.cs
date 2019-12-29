// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.Int16ArrayFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class Int16ArrayFormatter : IJsonFormatter<short[]>, IJsonFormatter
  {
    public static readonly Int16ArrayFormatter Default = new Int16ArrayFormatter();

    public void Serialize(
      ref JsonWriter writer,
      short[] value,
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
          writer.WriteInt16(value[0]);
        for (int index = 1; index < value.Length; ++index)
        {
          writer.WriteValueSeparator();
          writer.WriteInt16(value[index]);
        }
        writer.WriteEndArray();
      }
    }

    public short[] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (short[]) null;
      reader.ReadIsBeginArrayWithVerify();
      short[] array = new short[4];
      int count = 0;
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
      {
        if (array.Length < count)
          Array.Resize<short>(ref array, count * 2);
        array[count - 1] = reader.ReadInt16();
      }
      Array.Resize<short>(ref array, count);
      return array;
    }
  }
}
