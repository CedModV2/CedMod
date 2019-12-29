// Decompiled with JetBrains decompiler
// Type: MoreCast
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

public class MoreCast : MonoBehaviour
{
  public static bool BeamCast(
    Vector3 start,
    Vector3 end,
    Vector3 beamRadius,
    float beamStep,
    out List<RaycastHit> hitInfo,
    int layerMask,
    bool any)
  {
    hitInfo = new List<RaycastHit>();
    Vector3 vector3_1 = start;
    Vector3 vector3_2 = end;
    Vector3 start1 = vector3_1 - beamRadius;
    Vector3 end1 = vector3_2 - beamRadius;
    for (float num1 = -beamRadius.x; (double) num1 < (double) beamRadius.x; num1 += beamStep)
    {
      start1.y = start.y;
      end1.y = end.y;
      start1.x += beamStep;
      end1.x += beamStep;
      for (float num2 = -beamRadius.y; (double) num2 < (double) beamRadius.x; num2 += beamStep)
      {
        start1.z = start.z;
        end1.z = end.z;
        start1.y += beamStep;
        end1.y += beamStep;
        for (float num3 = -beamRadius.x; (double) num3 < (double) beamRadius.x; num3 += beamStep)
        {
          start1.z += beamStep;
          end1.z += beamStep;
          RaycastHit hitInfo1;
          bool flag = Physics.Linecast(start1, end1, out hitInfo1, layerMask);
          hitInfo.Add(hitInfo1);
          if (any & flag)
            return true;
          if (!flag && !any)
            return false;
        }
      }
    }
    return true;
  }

  public static bool BeamCast(
    Vector3 start,
    Vector3 end,
    Vector3 beamRadius,
    float beamStep,
    int layerMask,
    bool any)
  {
    return MoreCast.BeamCast(start, end, beamRadius, beamStep, out List<RaycastHit> _, layerMask, any);
  }
}
