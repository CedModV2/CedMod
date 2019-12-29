// Decompiled with JetBrains decompiler
// Type: SECTR_ULong
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

[Serializable]
public class SECTR_ULong
{
  [SerializeField]
  private int first;
  [SerializeField]
  private int second;

  public ulong value
  {
    get
    {
      return (ulong) this.second << 32 | (ulong) this.first;
    }
    set
    {
      this.first = (int) ((long) value & (long) uint.MaxValue);
      this.second = (int) (value >> 32);
    }
  }

  public SECTR_ULong(ulong newValue)
  {
    this.value = newValue;
  }

  public SECTR_ULong()
  {
    this.value = 0UL;
  }

  public override string ToString()
  {
    return string.Format("[ULong: value={0}, firstHalf={1}, secondHalf={2}]", (object) this.value, (object) this.first, (object) this.second);
  }

  public static bool operator >(SECTR_ULong a, ulong b)
  {
    return a.value > b;
  }

  public static bool operator >(ulong a, SECTR_ULong b)
  {
    return a > b.value;
  }

  public static bool operator <(SECTR_ULong a, ulong b)
  {
    return a.value < b;
  }

  public static bool operator <(ulong a, SECTR_ULong b)
  {
    return a < b.value;
  }
}
