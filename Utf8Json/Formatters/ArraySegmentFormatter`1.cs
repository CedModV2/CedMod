// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.ArraySegmentFormatter`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public class ArraySegmentFormatter<T> : IJsonFormatter<ArraySegment<T>>, IJsonFormatter
  {
    private static readonly ArrayPool<T> arrayPool = new ArrayPool<T>(99);

    public void Serialize(
      ref JsonWriter writer,
      ArraySegment<T> value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value.Array == null)
      {
        writer.WriteNull();
      }
      else
      {
        T[] array = value.Array;
        int offset = value.Offset;
        int count = value.Count;
        writer.WriteBeginArray();
        IJsonFormatter<T> formatterWithVerify = formatterResolver.GetFormatterWithVerify<T>();
        if (count != 0)
          formatterWithVerify.Serialize(ref writer, value.Array[offset], formatterResolver);
        for (int index = 1; index < count; ++index)
        {
          writer.WriteValueSeparator();
          formatterWithVerify.Serialize(ref writer, array[offset + index], formatterResolver);
        }
        writer.WriteEndArray();
      }
    }

    public ArraySegment<T> Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return new ArraySegment<T>();
      int count = 0;
      IJsonFormatter<T> formatterWithVerify = formatterResolver.GetFormatterWithVerify<T>();
      T[] array1 = ArraySegmentFormatter<T>.arrayPool.Rent();
      try
      {
        T[] array2 = array1;
        reader.ReadIsBeginArrayWithVerify();
        while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
        {
          if (array2.Length < count)
            Array.Resize<T>(ref array2, array2.Length * 2);
          array2[count - 1] = formatterWithVerify.Deserialize(ref reader, formatterResolver);
        }
        T[] array3 = new T[count];
        Array.Copy((Array) array2, (Array) array3, count);
        Array.Clear((Array) array1, 0, Math.Min(count, array1.Length));
        return new ArraySegment<T>(array3, 0, array3.Length);
      }
      finally
      {
        ArraySegmentFormatter<T>.arrayPool.Return(array1);
      }
    }
  }
}
