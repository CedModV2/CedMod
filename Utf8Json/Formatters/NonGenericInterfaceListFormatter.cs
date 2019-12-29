// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.NonGenericInterfaceListFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using System.Collections.Generic;

namespace Utf8Json.Formatters
{
  public sealed class NonGenericInterfaceListFormatter : IJsonFormatter<IList>, IJsonFormatter
  {
    public static readonly IJsonFormatter<IList> Default = (IJsonFormatter<IList>) new NonGenericInterfaceListFormatter();

    public void Serialize(
      ref JsonWriter writer,
      IList value,
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

    public IList Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (IList) null;
      int count = 0;
      IJsonFormatter<object> formatterWithVerify = formatterResolver.GetFormatterWithVerify<object>();
      List<object> objectList = new List<object>();
      reader.ReadIsBeginArrayWithVerify();
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
        objectList.Add(formatterWithVerify.Deserialize(ref reader, formatterResolver));
      return (IList) objectList;
    }
  }
}
