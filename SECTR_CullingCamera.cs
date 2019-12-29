// Decompiled with JetBrains decompiler
// Type: SECTR_CullingCamera
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof (Camera))]
[AddComponentMenu("SECTR/Vis/SECTR Culling Camera")]
public class SECTR_CullingCamera : MonoBehaviour
{
  private static List<SECTR_CullingCamera> allCullingCameras = new List<SECTR_CullingCamera>(4);
  private Dictionary<int, SECTR_Member.Child> hiddenRenderers = new Dictionary<int, SECTR_Member.Child>(16);
  private Dictionary<int, SECTR_Member.Child> hiddenLights = new Dictionary<int, SECTR_Member.Child>(16);
  private Dictionary<int, SECTR_Member.Child> hiddenTerrains = new Dictionary<int, SECTR_Member.Child>(2);
  private List<SECTR_Sector> initialSectors = new List<SECTR_Sector>(4);
  private Stack<SECTR_CullingCamera.VisibilityNode> nodeStack = new Stack<SECTR_CullingCamera.VisibilityNode>(10);
  private List<SECTR_CullingCamera.ClipVertex> portalVertices = new List<SECTR_CullingCamera.ClipVertex>(16);
  private List<Plane> newFrustum = new List<Plane>(16);
  private List<Plane> cullingPlanes = new List<Plane>(16);
  private List<List<Plane>> occluderFrustums = new List<List<Plane>>(10);
  private Dictionary<SECTR_Occluder, SECTR_Occluder> activeOccluders = new Dictionary<SECTR_Occluder, SECTR_Occluder>(10);
  private List<SECTR_CullingCamera.ClipVertex> occluderVerts = new List<SECTR_CullingCamera.ClipVertex>(10);
  private Dictionary<SECTR_Member.Child, int> shadowLights = new Dictionary<SECTR_Member.Child, int>(10);
  private List<SECTR_Sector> shadowSectors = new List<SECTR_Sector>(4);
  private Dictionary<SECTR_Sector, List<SECTR_Member.Child>> shadowSectorTable = new Dictionary<SECTR_Sector, List<SECTR_Member.Child>>(4);
  private Dictionary<int, SECTR_Member.Child> visibleRenderers = new Dictionary<int, SECTR_Member.Child>(1024);
  private Dictionary<int, SECTR_Member.Child> visibleLights = new Dictionary<int, SECTR_Member.Child>(256);
  private Dictionary<int, SECTR_Member.Child> visibleTerrains = new Dictionary<int, SECTR_Member.Child>(32);
  private Plane[] initialFrustumPlanes = new Plane[6];
  private Stack<List<Plane>> frustumPool = new Stack<List<Plane>>(32);
  private Stack<List<SECTR_Member.Child>> shadowLightPool = new Stack<List<SECTR_Member.Child>>(32);
  private Stack<Dictionary<int, SECTR_Member.Child>> threadVisibleListPool = new Stack<Dictionary<int, SECTR_Member.Child>>(4);
  private Stack<Dictionary<SECTR_Member.Child, int>> threadShadowLightPool = new Stack<Dictionary<SECTR_Member.Child, int>>(32);
  private Stack<List<Plane>> threadFrustumPool = new Stack<List<Plane>>(32);
  private Stack<List<List<Plane>>> threadOccluderPool = new Stack<List<List<Plane>>>(32);
  private List<Thread> workerThreads = new List<Thread>();
  private Queue<SECTR_CullingCamera.ThreadCullData> cullingWorkQueue = new Queue<SECTR_CullingCamera.ThreadCullData>(32);
  [SECTR_ToolTip("Allows multiple culling cameras to be active at once, but at the cost of some performance.")]
  public bool MultiCameraCulling = true;
  [SECTR_ToolTip("Distance to draw clipped frustums.", 0.0f, 100f)]
  public float GizmoDistance = 10f;
  [SECTR_ToolTip("Set to false to disable shadow culling post pass.", true)]
  public bool CullShadows = true;
  private Camera myCamera;
  private SECTR_Member cullingMember;
  private int renderersCulled;
  private int lightsCulled;
  private int terrainsCulled;
  private bool didCull;
  private bool runOnce;
  private int remainingThreadWork;
  [SECTR_ToolTip("Forces culling into a mode designed for 2D and iso games where the camera is always outside the scene.")]
  public bool SimpleCulling;
  [SECTR_ToolTip("Material to use to render the debug frustum mesh.")]
  public Material GizmoMaterial;
  [SECTR_ToolTip("Makes the Editor camera display the Game view's culling while playing in editor.")]
  public bool CullInEditor;
  [SECTR_ToolTip("Use another camera for culling properties.", true)]
  public Camera cullingProxy;
  [SECTR_ToolTip("Number of worker threads for culling. Do not set this too high or you may see hitching.", 0.0f, -1f)]
  public int NumWorkerThreads;

  public static List<SECTR_CullingCamera> All
  {
    get
    {
      return SECTR_CullingCamera.allCullingCameras;
    }
  }

  public int RenderersCulled
  {
    get
    {
      return this.renderersCulled;
    }
  }

  public int LightsCulled
  {
    get
    {
      return this.lightsCulled;
    }
  }

  public int TerrainsCulled
  {
    get
    {
      return this.terrainsCulled;
    }
  }

  public void ResetStats()
  {
    this.renderersCulled = 0;
    this.lightsCulled = 0;
    this.terrainsCulled = 0;
    this.runOnce = false;
  }

  private void OnEnable()
  {
    this.myCamera = this.GetComponent<Camera>();
    this.cullingMember = this.GetComponent<SECTR_Member>();
    SECTR_CullingCamera.allCullingCameras.Add(this);
    this.runOnce = false;
    int num = Mathf.Min(this.NumWorkerThreads, SystemInfo.processorCount);
    for (int index = 0; index < num; ++index)
    {
      Thread thread = new Thread(new ThreadStart(this._CullingWorker));
      thread.IsBackground = true;
      thread.Priority = System.Threading.ThreadPriority.Highest;
      thread.Start();
      this.workerThreads.Add(thread);
    }
  }

  private void OnDisable()
  {
    if (!this.MultiCameraCulling)
      this._UndoCulling();
    SECTR_CullingCamera.allCullingCameras.Remove(this);
    int count = this.workerThreads.Count;
    for (int index = 0; index < count; ++index)
      this.workerThreads[index].Abort();
  }

  private void OnDestroy()
  {
  }

