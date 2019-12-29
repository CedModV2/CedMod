// Decompiled with JetBrains decompiler
// Type: SECTR_Geometry
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

public static class SECTR_Geometry
{
  public const float kVERTEX_EPSILON = 0.001f;
  public const float kBOUNDS_CHEAT = 0.01f;

  public static Bounds ComputeBounds(Light light)
  {
    Bounds bounds;
    if ((bool) (UnityEngine.Object) light)
    {
      switch (light.type)
      {
        case LightType.Spot:
          Vector3 position = light.transform.position;
          bounds = new Bounds(position, Vector3.zero);
          Vector3 up = light.transform.up;
          Vector3 right = light.transform.right;
          Vector3 point = position + light.transform.forward * light.range;
          float num1 = Mathf.Tan((float) ((double) light.spotAngle * 0.5 * (Math.PI / 180.0))) * light.range;
          bounds.Encapsulate(point);
          Vector3 vector3_1 = point + up * num1;
          Vector3 vector3_2 = point + up * -num1;
          Vector3 vector3_3 = right * num1;
          Vector3 vector3_4 = right * -num1;
          bounds.Encapsulate(vector3_1 + vector3_3);
          bounds.Encapsulate(vector3_1 + vector3_4);
          bounds.Encapsulate(vector3_2 + vector3_3);
          bounds.Encapsulate(vector3_2 + vector3_4);
          break;
        case LightType.Point:
          float num2 = light.range * 2f;
          bounds = new Bounds(light.transform.position, new Vector3(num2, num2, num2));
          break;
        default:
          bounds = new Bounds(light.transform.position, new Vector3(0.01f, 0.01f, 0.01f));
          break;
      }
    }
    else
      bounds = new Bounds(light.transform.position, new Vector3(0.01f, 0.01f, 0.01f));
    return bounds;
  }

  public static Bounds ComputeBounds(Terrain terrain)
  {
    if (!(bool) (UnityEngine.Object) terrain)
      return new Bounds();
    Vector3 size = (UnityEngine.Object) terrain.terrainData != (UnityEngine.Object) null ? terrain.terrainData.size : Vector3.zero;
    Vector3 position = terrain.transform.position;
    return new Bounds(new Vector3(position.x + size.x * 0.5f, position.y + size.y * 0.5f, position.z + size.z * 0.5f), size);
  }

  public static bool FrustumIntersectsBounds(
    Bounds bounds,
    List<Plane> frustum,
    int inMask,
    out int outMask)
  {
    Vector3 center = bounds.center;
    Vector3 extents = bounds.extents;
    outMask = 0;
    int index1 = 0;
    for (int index2 = 1; index2 <= inMask; index2 += index2)
    {
      if ((index2 & inMask) != 0)
      {
        Plane plane = frustum[index1];
        if ((double) center.x * (double) plane.normal.x + (double) center.y * (double) plane.normal.y + (double) center.z * (double) plane.normal.z + (double) plane.distance + ((double) extents.x * (double) Mathf.Abs(plane.normal.x) + (double) extents.y * (double) Mathf.Abs(plane.normal.y) + (double) extents.z * (double) Mathf.Abs(plane.normal.z)) < 0.0)
          return false;
        outMask |= index2;
      }
      ++index1;
    }
    return true;
  }

  public static bool FrustumContainsBounds(Bounds bounds, List<Plane> frustum)
  {
    Vector3 center = bounds.center;
    Vector3 extents = bounds.extents;
    int count = frustum.Count;
    for (int index = 0; index < count; ++index)
    {
      Plane plane = frustum[index];
      float num1 = (float) ((double) center.x * (double) plane.normal.x + (double) center.y * (double) plane.normal.y + (double) center.z * (double) plane.normal.z) + plane.distance;
      float num2 = (float) ((double) extents.x * (double) Mathf.Abs(plane.normal.x) + (double) extents.y * (double) Mathf.Abs(plane.normal.y) + (double) extents.z * (double) Mathf.Abs(plane.normal.z));
      if ((double) num1 + (double) num2 < 0.0 || (double) num1 - (double) num2 < 0.0)
        return false;
    }
    return true;
  }

  public static bool BoundsContainsBounds(Bounds container, Bounds contained)
  {
    return container.Contains(contained.min) && container.Contains(contained.max);
  }

  public static bool BoundsIntersectsSphere(
    Bounds bounds,
    Vector3 sphereCenter,
    float sphereRadius)
  {
    return (double) Vector3.SqrMagnitude(Vector3.Min(Vector3.Max(sphereCenter, bounds.min), bounds.max) - sphereCenter) <= (double) sphereRadius * (double) sphereRadius;
  }

  public static Bounds ProjectBounds(Bounds bounds, Vector3 projection)
  {
    Vector3 point1 = bounds.min + projection;
    Vector3 point2 = bounds.max + projection;
    bounds.Encapsulate(point1);
    bounds.Encapsulate(point2);
    return bounds;
  }

  public static bool IsPointInFrontOfPlane(Vector3 position, Vector3 center, Vector3 normal)
  {
    Vector3 normalized = (position - center).normalized;
    return (double) Vector3.Dot(normal, normalized) > 0.0;
  }

  public static bool IsPolygonConvex(Vector3[] verts)
  {
    int length = verts.Length;
    if (length < 3)
      return false;
    float f = (float) (length - 2) * 3.141593f;
    for (int index = 0; index < length; ++index)
    {
      Vector3 vert1 = verts[index];
      Vector3 vert2 = verts[(index + 1) % length];
      Vector3 vert3 = verts[(index + 2) % length];
      Vector3 lhs = vert1 - vert2;
      lhs.Normalize();
      Vector3 vector3 = vert2;
      Vector3 rhs = vert3 - vector3;
      rhs.Normalize();
      f -= Mathf.Acos(Vector3.Dot(lhs, rhs));
    }
    return (double) Mathf.Abs(f) < 1.0 / 1000.0;
  }

  public static int CompareVectorsCW(Vector3 a, Vector3 b, Vector3 centroid, Vector3 normal)
  {
    Vector3 vector3 = Vector3.Cross(a - centroid, b - centroid);
    float magnitude = vector3.magnitude;
    if ((double) magnitude <= 1.0 / 1000.0)
      return 0;
    Vector3 rhs = vector3 / magnitude;
    return (double) Vector3.Dot(normal, rhs) <= 0.0 ? -1 : 1;
  }
}
