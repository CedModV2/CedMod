// Decompiled with JetBrains decompiler
// Type: CustomPlayerEffects.Scp207
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using UnityEngine;

namespace CustomPlayerEffects
{
  public class Scp207 : PlayerEffect
  {
    private float _damageCounter;

    public Scp207(PlayerStats ps, string apiName)
    {
      this.ApiName = apiName;
      this.Player = ps;
      this.Slot = ConsumableAndWearableItems.UsableItem.ItemSlot.Unwearable;
    }

    public override void ServerOnClassChange(RoleType previousClass, RoleType newClass)
    {
      this.ServerDisable();
    }

    public override void ServerOnScp500Use()
    {
      this.ServerDisable();
    }

    public override void ServerOnEscape()
    {
      if (!this.Enabled)
        return;
      AchievementManager.Achieve("escape207", true);
    }

    public override void OnUpdate()
    {
      if (!this.Enabled || !NetworkServer.active)
        return;
      this._damageCounter += Time.deltaTime;
      if ((double) this._damageCounter < 1.0)
        return;
      --this._damageCounter;
      float averageMovementSpeed = this.Player.GetComponent<PlyMovementSync>().AverageMovementSpeed;
      this.Player.HurtPlayer(new PlayerStats.HitInfo((double) averageMovementSpeed > 0.200000002980232 ? ((double) averageMovementSpeed > 4.0 ? ((double) averageMovementSpeed > 7.5 ? 1f : 0.4f) : 0.15f) : 0.1f, "SCP-207", DamageTypes.Scp207, 0), this.Player.gameObject);
    }
  }
}