  private void OnPreCull()
  {
    Camera camera = (UnityEngine.Object) this.cullingProxy != (UnityEngine.Object) null ? this.cullingProxy : this.myCamera;
    Vector3 position = camera.transform.position;
    float num1 = Mathf.Cos((float) ((double) Mathf.Max(camera.fieldOfView, camera.fieldOfView * camera.aspect) * 0.5 * (Math.PI / 180.0)));
    float num2 = (float) ((double) camera.nearClipPlane / (double) num1 * 1.00100004673004);
    if ((bool) (UnityEngine.Object) this.cullingProxy)
    {
      SECTR_CullingCamera component = this.cullingProxy.GetComponent<SECTR_CullingCamera>();
      if ((bool) (UnityEngine.Object) component)
      {
        this.SimpleCulling = component.SimpleCulling;
        this.CullShadows = component.CullShadows;
        if (this.MultiCameraCulling != component.MultiCameraCulling)
          this.runOnce = false;
        this.MultiCameraCulling = component.MultiCameraCulling;
      }
    }
    int count1 = SECTR_LOD.All.Count;
    for (int index = 0; index < count1; ++index)
      SECTR_LOD.All[index].SelectLOD(camera);
    int num3 = 0;
    if (!this.SimpleCulling)
    {
      if ((bool) (UnityEngine.Object) this.cullingMember && this.cullingMember.enabled)
      {
        this.initialSectors.Clear();
        this.initialSectors.AddRange((IEnumerable<SECTR_Sector>) this.cullingMember.Sectors);
      }
      else
        SECTR_Sector.GetContaining(ref this.initialSectors, new Bounds(position, new Vector3(num2, num2, num2)));
      num3 = this.initialSectors.Count;
      for (int index = 0; index < num3; ++index)
      {
        if (this.initialSectors[index].IsConnectedTerrain)
        {
          this.SimpleCulling = true;
          break;
        }
      }
    }
    if (this.SimpleCulling)
    {
      this.initialSectors.Clear();
      this.initialSectors.AddRange((IEnumerable<SECTR_Sector>) SECTR_Sector.All);
      num3 = this.initialSectors.Count;
    }
    if (!this.enabled || !camera.enabled || num3 <= 0)
      return;
    int count2 = this.workerThreads.Count;
    if (!this.MultiCameraCulling)
    {
      if (!this.runOnce)
      {
        this._HideAllMembers();
        this.runOnce = true;
      }
      else
        this._ApplyCulling(false);
    }
    else
      this._HideAllMembers();
    float shadowDistance = QualitySettings.shadowDistance;
    int count3 = SECTR_Member.All.Count;
    for (int index1 = 0; index1 < count3; ++index1)
    {
      SECTR_Member sectrMember = SECTR_Member.All[index1];
      if (sectrMember.ShadowLight)
      {
        int count4 = sectrMember.ShadowLights.Count;
        for (int index2 = 0; index2 < count4; ++index2)
        {
          SECTR_Member.Child shadowLight = sectrMember.ShadowLights[index2];
          if ((bool) (UnityEngine.Object) shadowLight.light)
          {
            shadowLight.shadowLightPosition = shadowLight.light.transform.position;
            shadowLight.shadowLightRange = shadowLight.light.range;
          }
          sectrMember.ShadowLights[index2] = shadowLight;
        }
      }
    }
    this.nodeStack.Clear();
    this.shadowLights.Clear();
    this.visibleRenderers.Clear();
    this.visibleLights.Clear();
    this.visibleTerrains.Clear();
    GeometryUtility.CalculateFrustumPlanes(camera, this.initialFrustumPlanes);
    for (int index = 0; index < num3; ++index)
      this.nodeStack.Push(new SECTR_CullingCamera.VisibilityNode(this, this.initialSectors[index], (SECTR_Portal) null, this.initialFrustumPlanes, true));
    while (this.nodeStack.Count > 0)
    {
      SECTR_CullingCamera.VisibilityNode visibilityNode = this.nodeStack.Pop();
      if (visibilityNode.frustumPlanes != null)
      {
        this.cullingPlanes.Clear();
        this.cullingPlanes.AddRange((IEnumerable<Plane>) visibilityNode.frustumPlanes);
        int count4 = this.cullingPlanes.Count;
        for (int index = 0; index < count4; ++index)
        {
          Plane cullingPlane1 = this.cullingPlanes[index];
          Plane cullingPlane2 = this.cullingPlanes[(index + 1) % this.cullingPlanes.Count];
          float num4 = Vector3.Dot(cullingPlane1.normal, cullingPlane2.normal);
          if ((double) num4 < -0.899999976158142 && (double) num4 > -0.990000009536743)
          {
            Vector3 lhs = cullingPlane1.normal + cullingPlane2.normal;
            Vector3 rhs = Vector3.Cross(cullingPlane1.normal, cullingPlane2.normal);
            Vector3 inNormal = lhs - Vector3.Dot(lhs, rhs) * rhs;
            inNormal.Normalize();
            Matrix4x4 matrix4x4 = new Matrix4x4();
            matrix4x4.SetRow(0, new Vector4(cullingPlane1.normal.x, cullingPlane1.normal.y, cullingPlane1.normal.z, 0.0f));
            matrix4x4.SetRow(1, new Vector4(cullingPlane2.normal.x, cullingPlane2.normal.y, cullingPlane2.normal.z, 0.0f));
            matrix4x4.SetRow(2, new Vector4(rhs.x, rhs.y, rhs.z, 0.0f));
            matrix4x4.SetRow(3, new Vector4(0.0f, 0.0f, 0.0f, 1f));
            Vector3 inPoint = matrix4x4.inverse.MultiplyPoint3x4(new Vector3(-cullingPlane1.distance, -cullingPlane2.distance, 0.0f));
            this.cullingPlanes.Insert(++index, new Plane(inNormal, inPoint));
          }
        }
        int count5 = this.cullingPlanes.Count;
        int num5 = 0;
        for (int index = 0; index < count5; ++index)
          num5 |= 1 << index;
        SECTR_Sector sector1 = visibilityNode.sector;
        Vector3 vector3_1;
        if (SECTR_Occluder.All.Count > 0)
        {
          List<SECTR_Occluder> occludersInSector = SECTR_Occluder.GetOccludersInSector(sector1);
          if (occludersInSector != null)
          {
            int count6 = occludersInSector.Count;
            for (int index1 = 0; index1 < count6; ++index1)
            {
              SECTR_Occluder key = occludersInSector[index1];
              if ((bool) (UnityEngine.Object) key.HullMesh && !this.activeOccluders.ContainsKey(key))
              {
                Matrix4x4 cullingMatrix = key.GetCullingMatrix(position);
                Vector3[] vertsCw = key.VertsCW;
                vector3_1 = cullingMatrix.MultiplyVector(-key.MeshNormal);
                Vector3 normalized = vector3_1.normalized;
                if (vertsCw != null && !SECTR_Geometry.IsPointInFrontOfPlane(position, key.Center, normalized))
                {
                  int length = vertsCw.Length;
                  this.occluderVerts.Clear();
                  Bounds bounds = new Bounds(key.transform.position, Vector3.zero);
                  for (int index2 = 0; index2 < length; ++index2)
                  {
                    Vector3 point = cullingMatrix.MultiplyPoint3x4(vertsCw[index2]);
                    bounds.Encapsulate(point);
                    this.occluderVerts.Add(new SECTR_CullingCamera.ClipVertex(new Vector4(point.x, point.y, point.z, 1f), 0.0f));
                  }
                  if (SECTR_Geometry.FrustumIntersectsBounds(key.BoundingBox, this.cullingPlanes, num5, out int _))
                  {
                    List<Plane> newFrustum;
                    if (this.frustumPool.Count > 0)
                    {
                      newFrustum = this.frustumPool.Pop();
                      newFrustum.Clear();
                    }
                    else
                      newFrustum = new List<Plane>(length + 1);
                    this._BuildFrustumFromHull(camera, true, this.occluderVerts, ref newFrustum);
                    newFrustum.Add(new Plane(normalized, key.Center));
                    this.occluderFrustums.Add(newFrustum);
                    this.activeOccluders[key] = key;
                  }
                }
              }
            }
          }
        }
        if (count2 > 0)
        {
          lock (this.cullingWorkQueue)
          {
            this.cullingWorkQueue.Enqueue(new SECTR_CullingCamera.ThreadCullData(sector1, this, position, this.cullingPlanes, this.occluderFrustums, num5, shadowDistance, this.SimpleCulling));
            Monitor.Pulse((object) this.cullingWorkQueue);
          }
          Interlocked.Increment(ref this.remainingThreadWork);
        }
        else
          SECTR_CullingCamera._FrustumCullSector(sector1, position, this.cullingPlanes, this.occluderFrustums, num5, shadowDistance, this.SimpleCulling, ref this.visibleRenderers, ref this.visibleLights, ref this.visibleTerrains, ref this.shadowLights);
        int num6 = this.SimpleCulling ? 0 : visibilityNode.sector.Portals.Count;
        for (int index1 = 0; index1 < num6; ++index1)
        {
          SECTR_Portal portal = visibilityNode.sector.Portals[index1];
          bool flag1 = (uint) (portal.Flags & SECTR_Portal.PortalFlags.PassThrough) > 0U;
          if ((bool) (UnityEngine.Object) portal.HullMesh | flag1 && (portal.Flags & SECTR_Portal.PortalFlags.Closed) == (SECTR_Portal.PortalFlags) 0)
          {
            bool forwardTraversal = (UnityEngine.Object) visibilityNode.sector == (UnityEngine.Object) portal.FrontSector;
            SECTR_Sector sector2 = forwardTraversal ? portal.BackSector : portal.FrontSector;
            bool flag2 = !(bool) (UnityEngine.Object) sector2;
            if (!flag2)
              flag2 = SECTR_Geometry.IsPointInFrontOfPlane(position, portal.Center, portal.Normal) != forwardTraversal;
            if (!flag2 && (bool) (UnityEngine.Object) visibilityNode.portal)
            {
              vector3_1 = portal.Center - visibilityNode.portal.Center;
              flag2 = (double) Vector3.Dot(vector3_1.normalized, visibilityNode.forwardTraversal ? visibilityNode.portal.ReverseNormal : visibilityNode.portal.Normal) < 0.0;
            }
            if (!flag2 && !flag1)
            {
              int count6 = this.occluderFrustums.Count;
              for (int index2 = 0; index2 < count6; ++index2)
              {
                if (SECTR_Geometry.FrustumContainsBounds(portal.BoundingBox, this.occluderFrustums[index2]))
                {
                  flag2 = true;
                  break;
                }
              }
            }
            if (!flag2)
            {
              if (!flag1)
              {
                this.portalVertices.Clear();
                Matrix4x4 localToWorldMatrix = portal.transform.localToWorldMatrix;
                Vector3[] vertsCw = portal.VertsCW;
                if (vertsCw != null)
                {
                  int length = vertsCw.Length;
                  for (int index2 = 0; index2 < length; ++index2)
                  {
                    Vector3 vector3_2 = localToWorldMatrix.MultiplyPoint3x4(vertsCw[index2]);
                    this.portalVertices.Add(new SECTR_CullingCamera.ClipVertex(new Vector4(vector3_2.x, vector3_2.y, vector3_2.z, 1f), 0.0f));
                  }
                }
              }
              this.newFrustum.Clear();
              if (!flag1 && !portal.IsPointInHull(position, num2))
              {
                int count6 = visibilityNode.frustumPlanes.Count;
                for (int index2 = 0; index2 < count6; ++index2)
                {
                  Plane frustumPlane = visibilityNode.frustumPlanes[index2];
                  Vector4 a = new Vector4(frustumPlane.normal.x, frustumPlane.normal.y, frustumPlane.normal.z, frustumPlane.distance);
                  bool flag3 = true;
                  bool flag4 = true;
                  for (int index3 = 0; index3 < this.portalVertices.Count; ++index3)
                  {
                    Vector4 vertex = this.portalVertices[index3].vertex;
                    float side = Vector4.Dot(a, vertex);
                    this.portalVertices[index3] = new SECTR_CullingCamera.ClipVertex(vertex, side);
                    flag3 = flag3 && (double) side > 0.0;
                    flag4 = flag4 && (double) side <= -1.0 / 1000.0;
                  }
                  if (flag4)
                  {
                    this.portalVertices.Clear();
                    break;
                  }
                  if (!flag3)
                  {
                    int count7 = this.portalVertices.Count;
                    for (int index3 = 0; index3 < count7; ++index3)
                    {
                      int index4 = (index3 + 1) % this.portalVertices.Count;
                      float side1 = this.portalVertices[index3].side;
                      float side2 = this.portalVertices[index4].side;
                      if ((double) side1 > 0.0 && (double) side2 <= -1.0 / 1000.0 || (double) side2 > 0.0 && (double) side1 <= -1.0 / 1000.0)
                      {
                        Vector4 vertex1 = this.portalVertices[index3].vertex;
                        Vector4 vertex2 = this.portalVertices[index4].vertex;
                        float num4 = side1 / Vector4.Dot(a, vertex1 - vertex2);
                        Vector4 vertex3 = vertex1 + num4 * (vertex2 - vertex1);
                        vertex3.w = 1f;
                        this.portalVertices.Insert(index3 + 1, new SECTR_CullingCamera.ClipVertex(vertex3, 0.0f));
                        ++count7;
                      }
                    }
                    int index5 = 0;
                    while (index5 < count7)
                    {
                      if ((double) this.portalVertices[index5].side < -1.0 / 1000.0)
                      {
                        this.portalVertices.RemoveAt(index5);
                        --count7;
                      }
                      else
                        ++index5;
                    }
                  }
                }
                this._BuildFrustumFromHull(camera, forwardTraversal, this.portalVertices, ref this.newFrustum);
              }
              else
                this.newFrustum.AddRange((IEnumerable<Plane>) this.initialFrustumPlanes);
              if (this.newFrustum.Count > 2)
                this.nodeStack.Push(new SECTR_CullingCamera.VisibilityNode(this, sector2, portal, this.newFrustum, forwardTraversal));
            }
          }
        }
      }
      if (visibilityNode.frustumPlanes != null)
      {
        visibilityNode.frustumPlanes.Clear();
        this.frustumPool.Push(visibilityNode.frustumPlanes);
      }
    }
    if (count2 > 0)
    {
      while (this.remainingThreadWork > 0)
      {
        while (this.cullingWorkQueue.Count > 0)
        {
          SECTR_CullingCamera.ThreadCullData cullData = new SECTR_CullingCamera.ThreadCullData();
          lock (this.cullingWorkQueue)
          {
            if (this.cullingWorkQueue.Count > 0)
              cullData = this.cullingWorkQueue.Dequeue();
          }
          if (cullData.cullingMode == SECTR_CullingCamera.ThreadCullData.CullingModes.Graph)
          {
            this._FrustumCullSectorThread(cullData);
            Interlocked.Decrement(ref this.remainingThreadWork);
          }
        }
      }
      this.remainingThreadWork = 0;
    }
    if (this.shadowLights.Count > 0 && this.CullShadows)
    {
      this.shadowSectorTable.Clear();
      Dictionary<SECTR_Member.Child, int>.Enumerator enumerator1 = this.shadowLights.GetEnumerator();
      while (enumerator1.MoveNext())
      {
        SECTR_Member.Child key1 = enumerator1.Current.Key;
        List<SECTR_Sector> sectrSectorList;
        if ((bool) (UnityEngine.Object) key1.member && key1.member.IsSector)
        {
          this.shadowSectors.Clear();
          this.shadowSectors.Add((SECTR_Sector) key1.member);
          sectrSectorList = this.shadowSectors;
        }
        else if ((bool) (UnityEngine.Object) key1.member && key1.member.Sectors.Count > 0)
        {
          sectrSectorList = key1.member.Sectors;
        }
        else
        {
          SECTR_Sector.GetContaining(ref this.shadowSectors, key1.lightBounds);
          sectrSectorList = this.shadowSectors;
        }
        int count4 = sectrSectorList.Count;
        for (int index = 0; index < count4; ++index)
        {
          SECTR_Sector key2 = sectrSectorList[index];
          List<SECTR_Member.Child> childList;
          if (!this.shadowSectorTable.TryGetValue(key2, out childList))
          {
            childList = this.shadowLightPool.Count > 0 ? this.shadowLightPool.Pop() : new List<SECTR_Member.Child>(16);
            this.shadowSectorTable[key2] = childList;
          }
          childList.Add(key1);
        }
      }
      Dictionary<SECTR_Sector, List<SECTR_Member.Child>>.Enumerator enumerator2 = this.shadowSectorTable.GetEnumerator();
      while (enumerator2.MoveNext())
      {
        SECTR_Sector key = enumerator2.Current.Key;
        List<SECTR_Member.Child> sectorShadowLights = enumerator2.Current.Value;
        if (count2 > 0)
        {
          lock (this.cullingWorkQueue)
          {
            this.cullingWorkQueue.Enqueue(new SECTR_CullingCamera.ThreadCullData(key, position, sectorShadowLights));
            Monitor.Pulse((object) this.cullingWorkQueue);
          }
          Interlocked.Increment(ref this.remainingThreadWork);
        }
        else
          SECTR_CullingCamera._ShadowCullSector(key, sectorShadowLights, ref this.visibleRenderers, ref this.visibleTerrains);
      }
      if (count2 > 0)
      {
        while (this.remainingThreadWork > 0)
        {
          while (this.cullingWorkQueue.Count > 0)
          {
            SECTR_CullingCamera.ThreadCullData cullData = new SECTR_CullingCamera.ThreadCullData();
            lock (this.cullingWorkQueue)
            {
              if (this.cullingWorkQueue.Count > 0)
                cullData = this.cullingWorkQueue.Dequeue();
            }
            if (cullData.cullingMode == SECTR_CullingCamera.ThreadCullData.CullingModes.Shadow)
            {
              this._ShadowCullSectorThread(cullData);
              Interlocked.Decrement(ref this.remainingThreadWork);
            }
          }
        }
        this.remainingThreadWork = 0;
      }
      enumerator2 = this.shadowSectorTable.GetEnumerator();
      while (enumerator2.MoveNext())
      {
        List<SECTR_Member.Child> childList = enumerator2.Current.Value;
        childList.Clear();
        this.shadowLightPool.Push(childList);
      }
    }
    this._ApplyCulling(true);
    int count8 = this.occluderFrustums.Count;
    for (int index = 0; index < count8; ++index)
    {
      this.occluderFrustums[index].Clear();
      this.frustumPool.Push(this.occluderFrustums[index]);
    }
    this.occluderFrustums.Clear();
    this.activeOccluders.Clear();
  }

