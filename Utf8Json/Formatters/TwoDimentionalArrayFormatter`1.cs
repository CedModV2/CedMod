// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.TwoDimentionalArrayFormatter`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class TwoDimentionalArrayFormatter<T> : IJsonFormatter<T[,]>, IJsonFormatter
  {
    public void Serialize(
      ref JsonWriter writer,
      T[,] value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value == null)
      {
        writer.WriteNull();
      }
      else
      {
        IJsonFormatter<T> formatterWithVerify = formatterResolver.GetFormatterWithVerify<T>();
        int length1 = value.GetLength(0);
        int length2 = value.GetLength(1);
        writer.WriteBeginArray();
        for (int index1 = 0; index1 < length1; ++index1)
        {
          if (index1 != 0)
            writer.WriteValueSeparator();
          writer.WriteBeginArray();
          for (int index2 = 0; index2 < length2; ++index2)
          {
            if (index2 != 0)
              writer.WriteValueSeparator();
            formatterWithVerify.Serialize(ref writer, value[index1, index2], formatterResolver);
          }
          writer.WriteEndArray();
        }
        writer.WriteEndArray();
      }
    }

    public T[,] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (T[,]) null;
      ArrayBuffer<ArrayBuffer<T>> arrayBuffer1 = new ArrayBuffer<ArrayBuffer<T>>(4);
      IJsonFormatter<T> formatterWithVerify = formatterResolver.GetFormatterWithVerify<T>();
      int length = 0;
      int count1 = 0;
      reader.ReadIsBeginArrayWithVerify();
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count1))
      {
        ArrayBuffer<T> arrayBuffer2 = new ArrayBuffer<T>(length == 0 ? 4 : length);
        int count2 = 0;
        reader.ReadIsBeginArrayWithVerify();
        while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count2))
          arrayBuffer2.Add(formatterWithVerify.Deserialize(ref reader, formatterResolver));
        length = arrayBuffer2.Size;
        arrayBuffer1.Add(arrayBuffer2);
      }
      T[,] objArray = new T[arrayBuffer1.Size, length];
      for (int index1 = 0; index1 < arrayBuffer1.Size; ++index1)
      {
        for (int index2 = 0; index2 < length; ++index2)
          objArray[index1, index2] = arrayBuffer1.Buffer[index1].Buffer[index2];
      }
      return objArray;
    }
  }
}
