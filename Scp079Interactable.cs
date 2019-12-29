// Decompiled with JetBrains decompiler
// Type: Scp079Interactable
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Scp079Interactable : MonoBehaviour
{
  public List<Scp079Interactable.ZoneAndRoom> currentZonesAndRooms = new List<Scp079Interactable.ZoneAndRoom>();
  public Scp079Interactable.InteractableType type;
  public bool sameRoomOnly;
  public GameObject optionalObject;
  public string optionalParameter;

  public void OnMapGenerate()
  {
    Vector3[] vector3Array = new Vector3[5]
    {
      Vector3.left,
      Vector3.right,
      Vector3.forward,
      Vector3.back,
      Vector3.down
    };
    foreach (Vector3 vector3 in vector3Array)
    {
      Scp079Interactable.ZoneAndRoom zoneAndRoom = new Scp079Interactable.ZoneAndRoom();
      RaycastHit hitInfo;
      if (Physics.Raycast(new Ray(this.transform.position + vector3, Vector3.up), out hitInfo, 50f, (int) Interface079.singleton.roomDetectionMask))
      {
        Transform transform = hitInfo.transform;
        while ((UnityEngine.Object) transform != (UnityEngine.Object) null && !transform.transform.name.Contains("ROOT", StringComparison.OrdinalIgnoreCase) && !(transform.gameObject.tag == "Room"))
          transform = transform.transform.parent;
        if ((UnityEngine.Object) transform != (UnityEngine.Object) null)
          zoneAndRoom = new Scp079Interactable.ZoneAndRoom()
          {
            currentRoom = transform.transform.name,
            currentZone = transform.transform.parent.name
          };
      }
      if (!this.currentZonesAndRooms.Contains(zoneAndRoom))
        this.currentZonesAndRooms.Add(zoneAndRoom);
    }
  }

  public bool IsVisible(string curZone, string curRoom)
  {
    if (!this.sameRoomOnly)
      return true;
    foreach (Scp079Interactable.ZoneAndRoom currentZonesAndRoom in this.currentZonesAndRooms)
    {
      if (currentZonesAndRoom.currentZone == curZone && currentZonesAndRoom.currentRoom == curRoom)
        return true;
    }
    return false;
  }

  [Serializable]
  public struct ZoneAndRoom : IEquatable<Scp079Interactable.ZoneAndRoom>
  {
    public string currentZone;
    public string currentRoom;

    public bool Equals(Scp079Interactable.ZoneAndRoom other)
    {
      return string.Equals(this.currentZone, other.currentZone) && string.Equals(this.currentRoom, other.currentRoom);
    }

    public override bool Equals(object obj)
    {
      return obj is Scp079Interactable.ZoneAndRoom other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return (this.currentZone != null ? this.currentZone.GetHashCode() : 0) * 397 ^ (this.currentRoom != null ? this.currentRoom.GetHashCode() : 0);
    }

    public static bool operator ==(
      Scp079Interactable.ZoneAndRoom left,
      Scp079Interactable.ZoneAndRoom right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(
      Scp079Interactable.ZoneAndRoom left,
      Scp079Interactable.ZoneAndRoom right)
    {
      return !left.Equals(right);
    }
  }

  public enum InteractableType
  {
    Camera,
    Door,
    Tesla,
    Light,
    Speaker,
    ElevatorTeleport,
    Lockdown,
    ElevatorUse,
  }
}
