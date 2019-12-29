// Decompiled with JetBrains decompiler
// Type: WeaponLaser
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class WeaponLaser : MonoBehaviour
{
  private WeaponManager manager;
  public GameObject forwardDirection;
  public Light light;
  public AnimationCurve sizeOverDistance;
  private Quaternion localRot;
  public float maxAngle;
  private Vector3 rotCam;
  private Vector3 rotBar;
  private Vector3 hitPoint;
  public LayerMask raycastMask;
  private RaycastHit hit;

  private void Awake()
  {
    this.manager = this.GetComponentInParent<WeaponManager>();
  }

  private void LateUpdate()
  {
    if ((Object) this.forwardDirection == (Object) null)
    {
      if (!((Object) this.light != (Object) null))
        return;
      this.light.enabled = false;
    }
    else
    {
      this.light.enabled = true;
      float num = Vector3.Angle(this.manager.camera.forward, this.forwardDirection.transform.forward);
      Quaternion rotation = this.manager.camera.rotation;
      this.rotCam = rotation.eulerAngles;
      rotation = this.forwardDirection.transform.rotation;
      this.rotBar = rotation.eulerAngles;
      this.rotBar.z = 0.0f;
      this.rotCam.z = 0.0f;
      Quaternion quaternion = Quaternion.Euler(this.rotBar - this.rotCam);
      this.localRot = (double) num > (double) this.maxAngle || this.manager.IsReloading() ? quaternion : Quaternion.Euler(Vector3.zero);
      Physics.Raycast(this.transform.position, this.transform.forward, out this.hit, 1000f, (int) this.raycastMask);
      this.hitPoint = this.hit.point;
      this.light.spotAngle = this.sizeOverDistance.Evaluate(this.hit.distance);
      this.light.transform.localPosition = this.hit.distance * 0.75f * Vector3.forward;
      this.transform.localRotation = this.localRot;
    }
  }

  private void OnDrawGizmos()
  {
    Gizmos.color = Color.cyan;
    Gizmos.DrawSphere(this.hit.point, 0.5f);
  }
}
