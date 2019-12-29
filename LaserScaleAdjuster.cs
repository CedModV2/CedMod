// Decompiled with JetBrains decompiler
// Type: LaserScaleAdjuster
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class LaserScaleAdjuster : MonoBehaviour
{
  public LayerMask RaycastMask;
  public AnimationCurve sizeOverDistance;
  public Transform targetTransform;

  private void Update()
  {
    float time = 30f;
    RaycastHit hitInfo;
    if (Physics.Raycast(new Ray(this.transform.position, this.transform.up), out hitInfo, 30f, (int) this.RaycastMask))
      time = hitInfo.distance;
    this.targetTransform.localScale = new Vector3(1f, this.sizeOverDistance.Evaluate(time), 1f);
  }
}
