// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.ListFormatter`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

namespace Utf8Json.Formatters
{
  public class ListFormatter<T> : IJsonFormatter<List<T>>, IJsonFormatter, IOverwriteJsonFormatter<List<T>>
  {
    private readonly CollectionDeserializeToBehaviour deserializeToBehaviour;

    public ListFormatter()
      : this(CollectionDeserializeToBehaviour.Add)
    {
    }

    public ListFormatter(
      CollectionDeserializeToBehaviour deserializeToBehaviour)
    {
      this.deserializeToBehaviour = deserializeToBehaviour;
    }

    public void Serialize(
      ref JsonWriter writer,
      List<T> value,
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
        if (value.Count != 0)
          formatterWithVerify.Serialize(ref writer, value[0], formatterResolver);
        for (int index = 1; index < value.Count; ++index)
        {
          writer.WriteValueSeparator();
          formatterWithVerify.Serialize(ref writer, value[index], formatterResolver);
        }
        writer.WriteEndArray();
      }
    }

    public List<T> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (List<T>) null;
      int count = 0;
      IJsonFormatter<T> formatterWithVerify = formatterResolver.GetFormatterWithVerify<T>();
      List<T> objList = new List<T>();
      reader.ReadIsBeginArrayWithVerify();
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
        objList.Add(formatterWithVerify.Deserialize(ref reader, formatterResolver));
      return objList;
    }

    public void DeserializeTo(
      ref List<T> value,
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return;
      int count = 0;
      IJsonFormatter<T> formatterWithVerify = formatterResolver.GetFormatterWithVerify<T>();
      List<T> objList = value;
      if (this.deserializeToBehaviour == CollectionDeserializeToBehaviour.OverwriteReplace)
        objList.Clear();
      reader.ReadIsBeginArrayWithVerify();
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
        objList.Add(formatterWithVerify.Deserialize(ref reader, formatterResolver));
    }
  }
}
