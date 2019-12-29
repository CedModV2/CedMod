// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.DictionaryFormatterBase`5
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

namespace Utf8Json.Formatters
{
  public abstract class DictionaryFormatterBase<TKey, TValue, TIntermediate, TEnumerator, TDictionary> : IJsonFormatter<TDictionary>, IJsonFormatter
    where TEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    where TDictionary : class, IEnumerable<KeyValuePair<TKey, TValue>>
  {
    public void Serialize(
      ref JsonWriter writer,
      TDictionary value,
      IJsonFormatterResolver formatterResolver)
    {
      if ((object) value == null)
      {
        writer.WriteNull();
      }
      else
      {
        IObjectPropertyNameFormatter<TKey> formatterWithVerify1 = formatterResolver.GetFormatterWithVerify<TKey>() as IObjectPropertyNameFormatter<TKey>;
        IJsonFormatter<TValue> formatterWithVerify2 = formatterResolver.GetFormatterWithVerify<TValue>();
        writer.WriteBeginObject();
        using (TEnumerator sourceEnumerator = this.GetSourceEnumerator(value))
        {
          if (formatterWithVerify1 != null)
          {
            if (sourceEnumerator.MoveNext())
            {
              KeyValuePair<TKey, TValue> current1 = sourceEnumerator.Current;
              formatterWithVerify1.SerializeToPropertyName(ref writer, current1.Key, formatterResolver);
              writer.WriteNameSeparator();
              formatterWithVerify2.Serialize(ref writer, current1.Value, formatterResolver);
              while (sourceEnumerator.MoveNext())
              {
                writer.WriteValueSeparator();
                KeyValuePair<TKey, TValue> current2 = sourceEnumerator.Current;
                formatterWithVerify1.SerializeToPropertyName(ref writer, current2.Key, formatterResolver);
                writer.WriteNameSeparator();
                formatterWithVerify2.Serialize(ref writer, current2.Value, formatterResolver);
              }
            }
          }
          else if (sourceEnumerator.MoveNext())
          {
            KeyValuePair<TKey, TValue> current1 = sourceEnumerator.Current;
            writer.WriteString(current1.Key.ToString());
            writer.WriteNameSeparator();
            formatterWithVerify2.Serialize(ref writer, current1.Value, formatterResolver);
            while (sourceEnumerator.MoveNext())
            {
              writer.WriteValueSeparator();
              KeyValuePair<TKey, TValue> current2 = sourceEnumerator.Current;
              writer.WriteString(current2.Key.ToString());
              writer.WriteNameSeparator();
              formatterWithVerify2.Serialize(ref writer, current2.Value, formatterResolver);
            }
          }
        }
        writer.WriteEndObject();
      }
    }

    public TDictionary Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return default (TDictionary);
      if (!(formatterResolver.GetFormatterWithVerify<TKey>() is IObjectPropertyNameFormatter<TKey> formatterWithVerify))
        throw new InvalidOperationException(typeof (TKey).ToString() + " does not support dictionary key deserialize.");
      IJsonFormatter<TValue> formatterWithVerify1 = formatterResolver.GetFormatterWithVerify<TValue>();
      reader.ReadIsBeginObjectWithVerify();
      TIntermediate intermediate = this.Create();
      int count = 0;
      while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
      {
        TKey key = formatterWithVerify.DeserializeFromPropertyName(ref reader, formatterResolver);
        reader.ReadIsNameSeparatorWithVerify();
        TValue obj = formatterWithVerify1.Deserialize(ref reader, formatterResolver);
        this.Add(ref intermediate, count - 1, key, obj);
      }
      return this.Complete(ref intermediate);
    }

    protected abstract TEnumerator GetSourceEnumerator(TDictionary source);

    protected abstract TIntermediate Create();

    protected abstract void Add(ref TIntermediate collection, int index, TKey key, TValue value);

    protected abstract TDictionary Complete(ref TIntermediate intermediateCollection);
  }
}
