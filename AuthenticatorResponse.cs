// Decompiled with JetBrains decompiler
// Type: AuthenticatorResponse
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json;

public readonly struct AuthenticatorResponse : IEquatable<AuthenticatorResponse>, IJsonSerializable
{
  public readonly bool success;
  public readonly bool verified;
  public readonly string error;
  public readonly string token;
  public readonly string[] messages;
  public readonly string[] actions;
  public readonly string[] authAccepted;
  public readonly AuthenticatiorAuthReject[] authRejected;

  [SerializationConstructor]
  public AuthenticatorResponse(
    bool success,
    bool verified,
    string error,
    string token,
    string[] messages,
    string[] actions,
    string[] authAccepted,
    AuthenticatiorAuthReject[] authRejected)
  {
    this.success = success;
    this.verified = verified;
    this.error = error;
    this.token = token;
    this.messages = messages;
    this.actions = actions;
    this.authAccepted = authAccepted;
    this.authRejected = authRejected;
  }

  public bool Equals(AuthenticatorResponse other)
  {
    return this.success == other.success && this.verified == other.verified && (string.Equals(this.error, other.error) && string.Equals(this.token, other.token)) && (this.messages == other.messages && this.actions == other.actions && this.authAccepted == other.authAccepted) && this.authRejected == other.authRejected;
  }

  public override bool Equals(object obj)
  {
    return obj is AuthenticatorResponse other && this.Equals(other);
  }

  public override int GetHashCode()
  {
    bool flag = this.success;
    int num = flag.GetHashCode() * 397;
    flag = this.verified;
    int hashCode = flag.GetHashCode();
    return ((((((num ^ hashCode) * 397 ^ (this.error != null ? this.error.GetHashCode() : 0)) * 397 ^ (this.token != null ? this.token.GetHashCode() : 0)) * 397 ^ (this.messages != null ? this.messages.GetHashCode() : 0)) * 397 ^ (this.actions != null ? this.actions.GetHashCode() : 0)) * 397 ^ (this.authAccepted != null ? this.authAccepted.GetHashCode() : 0)) * 397 ^ (this.authRejected != null ? this.authRejected.GetHashCode() : 0);
  }

  public static bool operator ==(AuthenticatorResponse left, AuthenticatorResponse right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(AuthenticatorResponse left, AuthenticatorResponse right)
  {
    return !left.Equals(right);
  }
}
