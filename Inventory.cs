// Decompiled with JetBrains decompiler
// Type: Inventory
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using Mirror;
using Security;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Inventory : NetworkBehaviour
{
  public static float TargetCrosshairAlpha = 1f;
  public Inventory.SyncListItemInfo items = new Inventory.SyncListItemInfo();
  private ItemType _prevIt = ItemType.None;
  private int _prevUniq = -1;
  private float _crosshairAlpha = 1f;
  public GameObject pickupPrefab;
  public Item[] availableItems;
  public GameObject camera;
  private WeaponManager _weaponManager;
  private RateLimit _iawRateLimit;
  private RateLimit _isyncRateLimit;
  private CharacterClassManager _ccm;
  private ConsumableAndWearableItems _cawi;
  private AmmoBox _ab;
  private float _pickupAnimation;
  private bool _ccmSet;
  public static bool CollectionModified;
  public static float InventoryCooldown;
  private static int _uniqId;
  [SyncVar(hook = "SetCurItem")]
  public ItemType curItem;
  [SyncVar]
  public int itemUniq;

  [Command]
  public void CmdSetUnic(int i)
  {
    if (this.isServer)
    {
      this.CallCmdSetUnic(i);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WritePackedInt32(i);
      this.SendCommandInternal(typeof (Inventory), nameof (CmdSetUnic), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  private void Start()
  {
    this._iawRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[3];
    this._isyncRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[4];
    this._ccm = this.GetComponent<CharacterClassManager>();
    this._weaponManager = this.GetComponent<WeaponManager>();
    this._cawi = this.GetComponent<ConsumableAndWearableItems>();
    this._ab = this.GetComponent<AmmoBox>();
    this._ccmSet = true;
    if (!this.isLocalPlayer)
      return;
    Pickup.Inv = this;
    Pickup.Instances = new List<Pickup>();
    Timing.RunCoroutine(this._RefreshPickups(), Segment.FixedUpdate);
  }

  public bool AllowChangeItem()
  {
    return !this._ccmSet || this._ccm.CurClass == RoleType.Spectator || (double) this._cawi.cooldown <= 0.0;
  }

  public void SetCurItem(ItemType ci)
  {
    if (!this.AllowChangeItem())
      return;
    this.NetworkcurItem = ci;
  }

  public Inventory.SyncItemInfo GetItemInHand()
  {
    foreach (Inventory.SyncItemInfo syncItemInfo in (SyncList<Inventory.SyncItemInfo>) this.items)
    {
      if (syncItemInfo.uniq == this.itemUniq)
        return syncItemInfo;
    }
    return new Inventory.SyncItemInfo();
  }

  private IEnumerator<float> _RefreshPickups()
  {
    Inventory inventory = this;
    while ((UnityEngine.Object) inventory != (UnityEngine.Object) null)
    {
      foreach (Pickup instance in Pickup.Instances)
      {
        if ((UnityEngine.Object) instance != (UnityEngine.Object) null)
          instance.CheckForRefresh();
        if (Inventory.CollectionModified)
        {
          Inventory.CollectionModified = false;
          break;
        }
      }
      yield return float.NegativeInfinity;
    }
  }

  private void RefreshModels()
  {
    for (ushort index = 0; (int) index < this.availableItems.Length; ++index)
    {
      try
      {
        Item availableItem = this.availableItems[(int) index];
        availableItem.firstpersonModel.SetActive(this.isLocalPlayer & availableItem.id == this.curItem);
      }
      catch
      {
      }
    }
  }

  public void DropItem(int id, int _s, int _b, int _o)
  {
    if (!this.isLocalPlayer)
      return;
    if (this.items[id].id == this.curItem)
      this.SetCurItem(ItemType.None);
    this.CmdDropItem(id);
  }

  public void ServerDropAll()
  {
    foreach (Inventory.SyncItemInfo syncItemInfo in (SyncList<Inventory.SyncItemInfo>) this.items)
      this.SetPickup(syncItemInfo.id, syncItemInfo.durability, this.transform.position, this.camera.transform.rotation, syncItemInfo.modSight, syncItemInfo.modBarrel, syncItemInfo.modOther);
    for (byte index = 0; index < (byte) 3; ++index)
    {
      if (this._ab.GetAmmo((int) index) != 0)
        this.SetPickup(this._ab.types[(int) index].inventoryID, (float) this._ab.GetAmmo((int) index), this.transform.position, this.camera.transform.rotation, 0, 0, 0);
    }
    this.items.Clear();
    this._ab.Networkamount = "0:0:0";
  }

  public void Clear()
  {
    this.items.Clear();
    this._ab.Networkamount = "0:0:0";
  }

  public int GetItemIndex()
  {
    for (int index = 0; index < this.items.Count; ++index)
    {
      if (this.itemUniq == this.items[index].uniq)
        return index;
    }
    return -1;
  }

  public void AddNewItem(ItemType id, float dur = -4.656647E+11f, int s = 0, int b = 0, int o = 0)
  {
    if (id < ItemType.KeycardJanitor)
      throw new Exception("Invalid item ID.");
    ++Inventory._uniqId;
    Item obj = new Item(this.GetItemByID(id));
    if (this.items.Count >= 8 && !obj.noEquipable)
      return;
    Inventory.SyncItemInfo syncItemInfo = new Inventory.SyncItemInfo()
    {
      id = obj.id,
      durability = obj.durability,
      uniq = Inventory._uniqId
    };
    if ((double) Math.Abs(dur - -4.656647E+11f) > 0.0500000007450581)
    {
      syncItemInfo.durability = dur;
      syncItemInfo.modSight = s;
      syncItemInfo.modBarrel = b;
      syncItemInfo.modOther = o;
    }
    else
    {
      for (int index = 0; index < this._weaponManager.weapons.Length; ++index)
      {
        if (this._weaponManager.weapons[index].inventoryID == id)
        {
          syncItemInfo.modSight = this._weaponManager.modPreferences[index, 0];
          syncItemInfo.modBarrel = this._weaponManager.modPreferences[index, 1];
          syncItemInfo.modOther = this._weaponManager.modPreferences[index, 2];
        }
      }
    }
    this.items.Add(syncItemInfo);
  }

  public Item GetItemByID(ItemType id)
  {
    foreach (Item availableItem in this.availableItems)
    {
      if (availableItem.id == id)
        return availableItem;
    }
    return (Item) null;
  }

  [Command]
  private void CmdSyncItem(ItemType i)
  {
    if (this.isServer)
    {
      this.CallCmdSyncItem(i);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WritePackedInt32((int) i);
      this.SendCommandInternal(typeof (Inventory), nameof (CmdSyncItem), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  private void Update()
  {
    if (this.isLocalPlayer)
    {
      if ((double) this._pickupAnimation > 0.0)
        this._pickupAnimation -= Time.deltaTime;
      Inventory.InventoryCooldown -= Time.deltaTime;
      this.CmdSyncItem(this.curItem);
      Item itemById = this.GetItemByID(this.curItem);
      Color white = Color.white;
      if (itemById != null)
      {
        Color crosshairColor1 = itemById.crosshairColor;
        Color crosshairColor2 = itemById.crosshairColor;
      }
      Mathf.Clamp((int) this.curItem, 0, this.availableItems.Length - 1);
      if (this._ccm.Classes.SafeGet(this._ccm.CurClass).forcedCrosshair != -1)
      {
        int forcedCrosshair = this._ccm.Classes.SafeGet(this._ccm.CurClass).forcedCrosshair;
      }
      this._crosshairAlpha = Mathf.Lerp(this._crosshairAlpha, Inventory.TargetCrosshairAlpha, Time.deltaTime * 5f);
    }
    if (this._prevIt == this.curItem)
      return;
    this.RefreshModels();
    this._prevIt = this.curItem;
    if (this.isLocalPlayer)
    {
      foreach (WeaponManager.Weapon weapon in this._weaponManager.weapons)
      {
        if (weapon.inventoryID == this.curItem)
        {
          if (weapon.useProceduralPickupAnimation)
            this._weaponManager.weaponInventoryGroup.localPosition = Vector3.down * 0.4f;
          this._pickupAnimation = 4f;
        }
      }
    }
    if (!NetworkServer.active)
      return;
    this.RefreshWeapon();
    this.UpdateUniqChange();
  }

  private void UpdateUniqChange()
  {
    this.GetComponent<PlayerEffectsController>().ChangeItem(this._prevUniq, this.itemUniq);
    this._prevUniq = this.itemUniq;
  }

  public bool WeaponReadyToInstantPickup()
  {
    return (double) this._pickupAnimation <= 0.0;
  }

  [ServerCallback]
  private void RefreshWeapon()
  {
    if (!NetworkServer.active)
      return;
    int num1 = 0;
    int num2 = -1;
    foreach (WeaponManager.Weapon weapon in this._weaponManager.weapons)
    {
      if (weapon.inventoryID == this.curItem)
        num2 = num1;
      ++num1;
    }
    this._weaponManager.NetworkcurWeapon = num2;
  }

  [Command]
  private void CmdDropItem(int itemInventoryIndex)
  {
    if (this.isServer)
    {
      this.CallCmdDropItem(itemInventoryIndex);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WritePackedInt32(itemInventoryIndex);
      this.SendCommandInternal(typeof (Inventory), nameof (CmdDropItem), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  public Pickup SetPickup(
    ItemType droppedItemId,
    float dur,
    Vector3 pos,
    Quaternion rot,
    int s,
    int b,
    int o)
  {
    if (droppedItemId < ItemType.KeycardJanitor)
      return (Pickup) null;
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.pickupPrefab);
    NetworkServer.Spawn(gameObject);
    gameObject.GetComponent<Pickup>().SetupPickup(new Pickup.PickupInfo()
    {
      position = pos,
      rotation = rot,
      itemId = droppedItemId,
      durability = dur,
      weaponMods = new int[3]{ s, b, o },
      ownerPlayer = this.gameObject
    });
    return gameObject.GetComponent<Pickup>();
  }

  public Inventory()
  {
    this.InitSyncObject((SyncObject) this.items);
  }

  static Inventory()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (Inventory), "CmdSetUnic", new NetworkBehaviour.CmdDelegate(Inventory.InvokeCmdCmdSetUnic));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Inventory), "CmdSyncItem", new NetworkBehaviour.CmdDelegate(Inventory.InvokeCmdCmdSyncItem));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Inventory), "CmdDropItem", new NetworkBehaviour.CmdDelegate(Inventory.InvokeCmdCmdDropItem));
  }

  private void MirrorProcessed()
  {
  }

  public ItemType NetworkcurItem
  {
    get
    {
      return this.curItem;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(1UL))
      {
        this.setSyncVarHookGuard(1UL, true);
        this.SetCurItem(value);
        this.setSyncVarHookGuard(1UL, false);
      }
      this.SetSyncVar<ItemType>(value, ref this.curItem, 1UL);
    }
  }

  public int NetworkitemUniq
  {
    get
    {
      return this.itemUniq;
    }
    [param: In] set
    {
      this.SetSyncVar<int>(value, ref this.itemUniq, 2UL);
    }
  }

  protected static void InvokeCmdCmdSetUnic(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSetUnic called on client.");
    else
      ((Inventory) obj).CallCmdSetUnic(reader.ReadPackedInt32());
  }

  protected static void InvokeCmdCmdSyncItem(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSyncItem called on client.");
    else
      ((Inventory) obj).CallCmdSyncItem((ItemType) reader.ReadPackedInt32());
  }

  protected static void InvokeCmdCmdDropItem(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdDropItem called on client.");
    else
      ((Inventory) obj).CallCmdDropItem(reader.ReadPackedInt32());
  }

  public void CallCmdSetUnic(int i)
  {
    this.NetworkitemUniq = i;
  }

  public void CallCmdSyncItem(ItemType i)
  {
    if (!this._isyncRateLimit.CanExecute(true) || !this.AllowChangeItem())
      return;
    foreach (Inventory.SyncItemInfo syncItemInfo in (SyncList<Inventory.SyncItemInfo>) this.items)
    {
      if (syncItemInfo.id == i)
      {
        this.NetworkcurItem = i;
        return;
      }
    }
    this.NetworkcurItem = ItemType.None;
  }

  public void CallCmdDropItem(int itemInventoryIndex)
  {
    if (!this._iawRateLimit.CanExecute(true) || itemInventoryIndex < 0 || itemInventoryIndex >= this.items.Count)
      return;
    Inventory.SyncItemInfo syncItemInfo = this.items[itemInventoryIndex];
    if (this.items[itemInventoryIndex].id != syncItemInfo.id)
      return;
    this.SetPickup(syncItemInfo.id, this.items[itemInventoryIndex].durability, this.transform.position, this.camera.transform.rotation, syncItemInfo.modSight, syncItemInfo.modBarrel, syncItemInfo.modOther);
    this.items.RemoveAt(itemInventoryIndex);
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WritePackedInt32((int) this.curItem);
      writer.WritePackedInt32(this.itemUniq);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WritePackedInt32((int) this.curItem);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WritePackedInt32(this.itemUniq);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      ItemType ci = (ItemType) reader.ReadPackedInt32();
      this.SetCurItem(ci);
      this.NetworkcurItem = ci;
      this.NetworkitemUniq = reader.ReadPackedInt32();
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
      {
        ItemType ci = (ItemType) reader.ReadPackedInt32();
        this.SetCurItem(ci);
        this.NetworkcurItem = ci;
      }
      if ((num & 2L) == 0L)
        return;
      this.NetworkitemUniq = reader.ReadPackedInt32();
    }
  }

  [Serializable]
  public struct SyncItemInfo : IEquatable<Inventory.SyncItemInfo>
  {
    public ItemType id;
    public float durability;
    public int uniq;
    public int modSight;
    public int modBarrel;
    public int modOther;

    public bool Equals(Inventory.SyncItemInfo other)
    {
      return this.id == other.id && (double) Math.Abs(this.durability - other.durability) < 0.00499999988824129 && (this.uniq == other.uniq && this.modSight == other.modSight) && this.modBarrel == other.modBarrel && this.modOther == other.modOther;
    }

    public override bool Equals(object obj)
    {
      return obj is Inventory.SyncItemInfo other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return (((((int) this.id * 397 ^ this.durability.GetHashCode()) * 397 ^ this.uniq) * 397 ^ this.modSight) * 397 ^ this.modBarrel) * 397 ^ this.modOther;
    }

    public static bool operator ==(Inventory.SyncItemInfo left, Inventory.SyncItemInfo right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(Inventory.SyncItemInfo left, Inventory.SyncItemInfo right)
    {
      return !left.Equals(right);
    }
  }

  public class SyncListItemInfo : SyncList<Inventory.SyncItemInfo>
  {
    public void ModifyDuration(int index, float value)
    {
      if (index < 0 || index >= this.Count)
        return;
      Inventory.SyncItemInfo syncItemInfo = this[index];
      syncItemInfo.durability = value;
      this[index] = syncItemInfo;
    }

    public void ModifyAttachments(int index, int s, int b, int o)
    {
      if (index < 0 || index >= this.Count)
        return;
      Inventory.SyncItemInfo syncItemInfo = this[index];
      syncItemInfo.modSight = s;
      syncItemInfo.modBarrel = b;
      syncItemInfo.modOther = o;
      this[index] = syncItemInfo;
    }

    protected override void SerializeItem(NetworkWriter writer, Inventory.SyncItemInfo item)
    {
      GeneratedNetworkCode._WriteSyncItemInfo_Inventory(writer, item);
    }

    protected override Inventory.SyncItemInfo DeserializeItem(NetworkReader reader)
    {
      return GeneratedNetworkCode._ReadSyncItemInfo_Inventory(reader);
    }
  }
}
