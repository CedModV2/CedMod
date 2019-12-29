// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NonGenericDictionaryFormatter`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections;

namespace Utf8Json.Formatters
{
  public sealed class NonGenericDictionaryFormatter<T> : IJsonFormatter<T>, IJsonFormatter
    where T : class, IDictionary, new()
  {
    public void Serialize(ref JsonWriter writer, T value, IJsonFormatterResolver formatterResolver)
    {
      if ((object) value == null)
      {
        writer.WriteNull();
      }
      else
      {
        IJsonFormatter<object> formatterWithVerify = formatterResolver.GetFormatterWithVerify<object>();
        writer.WriteBeginObject();
        IDictionaryEnumerator enumerator = value.GetEnumerator();
        try
        {
          if (enumerator.MoveNext())
          {
            DictionaryEntry current1 = (DictionaryEntry) enumerator.Current;
            writer.WritePropertyName(current1.Key.ToString());
            formatterWithVerify.Serialize(ref writer, current1.Value, formatterResolver);
            while (enumerator.MoveNext())
            {
              writer.WriteValueSeparator();
              DictionaryEntry current2 = (DictionaryEntry) enumerator.Current;
              writer.WritePropertyName(current2.Key.ToString());
              formatterWithVerify.Serialize(ref writer, current2.Value, formatterResolver);
            }
          }
        }
        finally
        {
          if (enumerator is IDisposable disposable)
            disposable.Dispose();
        }
        writer.WriteEndObject();
      }
    }

    public T Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return default (T);
      IJsonFormatter<object> formatterWithVerify = formatterResolver.GetFormatterWithVerify<object>();
      reader.ReadIsBeginObjectWithVerify();
      T obj1 = new T();
      int count = 0;
      while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
      {
        string str = reader.ReadPropertyName();
        object obj2 = formatterWithVerify.Deserialize(ref reader, formatterResolver);
        obj1.Add((object) str, obj2);
      }
      return obj1;
    }
  }
}
