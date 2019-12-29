// Decompiled with JetBrains decompiler
// Type: PlayerListSerialized
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using Utf8Json;

public readonly struct PlayerListSerialized : IEquatable<PlayerListSerialized>, IJsonSerializable
{
  public readonly List<string> objects;

  [SerializationConstructor]
  public PlayerListSerialized(List<string> objects)
  {
    this.objects = objects;
  }

  public bool Equals(PlayerListSerialized other)
  {
    return this.objects == other.objects;
  }

  public override bool Equals(object obj)
  {
    return obj is PlayerListSerialized other && this.Equals(other);
  }

  public override int GetHashCode()
  {
    return this.objects == null ? 0 : this.objects.GetHashCode();
  }

  public static bool operator ==(PlayerListSerialized left, PlayerListSerialized right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(PlayerListSerialized left, PlayerListSerialized right)
  {
    return !left.Equals(right);
  }
}
