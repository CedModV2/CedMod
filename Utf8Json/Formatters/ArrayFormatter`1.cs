// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.ArrayFormatter`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public class ArrayFormatter<T> : IJsonFormatter<T[]>, IJsonFormatter, IOverwriteJsonFormatter<T[]>
  {
    private static readonly ArrayPool<T> arrayPool = new ArrayPool<T>(99);
    private readonly CollectionDeserializeToBehaviour deserializeToBehaviour;

    public ArrayFormatter()
      : this(CollectionDeserializeToBehaviour.Add)
    {
    }

    public ArrayFormatter(
      CollectionDeserializeToBehaviour deserializeToBehaviour)
    {
      this.deserializeToBehaviour = deserializeToBehaviour;
    }

    public void Serialize(
      ref JsonWriter writer,
      T[] value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value == null)
      {
        writer.WriteNull();
      }
      else
      {
        writer.WriteBeginArray();
        IJsonFormatter<T> formatterWithVerify = formatterResolver.GetFormatterWithVerify<T>();
        if (value.Length != 0)
          formatterWithVerify.Serialize(ref writer, value[0], formatterResolver);
        for (int index = 1; index < value.Length; ++index)
        {
          writer.WriteValueSeparator();
          formatterWithVerify.Serialize(ref writer, value[index], formatterResolver);
        }
        writer.WriteEndArray();
      }
    }

    public T[] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (T[]) null;
      int count = 0;
      IJsonFormatter<T> formatterWithVerify = formatterResolver.GetFormatterWithVerify<T>();
      T[] array1 = ArrayFormatter<T>.arrayPool.Rent();
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
        T[] objArray = new T[count];
        Array.Copy((Array) array2, (Array) objArray, count);
        Array.Clear((Array) array1, 0, Math.Min(count, array1.Length));
        return objArray;
      }
      finally
      {
        ArrayFormatter<T>.arrayPool.Return(array1);
      }
    }

    public void DeserializeTo(
      ref T[] value,
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return;
      int count = 0;
      IJsonFormatter<T> formatterWithVerify = formatterResolver.GetFormatterWithVerify<T>();
      if (this.deserializeToBehaviour == CollectionDeserializeToBehaviour.Add)
      {
        T[] array1 = ArrayFormatter<T>.arrayPool.Rent();
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
          if (count == 0)
            return;
          T[] objArray = new T[value.Length + count];
          Array.Copy((Array) value, 0, (Array) objArray, 0, value.Length);
          Array.Copy((Array) array2, 0, (Array) objArray, value.Length, count);
          Array.Clear((Array) array1, 0, Math.Min(count, array1.Length));
        }
        finally
        {
          ArrayFormatter<T>.arrayPool.Return(array1);
        }
      }
      else
      {
        T[] array = value;
        reader.ReadIsBeginArrayWithVerify();
        while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
        {
          if (array.Length < count)
            Array.Resize<T>(ref array, array.Length * 2);
          array[count - 1] = formatterWithVerify.Deserialize(ref reader, formatterResolver);
        }
        Array.Resize<T>(ref array, count);
      }
    }
  }
}
