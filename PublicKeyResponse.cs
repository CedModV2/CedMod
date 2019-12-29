// Decompiled with JetBrains decompiler
// Type: PublicKeyResponse
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json;

public readonly struct PublicKeyResponse : IEquatable<PublicKeyResponse>, IJsonSerializable
{
  public readonly string key;
  public readonly string signature;
  public readonly string credits;

  [SerializationConstructor]
  public PublicKeyResponse(string key, string signature, string credits)
  {
    this.key = key;
    this.signature = signature;
    this.credits = credits;
  }

  public bool Equals(PublicKeyResponse other)
  {
    return this.key == other.key && this.signature == other.signature && this.credits == other.credits;
  }

  public override bool Equals(object obj)
  {
    return obj is PublicKeyResponse other && this.Equals(other);
  }

  public override int GetHashCode()
  {
    return (this.key != null ? this.key.GetHashCode() : 0) * 397 ^ (this.signature != null ? this.signature.GetHashCode() : 0) ^ (this.credits != null ? this.credits.GetHashCode() : 0);
  }

  public static bool operator ==(PublicKeyResponse left, PublicKeyResponse right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(PublicKeyResponse left, PublicKeyResponse right)
  {
    return !left.Equals(right);
  }
}
