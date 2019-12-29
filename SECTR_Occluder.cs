// Decompiled with JetBrains decompiler
// Type: SECTR_Occluder
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (SECTR_Member))]
[AddComponentMenu("SECTR/Vis/SECTR Occluder")]
public class SECTR_Occluder : SECTR_Hull
{
  private static List<SECTR_Occluder> allOccluders = new List<SECTR_Occluder>(32);
  private static Dictionary<SECTR_Sector, List<SECTR_Occluder>> occluderTable = new Dictionary<SECTR_Sector, List<SECTR_Occluder>>(32);
  private List<SECTR_Sector> currentSectors = new List<SECTR_Sector>(4);
  private SECTR_Member cachedMember;
  [SECTR_ToolTip("The axes that should orient towards the camera during culling (if any).")]
  public SECTR_Occluder.OrientationAxis AutoOrient;

  public static List<SECTR_Occluder> All
  {
    get
    {
      return SECTR_Occluder.allOccluders;
    }
  }

  public static List<SECTR_Occluder> GetOccludersInSector(SECTR_Sector sector)
  {
    List<SECTR_Occluder> sectrOccluderList = (List<SECTR_Occluder>) null;
    SECTR_Occluder.occluderTable.TryGetValue(sector, out sectrOccluderList);
    return sectrOccluderList;
  }

  public SECTR_Member Member
  {
    get
    {
      return this.cachedMember;
    }
  }

  public Vector3 MeshNormal
  {
    get
    {
      this.ComputeVerts();
      return this.meshNormal;
    }
  }

  public Matrix4x4 GetCullingMatrix(Vector3 cameraPos)
  {
    if (this.AutoOrient == SECTR_Occluder.OrientationAxis.None)
      return this.transform.localToWorldMatrix;
    this.ComputeVerts();
    Vector3 position = this.transform.position;
    Vector3 toDirection = cameraPos - position;
    switch (this.AutoOrient)
    {
      case SECTR_Occluder.OrientationAxis.XZ:
        toDirection.y = 0.0f;
        break;
      case SECTR_Occluder.OrientationAxis.XY:
        toDirection.z = 0.0f;
        break;
      case SECTR_Occluder.OrientationAxis.YZ:
        toDirection.x = 0.0f;
        break;
    }
    return Matrix4x4.TRS(position, Quaternion.FromToRotation(this.meshNormal, toDirection), this.transform.lossyScale);
  }

  private void OnEnable()
  {
    this.cachedMember = this.GetComponent<SECTR_Member>();
    this.cachedMember.Changed += new SECTR_Member.MembershipChanged(this._MembershipChanged);
    SECTR_Occluder.allOccluders.Add(this);
  }

  private void OnDisable()
  {
    SECTR_Occluder.allOccluders.Remove(this);
    this.cachedMember.Changed -= new SECTR_Member.MembershipChanged(this._MembershipChanged);
    this.cachedMember = (SECTR_Member) null;
  }

  private void _MembershipChanged(List<SECTR_Sector> left, List<SECTR_Sector> joined)
  {
    if (joined != null)
    {
      int count = joined.Count;
      for (int index = 0; index < count; ++index)
      {
        SECTR_Sector key = joined[index];
        if ((bool) (Object) key)
        {
          List<SECTR_Occluder> sectrOccluderList;
          if (!SECTR_Occluder.occluderTable.TryGetValue(key, out sectrOccluderList))
          {
            sectrOccluderList = new List<SECTR_Occluder>(4);
            SECTR_Occluder.occluderTable[key] = sectrOccluderList;
          }
          sectrOccluderList.Add(this);
          this.currentSectors.Add(key);
        }
      }
    }
    if (left == null)
      return;
    int count1 = left.Count;
    for (int index = 0; index < count1; ++index)
    {
      SECTR_Sector key = left[index];
      if ((bool) (Object) key && this.currentSectors.Contains(key))
      {
        List<SECTR_Occluder> sectrOccluderList;
        if (SECTR_Occluder.occluderTable.TryGetValue(key, out sectrOccluderList))
          sectrOccluderList.Remove(this);
        this.currentSectors.Remove(key);
      }
    }
  }

  public enum OrientationAxis
  {
    None,
    XYZ,
    XZ,
    XY,
    YZ,
  }
}
