// Decompiled with JetBrains decompiler
// Type: SECTR_LOD
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof (SECTR_Member))]
[AddComponentMenu("SECTR/Vis/SECTR LOD")]
public class SECTR_LOD : MonoBehaviour
{
  private static List<SECTR_LOD> allLODs = new List<SECTR_LOD>(128);
  private List<GameObject> toHide = new List<GameObject>(32);
  private List<SECTR_LOD.LODEntry> toShow = new List<SECTR_LOD.LODEntry>(32);
  public List<SECTR_LOD.LODSet> LODs = new List<SECTR_LOD.LODSet>();
  [SerializeField]
  [HideInInspector]
  private Vector3 boundsOffset;
  [SerializeField]
  [HideInInspector]
  private float boundsRadius;
  [SerializeField]
  [HideInInspector]
  private bool boundsUpdated;
  private int activeLOD;
  private bool siblingsDisabled;
  private SECTR_Member cachedMember;
  [SECTR_ToolTip("Determines which sibling components are disabled when the LOD is culled.", null, typeof (SECTR_LOD.SiblinglFlags))]
  public SECTR_LOD.SiblinglFlags CullSiblings;

  public static List<SECTR_LOD> All
  {
    get
    {
      return SECTR_LOD.allLODs;
    }
  }

  public void SelectLOD(Camera renderCamera)
  {
    if (!(bool) (UnityEngine.Object) renderCamera)
      return;
    if (!this.boundsUpdated)
      this._CalculateBounds();
    Vector3 b = this.transform.localToWorldMatrix.MultiplyPoint3x4(this.boundsOffset);
    float num1 = Vector3.Distance(renderCamera.transform.position, b);
    float num2 = (float) ((double) this.boundsRadius / ((double) Mathf.Tan((float) ((double) renderCamera.fieldOfView * 0.5 * (Math.PI / 180.0))) * (double) num1) * 2.0);
    int lodIndex = -1;
    int count = this.LODs.Count;
    for (int index = 0; index < count; ++index)
    {
      float threshold = this.LODs[index].Threshold;
      if (index == this.activeLOD)
        threshold -= threshold * 0.1f;
      if ((double) num2 >= (double) threshold)
      {
        lodIndex = index;
        break;
      }
    }
    if (lodIndex == this.activeLOD)
      return;
    this._ActivateLOD(lodIndex);
  }

  private void OnEnable()
  {
    SECTR_LOD.allLODs.Add(this);
    this.cachedMember = this.GetComponent<SECTR_Member>();
    SECTR_CullingCamera sectrCullingCamera = SECTR_CullingCamera.All.Count > 0 ? SECTR_CullingCamera.All[0] : (SECTR_CullingCamera) null;
    if ((bool) (UnityEngine.Object) sectrCullingCamera)
      this.SelectLOD(sectrCullingCamera.GetComponent<Camera>());
    else
      this._ActivateLOD(0);
  }

  private void OnDisable()
  {
    SECTR_LOD.allLODs.Remove(this);
    this.cachedMember = (SECTR_Member) null;
  }

  private void OnDrawGizmosSelected()
  {
    Gizmos.matrix = Matrix4x4.identity;
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(this.transform.localToWorldMatrix.MultiplyPoint(this.boundsOffset), this.boundsRadius);
  }

