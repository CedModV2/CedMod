// Decompiled with JetBrains decompiler
// Type: CustomPlayerEffects.Corroding
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using UnityEngine;

namespace CustomPlayerEffects
{
  public class Corroding : PlayerEffect
  {
    private float dmg = 1f;
    private float _damageCounter;
    public bool isInPd;

    public Corroding(PlayerStats ps, string apiName)
    {
      this.ApiName = apiName;
      this.Player = ps;
      this.Slot = ConsumableAndWearableItems.UsableItem.ItemSlot.Unwearable;
    }

    public override void ServerOnClassChange(RoleType previousClass, RoleType newClass)
    {
      this.ServerDisable();
    }

    public override void OnUpdate()
    {
      if (this.Player.isLocalPlayer)
      {
        OOF_Controller.singleton.spot.enabled = this.Enabled;
        OOF_Controller.singleton.glow.enabled = this.Enabled;
      }
      if (!this.Enabled || !NetworkServer.active)
        return;
      this._damageCounter += Time.deltaTime;
      if ((double) this._damageCounter < 1.0)
        return;
      --this._damageCounter;
      this.Player.HurtPlayer(new PlayerStats.HitInfo(this.dmg, "WORLD", DamageTypes.Pocket, 0), this.Player.gameObject);
      this.dmg += 0.1f;
    }

    public override void ServerOnChangeState(bool newState)
    {
      if (!newState)
      {
        this.dmg = 1f;
        SoundtrackManager.singleton.StopOverlay(1);
        this.isInPd = false;
      }
      else
      {
        if (!this.isInPd)
          return;
        SoundtrackManager.singleton.PlayOverlay(1);
      }
    }
  }
}
