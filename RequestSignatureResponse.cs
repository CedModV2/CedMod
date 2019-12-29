// Decompiled with JetBrains decompiler
// Type: RequestSignatureResponse
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json;

public readonly struct RequestSignatureResponse : IEquatable<RequestSignatureResponse>, IJsonSerializable
{
  public readonly bool success;
  public readonly string error;
  public readonly string auth;
  public readonly string badge;
  public readonly string pub;
  public readonly string nonce;

  [SerializationConstructor]
  public RequestSignatureResponse(
    bool success,
    string error,
    string auth,
    string badge,
    string pub,
    string nonce)
  {
    this.success = success;
    this.error = error;
    this.auth = auth == null ? (string) null : Misc.Base64Decode(auth);
    this.badge = badge == null ? (string) null : Misc.Base64Decode(badge);
    this.pub = pub == null ? (string) null : Misc.Base64Decode(pub);
    this.nonce = nonce;
  }

  public bool Equals(RequestSignatureResponse other)
  {
    return this.success == other.success && this.error == other.error && (this.auth == other.auth && this.badge == other.badge) && this.pub == other.pub && this.nonce == other.nonce;
  }

  public override bool Equals(object obj)
  {
    return obj is RequestSignatureResponse other && this.Equals(other);
  }

  public override int GetHashCode()
  {
    return ((((this.success.GetHashCode() * 397 ^ (this.error != null ? this.error.GetHashCode() : 0)) * 397 ^ (this.auth != null ? this.auth.GetHashCode() : 0)) * 397 ^ (this.badge != null ? this.badge.GetHashCode() : 0)) * 397 ^ (this.pub != null ? this.pub.GetHashCode() : 0)) * 397 ^ (this.nonce != null ? this.nonce.GetHashCode() : 0);
  }

  public static bool operator ==(RequestSignatureResponse left, RequestSignatureResponse right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(RequestSignatureResponse left, RequestSignatureResponse right)
  {
    return !left.Equals(right);
  }
}