  private void _ActivateLOD(int lodIndex)
  {
    this.toHide.Clear();
    this.toShow.Clear();
    if (this.activeLOD >= 0 && this.activeLOD < this.LODs.Count)
    {
      SECTR_LOD.LODSet loD = this.LODs[this.activeLOD];
      int count = loD.LODEntries.Count;
      for (int index = 0; index < count; ++index)
      {
        SECTR_LOD.LODEntry lodEntry = loD.LODEntries[index];
        if ((bool) (UnityEngine.Object) lodEntry.gameObject)
          this.toHide.Add(lodEntry.gameObject);
      }
    }
    if (lodIndex >= 0 && lodIndex < this.LODs.Count)
    {
      SECTR_LOD.LODSet loD = this.LODs[lodIndex];
      int count = loD.LODEntries.Count;
      for (int index = 0; index < count; ++index)
      {
        SECTR_LOD.LODEntry lodEntry = loD.LODEntries[index];
        if ((bool) (UnityEngine.Object) lodEntry.gameObject)
        {
          this.toHide.Remove(lodEntry.gameObject);
          this.toShow.Add(lodEntry);
        }
      }
    }
    int count1 = this.toHide.Count;
    for (int index = 0; index < count1; ++index)
      this.toHide[index].SetActive(false);
    int count2 = this.toShow.Count;
    for (int index = 0; index < count2; ++index)
    {
      SECTR_LOD.LODEntry lodEntry = this.toShow[index];
      lodEntry.gameObject.SetActive(true);
      if ((bool) (UnityEngine.Object) lodEntry.lightmapSource)
      {
        Renderer component = lodEntry.gameObject.GetComponent<Renderer>();
        if ((bool) (UnityEngine.Object) component)
        {
          component.lightmapIndex = lodEntry.lightmapSource.lightmapIndex;
          component.lightmapScaleOffset = lodEntry.lightmapSource.lightmapScaleOffset;
        }
      }
    }
    this.activeLOD = lodIndex;
    if (this.CullSiblings != (SECTR_LOD.SiblinglFlags) 0 && (this.activeLOD == -1 && !this.siblingsDisabled || this.activeLOD != -1 && this.siblingsDisabled))
    {
      this.siblingsDisabled = this.activeLOD == -1;
      if ((this.CullSiblings & SECTR_LOD.SiblinglFlags.Behaviors) != (SECTR_LOD.SiblinglFlags) 0)
      {
        MonoBehaviour[] components = this.gameObject.GetComponents<MonoBehaviour>();
        int length = components.Length;
        for (int index = 0; index < length; ++index)
        {
          MonoBehaviour monoBehaviour = components[index];
          if ((UnityEngine.Object) monoBehaviour != (UnityEngine.Object) this && (UnityEngine.Object) monoBehaviour != (UnityEngine.Object) this.cachedMember)
            monoBehaviour.enabled = !this.siblingsDisabled;
        }
      }
      if ((this.CullSiblings & SECTR_LOD.SiblinglFlags.Renderers) != (SECTR_LOD.SiblinglFlags) 0)
      {
        Renderer[] components = this.gameObject.GetComponents<Renderer>();
        int length = components.Length;
        for (int index = 0; index < length; ++index)
          components[index].enabled = !this.siblingsDisabled;
      }
      if ((this.CullSiblings & SECTR_LOD.SiblinglFlags.Lights) != (SECTR_LOD.SiblinglFlags) 0)
      {
        Light[] components = this.gameObject.GetComponents<Light>();
        int length = components.Length;
        for (int index = 0; index < length; ++index)
          components[index].enabled = !this.siblingsDisabled;
      }
      if ((this.CullSiblings & SECTR_LOD.SiblinglFlags.Colliders) != (SECTR_LOD.SiblinglFlags) 0)
      {
        Collider[] components = this.gameObject.GetComponents<Collider>();
        int length = components.Length;
        for (int index = 0; index < length; ++index)
          components[index].enabled = !this.siblingsDisabled;
      }
      if ((this.CullSiblings & SECTR_LOD.SiblinglFlags.RigidBodies) != (SECTR_LOD.SiblinglFlags) 0)
      {
        Rigidbody[] components = this.gameObject.GetComponents<Rigidbody>();
        int length = components.Length;
        for (int index = 0; index < length; ++index)
        {
          if (this.siblingsDisabled)
            components[index].Sleep();
          else
            components[index].WakeUp();
        }
      }
    }
    this.cachedMember.ForceUpdate(true);
  }

  private void _CalculateBounds()
  {
    Bounds bounds = new Bounds();
    int count1 = this.LODs.Count;
    bool flag = false;
    for (int index1 = 0; index1 < count1; ++index1)
    {
      SECTR_LOD.LODSet loD = this.LODs[index1];
      int count2 = loD.LODEntries.Count;
      for (int index2 = 0; index2 < count2; ++index2)
      {
        GameObject gameObject = loD.LODEntries[index2].gameObject;
        Renderer renderer = (bool) (UnityEngine.Object) gameObject ? gameObject.GetComponent<Renderer>() : (Renderer) null;
        if ((bool) (UnityEngine.Object) renderer && renderer.bounds.extents != Vector3.zero)
        {
          if (!flag)
          {
            bounds = renderer.bounds;
            flag = true;
          }
          else
            bounds.Encapsulate(renderer.bounds);
        }
      }
    }
    this.boundsOffset = this.transform.worldToLocalMatrix.MultiplyPoint(bounds.center);
    this.boundsRadius = bounds.extents.magnitude;
    this.boundsUpdated = true;
  }

  [Serializable]
  public class LODEntry
  {
    public GameObject gameObject;
    public Renderer lightmapSource;
  }

  [Serializable]
  public class LODSet
  {
    [SerializeField]
    private List<SECTR_LOD.LODEntry> lodEntries = new List<SECTR_LOD.LODEntry>(16);
    [SerializeField]
    private float threshold;

    public List<SECTR_LOD.LODEntry> LODEntries
    {
      get
      {
        return this.lodEntries;
      }
    }

    public float Threshold
    {
      get
      {
        return this.threshold;
      }
      set
      {
        this.threshold = value;
      }
    }

    public SECTR_LOD.LODEntry Add(GameObject gameObject, Renderer lightmapSource)
    {
      if (this.GetEntry(gameObject) != null)
        return (SECTR_LOD.LODEntry) null;
      SECTR_LOD.LODEntry lodEntry = new SECTR_LOD.LODEntry();
      lodEntry.gameObject = gameObject;
      lodEntry.lightmapSource = lightmapSource;
      this.lodEntries.Add(lodEntry);
      return lodEntry;
    }

    public void Remove(GameObject gameObject)
    {
      int index = 0;
      while (index < this.lodEntries.Count)
      {
        if ((UnityEngine.Object) this.lodEntries[index].gameObject == (UnityEngine.Object) gameObject)
          this.lodEntries.RemoveAt(index);
        else
          ++index;
      }
    }

    public SECTR_LOD.LODEntry GetEntry(GameObject gameObject)
    {
      int count = this.lodEntries.Count;
      for (int index = 0; index < count; ++index)
      {
        SECTR_LOD.LODEntry lodEntry = this.lodEntries[index];
        if ((UnityEngine.Object) lodEntry.gameObject == (UnityEngine.Object) gameObject)
          return lodEntry;
      }
      return (SECTR_LOD.LODEntry) null;
    }
  }

  [System.Flags]
  public enum SiblinglFlags
  {
    Behaviors = 1,
    Renderers = 2,
    Lights = 4,
    Colliders = 8,
    RigidBodies = 16, // 0x00000010
  }
}
