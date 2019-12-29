// Decompiled with JetBrains decompiler
// Type: GameCore.Log
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

namespace GameCore
{
  [Serializable]
  public struct Log : IEquatable<Log>
  {
    public string text;
    public Color color;
    public bool nospace;

    public Log(string t, Color c, bool b)
    {
      this.text = t;
      this.color = c;
      this.nospace = b;
    }

    public bool Equals(Log other)
    {
      return this.text == other.text && this.color == other.color && this.nospace == other.nospace;
    }

    public override bool Equals(object obj)
    {
      return obj is Log other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return ((this.text != null ? this.text.GetHashCode() : 0) * 397 ^ this.color.GetHashCode()) * 397 ^ this.nospace.GetHashCode();
    }

    public static bool operator ==(Log left, Log right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(Log left, Log right)
    {
      return !left.Equals(right);
    }
  }
}
