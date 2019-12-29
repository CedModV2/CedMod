// Decompiled with JetBrains decompiler
// Type: SECTR_Sector
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("SECTR/Core/SECTR Sector")]
public class SECTR_Sector : SECTR_Member
{
  private static List<SECTR_Sector> allSectors = new List<SECTR_Sector>(128);
  private List<SECTR_Portal> portals = new List<SECTR_Portal>(8);
  private List<SECTR_Member> members = new List<SECTR_Member>(32);
  private bool visited;
  [SECTR_ToolTip("The terrain Sector attached on the top side of this Sector.")]
  public SECTR_Sector TopTerrain;
  [SECTR_ToolTip("The terrain Sector attached on the bottom side of this Sector.")]
  public SECTR_Sector BottomTerrain;
  [SECTR_ToolTip("The terrain Sector attached on the left side of this Sector.")]
  public SECTR_Sector LeftTerrain;
  [SECTR_ToolTip("The terrain Sector attached on the right side of this Sector.")]
  public SECTR_Sector RightTerrain;

  private SECTR_Sector()
  {
    this.isSector = true;
  }

  public static List<SECTR_Sector> All
  {
    get
    {
      return SECTR_Sector.allSectors;
    }
  }

  public static void GetContaining(ref List<SECTR_Sector> sectors, Vector3 position)
  {
    sectors.Clear();
    int count = SECTR_Sector.allSectors.Count;
    for (int index = 0; index < count; ++index)
    {
      SECTR_Sector allSector = SECTR_Sector.allSectors[index];
      if (allSector.TotalBounds.Contains(position))
        sectors.Add(allSector);
    }
  }

  public static void GetContaining(ref List<SECTR_Sector> sectors, Bounds bounds)
  {
    sectors.Clear();
    int count = SECTR_Sector.allSectors.Count;
    for (int index = 0; index < count; ++index)
    {
      SECTR_Sector allSector = SECTR_Sector.allSectors[index];
      if (allSector.TotalBounds.Intersects(bounds))
        sectors.Add(allSector);
    }
  }

  public bool Visited
  {
    get
    {
      return this.visited;
    }
    set
    {
      this.visited = value;
    }
  }

  public List<SECTR_Portal> Portals
  {
    get
    {
      return this.portals;
    }
  }

  public List<SECTR_Member> Members
  {
    get
    {
      return this.members;
    }
  }

  public bool IsConnectedTerrain
  {
    get
    {
      return (bool) (Object) this.LeftTerrain || (bool) (Object) this.RightTerrain || (bool) (Object) this.TopTerrain || (bool) (Object) this.BottomTerrain;
    }
  }

  public void ConnectTerrainNeighbors()
  {
    Terrain terrain = SECTR_Sector.GetTerrain(this);
    if (!(bool) (Object) terrain)
      return;
    terrain.SetNeighbors(SECTR_Sector.GetTerrain(this.LeftTerrain), SECTR_Sector.GetTerrain(this.TopTerrain), SECTR_Sector.GetTerrain(this.RightTerrain), SECTR_Sector.GetTerrain(this.BottomTerrain));
  }

  public void DisonnectTerrainNeighbors()
  {
    Terrain terrain1 = SECTR_Sector.GetTerrain(this);
    if ((bool) (Object) terrain1)
      terrain1.SetNeighbors((Terrain) null, (Terrain) null, (Terrain) null, (Terrain) null);
    Terrain terrain2 = SECTR_Sector.GetTerrain(this.TopTerrain);
    if ((bool) (Object) terrain2)
      terrain2.SetNeighbors(SECTR_Sector.GetTerrain(this.TopTerrain.LeftTerrain), SECTR_Sector.GetTerrain(this.TopTerrain.TopTerrain), SECTR_Sector.GetTerrain(this.TopTerrain.RightTerrain), (Terrain) null);
    Terrain terrain3 = SECTR_Sector.GetTerrain(this.BottomTerrain);
    if ((bool) (Object) terrain3)
      terrain3.SetNeighbors(SECTR_Sector.GetTerrain(this.BottomTerrain.LeftTerrain), (Terrain) null, SECTR_Sector.GetTerrain(this.BottomTerrain.RightTerrain), SECTR_Sector.GetTerrain(this.BottomTerrain.BottomTerrain));
    Terrain terrain4 = SECTR_Sector.GetTerrain(this.LeftTerrain);
    if ((bool) (Object) terrain4)
      terrain4.SetNeighbors(SECTR_Sector.GetTerrain(this.LeftTerrain.LeftTerrain), SECTR_Sector.GetTerrain(this.LeftTerrain.TopTerrain), (Terrain) null, SECTR_Sector.GetTerrain(this.LeftTerrain.BottomTerrain));
    Terrain terrain5 = SECTR_Sector.GetTerrain(this.RightTerrain);
    if (!(bool) (Object) terrain5)
      return;
    terrain5.SetNeighbors((Terrain) null, SECTR_Sector.GetTerrain(this.RightTerrain.TopTerrain), SECTR_Sector.GetTerrain(this.RightTerrain.RightTerrain), SECTR_Sector.GetTerrain(this.RightTerrain.BottomTerrain));
  }

  public void Register(SECTR_Portal portal)
  {
    if (this.portals.Contains(portal))
      return;
    this.portals.Add(portal);
  }

  public void Deregister(SECTR_Portal portal)
  {
    this.portals.Remove(portal);
  }

  public void Register(SECTR_Member member)
  {
    this.members.Add(member);
  }

  public void Deregister(SECTR_Member member)
  {
    this.members.Remove(member);
  }

  protected override void OnEnable()
  {
    SECTR_Sector.allSectors.Add(this);
    if ((bool) (Object) this.TopTerrain || (bool) (Object) this.BottomTerrain || ((bool) (Object) this.RightTerrain || (bool) (Object) this.LeftTerrain))
      this.ConnectTerrainNeighbors();
    base.OnEnable();
  }

  protected override void OnDisable()
  {
    List<SECTR_Member> sectrMemberList = new List<SECTR_Member>((IEnumerable<SECTR_Member>) this.members);
    int count = sectrMemberList.Count;
    for (int index = 0; index < count; ++index)
    {
      SECTR_Member sectrMember = sectrMemberList[index];
      if ((bool) (Object) sectrMember)
        sectrMember.SectorDisabled(this);
    }
    SECTR_Sector.allSectors.Remove(this);
    base.OnDisable();
  }

  protected static Terrain GetTerrain(SECTR_Sector sector)
  {
    return (bool) (Object) sector ? ((bool) (Object) sector.childProxy ? (Component) sector.childProxy : (Component) sector).GetComponentInChildren<Terrain>() : (Terrain) null;
  }
}
