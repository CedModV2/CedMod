// Decompiled with JetBrains decompiler
// Type: WorkStation
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class WorkStation : NetworkBehaviour
{
  private string currentGroup = "unconnected";
  public float maxDistance = 3f;
  private static WorkStation updateRoot;
  private float animationCooldown;
  public Animator animator;
  public AudioClip beepClip;
  [SyncVar]
  public bool isTabletConnected;
  private Transform localply;
  private MeshRenderer[] meshRenderers;
  [SyncVar]
  private GameObject playerConnected;
  [SyncVar]
  public Offset position;
  public AudioClip powerOnClip;
  public AudioClip powerOffClip;
  private bool prevConn;
  private int prevGun;
  public WorkStation.WorkStationScreenGroup screenGroup;
  public GameObject ui_place;
  public GameObject ui_take;
  public GameObject ui_using;
  public GameObject ui_notablet;
  public GameObject laserDot;
  private uint ___playerConnectedNetId;

  private void Start()
  {
    WorkStation.updateRoot = this;
    this.Invoke("UnmuteSource", 10f);
    this.meshRenderers = this.GetComponentsInChildren<MeshRenderer>(true);
  }

  private void UnmuteSource()
  {
    this.GetComponent<AudioSource>().mute = false;
  }

  private void Update()
  {
    if (this.transform.localPosition != this.position.position)
    {
      this.transform.localPosition = this.position.position;
      this.transform.localRotation = Quaternion.Euler(this.position.rotation);
    }
    this.CheckConnectionChange();
    this.screenGroup.SetScreenByName(this.currentGroup);
    if ((double) this.animationCooldown < 0.0)
      return;
    this.animationCooldown -= Time.deltaTime;
  }

  private void CheckConnectionChange()
  {
    if (this.prevConn == this.isTabletConnected)
      return;
    this.prevConn = this.isTabletConnected;
    Timing.RunCoroutine(this.prevConn ? this._OnTabletConnected() : this._OnTabletDisconnected(), Segment.FixedUpdate);
  }

  private IEnumerator<float> _OnTabletConnected()
  {
    WorkStation workStation = this;
    workStation.GetComponent<AudioSource>().PlayOneShot(workStation.powerOnClip);
    workStation.animationCooldown = 6.5f;
    workStation.animator.SetBool("Connected", true);
    for (ushort i = 0; i < (ushort) 50; ++i)
      yield return float.NegativeInfinity;
    workStation.currentGroup = "connecting";
    while ((double) workStation.animationCooldown > 0.0)
      yield return float.NegativeInfinity;
    workStation.currentGroup = "mainmenu";
  }

  private IEnumerator<float> _OnTabletDisconnected()
  {
    WorkStation workStation = this;
    workStation.GetComponent<AudioSource>().PlayOneShot(workStation.powerOffClip);
    workStation.animationCooldown = 3.5f;
    workStation.animator.SetBool("Connected", false);
    workStation.currentGroup = "closingsession";
    while ((double) workStation.animationCooldown > 0.0)
      yield return float.NegativeInfinity;
    workStation.currentGroup = "unconnected";
  }

  public bool CanPlace(GameObject tabletOwner)
  {
    if (this.isTabletConnected || (UnityEngine.Object) this.NetworkplayerConnected != (UnityEngine.Object) null)
      return false;
    CharacterClassManager component = tabletOwner.GetComponent<CharacterClassManager>();
    return (!((UnityEngine.Object) component != (UnityEngine.Object) null) || component.Classes.SafeGet(component.CurClass).team != Team.SCP) && this.HasInInventory(tabletOwner);
  }

  private bool HasInInventory(GameObject tabletOwner)
  {
    return tabletOwner.GetComponent<Inventory>().items.Any<Inventory.SyncItemInfo>((Func<Inventory.SyncItemInfo, bool>) (item => item.id == ItemType.WeaponManagerTablet));
  }

  public bool CanTake(GameObject taker)
  {
    if (!this.isTabletConnected)
      return false;
    CharacterClassManager component = taker.GetComponent<CharacterClassManager>();
    return (!((UnityEngine.Object) taker != (UnityEngine.Object) this.NetworkplayerConnected) || !((UnityEngine.Object) this.NetworkplayerConnected != (UnityEngine.Object) null) || (double) Vector3.Distance(this.NetworkplayerConnected.transform.position, this.transform.position) >= 10.0) && (!((UnityEngine.Object) component != (UnityEngine.Object) null) || component.Classes.SafeGet(component.CurClass).team != Team.SCP) && taker.GetComponent<Inventory>().items.Count < 8;
  }

  public void UnconnectTablet(GameObject taker)
  {
    if (!this.CanTake(taker) || (double) this.animationCooldown > 0.0)
      return;
    taker.GetComponent<Inventory>().AddNewItem(ItemType.WeaponManagerTablet, -4.656647E+11f, 0, 0, 0);
    this.NetworkplayerConnected = (GameObject) null;
    this.NetworkisTabletConnected = false;
    this.animationCooldown = 3.5f;
  }

  public void ConnectTablet(GameObject tabletOwner)
  {
    if (!this.CanPlace(tabletOwner) || (double) this.animationCooldown > 0.0)
      return;
    Inventory component = tabletOwner.GetComponent<Inventory>();
    foreach (Inventory.SyncItemInfo syncItemInfo in (SyncList<Inventory.SyncItemInfo>) component.items)
    {
      if (syncItemInfo.id == ItemType.WeaponManagerTablet)
      {
        component.items.Remove(syncItemInfo);
        this.NetworkisTabletConnected = true;
        this.animationCooldown = 6.5f;
        this.NetworkplayerConnected = tabletOwner;
        break;
      }
    }
  }

  private void MirrorProcessed()
  {
  }

  public bool NetworkisTabletConnected
  {
    get
    {
      return this.isTabletConnected;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.isTabletConnected, 1UL);
    }
  }

  public GameObject NetworkplayerConnected
  {
    get
    {
      return this.GetSyncVarGameObject(this.___playerConnectedNetId, ref this.playerConnected);
    }
    [param: In] set
    {
      this.SetSyncVarGameObject(value, ref this.playerConnected, 2UL, ref this.___playerConnectedNetId);
    }
  }

  public Offset Networkposition
  {
    get
    {
      return this.position;
    }
    [param: In] set
    {
      this.SetSyncVar<Offset>(value, ref this.position, 4UL);
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteBoolean(this.isTabletConnected);
      writer.WriteGameObject(this.NetworkplayerConnected);
      GeneratedNetworkCode._WriteOffset_None(writer, this.position);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteBoolean(this.isTabletConnected);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WriteGameObject(this.NetworkplayerConnected);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 4L) != 0L)
    {
      GeneratedNetworkCode._WriteOffset_None(writer, this.position);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworkisTabletConnected = reader.ReadBoolean();
      this.___playerConnectedNetId = reader.ReadPackedUInt32();
      this.Networkposition = GeneratedNetworkCode._ReadOffset_None(reader);
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
        this.NetworkisTabletConnected = reader.ReadBoolean();
      if ((num & 2L) != 0L)
        this.___playerConnectedNetId = reader.ReadPackedUInt32();
      if ((num & 4L) == 0L)
        return;
      this.Networkposition = GeneratedNetworkCode._ReadOffset_None(reader);
    }
  }

  [Serializable]
  public class WorkStationScreenGroup
  {
    private string curScreen;
    public WorkStation.WorkStationScreenGroup.WorkStationScreen[] screens;
    private WorkStation station;

    public void SetWorkstation(WorkStation s)
    {
      this.station = s;
    }

    public void SetScreenByName(string _label)
    {
      if (this.curScreen == _label)
        return;
      this.curScreen = _label;
      foreach (WorkStation.WorkStationScreenGroup.WorkStationScreen screen in this.screens)
        screen.screenObject.SetActive(screen.label == _label);
    }

    [Serializable]
    public class WorkStationScreen
    {
      public string label;
      public GameObject screenObject;
    }
  }
}
