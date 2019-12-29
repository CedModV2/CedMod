// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NonGenericInterfaceEnumerableFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using System.Collections.Generic;

namespace Utf8Json.Formatters
{
  public sealed class NonGenericInterfaceEnumerableFormatter : IJsonFormatter<IEnumerable>, IJsonFormatter
  {
    public static readonly IJsonFormatter<IEnumerable> Default = (IJsonFormatter<IEnumerable>) new NonGenericInterfaceEnumerableFormatter();

    public void Serialize(
      ref JsonWriter writer,
      IEnumerable value,
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
        int num = 0;
        foreach (object obj in value)
        {
          if (num != 0)
            writer.WriteValueSeparator();
          formatterWithVerify.Serialize(ref writer, obj, formatterResolver);
        }
        writer.WriteEndArray();
      }
    }

    public IEnumerable Deserialize(
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (IEnumerable) null;
      int count = 0;
      IJsonFormatter<object> formatterWithVerify = formatterResolver.GetFormatterWithVerify<object>();
      List<object> objectList = new List<object>();
      reader.ReadIsBeginArrayWithVerify();
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
        objectList.Add(formatterWithVerify.Deserialize(ref reader, formatterResolver));
      return (IEnumerable) objectList;
    }
  }
}