  private void OnPostRender()
  {
    if (!this.MultiCameraCulling)
      return;
    this._UndoCulling();
  }

  private void _CullingWorker()
  {
    while (true)
    {
      SECTR_CullingCamera.ThreadCullData cullData;
      do
      {
        cullData = new SECTR_CullingCamera.ThreadCullData();
        lock (this.cullingWorkQueue)
        {
          while (this.cullingWorkQueue.Count == 0)
            Monitor.Wait((object) this.cullingWorkQueue);
          cullData = this.cullingWorkQueue.Dequeue();
        }
        switch (cullData.cullingMode)
        {
          case SECTR_CullingCamera.ThreadCullData.CullingModes.Graph:
            this._FrustumCullSectorThread(cullData);
            break;
          case SECTR_CullingCamera.ThreadCullData.CullingModes.Shadow:
            this._ShadowCullSectorThread(cullData);
            break;
        }
      }
      while (cullData.cullingMode != SECTR_CullingCamera.ThreadCullData.CullingModes.Graph && cullData.cullingMode != SECTR_CullingCamera.ThreadCullData.CullingModes.Shadow);
      Interlocked.Decrement(ref this.remainingThreadWork);
    }
  }

  private void _FrustumCullSectorThread(SECTR_CullingCamera.ThreadCullData cullData)
  {
    Dictionary<int, SECTR_Member.Child> visibleRenderers1 = (Dictionary<int, SECTR_Member.Child>) null;
    Dictionary<int, SECTR_Member.Child> visibleLights1 = (Dictionary<int, SECTR_Member.Child>) null;
    Dictionary<int, SECTR_Member.Child> visibleTerrains1 = (Dictionary<int, SECTR_Member.Child>) null;
    Dictionary<SECTR_Member.Child, int> shadowLights = (Dictionary<SECTR_Member.Child, int>) null;
    lock (this.threadVisibleListPool)
    {
      visibleRenderers1 = this.threadVisibleListPool.Count > 0 ? this.threadVisibleListPool.Pop() : new Dictionary<int, SECTR_Member.Child>(32);
      visibleLights1 = this.threadVisibleListPool.Count > 0 ? this.threadVisibleListPool.Pop() : new Dictionary<int, SECTR_Member.Child>(32);
      visibleTerrains1 = this.threadVisibleListPool.Count > 0 ? this.threadVisibleListPool.Pop() : new Dictionary<int, SECTR_Member.Child>(32);
    }
    lock (this.threadShadowLightPool)
      shadowLights = this.threadShadowLightPool.Count > 0 ? this.threadShadowLightPool.Pop() : new Dictionary<SECTR_Member.Child, int>(32);
    SECTR_CullingCamera._FrustumCullSector(cullData.sector, cullData.cameraPos, cullData.cullingPlanes, cullData.occluderFrustums, cullData.baseMask, cullData.shadowDistance, cullData.cullingSimpleCulling, ref visibleRenderers1, ref visibleLights1, ref visibleTerrains1, ref shadowLights);
    lock (this.visibleRenderers)
    {
      Dictionary<int, SECTR_Member.Child>.Enumerator enumerator = visibleRenderers1.GetEnumerator();
      while (enumerator.MoveNext())
      {
        Dictionary<int, SECTR_Member.Child> visibleRenderers2 = this.visibleRenderers;
        KeyValuePair<int, SECTR_Member.Child> current = enumerator.Current;
        int key = current.Key;
        current = enumerator.Current;
        SECTR_Member.Child child = current.Value;
        visibleRenderers2[key] = child;
      }
    }
    lock (this.visibleLights)
    {
      Dictionary<int, SECTR_Member.Child>.Enumerator enumerator = visibleLights1.GetEnumerator();
      while (enumerator.MoveNext())
      {
        Dictionary<int, SECTR_Member.Child> visibleLights2 = this.visibleLights;
        KeyValuePair<int, SECTR_Member.Child> current = enumerator.Current;
        int key = current.Key;
        current = enumerator.Current;
        SECTR_Member.Child child = current.Value;
        visibleLights2[key] = child;
      }
    }
    lock (this.visibleTerrains)
    {
      Dictionary<int, SECTR_Member.Child>.Enumerator enumerator = visibleTerrains1.GetEnumerator();
      while (enumerator.MoveNext())
      {
        Dictionary<int, SECTR_Member.Child> visibleTerrains2 = this.visibleTerrains;
        KeyValuePair<int, SECTR_Member.Child> current = enumerator.Current;
        int key = current.Key;
        current = enumerator.Current;
        SECTR_Member.Child child = current.Value;
        visibleTerrains2[key] = child;
      }
    }
    lock (this.shadowLights)
    {
      Dictionary<SECTR_Member.Child, int>.Enumerator enumerator = shadowLights.GetEnumerator();
      while (enumerator.MoveNext())
        this.shadowLights[enumerator.Current.Key] = 0;
    }
    lock (this.threadVisibleListPool)
    {
      visibleRenderers1.Clear();
      this.threadVisibleListPool.Push(visibleRenderers1);
      visibleLights1.Clear();
      this.threadVisibleListPool.Push(visibleLights1);
      visibleTerrains1.Clear();
      this.threadVisibleListPool.Push(visibleTerrains1);
    }
    lock (this.threadShadowLightPool)
    {
      shadowLights.Clear();
      this.threadShadowLightPool.Push(shadowLights);
    }
    lock (this.threadFrustumPool)
    {
      cullData.cullingPlanes.Clear();
      this.threadFrustumPool.Push(cullData.cullingPlanes);
      int count = cullData.occluderFrustums.Count;
      for (int index = 0; index < count; ++index)
      {
        cullData.occluderFrustums[index].Clear();
        this.threadFrustumPool.Push(cullData.occluderFrustums[index]);
      }
    }
    lock (this.threadOccluderPool)
    {
      cullData.occluderFrustums.Clear();
      this.threadOccluderPool.Push(cullData.occluderFrustums);
    }
  }

