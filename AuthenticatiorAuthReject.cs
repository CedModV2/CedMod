// Decompiled with JetBrains decompiler
// Type: AuthenticatiorAuthReject
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json;

public readonly struct AuthenticatiorAuthReject : IEquatable<AuthenticatiorAuthReject>, IJsonSerializable
{
  public readonly string Id;
  public readonly string Reason;

  [SerializationConstructor]
  public AuthenticatiorAuthReject(string id, string reason)
  {
    this.Id = id;
    this.Reason = reason;
  }

  public bool Equals(AuthenticatiorAuthReject other)
  {
    return string.Equals(this.Id, other.Id) && string.Equals(this.Reason, other.Reason);
  }

  public override bool Equals(object obj)
  {
    return obj is AuthenticatiorAuthReject other && this.Equals(other);
  }

  public override int GetHashCode()
  {
    return (this.Id != null ? this.Id.GetHashCode() : 0) * 397 ^ (this.Reason != null ? this.Reason.GetHashCode() : 0);
  }

  public static bool operator ==(AuthenticatiorAuthReject left, AuthenticatiorAuthReject right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(AuthenticatiorAuthReject left, AuthenticatiorAuthReject right)
  {
    return !left.Equals(right);
  }
}
