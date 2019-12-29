// Decompiled with JetBrains decompiler
// Type: WeaponShootAnimation
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class WeaponShootAnimation : MonoBehaviour
{
  public float curPosition;
  public Vector3 maxRecoilPos;
  public Vector3 maxRecoilRot;
  public float backSpeed;
  public float backY_Speed;
  private float yOverride;
  private float curY;

  private void LateUpdate()
  {
    if ((double) this.curPosition > 0.03)
      this.curPosition = Mathf.Lerp(this.curPosition, 0.0f, Time.deltaTime * this.backSpeed * this.curPosition);
    else
      this.curPosition -= Time.deltaTime * 0.1f;
    if ((double) this.curPosition < 0.0)
      this.curPosition = 0.0f;
    this.yOverride = Mathf.Lerp(0.0f, this.yOverride, this.curPosition);
    this.curY = Mathf.Lerp(this.curY, this.yOverride, Time.deltaTime * this.backY_Speed * this.curPosition);
    this.transform.localPosition = Vector3.Lerp(Vector3.zero, this.maxRecoilPos, this.curPosition);
    this.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.Euler(this.maxRecoilRot + Vector3.up * this.curY), this.curPosition);
  }

  public void Recoil(float f)
  {
    this.curPosition = Mathf.Clamp01(this.curPosition + f);
    this.yOverride = Random.Range(-10f, 10f) * f;
  }
}
