// Decompiled with JetBrains decompiler
// Type: CameraFocuser
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class CameraFocuser : MonoBehaviour
{
  public float targetFovScale = 1f;
  public Transform lookTarget;
  public float minimumAngle;

  private void OnTriggerStay(Collider other)
  {
    Scp049PlayerScript componentInParent = other.GetComponentInParent<Scp049PlayerScript>();
    if (!((Object) componentInParent != (Object) null) || !componentInParent.isLocalPlayer)
      return;
    this.transform.LookAt(this.lookTarget);
    double num = (double) Mathf.Clamp(Quaternion.Angle(componentInParent.plyCam.transform.rotation, this.transform.rotation), this.minimumAngle, 70f);
  }
}
