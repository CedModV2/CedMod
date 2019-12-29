// Decompiled with JetBrains decompiler
// Type: Authenticator.AuthenticatorPlayerObjects
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json;

namespace Authenticator
{
  public readonly struct AuthenticatorPlayerObjects : IEquatable<AuthenticatorPlayerObjects>, IJsonSerializable
  {
    public readonly AuthenticatorPlayerObject[] objects;

    [SerializationConstructor]
    public AuthenticatorPlayerObjects(AuthenticatorPlayerObject[] objects)
    {
      this.objects = objects;
    }

    public bool Equals(AuthenticatorPlayerObjects other)
    {
      return this.objects == other.objects;
    }

    public override bool Equals(object obj)
    {
      return obj is AuthenticatorPlayerObjects other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return this.objects == null ? 0 : this.objects.GetHashCode();
    }

    public static bool operator ==(
      AuthenticatorPlayerObjects left,
      AuthenticatorPlayerObjects right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(
      AuthenticatorPlayerObjects left,
      AuthenticatorPlayerObjects right)
    {
      return !left.Equals(right);
    }
  }
}
