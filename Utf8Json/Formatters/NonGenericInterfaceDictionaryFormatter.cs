// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NonGenericInterfaceDictionaryFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections;
using System.Collections.Generic;

namespace Utf8Json.Formatters
{
  public sealed class NonGenericInterfaceDictionaryFormatter : IJsonFormatter<IDictionary>, IJsonFormatter
  {
    public static readonly IJsonFormatter<IDictionary> Default = (IJsonFormatter<IDictionary>) new NonGenericInterfaceDictionaryFormatter();

    public void Serialize(
      ref JsonWriter writer,
      IDictionary value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value == null)
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

    public IDictionary Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (IDictionary) null;
      IJsonFormatter<object> formatterWithVerify = formatterResolver.GetFormatterWithVerify<object>();
      reader.ReadIsBeginObjectWithVerify();
      Dictionary<object, object> dictionary = new Dictionary<object, object>();
      int count = 0;
      while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
      {
        string str = reader.ReadPropertyName();
        object obj = formatterWithVerify.Deserialize(ref reader, formatterResolver);
        dictionary.Add((object) str, obj);
      }
      return (IDictionary) dictionary;
    }
  }
}
