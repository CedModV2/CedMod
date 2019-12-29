// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.ReadOnlyCollectionFormatter`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
  public sealed class ReadOnlyCollectionFormatter<T> : CollectionFormatterBase<T, ArrayBuffer<T>, ReadOnlyCollection<T>>
  {
    protected override void Add(ref ArrayBuffer<T> collection, int index, T value)
    {
      collection.Add(value);
    }

    protected override ReadOnlyCollection<T> Complete(
      ref ArrayBuffer<T> intermediateCollection)
    {
      return new ReadOnlyCollection<T>((IList<T>) intermediateCollection.ToArray());
    }

    protected override ArrayBuffer<T> Create()
    {
      return new ArrayBuffer<T>(4);
    }
  }
}
