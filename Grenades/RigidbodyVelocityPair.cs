// Decompiled with JetBrains decompiler
// Type: Grenades.RigidbodyVelocityPair
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

namespace Grenades
{
  public struct RigidbodyVelocityPair : IEquatable<RigidbodyVelocityPair>
  {
    public Vector3 linear;
    public Vector3 angular;

    public bool Equals(RigidbodyVelocityPair other)
    {
      return this.linear.Equals(other.linear) && this.angular.Equals(other.angular);
    }

    public override bool Equals(object obj)
    {
      return obj is RigidbodyVelocityPair other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return this.linear.GetHashCode() * 397 ^ this.angular.GetHashCode();
    }
  }
}
