// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.Lookup`2
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utf8Json.Formatters
{
  internal class Lookup<TKey, TElement> : ILookup<TKey, TElement>, IEnumerable<IGrouping<TKey, TElement>>, IEnumerable
  {
    private readonly Dictionary<TKey, IGrouping<TKey, TElement>> groupings;

    public Lookup(
      Dictionary<TKey, IGrouping<TKey, TElement>> groupings)
    {
      this.groupings = groupings;
    }

    public IEnumerable<TElement> this[TKey key]
    {
      get
      {
        return (IEnumerable<TElement>) this.groupings[key];
      }
    }

    public int Count
    {
      get
      {
        return this.groupings.Count;
      }
    }

    public bool Contains(TKey key)
    {
      return this.groupings.ContainsKey(key);
    }

    public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
    {
      return (IEnumerator<IGrouping<TKey, TElement>>) this.groupings.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) this.groupings.Values.GetEnumerator();
    }
  }
}