  private void _ShadowCullSectorThread(SECTR_CullingCamera.ThreadCullData cullData)
  {
    Dictionary<int, SECTR_Member.Child> visibleRenderers = (Dictionary<int, SECTR_Member.Child>) null;
    Dictionary<int, SECTR_Member.Child> visibleTerrains = (Dictionary<int, SECTR_Member.Child>) null;
    lock (this.threadVisibleListPool)
    {
      visibleRenderers = this.threadVisibleListPool.Count > 0 ? this.threadVisibleListPool.Pop() : new Dictionary<int, SECTR_Member.Child>(32);
      visibleTerrains = this.threadVisibleListPool.Count > 0 ? this.threadVisibleListPool.Pop() : new Dictionary<int, SECTR_Member.Child>(32);
    }
    SECTR_CullingCamera._ShadowCullSector(cullData.sector, cullData.sectorShadowLights, ref visibleRenderers, ref visibleTerrains);
    lock (this.visibleRenderers)
    {
      Dictionary<int, SECTR_Member.Child>.Enumerator enumerator = visibleRenderers.GetEnumerator();
      while (enumerator.MoveNext())
        this.visibleRenderers[enumerator.Current.Key] = enumerator.Current.Value;
    }
    lock (this.visibleTerrains)
    {
      Dictionary<int, SECTR_Member.Child>.Enumerator enumerator = visibleTerrains.GetEnumerator();
      while (enumerator.MoveNext())
        this.visibleTerrains[enumerator.Current.Key] = enumerator.Current.Value;
    }
    lock (this.threadVisibleListPool)
    {
      visibleRenderers.Clear();
      this.threadVisibleListPool.Push(visibleRenderers);
      visibleTerrains.Clear();
      this.threadVisibleListPool.Push(visibleTerrains);
    }
  }

