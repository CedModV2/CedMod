// Decompiled with JetBrains decompiler
// Type: Scp914.Scp914Recipe
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Scp914
{
  [Serializable]
  public struct Scp914Recipe : IEquatable<Scp914Recipe>
  {
    public ItemType itemID;
    public ItemType[] rough;
    public ItemType[] coarse;
    public ItemType[] oneToOne;
    public ItemType[] fine;
    public ItemType[] veryFine;

    public bool Equals(Scp914Recipe other)
    {
      return this.itemID == other.itemID && this.rough == other.rough && (this.coarse == other.coarse && this.oneToOne == other.oneToOne) && this.fine == other.fine && this.veryFine == other.veryFine;
    }

    public override bool Equals(object obj)
    {
      return obj is Scp914Recipe other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return (((((int) this.itemID * 397 ^ (this.rough != null ? this.rough.GetHashCode() : 0)) * 397 ^ (this.coarse != null ? this.coarse.GetHashCode() : 0)) * 397 ^ (this.oneToOne != null ? this.oneToOne.GetHashCode() : 0)) * 397 ^ (this.fine != null ? this.fine.GetHashCode() : 0)) * 397 ^ (this.veryFine != null ? this.veryFine.GetHashCode() : 0);
    }

    public static bool operator ==(Scp914Recipe left, Scp914Recipe right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(Scp914Recipe left, Scp914Recipe right)
    {
      return !left.Equals(right);
    }
  }
}
