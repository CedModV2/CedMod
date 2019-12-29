// Decompiled with JetBrains decompiler
// Type: ServerListItem
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json;

public readonly struct ServerListItem : IEquatable<ServerListItem>, IJsonSerializable
{
  public readonly string ip;
  public readonly ushort port;
  public readonly string players;
  public readonly string info;
  public readonly string pastebin;
  public readonly string version;
  public readonly bool friendlyFire;
  public readonly bool modded;
  public readonly bool whitelist;
  public readonly string official;

  [SerializationConstructor]
  public ServerListItem(
    string ip,
    ushort port,
    string players,
    string info,
    string pastebin,
    string version,
    bool friendlyFire,
    bool modded,
    bool whitelist,
    string official)
  {
    this.ip = ip;
    this.port = port;
    this.players = players;
    this.info = info;
    this.pastebin = pastebin;
    this.version = version;
    this.friendlyFire = friendlyFire;
    this.modded = modded;
    this.whitelist = whitelist;
    this.official = official;
  }

  public bool Equals(ServerListItem other)
  {
    return this.ip == other.ip && (int) this.port == (int) other.port && (this.players == other.players && this.info == other.info) && (this.pastebin == other.pastebin && this.version == other.version && (this.friendlyFire == other.friendlyFire && this.modded == other.modded)) && this.whitelist == other.whitelist && this.official == other.official;
  }

  public override bool Equals(object obj)
  {
    return obj is ServerListItem other && this.Equals(other);
  }

  public override int GetHashCode()
  {
    int num1 = ((((((this.ip != null ? this.ip.GetHashCode() : 0) * 397 ^ this.port.GetHashCode()) * 397 ^ (this.players != null ? this.players.GetHashCode() : 0)) * 397 ^ (this.info != null ? this.info.GetHashCode() : 0)) * 397 ^ (this.pastebin != null ? this.pastebin.GetHashCode() : 0)) * 397 ^ (this.version != null ? this.version.GetHashCode() : 0)) * 397;
    bool flag = this.friendlyFire;
    int hashCode1 = flag.GetHashCode();
    int num2 = (num1 ^ hashCode1) * 397;
    flag = this.modded;
    int hashCode2 = flag.GetHashCode();
    int num3 = (num2 ^ hashCode2) * 397;
    flag = this.whitelist;
    int hashCode3 = flag.GetHashCode();
    return (num3 ^ hashCode3) * 397 ^ (this.official != null ? this.official.GetHashCode() : 0);
  }

  public static bool operator ==(ServerListItem left, ServerListItem right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(ServerListItem left, ServerListItem right)
  {
    return !left.Equals(right);
  }
}
