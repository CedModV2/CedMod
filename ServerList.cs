// Decompiled with JetBrains decompiler
// Type: ServerList
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json;

public readonly struct ServerList : IEquatable<ServerList>, IJsonSerializable
{
  public readonly ServerListItem[] servers;

  [SerializationConstructor]
  public ServerList(ServerListItem[] servers)
  {
    this.servers = servers;
  }

  public bool Equals(ServerList other)
  {
    return this.servers == other.servers;
  }

  public override bool Equals(object obj)
  {
    return obj is ServerList other && this.Equals(other);
  }

  public override int GetHashCode()
  {
    return this.servers == null ? 0 : this.servers.GetHashCode();
  }

  public static bool operator ==(ServerList left, ServerList right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(ServerList left, ServerList right)
  {
    return !left.Equals(right);
  }
}