  private static void _FrustumCullSector(
    SECTR_Sector sector,
    Vector3 cameraPos,
    List<Plane> cullingPlanes,
    List<List<Plane>> occluderFrustums,
    int baseMask,
    float shadowDistance,
    bool forceGroupCull,
    ref Dictionary<int, SECTR_Member.Child> visibleRenderers,
    ref Dictionary<int, SECTR_Member.Child> visibleLights,
    ref Dictionary<int, SECTR_Member.Child> visibleTerrains,
    ref Dictionary<SECTR_Member.Child, int> shadowLights)
  {
    SECTR_CullingCamera._FrustumCull((SECTR_Member) sector, cameraPos, cullingPlanes, occluderFrustums, baseMask, shadowDistance, forceGroupCull, ref visibleRenderers, ref visibleLights, ref visibleTerrains, ref shadowLights);
    int count = sector.Members.Count;
    for (int index = 0; index < count; ++index)
    {
      SECTR_Member member = sector.Members[index];
      if (member.HasRenderBounds || member.HasLightBounds)
        SECTR_CullingCamera._FrustumCull(member, cameraPos, cullingPlanes, occluderFrustums, baseMask, shadowDistance, forceGroupCull, ref visibleRenderers, ref visibleLights, ref visibleTerrains, ref shadowLights);
    }
  }

