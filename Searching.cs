// Decompiled with JetBrains decompiler
// Type: Searching
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using Security;
using System;
using TMPro;
using UnityEngine;

public class Searching : NetworkBehaviour
{
  private ReferenceHub hub;
  private bool isHuman;
  private GameObject pickup;
  private Transform cam;
  private FirstPersonController fpc;
  private float timeToPickUp;
  private float errorMsgDur;
  private TextMeshProUGUI inventoryError;
  private LayerMask _playerInteract_Mask;
  private GameObject progressGO;
  public float rayDistance;
  public InventoryCategory[] categories;
  private GameObject _pickupObjectServer;
  private float _pickupProgressServer;
  private bool _pickupInProgressServer;
  private RateLimit _playerInteractRateLimit;
  public string inventoryErrorMessage;

  private void Start()
  {
    this._playerInteractRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[0];
    this.hub = ReferenceHub.GetHub(this.gameObject);
    this.fpc = this.GetComponent<FirstPersonController>();
    this.cam = this.GetComponent<Scp049PlayerScript>().plyCam.transform;
    this._playerInteract_Mask = this.GetComponent<PlayerInteract>().mask;
  }

  public void Init(bool isNotHuman)
  {
    this.isHuman = !isNotHuman;
  }

  private void Update()
  {
    if (!this._pickupInProgressServer)
      return;
    this._pickupProgressServer -= Time.deltaTime;
    if ((double) this._pickupProgressServer > -3.5)
      return;
    this._pickupInProgressServer = false;
    this._pickupProgressServer = 0.0f;
    this._pickupObjectServer = (GameObject) null;
  }

  public void ShowErrorMessage(string msg)
  {
    this.inventoryErrorMessage = msg;
    this.errorMsgDur = 4f;
  }

  private void ErrorMessage()
  {
    if ((double) this.errorMsgDur > 0.0)
      this.errorMsgDur -= Time.deltaTime;
    this.inventoryError.text = this.inventoryErrorMessage;
    this.inventoryError.gameObject.SetActive((double) this.errorMsgDur > 0.0);
  }

  private bool AllowPickup()
  {
    return this.isHuman && this.hub.handcuffs.CufferId < 0;
  }

