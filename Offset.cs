// Decompiled with JetBrains decompiler
// Type: Offset
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

[Serializable]
public struct Offset : IEquatable<Offset>
{
  public Vector3 position;
  public Vector3 rotation;
  public Vector3 scale;

  public bool Equals(Offset other)
  {
    return this.position == other.position && this.rotation == other.rotation && this.scale == other.scale;
  }

  public override bool Equals(object obj)
  {
    return obj is Offset other && this.Equals(other);
  }

  public override int GetHashCode()
  {
    return (this.position.GetHashCode() * 397 ^ this.rotation.GetHashCode()) * 397 ^ this.scale.GetHashCode();
  }

  public static bool operator ==(Offset left, Offset right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(Offset left, Offset right)
  {
    return !left.Equals(right);
  }
}
