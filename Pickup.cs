// Decompiled with JetBrains decompiler
// Type: Pickup
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof (Rigidbody))]
public class Pickup : NetworkBehaviour
{
  private ItemType previousId = ItemType.None;
  [NonSerialized]
  public Rigidbody Rb;
  public float searchTime;
  [SyncVar]
  public Pickup.PickupInfo info;
  public static Inventory Inv;
  public static List<Pickup> Instances;
  private GameObject model;

  public ItemType ItemId
  {
    get
    {
      return this.info.itemId;
    }
    set
    {
      Pickup.PickupInfo info = this.info;
      info.itemId = value;
      this.Networkinfo = info;
    }
  }

  public void SetIDFull(ItemType id)
  {
    this.ItemId = id;
    this.RefreshDurability(false, false);
  }

  public void SetupPickup(Pickup.PickupInfo pickupInfo)
  {
    this.Networkinfo = pickupInfo;
    this.transform.position = this.info.position;
    this.transform.rotation = this.info.rotation;
    this.RefreshModel();
    this.UpdatePosition();
  }

  [ServerCallback]
  private void UpdatePosition()
  {
    if (!NetworkServer.active || this.info.position == this.transform.position && this.info.rotation == this.transform.rotation)
      return;
    Pickup.PickupInfo info = this.info;
    info.position = this.transform.position;
    info.rotation = this.transform.rotation;
    this.Networkinfo = info;
  }

  public void CheckForRefresh()
  {
    this.UpdatePosition();
    if (this.previousId == this.info.itemId && (UnityEngine.Object) this.model != (UnityEngine.Object) null)
      return;
    this.previousId = this.info.itemId;
    this.RefreshModel();
  }

  private void LateUpdate()
  {
    if (!((UnityEngine.Object) this.model == (UnityEngine.Object) null) || Pickup.Instances.Contains(this))
      return;
    Pickup.Instances.Add(this);
    Inventory.CollectionModified = true;
  }

  private void RefreshModel()
  {
    if ((UnityEngine.Object) this.model != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.model.gameObject);
    Item itemById = Pickup.Inv.GetItemByID(this.info.itemId);
    this.model = UnityEngine.Object.Instantiate<GameObject>(itemById.prefab, this.transform);
    this.model.transform.localPosition = Vector3.zero;
    this.searchTime = itemById.pickingtime;
    Transform transform = this.transform;
    transform.position = this.info.position;
    transform.rotation = this.info.rotation;
    PlayerManager.localPlayer.GetComponent<WeaponManager>();
  }

  public void Delete()
  {
    NetworkServer.Destroy(this.gameObject);
  }

  private void Awake()
  {
    Inventory.CollectionModified = true;
    if (Pickup.Instances == null)
      Pickup.Instances = new List<Pickup>();
    while (Pickup.Instances.Contains((Pickup) null))
      Pickup.Instances.Remove((Pickup) null);
    Pickup.Instances.Add(this);
  }

  private IEnumerator Start()
  {
    Pickup pickup = this;
    pickup.Rb = pickup.GetComponent<Rigidbody>();
    if (!NetworkServer.active)
      pickup.Rb.isKinematic = true;
    yield return (object) new WaitForEndOfFrame();
    Inventory.CollectionModified = true;
    if (!Pickup.Instances.Contains(pickup))
      Pickup.Instances.Add(pickup);
  }

  private void OnDestroy()
  {
    if (Pickup.Instances == null)
      return;
    Pickup.Instances.Remove(this);
    Inventory.CollectionModified = true;
  }

  public void RefreshDurability(bool allowAmmoRenew = false, bool setupAttachments = false)
  {
    Item itemById = Pickup.Inv.GetItemByID(this.info.itemId);
    if (!itemById.noEquipable | allowAmmoRenew)
      this.info.durability = itemById.durability;
    if (!setupAttachments)
      return;
    foreach (WeaponManager.Weapon weapon in Pickup.Inv.GetComponent<WeaponManager>().weapons)
    {
      if (weapon.inventoryID == this.info.itemId)
      {
        try
        {
          this.info.weaponMods = new int[3];
          this.info.weaponMods[0] = Mathf.Max(0, UnityEngine.Random.Range(-weapon.mod_sights.Length / 2, weapon.mod_sights.Length));
          this.info.weaponMods[1] = Mathf.Max(0, UnityEngine.Random.Range(-weapon.mod_barrels.Length / 2, weapon.mod_barrels.Length));
          this.info.weaponMods[2] = Mathf.Max(0, UnityEngine.Random.Range(-weapon.mod_others.Length / 2, weapon.mod_others.Length));
        }
        catch (Exception ex)
        {
          Debug.Log((object) ex.StackTrace);
        }
      }
    }
  }

  private void MirrorProcessed()
  {
  }

  public Pickup.PickupInfo Networkinfo
  {
    get
    {
      return this.info;
    }
    [param: In] set
    {
      this.SetSyncVar<Pickup.PickupInfo>(value, ref this.info, 1UL);
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      GeneratedNetworkCode._WritePickupInfo_Pickup(writer, this.info);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      GeneratedNetworkCode._WritePickupInfo_Pickup(writer, this.info);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.Networkinfo = GeneratedNetworkCode._ReadPickupInfo_Pickup(reader);
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      this.Networkinfo = GeneratedNetworkCode._ReadPickupInfo_Pickup(reader);
    }
  }

  [Serializable]
  public struct PickupInfo : IEquatable<Pickup.PickupInfo>
  {
    public Vector3 position;
    public Quaternion rotation;
    public ItemType itemId;
    public float durability;
    public GameObject ownerPlayer;
    public int[] weaponMods;
    public bool locked;

    public bool Equals(Pickup.PickupInfo other)
    {
      return this.position == other.position && this.rotation == other.rotation && (this.itemId == other.itemId && (double) Math.Abs(this.durability - other.durability) < 0.00999999977648258) && ((UnityEngine.Object) this.ownerPlayer == (UnityEngine.Object) other.ownerPlayer && this.weaponMods == other.weaponMods) && this.locked == other.locked;
    }

    public override bool Equals(object obj)
    {
      return obj is Pickup.PickupInfo other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return ((((int) ((ItemType) ((this.position.GetHashCode() * 397 ^ this.rotation.GetHashCode()) * 397) ^ this.itemId) * 397 ^ this.durability.GetHashCode()) * 397 ^ ((UnityEngine.Object) this.ownerPlayer != (UnityEngine.Object) null ? this.ownerPlayer.GetHashCode() : 0)) * 397 ^ (this.weaponMods != null ? this.weaponMods.GetHashCode() : 0)) * 397 ^ this.locked.GetHashCode();
    }

    public static bool operator ==(Pickup.PickupInfo left, Pickup.PickupInfo right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(Pickup.PickupInfo left, Pickup.PickupInfo right)
    {
      return !left.Equals(right);
    }
  }
}
