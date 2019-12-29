// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.HashSetFormatter`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

namespace Utf8Json.Formatters
{
  public sealed class HashSetFormatter<T> : CollectionFormatterBase<T, HashSet<T>, HashSet<T>.Enumerator, HashSet<T>>
  {
    protected override void Add(ref HashSet<T> collection, int index, T value)
    {
      collection.Add(value);
    }

    protected override HashSet<T> Complete(ref HashSet<T> intermediateCollection)
    {
      return intermediateCollection;
    }

    protected override HashSet<T> Create()
    {
      return new HashSet<T>();
    }

    protected override HashSet<T>.Enumerator GetSourceEnumerator(HashSet<T> source)
    {
      return source.GetEnumerator();
    }
  }
}
