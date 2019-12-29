// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.UInt16ArrayFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Formatters
{
  public sealed class UInt16ArrayFormatter : IJsonFormatter<ushort[]>, IJsonFormatter
  {
    public static readonly UInt16ArrayFormatter Default = new UInt16ArrayFormatter();

    public void Serialize(
      ref JsonWriter writer,
      ushort[] value,
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
          writer.WriteUInt16(value[0]);
        for (int index = 1; index < value.Length; ++index)
        {
          writer.WriteValueSeparator();
          writer.WriteUInt16(value[index]);
        }
        writer.WriteEndArray();
      }
    }

    public ushort[] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (ushort[]) null;
      reader.ReadIsBeginArrayWithVerify();
      ushort[] array = new ushort[4];
      int count = 0;
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
      {
        if (array.Length < count)
          Array.Resize<ushort>(ref array, count * 2);
        array[count - 1] = reader.ReadUInt16();
      }
      Array.Resize<ushort>(ref array, count);
      return array;
    }
  }
}
