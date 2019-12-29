// Decompiled with JetBrains decompiler
// Type: CustomPlayerEffects.SinkHole
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

namespace CustomPlayerEffects
{
  public class SinkHole : PlayerEffect
  {
    [Range(1f, 99f)]
    public float slowAmount;

    public SinkHole(PlayerStats ps, string apiName)
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
      if (!this.Player.isLocalPlayer)
        return;
      OOF_Controller.singleton.spot.enabled = this.Enabled;
      OOF_Controller.singleton.glow.enabled = this.Enabled;
    }
  }
}
