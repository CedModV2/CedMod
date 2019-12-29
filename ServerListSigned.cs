// Decompiled with JetBrains decompiler
// Type: ServerListSigned
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json;

public readonly struct ServerListSigned : IEquatable<ServerListSigned>, IJsonSerializable
{
  public readonly string payload;
  public readonly ulong timestamp;
  public readonly string signature;

  [SerializationConstructor]
  public ServerListSigned(string payload, ulong timestamp, string signature)
  {
    this.payload = payload;
    this.timestamp = timestamp;
    this.signature = signature;
  }

  public bool Equals(ServerListSigned other)
  {
    return this.payload == other.payload && (long) this.timestamp == (long) other.timestamp && this.signature == other.signature;
  }

  public override bool Equals(object obj)
  {
    return obj is ServerListSigned other && this.Equals(other);
  }

  public override int GetHashCode()
  {
    return ((this.payload != null ? this.payload.GetHashCode() : 0) * 397 ^ this.timestamp.GetHashCode()) * 397 ^ (this.signature != null ? this.signature.GetHashCode() : 0);
  }

  public static bool operator ==(ServerListSigned left, ServerListSigned right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(ServerListSigned left, ServerListSigned right)
  {
    return !left.Equals(right);
  }
}