  [Command]
  private void CmdPickupItem(GameObject t)
  {
    if (this.isServer)
    {
      this.CallCmdPickupItem(t);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(t);
      this.SendCommandInternal(typeof (Searching), nameof (CmdPickupItem), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdStartPickup(GameObject t)
  {
    if (this.isServer)
    {
      this.CallCmdStartPickup(t);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(t);
      this.SendCommandInternal(typeof (Searching), nameof (CmdStartPickup), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdAbortPickup()
  {
    if (this.isServer)
    {
      this.CallCmdAbortPickup();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (Searching), nameof (CmdAbortPickup), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [TargetRpc]
  public void TargetShowWarning(NetworkConnection connection, string category, int max)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(category);
    writer.WritePackedInt32(max);
    this.SendTargetRPCInternal(connection, typeof (Searching), nameof (TargetShowWarning), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  public void AddItem(ItemType id, float dur, int[] mods)
  {
    if (id == ItemType.None)
      return;
    if (mods == null)
      mods = new int[3];
    if (mods.Length != 3)
      Array.Resize<int>(ref mods, 3);
    Item itemById = this.hub.inventory.GetItemByID(id);
    if (!itemById.noEquipable)
    {
      foreach (WeaponManager.Weapon weapon in this.GetComponent<WeaponManager>().weapons)
      {
        if (weapon.inventoryID == id)
        {
          mods[0] = Mathf.Clamp(mods[0], 0, weapon.mod_sights.Length - 1);
          mods[1] = Mathf.Clamp(mods[1], 0, weapon.mod_barrels.Length - 1);
          mods[2] = Mathf.Clamp(mods[2], 0, weapon.mod_others.Length - 1);
        }
      }
      this.hub.inventory.AddNewItem(id, (double) dur == -1.0 ? itemById.durability : dur, mods[0], mods[1], mods[2]);
    }
    else
    {
      string[] strArray = this.hub.ammoBox.amount.Split(':');
      for (ushort index = 0; index < (ushort) 3; ++index)
      {
        if (this.hub.ammoBox.types[(int) index].inventoryID == id)
          strArray[(int) index] = ((float) this.hub.ammoBox.GetAmmo((int) index) + dur).ToString();
      }
      this.hub.ammoBox.Networkamount = strArray[0] + ":" + strArray[1] + ":" + strArray[2];
    }
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeCmdCmdPickupItem(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdPickupItem called on client.");
    else
      ((Searching) obj).CallCmdPickupItem(reader.ReadGameObject());
  }

  protected static void InvokeCmdCmdStartPickup(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdStartPickup called on client.");
    else
      ((Searching) obj).CallCmdStartPickup(reader.ReadGameObject());
  }

  protected static void InvokeCmdCmdAbortPickup(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdAbortPickup called on client.");
    else
      ((Searching) obj).CallCmdAbortPickup();
  }

  public void CallCmdPickupItem(GameObject t)
  {
    if (!this._playerInteractRateLimit.CanExecute(true) || (UnityEngine.Object) t == (UnityEngine.Object) null || (!this.hub.characterClassManager.IsHuman() || (double) Vector3.Distance(this.GetComponent<PlyMovementSync>().RealModelPosition, t.transform.position) > 3.5))
      return;
    Pickup component = t.GetComponent<Pickup>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null || !this._pickupInProgressServer || ((UnityEngine.Object) t != (UnityEngine.Object) this._pickupObjectServer || (double) this._pickupProgressServer > 0.25) || component.info.locked)
      return;
    Item itemById1 = this.hub.inventory.GetItemByID(component.info.itemId);
    if (itemById1.noEquipable)
    {
      for (int type = 0; type < this.hub.ammoBox.types.Length; ++type)
      {
        if (this.hub.ammoBox.types[type].inventoryID == component.info.itemId)
        {
          int ammo = this.hub.ammoBox.GetAmmo(type);
          int num = this.hub.characterClassManager.Classes.SafeGet(this.hub.characterClassManager.CurClass).maxAmmo[type];
          int durability;
          for (durability = (int) component.info.durability; ammo < num && durability > 0; ++ammo)
            --durability;
          Pickup.PickupInfo info = component.info;
          info.durability = (float) durability;
          component.Networkinfo = info;
          if (durability <= 0)
            component.Delete();
          string[] strArray = this.hub.ammoBox.amount.Split(':');
          strArray[type] = ammo.ToString();
          this.hub.ammoBox.Networkamount = strArray[0] + ":" + strArray[1] + ":" + strArray[2];
        }
      }
    }
    else
    {
      ItemCategory itemCategory = itemById1.itemCategory;
      switch (itemCategory)
      {
        case ItemCategory.None:
        case ItemCategory.NoCategory:
          ItemType itemId = component.info.itemId;
          component.Delete();
          if (itemId == ItemType.None)
            break;
          this.AddItem(itemId, (UnityEngine.Object) t.GetComponent<Pickup>() == (UnityEngine.Object) null ? -1f : component.info.durability, component.info.weaponMods);
          break;
        default:
          int num = 0;
          foreach (Inventory.SyncItemInfo syncItemInfo in (SyncList<Inventory.SyncItemInfo>) this.hub.inventory.items)
          {
            Item itemById2 = this.hub.inventory.GetItemByID(syncItemInfo.id);
            if ((itemById2 != null ? (itemById2.itemCategory == itemCategory ? 1 : 0) : 0) != 0)
              ++num;
          }
          foreach (InventoryCategory category in this.categories)
          {
            if (category.itemType == itemCategory)
            {
              if (num >= category.maxItems)
                return;
              if (num == category.maxItems - 1 && !category.hideWarning)
                this.TargetShowWarning(this.connectionToClient, category.label, category.maxItems);
            }
          }
          goto case ItemCategory.None;
      }
    }
  }

  public void CallCmdStartPickup(GameObject t)
  {
    if (!this._playerInteractRateLimit.CanExecute(true) || (UnityEngine.Object) t == (UnityEngine.Object) null || (!this.hub.characterClassManager.IsHuman() || (double) Vector3.Distance(this.GetComponent<PlyMovementSync>().RealModelPosition, t.transform.position) > 3.5))
      return;
    Pickup component = t.GetComponent<Pickup>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      return;
    this._pickupObjectServer = t;
    this._pickupProgressServer = component.searchTime;
    this._pickupInProgressServer = true;
  }

  public void CallCmdAbortPickup()
  {
    if (!this._playerInteractRateLimit.CanExecute(true))
      return;
    this._pickupInProgressServer = false;
    this._pickupObjectServer = (GameObject) null;
    this._pickupProgressServer = 4144959f;
  }

  protected static void InvokeRpcTargetShowWarning(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetShowWarning called on server.");
    else
      ((Searching) obj).CallTargetShowWarning(ClientScene.readyConnection, reader.ReadString(), reader.ReadPackedInt32());
  }

  public void CallTargetShowWarning(NetworkConnection connection, string category, int max)
  {
    if (category == null)
      return;
    string str = "<color=yellow>" + (object) max + "</color>";
    this.ShowErrorMessage(string.Format(TranslationReader.Get("InventoryErrors", 3, "You have now reached the limit of items of this type: <color=yellow>{0}</color> ({1}/{1})"), (object) category, (object) str));
  }

  static Searching()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (Searching), "CmdPickupItem", new NetworkBehaviour.CmdDelegate(Searching.InvokeCmdCmdPickupItem));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Searching), "CmdStartPickup", new NetworkBehaviour.CmdDelegate(Searching.InvokeCmdCmdStartPickup));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Searching), "CmdAbortPickup", new NetworkBehaviour.CmdDelegate(Searching.InvokeCmdCmdAbortPickup));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Searching), "TargetShowWarning", new NetworkBehaviour.CmdDelegate(Searching.InvokeRpcTargetShowWarning));
  }
}
