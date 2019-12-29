// Decompiled with JetBrains decompiler
// Type: PlayerEffect
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;

public abstract class PlayerEffect
{
  public string ApiName;
  public bool Enabled;
  public ConsumableAndWearableItems.UsableItem.ItemSlot Slot;
  protected PlayerStats Player;

  public void ServerEnable()
  {
    if (this.Enabled)
      return;
    this.Enabled = true;
    if (NetworkServer.active)
      this.Player.GetComponent<PlayerEffectsController>().Resync();
    this.ServerOnChangeState(true);
  }

  public void ServerDisable()
  {
    if (!this.Enabled)
      return;
    this.Enabled = false;
    if (NetworkServer.active)
      this.Player.GetComponent<PlayerEffectsController>().Resync();
    this.ServerOnChangeState(false);
  }

  public virtual void OnUpdate()
  {
  }

  public virtual void ServerOnScp500Use()
  {
  }

  public virtual void ServerOnChangeState(bool newState)
  {
  }

  public virtual void ServerOnClassChange(RoleType previousClass, RoleType newClass)
  {
  }

  public virtual void ServerOnItemChange(
    int oldItemUniq,
    int newItemUniq,
    ItemType oldItem,
    ItemType newItem)
  {
  }

  public virtual void ServerOnEscape()
  {
  }
}
