// Decompiled with JetBrains decompiler
// Type: PlayerPositionData
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using RemoteAdmin;
using System;
using UnityEngine;

public struct PlayerPositionData : IEquatable<PlayerPositionData>
{
  public Vector3 position;
  public readonly float rotation;
  public readonly int playerID;
  public readonly bool uses268;

  public PlayerPositionData(Vector3 pos, float rotY, int id, bool uses268 = false)
  {
    this.position = pos;
    this.rotation = rotY;
    this.playerID = id;
    this.uses268 = uses268;
  }

  public PlayerPositionData(GameObject player)
  {
    this.playerID = player.GetComponent<QueryProcessor>().PlayerId;
    this.uses268 = player.GetComponent<WeaponManager>().scp268.Enabled;
    Scp079PlayerScript component1 = player.GetComponent<Scp079PlayerScript>();
    if (component1.iAm079)
    {
      try
      {
        this.position = string.IsNullOrEmpty(component1.Speaker) ? Vector3.up * 7979f : GameObject.Find(component1.Speaker).transform.position;
      }
      catch
      {
        this.position = Vector3.up * 7970f;
      }
      this.rotation = 0.0f;
    }
    else
    {
      PlyMovementSync component2 = player.GetComponent<PlyMovementSync>();
      this.position = ReferenceHub.GetHub(player).characterClassManager.CurClass == RoleType.Spectator ? Vector3.up * 6000f : component2.RealModelPosition;
      this.rotation = component2.Rotations.y;
    }
  }

  public bool Equals(PlayerPositionData other)
  {
    return this.position == other.position && (double) this.rotation == (double) other.rotation && this.playerID == other.playerID && this.uses268 == other.uses268;
  }

  public override bool Equals(object obj)
  {
    return obj is PlayerPositionData other && this.Equals(other);
  }

  public override int GetHashCode()
  {
    return ((this.position.GetHashCode() * 397 ^ this.rotation.GetHashCode()) * 397 ^ this.playerID) * 397 ^ this.uses268.GetHashCode();
  }

  public static bool operator ==(PlayerPositionData left, PlayerPositionData right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(PlayerPositionData left, PlayerPositionData right)
  {
    return !left.Equals(right);
  }
}
