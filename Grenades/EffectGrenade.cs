// Decompiled with JetBrains decompiler
// Type: Grenades.EffectGrenade
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

namespace Grenades
{
  public class EffectGrenade : Grenade
  {
    [Header("Shake Effect")]
    public GameObject clientGrenadeEffect;
    public bool DisableGameObject;
    public GameObject serverGrenadeEffect;
    public AnimationCurve shakeOverDistance;

    public override bool ServersideExplosion()
    {
      if ((Object) this.serverGrenadeEffect != (Object) null)
      {
        Transform transform = this.transform;
        Object.Instantiate<GameObject>(this.serverGrenadeEffect, transform.position, transform.rotation);
      }
      return base.ServersideExplosion();
    }

    private void MirrorProcessed()
    {
    }
  }
}
