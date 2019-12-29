// Decompiled with JetBrains decompiler
// Type: Authenticator.AuthenticatorPlayerObject
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json;

namespace Authenticator
{
  public readonly struct AuthenticatorPlayerObject : IEquatable<AuthenticatorPlayerObject>, IJsonSerializable
  {
    public readonly string Id;
    public readonly string Ip;
    public readonly string RequestIp;
    public readonly string Asn;
    public readonly string AuthSerial;
    public readonly string VacSession;

    [SerializationConstructor]
    public AuthenticatorPlayerObject(
      string Id,
      string Ip,
      string RequestIp,
      string Asn,
      string AuthSerial,
      string VacSession)
    {
      this.Id = Id;
      this.Ip = Ip;
      this.RequestIp = RequestIp;
      this.Asn = Asn;
      this.AuthSerial = AuthSerial;
      this.VacSession = VacSession;
    }

    public bool Equals(AuthenticatorPlayerObject other)
    {
      return this.Id == other.Id && this.Ip == other.Ip && (this.RequestIp == other.RequestIp && this.Asn == other.Asn) && this.AuthSerial == other.AuthSerial && this.VacSession == other.VacSession;
    }

    public override bool Equals(object obj)
    {
      return obj is AuthenticatorPlayerObject other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return (((((this.Id != null ? this.Id.GetHashCode() : 0) * 397 ^ (this.Ip != null ? this.Ip.GetHashCode() : 0)) * 397 ^ (this.RequestIp != null ? this.RequestIp.GetHashCode() : 0)) * 397 ^ (this.Asn != null ? this.Asn.GetHashCode() : 0)) * 397 ^ (this.AuthSerial != null ? this.AuthSerial.GetHashCode() : 0)) * 397 ^ (this.VacSession != null ? this.VacSession.GetHashCode() : 0);
    }

    public static bool operator ==(AuthenticatorPlayerObject left, AuthenticatorPlayerObject right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(AuthenticatorPlayerObject left, AuthenticatorPlayerObject right)
    {
      return !left.Equals(right);
    }
  }
}
