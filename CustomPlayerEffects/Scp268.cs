// Decompiled with JetBrains decompiler
// Type: CustomPlayerEffects.Scp268
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using UnityEngine;

namespace CustomPlayerEffects
{
  public class Scp268 : PlayerEffect
  {
    private float curTime;
    private const float maxTime = 15f;
    private OOF_Controller effectsController;
    private float animationTime;
    private float prevAnim;

    public Scp268(PlayerStats ps, string apiName)
    {
      this.ApiName = apiName;
      this.Player = ps;
      this.Slot = ConsumableAndWearableItems.UsableItem.ItemSlot.Head;
      this.effectsController = this.Player.GetComponentInChildren<OOF_Controller>();
    }

    public override void ServerOnClassChange(RoleType previousClass, RoleType newClass)
    {
      this.ServerDisable();
    }

    public override void ServerOnChangeState(bool newState)
    {
      if (!newState)
        return;
      this.curTime = 0.0f;
    }

    public override void OnUpdate()
    {
      if (NetworkServer.active && this.Enabled)
      {
        this.curTime += Time.deltaTime;
        if ((double) this.curTime > 15.0)
          this.ServerDisable();
        bool flag = false;
        foreach (Inventory.SyncItemInfo syncItemInfo in (SyncList<Inventory.SyncItemInfo>) this.Player.GetComponent<Inventory>().items)
        {
          if (syncItemInfo.id == ItemType.SCP268)
            flag = true;
        }
        if (!flag)
          this.ServerDisable();
      }
      if (!this.Player.isLocalPlayer)
        return;
      if (this.Enabled)
      {
        this.animationTime += Time.deltaTime;
        if ((double) this.animationTime > 1.0)
          this.animationTime = 1f;
      }
      else
      {
        this.animationTime -= Time.deltaTime * 2f;
        if ((double) this.animationTime < 0.0)
          this.animationTime = 0.0f;
      }
      if ((double) this.prevAnim == (double) this.animationTime)
        return;
      this.prevAnim = this.animationTime;
      this.effectsController.hatVignette.enabled = this.effectsController.hatColorRgb.enabled = (double) this.animationTime > 0.0;
      this.effectsController.hatVignette.Vignetting = this.effectsController.hatVignette.VignettingFull = this.animationTime;
      this.effectsController.hatVignette.VignettingColor = (Color) new Color32((byte) 0, (byte) 1, (byte) 2, byte.MaxValue);
      this.effectsController.hatColorRgb.Blue = this.animationTime * 0.98f;
      this.effectsController.hatColorRgb.Brightness = this.animationTime * -0.97f;
      this.effectsController.hatColorRgb.Red = this.effectsController.hatColorRgb.Green = this.animationTime * 0.97f;
    }
  }
}
