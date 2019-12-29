// Decompiled with JetBrains decompiler
// Type: CPPS_Sniper
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.PostProcessing;

public class CPPS_Sniper : CustomPostProcessingSight
{
  private void Awake()
  {
    this.wm = this.GetComponentInParent<WeaponManager>();
    if (!((Object) this.wm == (Object) null))
      return;
    Object.Destroy((Object) this);
  }

  private void Start()
  {
    if (!this.wm.isLocalPlayer)
      Object.Destroy((Object) this);
    else
      this.ppb = this.wm.weaponModelCamera.GetComponent<PostProcessingBehaviour>();
  }

  private void Update()
  {
    this.canvas.SetActive(this.ppb.profile.name.Equals(this.targetProfile.name));
  }
}
