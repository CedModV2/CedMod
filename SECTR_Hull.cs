// Decompiled with JetBrains decompiler
// Type: SECTR_Hull
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

public abstract class SECTR_Hull : MonoBehaviour
{
  private Vector3 meshCentroid = Vector3.zero;
  protected Vector3 meshNormal = Vector3.forward;
  private Mesh previousMesh;
  private Vector3[] vertsCW;
  [SECTR_ToolTip("Convex, planar mesh that defines the portal shape.")]
  public Mesh HullMesh;

  public Vector3[] VertsCW
  {
    get
    {
      this.ComputeVerts();
      return this.vertsCW;
    }
  }

  public Vector3 Normal
  {
    get
    {
      this.ComputeVerts();
      return this.transform.rotation * this.meshNormal;
    }
  }

  public Vector3 ReverseNormal
  {
    get
    {
      this.ComputeVerts();
      return this.transform.rotation * -this.meshNormal;
    }
  }

  public Vector3 Center
  {
    get
    {
      this.ComputeVerts();
      return this.transform.localToWorldMatrix.MultiplyPoint3x4(this.meshCentroid);
    }
  }

  public Plane HullPlane
  {
    get
    {
      this.ComputeVerts();
      return new Plane(this.Normal, this.Center);
    }
  }

  public Plane ReverseHullPlane
  {
    get
    {
      this.ComputeVerts();
      return new Plane(this.ReverseNormal, this.Center);
    }
  }

  public Bounds BoundingBox
  {
    get
    {
      Bounds bounds = new Bounds(this.transform.position, Vector3.zero);
      if ((bool) (UnityEngine.Object) this.HullMesh)
      {
        this.ComputeVerts();
        if (this.vertsCW != null)
        {
          Matrix4x4 localToWorldMatrix = this.transform.localToWorldMatrix;
          int length = this.vertsCW.Length;
          for (int index = 0; index < length; ++index)
            bounds.Encapsulate(localToWorldMatrix.MultiplyPoint3x4(this.vertsCW[index]));
        }
      }
      return bounds;
    }
  }

  public bool IsPointInHull(Vector3 p, float distanceTolerance)
  {
    this.ComputeVerts();
    Vector3 vector3_1 = this.transform.worldToLocalMatrix.MultiplyPoint3x4(p);
    Vector3 vector3_2 = vector3_1 - Vector3.Dot(vector3_1 - this.meshCentroid, this.meshNormal) * this.meshNormal;
    if (this.vertsCW == null || (double) Vector3.SqrMagnitude(vector3_1 - vector3_2) >= (double) distanceTolerance * (double) distanceTolerance)
      return false;
    float f1 = 6.283185f;
    int length = this.vertsCW.Length;
    for (int index = 0; index < length; ++index)
    {
      Vector3 lhs = this.vertsCW[index] - vector3_2;
      Vector3 rhs = this.vertsCW[(index + 1) % length] - vector3_2;
      float num = lhs.magnitude * rhs.magnitude;
      if ((double) num < 1.0 / 1000.0)
        return true;
      float f2 = Vector3.Dot(lhs, rhs) / num;
      f1 -= Mathf.Acos(f2);
    }
    return (double) Mathf.Abs(f1) < 1.0 / 1000.0;
  }

  protected void ComputeVerts()
  {
    if (!((UnityEngine.Object) this.HullMesh != (UnityEngine.Object) this.previousMesh))
      return;
    if ((bool) (UnityEngine.Object) this.HullMesh)
    {
      int vertexCount = this.HullMesh.vertexCount;
      this.vertsCW = new Vector3[vertexCount];
      this.meshCentroid = Vector3.zero;
      for (int index = 0; index < vertexCount; ++index)
      {
        Vector3 vertex = this.HullMesh.vertices[index];
        this.vertsCW[index] = vertex;
        this.meshCentroid += vertex;
      }
      this.meshCentroid /= (float) this.HullMesh.vertexCount;
      this.meshNormal = Vector3.zero;
      int length = this.HullMesh.normals.Length;
      for (int index = 0; index < length; ++index)
        this.meshNormal += this.HullMesh.normals[index];
      this.meshNormal /= (float) this.HullMesh.normals.Length;
      this.meshNormal.Normalize();
      bool flag = true;
      for (int index = 0; index < vertexCount; ++index)
      {
        Vector3 vector3_1 = this.vertsCW[index];
        Vector3 vector3_2 = vector3_1 - Vector3.Dot(vector3_1 - this.meshCentroid, this.meshNormal) * this.meshNormal;
        flag = flag && (double) Vector3.SqrMagnitude(vector3_1 - vector3_2) < 1.0 / 1000.0;
        this.vertsCW[index] = vector3_2;
      }
      if (!flag)
        Debug.LogWarning((object) ("Occluder mesh of " + this.name + " is not planar!"));
      Array.Sort<Vector3>(this.vertsCW, (Comparison<Vector3>) ((a, b) => SECTR_Geometry.CompareVectorsCW(a, b, this.meshCentroid, this.meshNormal) * -1));
      if (!SECTR_Geometry.IsPolygonConvex(this.vertsCW))
        Debug.LogWarning((object) ("Occluder mesh of " + this.name + " is not convex!"));
    }
    else
    {
      this.meshNormal = Vector3.zero;
      this.meshCentroid = Vector3.zero;
      this.vertsCW = (Vector3[]) null;
    }
    this.previousMesh = this.HullMesh;
  }
}