  private static void _FrustumCull(
    SECTR_Member member,
    Vector3 cameraPos,
    List<Plane> frustumPlanes,
    List<List<Plane>> occluders,
    int baseMask,
    float shadowDistance,
    bool forceGroupCull,
    ref Dictionary<int, SECTR_Member.Child> visibleRenderers,
    ref Dictionary<int, SECTR_Member.Child> visibleLights,
    ref Dictionary<int, SECTR_Member.Child> visibleTerrains,
    ref Dictionary<SECTR_Member.Child, int> shadowLights)
  {
    int inMask1 = baseMask;
    int inMask2 = baseMask;
    int outMask1 = 0;
    int outMask2 = 0;
    bool flag1 = member.CullEachChild && !forceGroupCull;
    bool flag2 = member.HasRenderBounds && SECTR_Geometry.FrustumIntersectsBounds(member.RenderBounds, frustumPlanes, inMask1, out outMask1);
    bool flag3 = member.HasLightBounds && SECTR_Geometry.FrustumIntersectsBounds(member.LightBounds, frustumPlanes, inMask2, out outMask2);
    int count1 = occluders.Count;
    for (int index = 0; index < count1 && flag2 | flag3; ++index)
    {
      List<Plane> occluder = occluders[index];
      if (flag2)
        flag2 = !SECTR_Geometry.FrustumContainsBounds(member.RenderBounds, occluder);
      if (flag3)
        flag3 = !SECTR_Geometry.FrustumContainsBounds(member.LightBounds, occluder);
    }
    if (flag2)
    {
      int count2 = member.Renderers.Count;
      for (int index = 0; index < count2; ++index)
      {
        SECTR_Member.Child renderer = member.Renderers[index];
        if (renderer.renderHash != 0 && !visibleRenderers.ContainsKey(renderer.renderHash) && (!flag1 || SECTR_CullingCamera._IsVisible(renderer.rendererBounds, frustumPlanes, outMask1, occluders)))
          visibleRenderers.Add(renderer.renderHash, renderer);
      }
      int count3 = member.Terrains.Count;
      for (int index = 0; index < count3; ++index)
      {
        SECTR_Member.Child terrain = member.Terrains[index];
        if (terrain.terrainHash != 0 && !visibleTerrains.ContainsKey(terrain.terrainHash) && (!flag1 || SECTR_CullingCamera._IsVisible(terrain.terrainBounds, frustumPlanes, outMask1, occluders)))
          visibleTerrains.Add(terrain.terrainHash, terrain);
      }
    }
    if (!flag3)
      return;
    int count4 = member.Lights.Count;
    for (int index = 0; index < count4; ++index)
    {
      SECTR_Member.Child light = member.Lights[index];
      if (light.lightHash != 0 && !visibleLights.ContainsKey(light.lightHash) && (!flag1 || SECTR_CullingCamera._IsVisible(light.lightBounds, frustumPlanes, outMask1, occluders)))
      {
        visibleLights.Add(light.lightHash, light);
        if (light.shadowLight && !shadowLights.ContainsKey(light) && (double) Vector3.Distance(cameraPos, light.shadowLightPosition) - (double) light.shadowLightRange <= (double) shadowDistance)
          shadowLights.Add(light, 0);
      }
    }
  }

  private static void _ShadowCullSector(
    SECTR_Sector sector,
    List<SECTR_Member.Child> sectorShadowLights,
    ref Dictionary<int, SECTR_Member.Child> visibleRenderers,
    ref Dictionary<int, SECTR_Member.Child> visibleTerrains)
  {
    if (sector.ShadowCaster)
      SECTR_CullingCamera._ShadowCull((SECTR_Member) sector, sectorShadowLights, ref visibleRenderers, ref visibleTerrains);
    int count = sector.Members.Count;
    for (int index = 0; index < count; ++index)
    {
      SECTR_Member member = sector.Members[index];
      if (member.ShadowCaster)
        SECTR_CullingCamera._ShadowCull(member, sectorShadowLights, ref visibleRenderers, ref visibleTerrains);
    }
  }

  private static void _ShadowCull(
    SECTR_Member member,
    List<SECTR_Member.Child> shadowLights,
    ref Dictionary<int, SECTR_Member.Child> visibleRenderers,
    ref Dictionary<int, SECTR_Member.Child> visibleTerrains)
  {
    int count1 = shadowLights.Count;
    int count2 = member.ShadowCasters.Count;
    if (member.CullEachChild)
    {
      for (int index1 = 0; index1 < count2; ++index1)
      {
        SECTR_Member.Child shadowCaster = member.ShadowCasters[index1];
        if (shadowCaster.renderHash != 0 && !visibleRenderers.ContainsKey(shadowCaster.renderHash))
        {
          for (int index2 = 0; index2 < count1; ++index2)
          {
            SECTR_Member.Child shadowLight = shadowLights[index2];
            if ((shadowLight.shadowCullingMask & 1 << (int) shadowCaster.layer) != 0 && (shadowLight.shadowLightType == LightType.Spot && shadowCaster.rendererBounds.Intersects(shadowLight.lightBounds) || shadowLight.shadowLightType == LightType.Point && SECTR_Geometry.BoundsIntersectsSphere(shadowCaster.rendererBounds, shadowLight.shadowLightPosition, shadowLight.shadowLightRange)))
            {
              visibleRenderers.Add(shadowCaster.renderHash, shadowCaster);
              break;
            }
          }
        }
        if (shadowCaster.terrainHash != 0 && !visibleTerrains.ContainsKey(shadowCaster.terrainHash))
        {
          for (int index2 = 0; index2 < count1; ++index2)
          {
            SECTR_Member.Child shadowLight = shadowLights[index2];
            if ((shadowLight.shadowCullingMask & 1 << (int) shadowCaster.layer) != 0 && (shadowLight.shadowLightType == LightType.Spot && shadowCaster.terrainBounds.Intersects(shadowLight.lightBounds) || shadowLight.shadowLightType == LightType.Point && SECTR_Geometry.BoundsIntersectsSphere(shadowCaster.terrainBounds, shadowLight.shadowLightPosition, shadowLight.shadowLightRange)))
            {
              visibleTerrains.Add(shadowCaster.terrainHash, shadowCaster);
              break;
            }
          }
        }
      }
    }
    else
    {
      for (int index1 = 0; index1 < count1; ++index1)
      {
        SECTR_Member.Child shadowLight = shadowLights[index1];
        if ((shadowLight.shadowLightType == LightType.Spot ? (member.RenderBounds.Intersects(shadowLight.lightBounds) ? 1 : 0) : (SECTR_Geometry.BoundsIntersectsSphere(member.RenderBounds, shadowLight.shadowLightPosition, shadowLight.shadowLightRange) ? 1 : 0)) != 0)
        {
          int shadowCullingMask = shadowLight.shadowCullingMask;
          for (int index2 = 0; index2 < count2; ++index2)
          {
            SECTR_Member.Child shadowCaster = member.ShadowCasters[index2];
            if (shadowCaster.renderHash != 0 && shadowCaster.terrainHash != 0)
            {
              if ((shadowCullingMask & 1 << (int) shadowCaster.layer) != 0)
              {
                if (!visibleRenderers.ContainsKey(shadowCaster.renderHash))
                  visibleRenderers.Add(shadowCaster.renderHash, shadowCaster);
                if (!visibleTerrains.ContainsKey(shadowCaster.terrainHash))
                  visibleTerrains.Add(shadowCaster.terrainHash, shadowCaster);
              }
            }
            else if (shadowCaster.renderHash != 0 && !visibleRenderers.ContainsKey(shadowCaster.renderHash) && (shadowCullingMask & 1 << (int) shadowCaster.layer) != 0)
              visibleRenderers.Add(shadowCaster.renderHash, shadowCaster);
            else if (shadowCaster.terrainHash != 0 && !visibleTerrains.ContainsKey(shadowCaster.terrainHash) && (shadowCullingMask & 1 << (int) shadowCaster.layer) != 0)
              visibleTerrains.Add(shadowCaster.terrainHash, shadowCaster);
          }
        }
      }
    }
  }

