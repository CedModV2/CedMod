// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.InterfaceCollectionFormatter`1
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

namespace Utf8Json.Formatters
{
  public sealed class InterfaceCollectionFormatter<T> : CollectionFormatterBase<T, List<T>, ICollection<T>>
  {
    protected override void Add(ref List<T> collection, int index, T value)
    {
      collection.Add(value);
    }

    protected override List<T> Create()
    {
      return new List<T>();
    }

    protected override ICollection<T> Complete(ref List<T> intermediateCollection)
    {
      return (ICollection<T>) intermediateCollection;
    }
  }
}
