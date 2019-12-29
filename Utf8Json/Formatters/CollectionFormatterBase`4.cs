// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.CollectionFormatterBase`4
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

namespace Utf8Json.Formatters
{
  public abstract class CollectionFormatterBase<TElement, TIntermediate, TEnumerator, TCollection> : IJsonFormatter<TCollection>, IJsonFormatter, IOverwriteJsonFormatter<TCollection>
    where TEnumerator : IEnumerator<TElement>
    where TCollection : class, IEnumerable<TElement>
  {
    public void Serialize(
      ref JsonWriter writer,
      TCollection value,
      IJsonFormatterResolver formatterResolver)
    {
      if ((object) value == null)
      {
        writer.WriteNull();
      }
      else
      {
        writer.WriteBeginArray();
        IJsonFormatter<TElement> formatterWithVerify = formatterResolver.GetFormatterWithVerify<TElement>();
        using (TEnumerator sourceEnumerator = this.GetSourceEnumerator(value))
        {
          bool flag = true;
          while (sourceEnumerator.MoveNext())
          {
            if (flag)
              flag = false;
            else
              writer.WriteValueSeparator();
            formatterWithVerify.Serialize(ref writer, sourceEnumerator.Current, formatterResolver);
          }
        }
        writer.WriteEndArray();
      }
    }

    public TCollection Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return default (TCollection);
      IJsonFormatter<TElement> formatterWithVerify = formatterResolver.GetFormatterWithVerify<TElement>();
      TIntermediate intermediate = this.Create();
      int count = 0;
      reader.ReadIsBeginArrayWithVerify();
      while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
        this.Add(ref intermediate, count - 1, formatterWithVerify.Deserialize(ref reader, formatterResolver));
      return this.Complete(ref intermediate);
    }

    public void DeserializeTo(
      ref TCollection value,
      ref JsonReader reader,
      IJsonFormatterResolver formatterResolver)
    {
      if (!this.SupportedOverwriteBehaviour.HasValue)
      {
        value = this.Deserialize(ref reader, formatterResolver);
      }
      else
      {
        if (reader.ReadIsNull())
          return;
        IJsonFormatter<TElement> formatterWithVerify = formatterResolver.GetFormatterWithVerify<TElement>();
        this.ClearOnOverwriteDeserialize(ref value);
        int count = 0;
        reader.ReadIsBeginArrayWithVerify();
        while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
          this.AddOnOverwriteDeserialize(ref value, count - 1, formatterWithVerify.Deserialize(ref reader, formatterResolver));
      }
    }

    protected abstract TEnumerator GetSourceEnumerator(TCollection source);

    protected abstract TIntermediate Create();

    protected abstract void Add(ref TIntermediate collection, int index, TElement value);

    protected abstract TCollection Complete(ref TIntermediate intermediateCollection);

    protected virtual CollectionDeserializeToBehaviour? SupportedOverwriteBehaviour
    {
      get
      {
        return new CollectionDeserializeToBehaviour?();
      }
    }

    protected virtual void ClearOnOverwriteDeserialize(ref TCollection value)
    {
    }

    protected virtual void AddOnOverwriteDeserialize(
      ref TCollection collection,
      int index,
      TElement value)
    {
    }
  }
}
