// Decompiled with JetBrains decompiler
// Type: RenewResponse
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json;

public readonly struct RenewResponse : IEquatable<RenewResponse>, IJsonSerializable
{
  public readonly bool success;
  public readonly string error;
  public readonly string id;
  public readonly string nonce;
  public readonly string country;
  public readonly byte flags;
  public readonly ulong expiration;
  public readonly string preauth;
  public readonly string globalBan;
  public readonly ushort lifetime;

  [SerializationConstructor]
  public RenewResponse(
    bool success,
    string error,
    string id,
    string nonce,
    string country,
    byte flags,
    ulong expiration,
    string preauth,
    string globalBan,
    ushort lifetime)
  {
    this.success = success;
    this.error = error;
    this.id = id;
    this.country = country;
    this.nonce = nonce;
    this.flags = flags;
    this.expiration = expiration;
    this.preauth = preauth;
    this.globalBan = globalBan;
    this.lifetime = lifetime;
  }

  public bool Equals(RenewResponse other)
  {
    return this.success == other.success && this.error == other.error && (this.id == other.id && this.nonce == other.nonce) && (this.country == other.country && (int) this.flags == (int) other.flags && ((long) this.expiration == (long) other.expiration && this.preauth == other.preauth)) && this.globalBan == other.globalBan && (int) this.lifetime == (int) other.lifetime;
  }

  public override bool Equals(object obj)
  {
    return obj is RenewResponse other && this.Equals(other);
  }

  public override int GetHashCode()
  {
    return ((((((((this.success.GetHashCode() * 397 ^ (this.error != null ? this.error.GetHashCode() : 0)) * 397 ^ (this.id != null ? this.id.GetHashCode() : 0)) * 397 ^ (this.nonce != null ? this.nonce.GetHashCode() : 0)) * 397 ^ (this.country != null ? this.country.GetHashCode() : 0)) * 397 ^ this.flags.GetHashCode()) * 397 ^ this.expiration.GetHashCode()) * 397 ^ (this.preauth != null ? this.preauth.GetHashCode() : 0)) * 397 ^ (this.globalBan != null ? this.globalBan.GetHashCode() : 0)) * 397 ^ this.lifetime.GetHashCode();
  }

  public static bool operator ==(RenewResponse left, RenewResponse right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(RenewResponse left, RenewResponse right)
  {
    return !left.Equals(right);
  }
}
