// Decompiled with JetBrains decompiler
// Type: RoomManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
  public int useSimulator = -1;
  public List<RoomManager.Room> rooms = new List<RoomManager.Room>();
  public List<RoomManager.RoomPosition> positions = new List<RoomManager.RoomPosition>();
  public bool isGenerated;

  private void Start()
  {
    if (this.useSimulator == -1)
      return;
    this.GenerateMap(this.useSimulator);
  }

  public void GenerateMap(int seed)
  {
    UnityEngine.Object.FindObjectOfType<GameCore.Console>();
    if (!TutorialManager.status)
    {
      this.GetComponent<PocketDimensionGenerator>().GenerateMap(seed);
      for (int index = 0; index < this.positions.Count; ++index)
      {
        this.positions[index].point.name = "POINT" + (object) index;
        if (!((UnityEngine.Object) this.positions[index].point.GetComponent<Point>() != (UnityEngine.Object) null))
        {
          Debug.LogError((object) "RoomManager: Missing 'Point' script at current position.");
          return;
        }
      }
      UnityEngine.Random.InitState(seed);
      GameCore.Console.AddLog("[MG REPLY]: Successfully recieved map seed!", (Color) new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue), true);
      List<RoomManager.RoomPosition> positions = this.positions;
      GameCore.Console.AddLog("[MG TASK]: Setting rooms positions...", (Color) new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue), false);
      foreach (RoomManager.Room room in this.rooms)
      {
        GameCore.Console.AddLog("\t\t[MG INFO]: " + room.label + " is about to set!", (Color) new Color32((byte) 120, (byte) 120, (byte) 120, byte.MaxValue), true);
        List<int> intList = new List<int>();
        for (int index = 0; index < positions.Count; ++index)
        {
          if (this.positions[index].type.Equals(room.type))
          {
            bool flag = true;
            foreach (Point componentsInChild in room.roomPrefab.GetComponentsInChildren<Point>())
            {
              if (this.positions[index].point.name == componentsInChild.gameObject.name)
                flag = false;
            }
            if (flag)
              intList.Add(index);
          }
        }
        foreach (int index in intList)
        {
          foreach (Point componentsInChild in room.roomPrefab.GetComponentsInChildren<Point>())
          {
            if (this.positions[index].point.name == componentsInChild.gameObject.name)
              intList.Remove(index);
          }
        }
        int index1 = intList[UnityEngine.Random.Range(0, intList.Count)];
        RoomManager.RoomPosition position = this.positions[index1];
        GameObject roomPrefab = room.roomPrefab;
        room.readonlyPoint = position.point;
        roomPrefab.transform.SetParent(position.point);
        roomPrefab.transform.localPosition = room.roomOffset.position;
        roomPrefab.transform.localRotation = Quaternion.Euler(room.roomOffset.rotation);
        roomPrefab.transform.localScale = room.roomOffset.scale;
        roomPrefab.SetActive(true);
        this.positions.RemoveAt(index1);
      }
    }
    GameCore.Console.AddLog("--Map successfully generated--", (Color) new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue), false);
    this.isGenerated = true;
  }

  [Serializable]
  public class Room
  {
    public string label;
    public Offset roomOffset;
    public GameObject roomPrefab;
    public string type;
    public Transform readonlyPoint;
    public Offset iconoffset;
  }

  [Serializable]
  public struct RoomPosition : IEquatable<RoomManager.RoomPosition>
  {
    public string type;
    public Transform point;
    public RectTransform ui_point;

    public bool Equals(RoomManager.RoomPosition other)
    {
      return string.Equals(this.type, other.type) && (UnityEngine.Object) this.point == (UnityEngine.Object) other.point && (UnityEngine.Object) this.ui_point == (UnityEngine.Object) other.ui_point;
    }

    public override bool Equals(object obj)
    {
      return obj is RoomManager.RoomPosition other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return ((this.type != null ? this.type.GetHashCode() : 0) * 397 ^ ((UnityEngine.Object) this.point != (UnityEngine.Object) null ? this.point.GetHashCode() : 0)) * 397 ^ ((UnityEngine.Object) this.ui_point != (UnityEngine.Object) null ? this.ui_point.GetHashCode() : 0);
    }

    public static bool operator ==(RoomManager.RoomPosition left, RoomManager.RoomPosition right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(RoomManager.RoomPosition left, RoomManager.RoomPosition right)
    {
      return !left.Equals(right);
    }
  }
}
