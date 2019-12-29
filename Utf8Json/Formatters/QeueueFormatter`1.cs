// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.QeueueFormatter`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

namespace Utf8Json.Formatters
{
  public sealed class QeueueFormatter<T> : CollectionFormatterBase<T, Queue<T>, Queue<T>.Enumerator, Queue<T>>
  {
    private readonly CollectionDeserializeToBehaviour deserializeToBehaviour;

    public QeueueFormatter()
      : this(CollectionDeserializeToBehaviour.Add)
    {
    }

    public QeueueFormatter(
      CollectionDeserializeToBehaviour deserializeToBehaviour)
    {
      this.deserializeToBehaviour = deserializeToBehaviour;
    }

    protected override void Add(ref Queue<T> collection, int index, T value)
    {
      collection.Enqueue(value);
    }

    protected override Queue<T> Create()
    {
      return new Queue<T>();
    }

    protected override Queue<T>.Enumerator GetSourceEnumerator(Queue<T> source)
    {
      return source.GetEnumerator();
    }

    protected override Queue<T> Complete(ref Queue<T> intermediateCollection)
    {
      return intermediateCollection;
    }

    protected override CollectionDeserializeToBehaviour? SupportedOverwriteBehaviour
    {
      get
      {
        return new CollectionDeserializeToBehaviour?(this.deserializeToBehaviour);
      }
    }

    protected override void AddOnOverwriteDeserialize(ref Queue<T> collection, int index, T value)
    {
      collection.Enqueue(value);
    }

    protected override void ClearOnOverwriteDeserialize(ref Queue<T> value)
    {
      value.Clear();
    }
  }
}
