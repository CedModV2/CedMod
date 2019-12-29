// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.UInt32ArrayFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class UInt32ArrayFormatter : IJsonFormatter<uint[]>, IJsonFormatter
  {
    public static readonly UInt32ArrayFormatter Default = new UInt32ArrayFormatter();

    public void Serialize(
      ref JsonWriter writer,
      uint[] value,
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
          writer.WriteUInt32(value[0]);
        for (int index = 1; index < value.Length; ++index)
        {
          writer.WriteValueSeparator();
          writer.WriteUInt32(value[index]);
        }
        writer.WriteEndArray();
      }
    }

    public uint[] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (uint[]) null;
      reader.ReadIsBeginArrayWithVerify();
      uint[] array = new uint[4];
      int count = 0;
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
      {
        if (array.Length < count)
          Array.Resize<uint>(ref array, count * 2);
        array[count - 1] = reader.ReadUInt32();
      }
      Array.Resize<uint>(ref array, count);
      return array;
    }
  }
}
