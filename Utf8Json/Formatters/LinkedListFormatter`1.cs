// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.LinkedListFormatter`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

namespace Utf8Json.Formatters
{
  public sealed class LinkedListFormatter<T> : CollectionFormatterBase<T, LinkedList<T>, LinkedList<T>.Enumerator, LinkedList<T>>
  {
    private readonly CollectionDeserializeToBehaviour deserializeToBehaviour;

    public LinkedListFormatter()
      : this(CollectionDeserializeToBehaviour.Add)
    {
    }

    public LinkedListFormatter(
      CollectionDeserializeToBehaviour deserializeToBehaviour)
    {
      this.deserializeToBehaviour = deserializeToBehaviour;
    }

    protected override void Add(ref LinkedList<T> collection, int index, T value)
    {
      collection.AddLast(value);
    }

    protected override LinkedList<T> Complete(ref LinkedList<T> intermediateCollection)
    {
      return intermediateCollection;
    }

    protected override LinkedList<T> Create()
    {
      return new LinkedList<T>();
    }

    protected override LinkedList<T>.Enumerator GetSourceEnumerator(LinkedList<T> source)
    {
      return source.GetEnumerator();
    }

    protected override CollectionDeserializeToBehaviour? SupportedOverwriteBehaviour
    {
      get
      {
        return new CollectionDeserializeToBehaviour?(this.deserializeToBehaviour);
      }
    }

    protected override void AddOnOverwriteDeserialize(
      ref LinkedList<T> collection,
      int index,
      T value)
    {
      collection.AddLast(value);
    }

    protected override void ClearOnOverwriteDeserialize(ref LinkedList<T> value)
    {
      value.Clear();
    }
  }
}