  private static bool _IsVisible(
    Bounds childBounds,
    List<Plane> frustumPlanes,
    int parentMask,
    List<List<Plane>> occluders)
  {
    if (!SECTR_Geometry.FrustumIntersectsBounds(childBounds, frustumPlanes, parentMask, out int _))
      return false;
    int count = occluders.Count;
    for (int index = 0; index < count; ++index)
    {
      if (SECTR_Geometry.FrustumContainsBounds(childBounds, occluders[index]))
        return false;
    }
    return true;
  }

  private void _HideAllMembers()
  {
    int count1 = SECTR_Member.All.Count;
    for (int index1 = 0; index1 < count1; ++index1)
    {
      SECTR_Member sectrMember = SECTR_Member.All[index1];
      int count2 = sectrMember.Renderers.Count;
      for (int index2 = 0; index2 < count2; ++index2)
      {
        SECTR_Member.Child renderer = sectrMember.Renderers[index2];
        renderer.renderCulled = true;
        if ((bool) (UnityEngine.Object) renderer.renderer)
          renderer.renderer.enabled = false;
        this.hiddenRenderers[renderer.renderHash] = renderer;
      }
      int count3 = sectrMember.Lights.Count;
      for (int index2 = 0; index2 < count3; ++index2)
      {
        SECTR_Member.Child light = sectrMember.Lights[index2];
        light.lightCulled = true;
        if ((bool) (UnityEngine.Object) light.light)
          light.light.enabled = false;
        this.hiddenLights[light.lightHash] = light;
      }
      int count4 = sectrMember.Terrains.Count;
      for (int index2 = 0; index2 < count4; ++index2)
      {
        SECTR_Member.Child terrain = sectrMember.Terrains[index2];
        terrain.terrainCulled = true;
        if ((bool) (UnityEngine.Object) terrain.terrain)
        {
          terrain.terrain.drawHeightmap = false;
          terrain.terrain.drawTreesAndFoliage = false;
        }
        this.hiddenTerrains[terrain.terrainHash] = terrain;
      }
    }
  }

  private void _ApplyCulling(bool visible)
  {
    Dictionary<int, SECTR_Member.Child>.Enumerator enumerator1 = this.visibleRenderers.GetEnumerator();
    KeyValuePair<int, SECTR_Member.Child> current;
    while (enumerator1.MoveNext())
    {
      current = enumerator1.Current;
      SECTR_Member.Child child1 = current.Value;
      if ((bool) (UnityEngine.Object) child1.renderer)
        child1.renderer.enabled = visible;
      child1.renderCulled = !visible;
      if (visible)
      {
        Dictionary<int, SECTR_Member.Child> hiddenRenderers = this.hiddenRenderers;
        current = enumerator1.Current;
        int key = current.Key;
        hiddenRenderers.Remove(key);
      }
      else
      {
        Dictionary<int, SECTR_Member.Child> hiddenRenderers = this.hiddenRenderers;
        current = enumerator1.Current;
        int key = current.Key;
        SECTR_Member.Child child2 = child1;
        hiddenRenderers[key] = child2;
      }
    }
    if (visible)
      this.renderersCulled = this.hiddenRenderers.Count;
    Dictionary<int, SECTR_Member.Child>.Enumerator enumerator2 = this.visibleLights.GetEnumerator();
    while (enumerator2.MoveNext())
    {
      current = enumerator2.Current;
      SECTR_Member.Child child1 = current.Value;
      if ((bool) (UnityEngine.Object) child1.light)
        child1.light.enabled = visible;
      child1.lightCulled = !visible;
      if (visible)
      {
        Dictionary<int, SECTR_Member.Child> hiddenLights = this.hiddenLights;
        current = enumerator2.Current;
        int key = current.Key;
        hiddenLights.Remove(key);
      }
      else
      {
        Dictionary<int, SECTR_Member.Child> hiddenLights = this.hiddenLights;
        current = enumerator2.Current;
        int key = current.Key;
        SECTR_Member.Child child2 = child1;
        hiddenLights[key] = child2;
      }
    }
    if (visible)
      this.lightsCulled = this.hiddenLights.Count;
    Dictionary<int, SECTR_Member.Child>.Enumerator enumerator3 = this.visibleTerrains.GetEnumerator();
    while (enumerator3.MoveNext())
    {
      current = enumerator3.Current;
      SECTR_Member.Child child1 = current.Value;
      if ((bool) (UnityEngine.Object) child1.terrain)
      {
        child1.terrain.drawHeightmap = visible;
        child1.terrain.drawTreesAndFoliage = visible;
      }
      child1.terrainCulled = !visible;
      if (visible)
      {
        Dictionary<int, SECTR_Member.Child> hiddenTerrains = this.hiddenTerrains;
        current = enumerator3.Current;
        int key = current.Key;
        hiddenTerrains.Remove(key);
      }
      else
      {
        Dictionary<int, SECTR_Member.Child> hiddenTerrains = this.hiddenTerrains;
        current = enumerator3.Current;
        int key = current.Key;
        SECTR_Member.Child child2 = child1;
        hiddenTerrains[key] = child2;
      }
    }
    if (visible)
      this.terrainsCulled = this.hiddenTerrains.Count;
    this.didCull = true;
  }

