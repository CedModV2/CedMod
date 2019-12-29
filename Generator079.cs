// Decompiled with JetBrains decompiler
// Type: Generator079
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class Generator079 : NetworkBehaviour
{
  public static List<Generator079> generators = new List<Generator079>();
  public float startDuration = 90f;
  private readonly float[] rotations = new float[6]
  {
    -57f,
    -38f,
    -22f,
    0.0f,
    22f,
    38f
  };
  private Animator anim;
  public Animator tabletAnim;
  [SyncVar]
  public bool isDoorOpen;
  [SyncVar]
  public bool isDoorUnlocked;
  [SyncVar]
  public bool isTabletConnected;
  [SyncVar(hook = "SetTime")]
  public float remainingPowerup;
  private float doorAnimationCooldown;
  private float tabletAnimCooldown;
  private float deniedCooldown;
  private float localTime;
  private bool prevConn;
  private AudioSource asource;
  public MeshRenderer keycardRenderer;
  public MeshRenderer wmtRenderer;
  public MeshRenderer epsenRenderer;
  public MeshRenderer epsdisRenderer;
  public MeshRenderer cancel1Rend;
  public MeshRenderer cancel2Rend;
  public Material matLocked;
  public Material matUnlocked;
  public Material matDenied;
  public Material matLedBlack;
  public Material matLetGreen;
  public Material matLetBlue;
  public Material cancel1MatDis;
  public Material cancel2MatDis;
  public Material cancel1MatEn;
  public Material cancel2MatEn;
  public AudioClip clipOpen;
  public AudioClip clipClose;
  public AudioClip beepSound;
  public AudioClip unlockSound;
  public AudioClip clipConnect;
  public AudioClip clipDisconnect;
  public AudioClip clipCounter;
  public Transform tabletEjectionPoint;
  public Transform localArrow;
  public Transform totalArrow;
  public float localVoltage;
  [SyncVar]
  public int totalVoltage;
  private string _curRoom;
  public static Generator079 mainGenerator;
  public bool forcedOvercharge;
  private string prevMin;
  private bool prevReady;
  private bool prevFinish;
  private bool prevUnlocked;

  public string curRoom
  {
    get
    {
      if (string.IsNullOrEmpty(this._curRoom) && this.transform.localPosition != Vector3.zero)
      {
        RaycastHit hitInfo1;
        Physics.Raycast(new Ray(this.transform.position - this.transform.forward, Vector3.up), out hitInfo1, 5f, (int) Interface079.singleton.roomDetectionMask);
        Transform transform = hitInfo1.transform;
        if (!(bool) (UnityEngine.Object) transform)
        {
          RaycastHit hitInfo2;
          Physics.Raycast(new Ray(this.transform.position - this.transform.forward, Vector3.down), out hitInfo2, 5f, (int) Interface079.singleton.roomDetectionMask);
          transform = hitInfo2.transform;
        }
        if ((bool) (UnityEngine.Object) transform)
        {
          while ((UnityEngine.Object) transform != (UnityEngine.Object) null && !transform.transform.name.Contains("ROOT", StringComparison.OrdinalIgnoreCase) && !(transform.gameObject.tag == "Room"))
            transform = transform.transform.parent;
          if ((UnityEngine.Object) transform != (UnityEngine.Object) null)
            this._curRoom = transform.transform.name;
        }
      }
      return this._curRoom;
    }
  }

  private void SetTime(float time)
  {
    this.NetworkremainingPowerup = time;
    if ((double) Mathf.Abs(time - this.localTime) <= 1.0)
      return;
    this.localTime = time;
  }

  private void Awake()
  {
    if (!this.name.Contains("("))
      Generator079.mainGenerator = this;
    this.asource = this.GetComponent<AudioSource>();
    this.anim = this.GetComponent<Animator>();
    Generator079.generators.Clear();
  }

  private void Start()
  {
    if (NetworkServer.active)
      this.localTime = this.NetworkremainingPowerup = this.startDuration;
    Generator079.generators.Add(this);
  }

  private void Update()
  {
    if ((double) this.tabletAnimCooldown >= -1.0)
      this.tabletAnimCooldown -= Time.deltaTime;
    if (NetworkServer.active && Generator079.mainGenerator.forcedOvercharge)
    {
      if ((double) this.remainingPowerup > -5.0)
        this.NetworkremainingPowerup = -5f;
      if (this.isTabletConnected)
        this.EjectTablet();
      this.NetworkisDoorUnlocked = true;
    }
    this.anim.SetBool("isOpen", this.isDoorOpen);
    this.localArrow.transform.localRotation = Quaternion.Lerp(this.localArrow.transform.localRotation, Quaternion.Euler(0.0f, Mathf.Lerp(-40f, 40f, this.localVoltage), 0.0f), Time.deltaTime * 2f);
    this.totalArrow.transform.localRotation = Quaternion.Lerp(this.totalArrow.transform.localRotation, Quaternion.Euler(0.0f, this.rotations[Mathf.Clamp(Generator079.mainGenerator.totalVoltage, 0, 5)], 0.0f), Time.deltaTime * 2f);
    if ((double) this.doorAnimationCooldown >= 0.0)
      this.doorAnimationCooldown -= Time.deltaTime;
    if ((double) this.deniedCooldown < 0.0)
      return;
    this.deniedCooldown -= Time.deltaTime;
    if ((double) this.deniedCooldown >= 0.0)
      return;
    this.keycardRenderer.sharedMaterial = this.isDoorUnlocked ? this.matUnlocked : this.matLocked;
  }

  private void LateUpdate()
  {
    if ((double) Mathf.Abs(this.localTime - this.remainingPowerup) > 1.29999995231628 || (double) this.remainingPowerup == 0.0)
      this.localTime = this.remainingPowerup;
    if (this.prevConn && (double) this.tabletAnimCooldown <= 0.0 && ((double) this.localTime > 0.0 && !Generator079.mainGenerator.forcedOvercharge))
    {
      if (NetworkServer.active && (double) this.remainingPowerup > 0.0)
      {
        this.NetworkremainingPowerup = this.remainingPowerup - Time.deltaTime;
        if ((double) this.remainingPowerup < 0.0)
          this.NetworkremainingPowerup = 0.0f;
        this.localTime = this.remainingPowerup;
      }
      this.localTime -= Time.deltaTime;
      if ((double) this.localTime < 0.0)
        this.localTime = 0.0f;
    }
    else
    {
      if (NetworkServer.active && this.prevConn && ((double) this.localTime <= 0.0 && this.isTabletConnected) && !Generator079.mainGenerator.forcedOvercharge)
      {
        int curr = Generator079.generators.Count<Generator079>((Func<Generator079, bool>) (gen => (double) gen.localTime <= 0.0));
        this.NetworkremainingPowerup = 0.0f;
        this.localTime = 0.0f;
        this.EjectTablet();
        this.RpcNotify(curr);
        if (curr < 5)
          PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement("JAM_" + UnityEngine.Random.Range(0, 100).ToString("000") + "_" + (object) UnityEngine.Random.Range(2, 5) + " SCP079RECON" + (object) curr, false, true);
        else
          Recontainer079.BeginContainment(false);
        Generator079.mainGenerator.NetworktotalVoltage = curr;
      }
      if (!this.prevConn && NetworkServer.active && ((double) this.tabletAnimCooldown < 0.0 && (double) this.remainingPowerup < (double) this.startDuration - 1.0) && (double) this.remainingPowerup > 0.0)
        this.NetworkremainingPowerup = this.remainingPowerup + Time.deltaTime;
    }
    this.localVoltage = 1f - Mathf.InverseLerp(0.0f, this.startDuration, this.localTime);
    this.CheckTabletConnectionStatus();
    this.CheckFinish();
    this.Unlock();
  }

  public void Interact(GameObject person, string command)
  {
    if (command.StartsWith("EPS_DOOR"))
      this.OpenClose(person);
    else if (command.StartsWith("EPS_TABLET"))
    {
      if (this.isTabletConnected || !this.isDoorOpen || ((double) this.localTime <= 0.0 || Generator079.mainGenerator.forcedOvercharge))
        return;
      Inventory component = person.GetComponent<Inventory>();
      foreach (Inventory.SyncItemInfo syncItemInfo in (SyncList<Inventory.SyncItemInfo>) component.items)
      {
        if (syncItemInfo.id == ItemType.WeaponManagerTablet)
        {
          component.items.Remove(syncItemInfo);
          this.NetworkisTabletConnected = true;
          break;
        }
      }
    }
    else if (command.StartsWith("EPS_CANCEL"))
      this.EjectTablet();
    else
      Debug.LogError((object) ("Unknown command: " + command));
  }

  public void EjectTablet()
  {
    if (!this.isTabletConnected)
      return;
    this.NetworkisTabletConnected = false;
    PlayerManager.localPlayer.GetComponent<Inventory>().SetPickup(ItemType.WeaponManagerTablet, 0.0f, this.tabletEjectionPoint.position, this.tabletEjectionPoint.rotation, 0, 0, 0);
  }

  private void CheckTabletConnectionStatus()
  {
    if (this.prevConn != this.isTabletConnected)
    {
      this.prevConn = this.isTabletConnected;
      this.tabletAnimCooldown = 1f;
      this.tabletAnim.SetBool("b", this.prevConn);
      this.asource.PlayOneShot(this.prevConn ? this.clipConnect : this.clipDisconnect);
    }
    bool flag = this.prevConn && (double) this.tabletAnimCooldown <= 0.0;
    if (this.prevReady == flag)
      return;
    this.prevReady = flag;
    this.wmtRenderer.sharedMaterial = flag ? this.matLetBlue : this.matLedBlack;
    this.cancel1Rend.sharedMaterial = (double) this.localTime <= 0.0 || !this.prevConn || (double) this.tabletAnimCooldown > 0.0 ? this.cancel1MatDis : this.cancel1MatEn;
    this.cancel2Rend.sharedMaterial = (double) this.localTime <= 0.0 || !this.prevConn || (double) this.tabletAnimCooldown > 0.0 ? this.cancel2MatDis : this.cancel2MatEn;
  }

  private void CheckFinish()
  {
    if (this.prevFinish || (double) this.localTime > 0.0)
      return;
    this.prevFinish = true;
    this.epsenRenderer.sharedMaterial = this.matLetGreen;
    this.epsdisRenderer.sharedMaterial = this.matLedBlack;
    this.asource.PlayOneShot(this.unlockSound);
  }

  private void OpenClose(GameObject person)
  {
    Inventory component = person.GetComponent<Inventory>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null || (double) this.doorAnimationCooldown > 0.0 || (double) this.deniedCooldown > 0.0)
      return;
    if (!this.isDoorUnlocked)
    {
      bool flag = person.GetComponent<ServerRoles>().BypassMode;
      if (component.curItem > ItemType.KeycardJanitor)
      {
        foreach (string permission in component.GetItemByID(component.curItem).permissions)
        {
          if (permission == "ARMORY_LVL_2")
            flag = true;
        }
      }
      if (flag)
      {
        this.NetworkisDoorUnlocked = true;
        this.doorAnimationCooldown = 0.5f;
      }
      else
        this.RpcDenied();
    }
    else
    {
      this.doorAnimationCooldown = 1.5f;
      this.NetworkisDoorOpen = !this.isDoorOpen;
      this.RpcDoSound(this.isDoorOpen);
    }
  }

  [ClientRpc]
  private void RpcDenied()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (Generator079), nameof (RpcDenied), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  public void RpcOvercharge()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (Generator079), nameof (RpcOvercharge), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  public void RpcCustomOverchargeForOurBeautifulModCreators(float forceDuration, bool onlyHeavy)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteSingle(forceDuration);
    writer.WriteBoolean(onlyHeavy);
    this.SendRPCInternal(typeof (Generator079), nameof (RpcCustomOverchargeForOurBeautifulModCreators), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void Unlock()
  {
    if (this.prevUnlocked == this.isDoorUnlocked)
      return;
    this.prevUnlocked = true;
    this.asource.PlayOneShot(this.unlockSound);
    this.keycardRenderer.sharedMaterial = this.matUnlocked;
  }

  [ClientRpc]
  private void RpcNotify(int curr)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WritePackedInt32(curr);
    this.SendRPCInternal(typeof (Generator079), nameof (RpcNotify), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  private void RpcDoSound(bool isOpen)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteBoolean(isOpen);
    this.SendRPCInternal(typeof (Generator079), nameof (RpcDoSound), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  static Generator079()
  {
    NetworkBehaviour.RegisterRpcDelegate(typeof (Generator079), "RpcDenied", new NetworkBehaviour.CmdDelegate(Generator079.InvokeRpcRpcDenied));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Generator079), "RpcOvercharge", new NetworkBehaviour.CmdDelegate(Generator079.InvokeRpcRpcOvercharge));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Generator079), "RpcCustomOverchargeForOurBeautifulModCreators", new NetworkBehaviour.CmdDelegate(Generator079.InvokeRpcRpcCustomOverchargeForOurBeautifulModCreators));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Generator079), "RpcNotify", new NetworkBehaviour.CmdDelegate(Generator079.InvokeRpcRpcNotify));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Generator079), "RpcDoSound", new NetworkBehaviour.CmdDelegate(Generator079.InvokeRpcRpcDoSound));
  }

  private void MirrorProcessed()
  {
  }

  public bool NetworkisDoorOpen
  {
    get
    {
      return this.isDoorOpen;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.isDoorOpen, 1UL);
    }
  }

  public bool NetworkisDoorUnlocked
  {
    get
    {
      return this.isDoorUnlocked;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.isDoorUnlocked, 2UL);
    }
  }

  public bool NetworkisTabletConnected
  {
    get
    {
      return this.isTabletConnected;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.isTabletConnected, 4UL);
    }
  }

  public float NetworkremainingPowerup
  {
    get
    {
      return this.remainingPowerup;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(8UL))
      {
        this.setSyncVarHookGuard(8UL, true);
        this.SetTime(value);
        this.setSyncVarHookGuard(8UL, false);
      }
      this.SetSyncVar<float>(value, ref this.remainingPowerup, 8UL);
    }
  }

  public int NetworktotalVoltage
  {
    get
    {
      return this.totalVoltage;
    }
    [param: In] set
    {
      this.SetSyncVar<int>(value, ref this.totalVoltage, 16UL);
    }
  }

  protected static void InvokeRpcRpcDenied(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcDenied called on server.");
    else
      ((Generator079) obj).CallRpcDenied();
  }

  protected static void InvokeRpcRpcOvercharge(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcOvercharge called on server.");
    else
      ((Generator079) obj).CallRpcOvercharge();
  }

  protected static void InvokeRpcRpcCustomOverchargeForOurBeautifulModCreators(
    NetworkBehaviour obj,
    NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcCustomOverchargeForOurBeautifulModCreators called on server.");
    else
      ((Generator079) obj).CallRpcCustomOverchargeForOurBeautifulModCreators(reader.ReadSingle(), reader.ReadBoolean());
  }

  protected static void InvokeRpcRpcNotify(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcNotify called on server.");
    else
      ((Generator079) obj).CallRpcNotify(reader.ReadPackedInt32());
  }

  protected static void InvokeRpcRpcDoSound(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcDoSound called on server.");
    else
      ((Generator079) obj).CallRpcDoSound(reader.ReadBoolean());
  }

  public void CallRpcDenied()
  {
    this.deniedCooldown = 0.5f;
  }

  public void CallRpcOvercharge()
  {
    foreach (FlickerableLight flickerableLight in UnityEngine.Object.FindObjectsOfType<FlickerableLight>())
    {
      Scp079Interactable component = flickerableLight.GetComponent<Scp079Interactable>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null || component.currentZonesAndRooms[0].currentZone == "HeavyRooms")
        flickerableLight.EnableFlickering(10f);
    }
  }

  public void CallRpcCustomOverchargeForOurBeautifulModCreators(float forceDuration, bool onlyHeavy)
  {
    foreach (FlickerableLight flickerableLight in UnityEngine.Object.FindObjectsOfType<FlickerableLight>())
    {
      Scp079Interactable component = flickerableLight.GetComponent<Scp079Interactable>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null || !onlyHeavy || component.currentZonesAndRooms[0].currentZone == "HeavyRooms")
        flickerableLight.EnableFlickering(forceDuration);
    }
  }

  public void CallRpcNotify(int curr)
  {
  }

  public void CallRpcDoSound(bool isOpen)
  {
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteBoolean(this.isDoorOpen);
      writer.WriteBoolean(this.isDoorUnlocked);
      writer.WriteBoolean(this.isTabletConnected);
      writer.WriteSingle(this.remainingPowerup);
      writer.WritePackedInt32(this.totalVoltage);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteBoolean(this.isDoorOpen);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WriteBoolean(this.isDoorUnlocked);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 4L) != 0L)
    {
      writer.WriteBoolean(this.isTabletConnected);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 8L) != 0L)
    {
      writer.WriteSingle(this.remainingPowerup);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 16L) != 0L)
    {
      writer.WritePackedInt32(this.totalVoltage);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworkisDoorOpen = reader.ReadBoolean();
      this.NetworkisDoorUnlocked = reader.ReadBoolean();
      this.NetworkisTabletConnected = reader.ReadBoolean();
      float time = reader.ReadSingle();
      this.SetTime(time);
      this.NetworkremainingPowerup = time;
      this.NetworktotalVoltage = reader.ReadPackedInt32();
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
        this.NetworkisDoorOpen = reader.ReadBoolean();
      if ((num & 2L) != 0L)
        this.NetworkisDoorUnlocked = reader.ReadBoolean();
      if ((num & 4L) != 0L)
        this.NetworkisTabletConnected = reader.ReadBoolean();
      if ((num & 8L) != 0L)
      {
        float time = reader.ReadSingle();
        this.SetTime(time);
        this.NetworkremainingPowerup = time;
      }
      if ((num & 16L) == 0L)
        return;
      this.NetworktotalVoltage = reader.ReadPackedInt32();
    }
  }
}
