// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.SByteArrayFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class SByteArrayFormatter : IJsonFormatter<sbyte[]>, IJsonFormatter
  {
    public static readonly SByteArrayFormatter Default = new SByteArrayFormatter();

    public void Serialize(
      ref JsonWriter writer,
      sbyte[] value,
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
          writer.WriteSByte(value[0]);
        for (int index = 1; index < value.Length; ++index)
        {
          writer.WriteValueSeparator();
          writer.WriteSByte(value[index]);
        }
        writer.WriteEndArray();
      }
    }

    public sbyte[] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (sbyte[]) null;
      reader.ReadIsBeginArrayWithVerify();
      sbyte[] array = new sbyte[4];
      int count = 0;
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
      {
        if (array.Length < count)
          Array.Resize<sbyte>(ref array, count * 2);
        array[count - 1] = reader.ReadSByte();
      }
      Array.Resize<sbyte>(ref array, count);
      return array;
    }
  }
}
