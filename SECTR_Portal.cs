// Decompiled with JetBrains decompiler
// Type: SECTR_Portal
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("SECTR/Core/SECTR Portal")]
public class SECTR_Portal : SECTR_Hull
{
  private static List<SECTR_Portal> allPortals = new List<SECTR_Portal>(128);
  [SerializeField]
  [HideInInspector]
  private SECTR_Sector frontSector;
  [SerializeField]
  [HideInInspector]
  private SECTR_Sector backSector;
  private bool visited;
  [SECTR_ToolTip("Flags for this Portal. Used in graph traversals and the like.", null, typeof (SECTR_Portal.PortalFlags))]
  public SECTR_Portal.PortalFlags Flags;

  public static List<SECTR_Portal> All
  {
    get
    {
      return SECTR_Portal.allPortals;
    }
  }

  public void Setup()
  {
    int num = Mathf.RoundToInt(Vector3.Angle(this.transform.forward, Vector3.forward)) % 180;
    this.transform.position += Vector3.up / 2f;
    RaycastHit hitInfo;
    if (Physics.Raycast(this.transform.position - this.transform.forward, Vector3.down, out hitInfo))
      this.FrontSector = hitInfo.collider.GetComponentInParent<SECTR_Sector>();
    if (!Physics.Raycast(this.transform.position + this.transform.forward, Vector3.down, out hitInfo))
      return;
    this.BackSector = hitInfo.collider.GetComponentInParent<SECTR_Sector>();
  }

  public Vector3 GetRandomSectorPos()
  {
    return UnityEngine.Random.Range(0, 100) >= 50 ? this.backSector.transform.position : this.frontSector.transform.position;
  }

  public SECTR_Sector FrontSector
  {
    set
    {
      if (!((UnityEngine.Object) this.frontSector != (UnityEngine.Object) value))
        return;
      if ((bool) (UnityEngine.Object) this.frontSector)
        this.frontSector.Deregister(this);
      this.frontSector = value;
      if (!(bool) (UnityEngine.Object) this.frontSector)
        return;
      this.frontSector.Register(this);
    }
    get
    {
      return !(bool) (UnityEngine.Object) this.frontSector || !this.frontSector.enabled ? (SECTR_Sector) null : this.frontSector;
    }
  }

  public SECTR_Sector BackSector
  {
    set
    {
      if (!((UnityEngine.Object) this.backSector != (UnityEngine.Object) value))
        return;
      if ((bool) (UnityEngine.Object) this.backSector)
        this.backSector.Deregister(this);
      this.backSector = value;
      if (!(bool) (UnityEngine.Object) this.backSector)
        return;
      this.backSector.Register(this);
    }
    get
    {
      return !(bool) (UnityEngine.Object) this.backSector || !this.backSector.enabled ? (SECTR_Sector) null : this.backSector;
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

  public IEnumerable<SECTR_Sector> GetSectors()
  {
    yield return this.FrontSector;
    yield return this.BackSector;
  }

  public void SetFlag(SECTR_Portal.PortalFlags flag, bool on)
  {
    if (on)
      this.Flags |= flag;
    else
      this.Flags &= ~flag;
  }

  private void OnEnable()
  {
    SECTR_Portal.allPortals.Add(this);
    if ((bool) (UnityEngine.Object) this.frontSector)
      this.frontSector.Register(this);
    if (!(bool) (UnityEngine.Object) this.backSector)
      return;
    this.backSector.Register(this);
  }

  private void OnDisable()
  {
    SECTR_Portal.allPortals.Remove(this);
    if ((bool) (UnityEngine.Object) this.frontSector)
      this.frontSector.Deregister(this);
    if (!(bool) (UnityEngine.Object) this.backSector)
      return;
    this.backSector.Deregister(this);
  }

  [System.Flags]
  public enum PortalFlags
  {
    Closed = 1,
    Locked = 2,
    PassThrough = 4,
  }
}
