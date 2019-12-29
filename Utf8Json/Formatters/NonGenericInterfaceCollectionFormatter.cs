// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NonGenericInterfaceCollectionFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections;
using System.Collections.Generic;

namespace Utf8Json.Formatters
{
  public sealed class NonGenericInterfaceCollectionFormatter : IJsonFormatter<ICollection>, IJsonFormatter
  {
    public static readonly IJsonFormatter<ICollection> Default = (IJsonFormatter<ICollection>) new NonGenericInterfaceCollectionFormatter();

    public void Serialize(
      ref JsonWriter writer,
      ICollection value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value == null)
      {
        writer.WriteNull();
      }
      else
      {
        IJsonFormatter<object> formatterWithVerify = formatterResolver.GetFormatterWithVerify<object>();
        writer.WriteBeginArray();
        IEnumerator enumerator = value.GetEnumerator();
        try
        {
          if (enumerator.MoveNext())
          {
            formatterWithVerify.Serialize(ref writer, enumerator.Current, formatterResolver);
            while (enumerator.MoveNext())
            {
              writer.WriteValueSeparator();
              formatterWithVerify.Serialize(ref writer, enumerator.Current, formatterResolver);
            }
          }
        }
        finally
        {
          if (enumerator is IDisposable disposable)
            disposable.Dispose();
        }
        writer.WriteEndArray();
      }
    }

    public ICollection Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (ICollection) null;
      int count = 0;
      IJsonFormatter<object> formatterWithVerify = formatterResolver.GetFormatterWithVerify<object>();
      List<object> objectList = new List<object>();
      reader.ReadIsBeginArrayWithVerify();
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
        objectList.Add(formatterWithVerify.Deserialize(ref reader, formatterResolver));
      return (ICollection) objectList;
    }
  }
}
