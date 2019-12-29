// Decompiled with JetBrains decompiler
// Type: Door
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Assets._Scripts.RemoteAdmin;
using MEC;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Door : NetworkBehaviour, IComparable
{
  public int status = -1;
  [HideInInspector]
  public List<GameObject> buttons = new List<GameObject>();
  public AudioSource soundsource;
  public AudioClip sound_checkpointWarning;
  public AudioClip sound_denied;
  public MovingStatus moving;
  public GameObject destroyedPrefab;
  public Vector3 localPos;
  public Quaternion localRot;
  internal DoorRemoteAdminButton RemoteAdminButton;
  private SECTR_Portal _portal;
  public Animator[] parts;
  public AudioClip[] sound_open;
  public AudioClip[] sound_close;
  private Rigidbody[] _destoryedRb;
  public int doorType;
  public float curCooldown;
  public float cooldown;
  public bool dontOpenOnWarhead;
  public bool blockAfterDetonation;
  public bool lockdown;
  public bool warheadlock;
  public bool commandlock;
  public bool decontlock;
  public bool GrenadesResistant;
  private bool _buffedStatus;
  private bool _wasLocked;
  private bool _prevDestroyed;
  private bool _deniedInProgress;
  public float scp079Lockdown;
  private bool isLockedBy079;
  public string DoorName;
  public string permissionLevel;
  [SyncVar(hook = "DestroyDoor")]
  public bool destroyed;
  [SyncVar(hook = "SetState")]
  public bool isOpen;
  [SyncVar(hook = "SetLock")]
  public bool locked;

  private void Start()
  {
    this.scp079Lockdown = -3f;
    Timing.RunCoroutine(this._Start(), Segment.FixedUpdate);
  }

  private void Update079Lock()
  {
    bool flag = false;
    this.GetComponent<NetworkIdentity>();
    string str = this.transform.parent.name + "/" + this.transform.name;
    foreach (Scp079PlayerScript instance in Scp079PlayerScript.instances)
    {
      foreach (string lockedDoor in (SyncList<string>) instance.lockedDoors)
      {
        if (lockedDoor == str)
          flag = true;
      }
    }
    if (flag == this.isLockedBy079)
      return;
    this.isLockedBy079 = flag;
    this.UpdateLock();
  }

  public void LockBy079()
  {
    this.isLockedBy079 = true;
    this.UpdateLock();
  }

  private void LateUpdate()
  {
    if (this.isLockedBy079)
      this.Update079Lock();
    if (this._prevDestroyed != this.destroyed)
    {
      GameObject gameObject = GameObject.Find("Host");
      if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null && gameObject.GetComponent<RandomSeedSync>().mapGenerated)
        this.StartCoroutine(this.RefreshDestroyAnimation());
    }
    if ((double) this.curCooldown >= 0.0)
      this.curCooldown -= Time.deltaTime;
    if (NetworkServer.active && (double) this.scp079Lockdown >= -3.0)
    {
      this.scp079Lockdown -= Time.deltaTime;
      this.UpdateLock();
    }
    if (!this._deniedInProgress && (!this.locked || this.permissionLevel == "UNACCESSIBLE"))
    {
      if ((double) this.curCooldown >= 0.0 && this.status != 3)
      {
        if ((UnityEngine.Object) this.sound_checkpointWarning == (UnityEngine.Object) null)
        {
          if ((UnityEngine.Object) this._portal != (UnityEngine.Object) null)
            this._portal.Flags = (SECTR_Portal.PortalFlags) 0;
          this.SetActiveStatus(2);
        }
      }
      else
      {
        if ((UnityEngine.Object) this._portal != (UnityEngine.Object) null)
          this._portal.Flags = this.isOpen | this.destroyed ? (SECTR_Portal.PortalFlags) 0 : SECTR_Portal.PortalFlags.Closed;
        this.SetActiveStatus(this.isOpen ? 1 : 0);
      }
    }
    if (this.locked && this.permissionLevel != "UNACCESSIBLE")
    {
      if ((UnityEngine.Object) this._portal != (UnityEngine.Object) null)
        this._portal.Flags = this.isOpen | this.destroyed | this.moving.moving ? (SECTR_Portal.PortalFlags) 0 : SECTR_Portal.PortalFlags.Closed;
      if (this._wasLocked)
        return;
      this._wasLocked = true;
      this.SetActiveStatus(4);
    }
    else
    {
      if (!this._wasLocked)
        return;
      this._wasLocked = false;
      if (this.doorType != 3)
        return;
      this.SetState(false);
      if (!NetworkServer.active)
        return;
      this.RpcDoSound();
    }
  }

  public int CompareTo(object obj)
  {
    return string.CompareOrdinal(this.DoorName, ((Door) obj).DoorName);
  }

  private void SetLock(bool l)
  {
    this.Networklocked = l;
    if (!((UnityEngine.Object) this.RemoteAdminButton != (UnityEngine.Object) null))
      return;
    this.RemoteAdminButton.UpdateColor();
  }

  public void UpdateLock()
  {
    this.Networklocked = this.permissionLevel != "UNACCESSIBLE" && this.commandlock | this.lockdown | this.warheadlock | this.decontlock | (double) this.scp079Lockdown > 0.0 | this.isLockedBy079;
  }

  public void SetPortal(SECTR_Portal p)
  {
    this._portal = p;
  }

  public void SetLocalPos()
  {
    this.localPos = this.transform.localPosition;
    this.localRot = this.transform.localRotation;
  }

  private IEnumerator<float> _UpdatePosition()
  {
    foreach (Animator part in this.parts)
      part.SetBool("isOpen", this.isOpen);
    if (!((UnityEngine.Object) this.sound_checkpointWarning == (UnityEngine.Object) null) && this.isOpen)
    {
      this._deniedInProgress = true;
      this.moving.moving = true;
      if (!this.locked)
        this.SetActiveStatus(2);
      float t = 0.0f;
      byte i;
      while ((double) t < 5.0)
      {
        t += 0.1f;
        for (i = (byte) 0; i < (byte) 5; ++i)
          yield return 0.0f;
        if ((double) this.curCooldown < 0.0 && !this.locked)
          this.SetActiveStatus(1);
      }
      if (this.locked)
      {
        this.moving.moving = false;
        this._deniedInProgress = false;
      }
      else
      {
        this.soundsource.PlayOneShot(this.sound_checkpointWarning);
        this.SetActiveStatus(5);
        for (i = (byte) 0; i < (byte) 100; ++i)
          yield return 0.0f;
        this.SetActiveStatus(0);
        this.moving.moving = false;
        this._deniedInProgress = false;
        this.SetState(false);
      }
    }
  }

  public void SetState(bool open)
  {
    this.NetworkisOpen = open;
    this.ForceCooldown(this.cooldown);
  }

  public void SetStateWithSound(bool open)
  {
    if (this.isOpen != open)
      this.RpcDoSound();
    this.NetworkisOpen = open;
    this.ForceCooldown(this.cooldown);
  }

  public void DestroyDoor(bool b)
  {
    this.Networkdestroyed = b && (UnityEngine.Object) this.destroyedPrefab != (UnityEngine.Object) null;
    if (!((UnityEngine.Object) this.RemoteAdminButton != (UnityEngine.Object) null))
      return;
    this.RemoteAdminButton.UpdateColor();
  }

  private IEnumerator RefreshDestroyAnimation()
  {
    Door door = this;
    foreach (Animator part in door.parts)
    {
      if (part.gameObject.activeSelf)
      {
        part.gameObject.SetActive(false);
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(door.destroyedPrefab, part.transform);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        gameObject.transform.localScale = Vector3.one;
        gameObject.transform.parent = (Transform) null;
        int num = 0;
        door._destoryedRb = gameObject.GetComponentsInChildren<Rigidbody>();
        Vector3 vector3_1 = (UnityEngine.Object) door._portal == (UnityEngine.Object) null ? Vector3.one : door._portal.GetRandomSectorPos();
        foreach (Rigidbody rigidbody in door._destoryedRb)
        {
          rigidbody.GetComponent<Collider>().isTrigger = true;
          rigidbody.transform.parent = (Transform) null;
          Vector3 vector3_2 = vector3_1 - door.transform.position;
          vector3_2.y = 0.0f;
          vector3_2 = vector3_2.normalized;
          rigidbody.velocity = (num == 1 || num == 2 ? -vector3_2 : vector3_2) * (float) UnityEngine.Random.Range(7, 9);
          ++num;
        }
      }
    }
    yield return (object) new WaitForSeconds(0.15f);
    foreach (Component component in door._destoryedRb)
      component.GetComponent<Collider>().isTrigger = false;
    yield return (object) new WaitForSeconds(5f);
    foreach (Rigidbody rigidbody in door._destoryedRb)
    {
      rigidbody.isKinematic = true;
      rigidbody.GetComponent<Collider>().enabled = false;
    }
  }

  private IEnumerator<float> _Start()
  {
    Door door = this;
    foreach (Renderer componentsInChild in door.GetComponentsInChildren(typeof (Renderer)))
    {
      if (componentsInChild.CompareTag("DoorButton"))
        door.buttons.Add(componentsInChild.gameObject);
    }
    door.SetActiveStatus(0);
    float time = 0.0f;
    while ((double) time < 10.0)
    {
      time += 0.02f;
      if (door._buffedStatus != door.isOpen)
      {
        door._buffedStatus = door.isOpen;
        door.ForceCooldown(door.cooldown);
        break;
      }
      yield return 0.0f;
    }
  }

  public void UpdatePos()
  {
    if (this.localPos == Vector3.zero)
      return;
    this.transform.localPosition = this.localPos;
    this.transform.localRotation = this.localRot;
  }

  public void SetZero()
  {
    this.localPos = Vector3.zero;
  }

  public bool ChangeState(bool force = false)
  {
    if ((double) this.curCooldown >= 0.0 || this.moving.moving || this._deniedInProgress || (this.locked && !force || Recontainer079.isLocked && this.GetComponent<Scp079Interactable>().currentZonesAndRooms[0].currentZone == "HeavyRooms"))
      return false;
    this.moving.moving = true;
    this.SetState(!this.isOpen);
    this.RpcDoSound();
    return true;
  }

  public bool ChangeState079()
  {
    if ((double) this.curCooldown >= 0.0 || this.moving.moving || this._deniedInProgress || this.permissionLevel != "UNACCESSIBLE" && this.commandlock | this.lockdown | this.warheadlock | this.decontlock)
      return false;
    this.moving.moving = true;
    this.SetState(!this.isOpen);
    this.RpcDoSound();
    return true;
  }

  public void OpenDecontamination()
  {
    if (this.permissionLevel == "UNACCESSIBLE")
      return;
    this.decontlock = true;
    if (!this.isOpen)
      this.RpcDoSound();
    this.moving.moving = true;
    this.SetState(true);
    this.UpdateLock();
  }

  public void CloseDecontamination()
  {
    if (this.permissionLevel == "UNACCESSIBLE" || (double) this.transform.position.y < -100.0 || (double) this.transform.position.y > 100.0)
      return;
    this.decontlock = true;
    if (this.isOpen)
      this.RpcDoSound();
    this.moving.moving = true;
    this.SetState(false);
    this.UpdateLock();
  }

  public void OpenWarhead(bool force, bool lockDoor)
  {
    if (this.permissionLevel == "UNACCESSIBLE" || this.dontOpenOnWarhead && !force)
      return;
    if (lockDoor)
      this.warheadlock = true;
    if (this.locked && !force || !force && this.permissionLevel == "CONT_LVL_3")
      return;
    if (!this.isOpen)
      this.RpcDoSound();
    this.moving.moving = true;
    this.SetState(true);
    this.UpdateLock();
  }

  [ClientRpc(channel = 5)]
  public void RpcDoSound()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (Door), nameof (RpcDoSound), writer, 5);
    NetworkWriterPool.Recycle(writer);
  }

  public void SetActiveStatus(int s)
  {
    if (this.status == s)
      return;
    this.status = s;
  }

  public void ForceCooldown(float cd)
  {
    this.curCooldown = cd;
    Timing.RunCoroutine(this._UpdatePosition(), Segment.FixedUpdate);
  }

  private void MirrorProcessed()
  {
  }

  public bool Networkdestroyed
  {
    get
    {
      return this.destroyed;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(1UL))
      {
        this.setSyncVarHookGuard(1UL, true);
        this.DestroyDoor(value);
        this.setSyncVarHookGuard(1UL, false);
      }
      this.SetSyncVar<bool>(value, ref this.destroyed, 1UL);
    }
  }

  public bool NetworkisOpen
  {
    get
    {
      return this.isOpen;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(2UL))
      {
        this.setSyncVarHookGuard(2UL, true);
        this.SetState(value);
        this.setSyncVarHookGuard(2UL, false);
      }
      this.SetSyncVar<bool>(value, ref this.isOpen, 2UL);
    }
  }

  public bool Networklocked
  {
    get
    {
      return this.locked;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(4UL))
      {
        this.setSyncVarHookGuard(4UL, true);
        this.SetLock(value);
        this.setSyncVarHookGuard(4UL, false);
      }
      this.SetSyncVar<bool>(value, ref this.locked, 4UL);
    }
  }

  protected static void InvokeRpcRpcDoSound(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcDoSound called on server.");
    else
      ((Door) obj).CallRpcDoSound();
  }

  public void CallRpcDoSound()
  {
  }

  static Door()
  {
    NetworkBehaviour.RegisterRpcDelegate(typeof (Door), "RpcDoSound", new NetworkBehaviour.CmdDelegate(Door.InvokeRpcRpcDoSound));
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteBoolean(this.destroyed);
      writer.WriteBoolean(this.isOpen);
      writer.WriteBoolean(this.locked);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteBoolean(this.destroyed);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WriteBoolean(this.isOpen);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 4L) != 0L)
    {
      writer.WriteBoolean(this.locked);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      bool b = reader.ReadBoolean();
      this.DestroyDoor(b);
      this.Networkdestroyed = b;
      bool open = reader.ReadBoolean();
      this.SetState(open);
      this.NetworkisOpen = open;
      bool l = reader.ReadBoolean();
      this.SetLock(l);
      this.Networklocked = l;
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
      {
        bool b = reader.ReadBoolean();
        this.DestroyDoor(b);
        this.Networkdestroyed = b;
      }
      if ((num & 2L) != 0L)
      {
        bool open = reader.ReadBoolean();
        this.SetState(open);
        this.NetworkisOpen = open;
      }
      if ((num & 4L) == 0L)
        return;
      bool l = reader.ReadBoolean();
      this.SetLock(l);
      this.Networklocked = l;
    }
  }
}