  private void _UndoCulling()
  {
    if (!this.didCull)
      return;
    Dictionary<int, SECTR_Member.Child>.Enumerator enumerator1 = this.hiddenRenderers.GetEnumerator();
    while (enumerator1.MoveNext())
    {
      SECTR_Member.Child child = enumerator1.Current.Value;
      if ((bool) (UnityEngine.Object) child.renderer)
        child.renderer.enabled = true;
      child.renderCulled = false;
    }
    this.hiddenRenderers.Clear();
    Dictionary<int, SECTR_Member.Child>.Enumerator enumerator2 = this.hiddenLights.GetEnumerator();
    while (enumerator2.MoveNext())
    {
      SECTR_Member.Child child = enumerator2.Current.Value;
      if ((bool) (UnityEngine.Object) child.light)
        child.light.enabled = true;
      child.lightCulled = false;
    }
    this.hiddenLights.Clear();
    Dictionary<int, SECTR_Member.Child>.Enumerator enumerator3 = this.hiddenTerrains.GetEnumerator();
    while (enumerator3.MoveNext())
    {
      SECTR_Member.Child child = enumerator3.Current.Value;
      Terrain terrain = child.terrain;
      if ((bool) (UnityEngine.Object) child.terrain)
      {
        terrain.drawHeightmap = true;
        terrain.drawTreesAndFoliage = true;
      }
      child.terrainCulled = false;
    }
    this.hiddenTerrains.Clear();
    this.didCull = false;
  }

  private void _BuildFrustumFromHull(
    Camera cullingCamera,
    bool forwardTraversal,
    List<SECTR_CullingCamera.ClipVertex> portalVertices,
    ref List<Plane> newFrustum)
  {
    int count = portalVertices.Count;
    if (count <= 2)
      return;
    for (int index = 0; index < count; ++index)
    {
      Vector3 vertex = (Vector3) portalVertices[index].vertex;
      Vector3 vector3_1 = (Vector3) portalVertices[(index + 1) % count].vertex - vertex;
      if ((double) Vector3.SqrMagnitude(vector3_1) > 1.0 / 1000.0)
      {
        Vector3 vector3_2 = vertex - cullingCamera.transform.position;
        Vector3 inNormal = forwardTraversal ? Vector3.Cross(vector3_1, vector3_2) : Vector3.Cross(vector3_2, vector3_1);
        inNormal.Normalize();
        newFrustum.Add(new Plane(inNormal, vertex));
      }
    }
  }

  private struct VisibilityNode
  {
    public SECTR_Sector sector;
    public SECTR_Portal portal;
    public List<Plane> frustumPlanes;
    public bool forwardTraversal;

    public VisibilityNode(
      SECTR_CullingCamera cullingCamera,
      SECTR_Sector sector,
      SECTR_Portal portal,
      Plane[] frustumPlanes,
      bool forwardTraversal)
    {
      this.sector = sector;
      this.portal = portal;
      if (frustumPlanes == null)
        this.frustumPlanes = (List<Plane>) null;
      else if (cullingCamera.frustumPool.Count > 0)
      {
        this.frustumPlanes = cullingCamera.frustumPool.Pop();
        this.frustumPlanes.AddRange((IEnumerable<Plane>) frustumPlanes);
      }
      else
        this.frustumPlanes = new List<Plane>((IEnumerable<Plane>) frustumPlanes);
      this.forwardTraversal = forwardTraversal;
    }

    public VisibilityNode(
      SECTR_CullingCamera cullingCamera,
      SECTR_Sector sector,
      SECTR_Portal portal,
      List<Plane> frustumPlanes,
      bool forwardTraversal)
    {
      this.sector = sector;
      this.portal = portal;
      if (frustumPlanes == null)
        this.frustumPlanes = (List<Plane>) null;
      else if (cullingCamera.frustumPool.Count > 0)
      {
        this.frustumPlanes = cullingCamera.frustumPool.Pop();
        this.frustumPlanes.AddRange((IEnumerable<Plane>) frustumPlanes);
      }
      else
        this.frustumPlanes = new List<Plane>((IEnumerable<Plane>) frustumPlanes);
      this.forwardTraversal = forwardTraversal;
    }
  }

  private struct ClipVertex
  {
    public Vector4 vertex;
    public float side;

    public ClipVertex(Vector4 vertex, float side)
    {
      this.vertex = vertex;
      this.side = side;
    }
  }

  private struct ThreadCullData
  {
    public SECTR_Sector sector;
    public Vector3 cameraPos;
    public List<Plane> cullingPlanes;
    public List<List<Plane>> occluderFrustums;
    public int baseMask;
    public float shadowDistance;
    public bool cullingSimpleCulling;
    public List<SECTR_Member.Child> sectorShadowLights;
    public SECTR_CullingCamera.ThreadCullData.CullingModes cullingMode;

    public ThreadCullData(
      SECTR_Sector sector,
      SECTR_CullingCamera cullingCamera,
      Vector3 cameraPos,
      List<Plane> cullingPlanes,
      List<List<Plane>> occluderFrustums,
      int baseMask,
      float shadowDistance,
      bool cullingSimpleCulling)
    {
      this.sector = sector;
      this.cameraPos = cameraPos;
      this.baseMask = baseMask;
      this.shadowDistance = shadowDistance;
      this.cullingSimpleCulling = cullingSimpleCulling;
      this.sectorShadowLights = (List<SECTR_Member.Child>) null;
      lock (cullingCamera.threadOccluderPool)
        this.occluderFrustums = cullingCamera.threadOccluderPool.Count > 0 ? cullingCamera.threadOccluderPool.Pop() : new List<List<Plane>>(occluderFrustums.Count);
      lock (cullingCamera.threadFrustumPool)
      {
        if (cullingCamera.threadFrustumPool.Count > 0)
        {
          this.cullingPlanes = cullingCamera.threadFrustumPool.Pop();
          this.cullingPlanes.AddRange((IEnumerable<Plane>) cullingPlanes);
        }
        else
          this.cullingPlanes = new List<Plane>((IEnumerable<Plane>) cullingPlanes);
        int count = occluderFrustums.Count;
        for (int index = 0; index < count; ++index)
        {
          List<Plane> planeList;
          if (cullingCamera.threadFrustumPool.Count > 0)
          {
            planeList = cullingCamera.threadFrustumPool.Pop();
            planeList.AddRange((IEnumerable<Plane>) occluderFrustums[index]);
          }
          else
            planeList = new List<Plane>((IEnumerable<Plane>) occluderFrustums[index]);
          this.occluderFrustums.Add(planeList);
        }
      }
      this.cullingMode = SECTR_CullingCamera.ThreadCullData.CullingModes.Graph;
    }

    public ThreadCullData(
      SECTR_Sector sector,
      Vector3 cameraPos,
      List<SECTR_Member.Child> sectorShadowLights)
    {
      this.sector = sector;
      this.cameraPos = cameraPos;
      this.cullingPlanes = (List<Plane>) null;
      this.occluderFrustums = (List<List<Plane>>) null;
      this.baseMask = 0;
      this.shadowDistance = 0.0f;
      this.cullingSimpleCulling = false;
      this.sectorShadowLights = sectorShadowLights;
      this.cullingMode = SECTR_CullingCamera.ThreadCullData.CullingModes.Shadow;
    }

    public enum CullingModes
    {
      None,
      Graph,
      Shadow,
    }
  }
}
