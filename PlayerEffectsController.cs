// Decompiled with JetBrains decompiler
// Type: PlayerEffectsController
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using CustomPlayerEffects;
using Mirror;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayerEffectsController : NetworkBehaviour
{
  private List<PlayerEffect> allEffects = new List<PlayerEffect>();
  private RoleType prevClass = RoleType.None;
  private CharacterClassManager ccm;
  private PlayerStats ps;
  [SyncVar]
  private string syncEffects;

  private void Awake()
  {
    this.ps = this.GetComponent<PlayerStats>();
    this.ccm = this.ps.ccm;
    this.allEffects.Add((PlayerEffect) new Scp207(this.ps, "SCP-207"));
    this.allEffects.Add((PlayerEffect) new Scp268(this.ps, "SCP-268"));
    this.allEffects.Add((PlayerEffect) new Corroding(this.ps, "Corroding"));
    this.allEffects.Add((PlayerEffect) new SinkHole(this.ps, "SinkHole"));
    if (!NetworkServer.active)
      return;
    this.Resync();
  }

  [Server]
  public void Use500()
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Void PlayerEffectsController::Use500()' called on client");
    }
    else
    {
      foreach (PlayerEffect allEffect in this.allEffects)
        allEffect.ServerOnScp500Use();
      if ((double) this.ps.GetHealthPercent() < 0.75)
        return;
      this.ps.TargetAchieve(this.ps.connectionToClient, "crisisaverted");
    }
  }

  public List<PlayerEffect> GetAllEffects()
  {
    return this.allEffects;
  }

  public void ChangeItem(int oldItemUniq, int newItemUniq)
  {
    ItemType oldItem = ItemType.None;
    ItemType newItem = ItemType.None;
    foreach (Inventory.SyncItemInfo syncItemInfo in (SyncList<Inventory.SyncItemInfo>) this.ccm.GetComponent<Inventory>().items)
    {
      if (oldItemUniq == syncItemInfo.uniq)
        oldItem = syncItemInfo.id;
      if (newItemUniq == syncItemInfo.uniq)
        newItem = syncItemInfo.id;
    }
    foreach (PlayerEffect allEffect in this.allEffects)
      allEffect.ServerOnItemChange(oldItemUniq, newItemUniq, oldItem, newItem);
  }

  private void Update()
  {
    for (int index = 0; index < this.allEffects.Count; ++index)
    {
      if (!NetworkServer.active)
        this.allEffects[index].Enabled = this.syncEffects[index] == '1';
      this.allEffects[index].OnUpdate();
      if (NetworkServer.active && this.ccm.CurClass != this.prevClass)
        this.allEffects[index].ServerOnClassChange(this.prevClass, this.ccm.CurClass);
    }
    this.prevClass = this.ccm.CurClass;
  }

  [Server]
  public void EnableEffect(string apiName)
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Void PlayerEffectsController::EnableEffect(System.String)' called on client");
    }
    else
    {
      foreach (PlayerEffect allEffect in this.allEffects)
      {
        if (allEffect.ApiName == apiName)
          allEffect.ServerEnable();
      }
    }
  }

  [Server]
  public void DisableEffect(string apiName)
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Void PlayerEffectsController::DisableEffect(System.String)' called on client");
    }
    else
    {
      foreach (PlayerEffect allEffect in this.allEffects)
      {
        if (allEffect.ApiName == apiName)
          allEffect.ServerDisable();
      }
    }
  }

  public T GetEffect<T>(string apiName) where T : class
  {
    foreach (PlayerEffect allEffect in this.allEffects)
    {
      if (allEffect.ApiName == apiName)
        return allEffect as T;
    }
    return default (T);
  }

  [Server]
  public void Resync()
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Void PlayerEffectsController::Resync()' called on client");
    }
    else
    {
      string empty = string.Empty;
      foreach (PlayerEffect allEffect in this.allEffects)
        empty += (allEffect.Enabled ? '1' : '0').ToString();
      this.NetworksyncEffects = empty;
    }
  }

  private void MirrorProcessed()
  {
  }

  public string NetworksyncEffects
  {
    get
    {
      return this.syncEffects;
    }
    [param: In] set
    {
      this.SetSyncVar<string>(value, ref this.syncEffects, 1UL);
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteString(this.syncEffects);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteString(this.syncEffects);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworksyncEffects = reader.ReadString();
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      this.NetworksyncEffects = reader.ReadString();
    }
  }
}
