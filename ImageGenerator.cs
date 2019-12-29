// Decompiled with JetBrains decompiler
// Type: ImageGenerator
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ImageGenerator : MonoBehaviour
{
  public static ImageGenerator[] ZoneGenerators = new ImageGenerator[3];
  public List<ImageGenerator.ColorMap> colorMap = new List<ImageGenerator.ColorMap>();
  public List<ImageGenerator.Room> availableRooms = new List<ImageGenerator.Room>();
  public List<GameObject> doors = new List<GameObject>();
  private List<ImageGenerator.MinimapElement> minimap = new List<ImageGenerator.MinimapElement>();
  public int height;
  public Texture2D[] maps;
  private Texture2D map;
  private Color[] copy;
  private string alias;
  public float gridSize;
  public float minimapSize;
  public ImageGenerator.MinimapLegend[] legend;
  public RectTransform minimapTarget;
  private Vector3 offset;
  public float y_offset;
  public static PocketDimensionGenerator pocketDimensionGenerator;
  private Transform entrRooms;
  public Font minimapFont;
  public ImageGenerator.RoomsOfType[] roomsOfType;

  public bool GenerateMap(int seed, string newAlias, out string blackbox)
  {
    blackbox = string.Empty;
    this.alias = newAlias;
    if (!NonFacilityCompatibility.currentSceneSettings.enableWorldGeneration)
      return true;
    try
    {
      blackbox = "Activating available rooms.";
      foreach (ImageGenerator.Room availableRoom in this.availableRooms)
      {
        foreach (GameObject gameObject in availableRoom.room)
          gameObject.SetActive(false);
      }
      ImageGenerator.pocketDimensionGenerator = this.GetComponent<PocketDimensionGenerator>();
      ImageGenerator.pocketDimensionGenerator.GenerateMap(seed);
      blackbox = "Randomizing...";
      UnityEngine.Random.InitState(seed);
      blackbox = "Picking up a map atlas...";
      this.map = this.maps[UnityEngine.Random.Range(0, this.maps.Length)];
      blackbox = "Entrance Zone initializing...";
      this.InitEntrance();
      blackbox = "Getting pixels...";
      this.copy = this.map.GetPixels();
      blackbox = "Checking rooms...";
      this.GeneratorTask_CheckRooms();
      blackbox = "Removing not required rooms...";
      this.GeneratorTask_RemoveNotRequired();
      blackbox = "Setting up rooms...";
      this.GeneratorTask_SetRooms();
      blackbox = "Cleaning up...";
      this.GeneratorTask_Cleanup();
      blackbox = "Calculating door spawnpoints...";
      this.GeneratorTask_RemoveDoubledDoorPoints();
      blackbox = "Reventing map...";
      this.map.SetPixels(this.copy);
      this.map.Apply();
      if ((UnityEngine.Object) this.entrRooms != (UnityEngine.Object) null)
        this.entrRooms.parent = (Transform) null;
      blackbox = "Completed.";
    }
    catch (Exception ex)
    {
      blackbox = blackbox + "\nError: " + ex.Message;
      return false;
    }
    return true;
  }

  private void InitEntrance()
  {
    if (this.height != -1001)
      return;
    Transform transform = GameObject.Find("HCZ_EZ_Checkpoint").transform;
    this.entrRooms = GameObject.Find("EntranceRooms").transform;
    for (int y = 0; y < this.map.height; ++y)
    {
      for (int x = 0; x < this.map.width; ++x)
      {
        if (this.map.GetPixel(x, y) == Color.white)
          this.offset = -new Vector3((float) x * this.gridSize, 0.0f, (float) y * this.gridSize) / 3f;
      }
    }
    this.offset += Vector3.up;
  }

  private void GeneratorTask_Cleanup()
  {
    foreach (ImageGenerator.RoomsOfType roomsOfType in this.roomsOfType)
    {
      foreach (ImageGenerator.Room room in roomsOfType.roomsOfType)
      {
        foreach (GameObject gameObject in room.room)
        {
          if (room.type != ImageGenerator.RoomType.Prison)
            gameObject.SetActive(false);
        }
      }
    }
  }

  private void GeneratorTask_RemoveDoubledDoorPoints()
  {
    if (this.doors.Count == 0)
      return;
    List<GameObject> list = ((IEnumerable<GameObject>) GameObject.FindGameObjectsWithTag("DoorPoint" + (object) this.height)).ToList<GameObject>();
    foreach (GameObject gameObject1 in list)
    {
      foreach (GameObject gameObject2 in list)
      {
        if ((double) Vector3.Distance(gameObject1.transform.position, gameObject2.transform.position) < 2.0 && !((UnityEngine.Object) gameObject1 == (UnityEngine.Object) gameObject2))
        {
          UnityEngine.Object.DestroyImmediate((UnityEngine.Object) gameObject2);
          this.GeneratorTask_RemoveDoubledDoorPoints();
          return;
        }
      }
    }
    List<SECTR_Portal> sectrPortalList = new List<SECTR_Portal>();
    for (int index = 0; index < this.doors.Count; ++index)
    {
      try
      {
        if (index < list.Count)
        {
          this.doors[index].transform.position = list[index].transform.position;
          this.doors[index].transform.rotation = list[index].transform.rotation;
          SECTR_Portal component = list[index].GetComponent<SECTR_Portal>();
          if (!((UnityEngine.Object) component == (UnityEngine.Object) null))
          {
            sectrPortalList.Add(component);
            if (this.height % 2 == 0)
              this.doors[index].GetComponent<Door>().SetPortal(component);
          }
        }
        else
          this.doors[index].SetActive(false);
      }
      catch
      {
        RandomSeedSync.DebugError("The bank of doors is empty for " + this.alias + "!", false);
      }
    }
    foreach (SECTR_Portal sectrPortal in sectrPortalList)
      sectrPortal.Setup();
  }

  private void GeneratorTask_SetRooms()
  {
    for (int y = 0; y < this.map.height; ++y)
    {
      for (int x = 0; x < this.map.width; ++x)
      {
        Color pixel = this.map.GetPixel(x, y);
        foreach (ImageGenerator.ColorMap color in this.colorMap)
        {
          if (!(color.color != pixel))
            this.PlaceRoom(new Vector2((float) x, (float) y) + color.centerOffset, color);
        }
      }
    }
  }

  private void GeneratorTask_RemoveNotRequired()
  {
    foreach (ImageGenerator.ColorMap color in this.colorMap)
    {
      bool flag = false;
      while (!flag)
      {
        int num = 0;
        foreach (ImageGenerator.Room room in this.roomsOfType[(int) color.type].roomsOfType)
          num += room.room.Count;
        if (num > this.roomsOfType[(int) color.type].amount)
        {
          flag = true;
          foreach (ImageGenerator.Room room in this.roomsOfType[(int) color.type].roomsOfType)
          {
            if (!room.required && room.room.Count > 0)
            {
              room.room[0].SetActive(false);
              room.room.RemoveAt(0);
              flag = false;
              break;
            }
          }
        }
        else
          break;
      }
    }
  }

  private void GeneratorTask_CheckRooms()
  {
    for (int y = 0; y < this.map.height; ++y)
    {
      for (int x = 0; x < this.map.width; ++x)
      {
        Color pixel = this.map.GetPixel(x, y);
        foreach (ImageGenerator.ColorMap color in this.colorMap)
        {
          ImageGenerator.ColorMap item = color;
          if (!(item.color != pixel))
          {
            this.BlankSquare(new Vector2((float) x, (float) y) + item.centerOffset);
            ++this.roomsOfType[(int) item.type].amount;
            List<ImageGenerator.Room> roomList = new List<ImageGenerator.Room>();
            bool flag1 = this.availableRooms.Any<ImageGenerator.Room>((Func<ImageGenerator.Room, bool>) (room => room.type == item.type && room.room.Count > 0 && room.required));
            bool flag2;
            do
            {
              flag2 = false;
              for (int index = 0; index < this.availableRooms.Count; ++index)
              {
                if (this.availableRooms[index].type == item.type && this.availableRooms[index].room.Count > 0 && !(!this.availableRooms[index].required & flag1))
                {
                  roomList.Add(new ImageGenerator.Room(this.availableRooms[index]));
                  this.availableRooms.RemoveAt(index);
                  flag2 = true;
                  break;
                }
              }
            }
            while (flag2);
            foreach (ImageGenerator.Room r in roomList)
              this.roomsOfType[(int) item.type].roomsOfType.Add(new ImageGenerator.Room(r));
          }
        }
      }
    }
    this.map.SetPixels(this.copy);
    this.map.Apply();
  }

  private void PlaceRoom(Vector2 pos, ImageGenerator.ColorMap type)
  {
    string str = "";
    try
    {
      str = "ERR#1 (marking bitmap)";
      this.BlankSquare(pos);
      str = "ERR#2 (looping)";
      ImageGenerator.Room room;
      do
      {
        str = "ERR#3 (randomizing)";
        int index = UnityEngine.Random.Range(0, this.roomsOfType[(int) type.type].roomsOfType.Count);
        str = string.Format("ERR#4 ({0} rooms remaining)", (object) this.roomsOfType[(int) type.type].roomsOfType.Count);
        room = this.roomsOfType[(int) type.type].roomsOfType[index];
        if (room.room.Count == 0)
        {
          str = "ERR#5 (randomizing)";
          this.roomsOfType[(int) type.type].roomsOfType.RemoveAt(index);
        }
      }
      while (room.room.Count == 0);
      room.room[0].transform.localPosition = new Vector3((float) ((double) pos.x * (double) this.gridSize / 3.0), (float) this.height, (float) ((double) pos.y * (double) this.gridSize / 3.0)) + this.offset;
      room.room[0].transform.localRotation = Quaternion.Euler(Vector3.up * (type.rotationY + this.y_offset));
      str = "ERR#6 (preparing minimap)";
      if ((UnityEngine.Object) this.minimapTarget != (UnityEngine.Object) null)
      {
        ImageGenerator.MinimapLegend minimapLegend1 = (ImageGenerator.MinimapLegend) null;
        foreach (ImageGenerator.MinimapLegend minimapLegend2 in this.legend)
        {
          if (room.room[0].name.Contains(minimapLegend2.containsInName))
            minimapLegend1 = minimapLegend2;
        }
        if (minimapLegend1 != null)
          this.minimap.Add(new ImageGenerator.MinimapElement()
          {
            icon = minimapLegend1.icon,
            position = pos,
            roomName = minimapLegend1.label,
            rotation = (int) type.rotationY,
            roomSource = room.room[0].gameObject
          });
      }
      str = "ERR#7 (list element removal)";
      room.room[0].SetActive(true);
      room.room.RemoveAt(0);
    }
    catch (Exception ex)
    {
      RandomSeedSync.DebugError("Failed to generate a room of " + this.alias + " zone (TYPE#" + type.type.ToString() + "). Error code - " + str + " | Debug info - " + ex.Message, false);
    }
  }

  private void BlankSquare(Vector2 centerPoint)
  {
    centerPoint = new Vector2(centerPoint.x - 1f, centerPoint.y - 1f);
    for (ushort index1 = 0; index1 < (ushort) 3; ++index1)
    {
      for (ushort index2 = 0; index2 < (ushort) 3; ++index2)
        this.map.SetPixel((int) centerPoint.x + (int) index1, (int) centerPoint.y + (int) index2, new Color(0.3921f, 0.3921f, 0.3921f, 1f));
    }
    this.map.Apply();
  }

  private void Awake()
  {
    int index = -1;
    switch (this.height)
    {
      case -1001:
        index = 2;
        break;
      case -1000:
        index = 1;
        break;
      case 0:
        index = 0;
        break;
    }
    if (index < 0)
      RandomSeedSync.DebugError("The array of Image Generators could not be set up. Height: " + (object) this.height, false);
    else
      ImageGenerator.ZoneGenerators[index] = this;
    foreach (GameObject door in this.doors)
    {
      if ((UnityEngine.Object) door != (UnityEngine.Object) null)
        door.GetComponent<Door>().SetZero();
    }
  }

  [Serializable]
  public class ColorMap
  {
    public Color color = Color.white;
    public ImageGenerator.RoomType type;
    public float rotationY;
    public Vector2 centerOffset;
  }

  [Serializable]
  public class RoomsOfType
  {
    public List<ImageGenerator.Room> roomsOfType = new List<ImageGenerator.Room>();
    public int amount;
  }

  [Serializable]
  public class Room
  {
    public List<GameObject> room = new List<GameObject>();
    public ImageGenerator.RoomType type;
    public bool required;
    public Texture iconMinimap;
    public string label;

    public Room(ImageGenerator.Room r)
    {
      this.room = r.room;
      this.type = r.type;
      this.required = r.required;
    }
  }

  [Serializable]
  public class MinimapElement
  {
    public string roomName;
    public Texture icon;
    public Vector2 position;
    public int rotation;
    public GameObject roomSource;
  }

  [Serializable]
  public class MinimapLegend
  {
    public string containsInName;
    public Texture icon;
    public string label;
  }

  public enum RoomType
  {
    Straight,
    Curve,
    RoomT,
    Cross,
    Endoff,
    Prison,
  }
}
