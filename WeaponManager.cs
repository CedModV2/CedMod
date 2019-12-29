// Decompiled with JetBrains decompiler
// Type: WeaponManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using CustomPlayerEffects;
using Mirror;
using Security;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

public class WeaponManager : NetworkBehaviour
{
  [SyncVar]
  public int curWeapon = -1;
  public float normalFov = 70f;
  public float overallDamagerFactor = 1.65f;
  private readonly RateLimit _placeDecalRateLimit = new RateLimit(100, 2.2f, (NetworkConnection) null);
  private int prevSyncGun = -1;
  public bool flashlightEnabled = true;
  private bool firstSet = true;
  private int decalDuration = 4;
  [SyncVar]
  public bool friendlyFire;
  [SyncVar]
  public bool syncZoomed;
  private ReferenceHub hub;
  private BloodDrawer drawer;
  private WeaponShootAnimation weaponShootAnimation;
  private FirstPersonController fpc;
  private KeyCode kc_fire;
  private KeyCode kc_reload;
  private KeyCode kc_zoom;
  private float fireCooldown;
  private float reloadCooldown;
  private float zoomCooldown;
  private Light globalLightSource;
  [HideInInspector]
  public Scp268 scp268;
  private int _reloadingWeapon;
  public Transform camera;
  public Transform weaponInventoryGroup;
  public Camera weaponModelCamera;
  public float fovAdjustingSpeed;
  public bool zoomed;
  public AnimationCurve viewBob;
  public LayerMask raycastMask;
  public LayerMask raycastServerMask;
  public LayerMask bloodMask;
  public HitboxIdentity[] hitboxes;
  public WeaponManager.Weapon[] weapons;
  public WeaponLaser laserLight;
  public Transform globalLight;
  public int[,] modPreferences;
  public AudioClip noAmmoClip;
  public bool noAmmoPlayed;
  private RateLimit _modRateLimit;
  private RateLimit _iawRateLimit;
  private bool prevFlash;
  [SyncVar]
  public bool syncFlash;
  public bool forceSyncModsNextFrame;

  private void Start()
  {
    if (NetworkServer.active)
    {
      this._modRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[1];
      this._iawRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[3];
    }
    this.hub = ReferenceHub.GetHub(this.gameObject);
    this.scp268 = this.GetComponent<PlayerEffectsController>().GetEffect<Scp268>("SCP-268");
    this.fpc = this.GetComponent<FirstPersonController>();
    this.weaponShootAnimation = this.GetComponentInChildren<WeaponShootAnimation>();
    this.drawer = this.GetComponent<BloodDrawer>();
    this.NetworkfriendlyFire = ServerConsole.FriendlyFire;
    this.kc_fire = NewInput.GetKey("Shoot");
    this.kc_reload = NewInput.GetKey("Reload");
    this.kc_zoom = NewInput.GetKey("Zoom");
    this.globalLightSource = this.globalLight.GetComponentInChildren<Light>();
    this._reloadingWeapon = -100;
    if (this.isLocalPlayer)
    {
      string info = string.Empty;
      for (int index1 = 0; index1 < this.weapons.Length; ++index1)
      {
        for (byte index2 = 0; index2 < (byte) 3; ++index2)
          info = info + (object) PlayerPrefsSl.Get("W_" + (object) index1 + "_" + (object) index2, 0) + ":";
        info += "#";
      }
      this.CmdChangeModPreferences(info);
    }
    else
      UnityEngine.Object.Destroy((UnityEngine.Object) this.weaponModelCamera.gameObject);
  }

  public bool IsReloading()
  {
    return (double) this.reloadCooldown > 0.0;
  }

  private void Update()
  {
    this.DeductCooldown();
    if (!this.isLocalPlayer)
      return;
    this.DoLaserStuff();
    this.UpdateFov();
  }

  private void LateUpdate()
  {
    if (this.isLocalPlayer && !Cursor.visible && (this.curWeapon >= 0 && Input.GetKeyDown(NewInput.GetKey("Toggle flashlight"))) && this.weapons[this.curWeapon].mod_others[this.hub.inventory.GetItemInHand().modOther].name.Contains("flashlight", StringComparison.OrdinalIgnoreCase))
      this.flashlightEnabled = !this.flashlightEnabled;
    this.UpdateGlobalLight();
    this.RefreshMods();
  }

  private void RefreshMods()
  {
    if (this.curWeapon < 0)
    {
      this.prevSyncGun = -1;
    }
    else
    {
      bool flag = false;
      Inventory.SyncItemInfo itemInHand = this.hub.inventory.GetItemInHand();
      if (this.curWeapon != this.prevSyncGun)
      {
        this.prevSyncGun = this.curWeapon;
        flag = true;
      }
      else if (!this.isLocalPlayer)
      {
        try
        {
          if (this.prevFlash != this.syncFlash)
          {
            flag = true;
            this.prevFlash = this.syncFlash;
          }
          else if (!this.weapons[this.curWeapon].mod_sights[itemInHand.modSight].prefab_thirdperson.activeSelf)
            flag = true;
          else if (!this.weapons[this.curWeapon].mod_barrels[itemInHand.modBarrel].prefab_thirdperson.activeSelf)
            flag = true;
          else if (!this.weapons[this.curWeapon].mod_others[itemInHand.modOther].prefab_thirdperson.activeSelf)
            flag = true;
        }
        catch
        {
        }
      }
      else if (this.prevFlash != this.flashlightEnabled)
      {
        flag = true;
        this.prevFlash = this.flashlightEnabled;
      }
      else if (!this.weapons[this.curWeapon].mod_sights[itemInHand.modSight].prefab_firstperson.activeSelf)
        flag = true;
      else if (!this.weapons[this.curWeapon].mod_barrels[itemInHand.modBarrel].prefab_firstperson.activeSelf)
        flag = true;
      else if (!this.weapons[this.curWeapon].mod_others[itemInHand.modOther].prefab_firstperson.activeSelf)
        flag = true;
      if (!flag)
        return;
      if (this.isLocalPlayer)
        this.CmdSyncFlash(this.flashlightEnabled);
      this.weapons[this.curWeapon].RefreshMods(new int[3]
      {
        itemInHand.modSight,
        itemInHand.modBarrel,
        itemInHand.modOther
      }, (this.isLocalPlayer ? (this.flashlightEnabled ? 1 : 0) : (this.syncFlash ? 1 : 0)) != 0);
    }
  }

  private void UpdateGlobalLight()
  {
    if (!((UnityEngine.Object) this.globalLightSource != (UnityEngine.Object) null))
      return;
    this.globalLightSource.intensity = Mathf.Lerp(0.1f, 0.22f, (float) (((double) Vector3.Dot(this.transform.forward, Vector3.forward) + 1.0) / 2.0));
  }

  [Command]
  private void CmdSyncFlash(bool b)
  {
    if (this.isServer)
    {
      this.CallCmdSyncFlash(b);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBoolean(b);
      this.SendCommandInternal(typeof (WeaponManager), nameof (CmdSyncFlash), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  public bool ZoomInProgress()
  {
    return (double) this.zoomCooldown > 0.0;
  }

  private void DeductCooldown()
  {
    if ((double) this.fireCooldown >= 0.0)
      this.fireCooldown -= Time.deltaTime;
    if ((double) this.reloadCooldown >= 0.0)
      this.reloadCooldown -= Time.deltaTime;
    if ((double) this.zoomCooldown < 0.0)
      return;
    this.zoomCooldown -= Time.deltaTime;
  }

  [ClientCallback]
  private void DoLaserStuff()
  {
    if (!NetworkClient.active)
      return;
    if (this.curWeapon < 0)
    {
      this.laserLight.forwardDirection = (GameObject) null;
    }
    else
    {
      this.laserLight.maxAngle = this.weapons[this.curWeapon].maxAngleLaser;
      this.laserLight.forwardDirection = this.weapons[this.curWeapon].allEffects.laserDirection;
      this.laserLight.transform.localPosition = Vector3.Lerp(this.laserLight.transform.localPosition, -(this.zoomed ? this.weapons[this.curWeapon].allEffects.zoomPositionOffset : this.weapons[this.curWeapon].additionalUnfocusedOffset), Time.deltaTime * 8f);
    }
  }

  [ClientCallback]
  private void UpdateFov()
  {
    if (!NetworkClient.active)
      return;
    float b1 = this.normalFov;
    float b2 = this.normalFov - 10f;
    bool flag = this.curWeapon >= 0 && (UnityEngine.Object) this.weapons[this.curWeapon].allEffects.customProfile != (UnityEngine.Object) null;
    if (this.curWeapon >= 0 && this.zoomed && !flag)
    {
      b1 = this.weapons[this.curWeapon].allEffects.zoomFov;
      b2 = this.weapons[this.curWeapon].allEffects.weaponCameraFov;
    }
    Camera component = this.camera.GetComponent<Camera>();
    component.fieldOfView = flag ? b1 : Mathf.Lerp(component.fieldOfView, b1, Time.deltaTime * this.fovAdjustingSpeed);
    this.weaponModelCamera.fieldOfView = flag ? b2 : Mathf.Lerp(this.weaponModelCamera.fieldOfView, b2, Time.deltaTime * this.fovAdjustingSpeed);
  }

  [Command]
  private void CmdSetZoom(bool b)
  {
    if (this.isServer)
    {
      this.CallCmdSetZoom(b);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBoolean(b);
      this.SendCommandInternal(typeof (WeaponManager), nameof (CmdSetZoom), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  public int AmmoLeft()
  {
    int itemIndex = this.hub.inventory.GetItemIndex();
    return itemIndex >= 0 && this.curWeapon >= 0 ? (int) this.hub.inventory.items[itemIndex].durability : -1;
  }

  [Command]
  public void CmdChangeModPreferences(string info)
  {
    if (this.isServer)
    {
      this.CallCmdChangeModPreferences(info);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(info);
      this.SendCommandInternal(typeof (WeaponManager), nameof (CmdChangeModPreferences), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdShoot(
    GameObject target,
    string hitboxType,
    Vector3 dir,
    Vector3 sourcePos,
    Vector3 targetPos)
  {
    if (this.isServer)
    {
      this.CallCmdShoot(target, hitboxType, dir, sourcePos, targetPos);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(target);
      writer.WriteString(hitboxType);
      writer.WriteVector3(dir);
      writer.WriteVector3(sourcePos);
      writer.WriteVector3(targetPos);
      this.SendCommandInternal(typeof (WeaponManager), nameof (CmdShoot), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdReload(bool animationOnly)
  {
    if (this.isServer)
    {
      this.CallCmdReload(animationOnly);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBoolean(animationOnly);
      this.SendCommandInternal(typeof (WeaponManager), nameof (CmdReload), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [ServerCallback]
  private void PlaceDecal(bool isBlood, Ray ray, int classId, float distanceAddition)
  {
    RaycastHit hitInfo;
    if (!NetworkServer.active || !this.hub.characterClassManager.Classes.CheckBounds(classId) || !Physics.Raycast(ray, out hitInfo, isBlood ? 10f + distanceAddition : 100f, (int) this.bloodMask))
      return;
    this.RpcPlaceDecal(isBlood, isBlood ? this.hub.characterClassManager.Classes.SafeGet(classId).bloodType : classId, hitInfo.point + hitInfo.normal * 0.01f, Quaternion.FromToRotation(Vector3.up, hitInfo.normal));
  }

  [ClientRpc]
  private void RpcPlaceDecal(bool isBlood, int type, Vector3 pos, Quaternion rot)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteBoolean(isBlood);
    writer.WritePackedInt32(type);
    writer.WriteVector3(pos);
    writer.WriteQuaternion(rot);
    this.SendRPCInternal(typeof (WeaponManager), nameof (RpcPlaceDecal), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  private void RpcConfirmShot(bool hitmarker, int weapon)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteBoolean(hitmarker);
    writer.WritePackedInt32(weapon);
    this.SendRPCInternal(typeof (WeaponManager), nameof (RpcConfirmShot), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  private void RpcReload(int weapon)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WritePackedInt32(weapon);
    this.SendRPCInternal(typeof (WeaponManager), nameof (RpcReload), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private IEnumerator<float> _Reload()
  {
    int itemIndex = this.hub.inventory.GetItemIndex();
    if (itemIndex >= 0 && itemIndex < this.hub.inventory.items.Count && ((double) this.hub.inventory.items[itemIndex].durability < (double) this.weapons[this.curWeapon].maxAmmo && this.hub.ammoBox.GetAmmo(this.weapons[this.curWeapon].ammoType) > 0) && !this.zoomed)
    {
      Animator a = this.hub.inventory.GetItemByID(this.hub.inventory.curItem).firstpersonModel.GetComponent<Animator>();
      int w = this.curWeapon;
      this.hub.animationController.gunSource.PlayOneShot(this.weapons[this.curWeapon].reloadClip);
      this.reloadCooldown = this.weapons[w].reloadingTime;
      a.SetBool("Reloading", true);
      if ((double) Math.Abs(this.hub.inventory.items[itemIndex].durability) < 0.00999999977648258)
      {
        if (this.weapons[this.curWeapon].reloadingNoammoAnims.Length != 0)
          a.Play(this.weapons[this.curWeapon].reloadingNoammoAnims[UnityEngine.Random.Range(0, this.weapons[this.curWeapon].reloadingNoammoAnims.Length)], 0, 0.0f);
      }
      else if (this.weapons[this.curWeapon].reloadingAnims.Length != 0)
        a.Play(this.weapons[this.curWeapon].reloadingAnims[UnityEngine.Random.Range(0, this.weapons[this.curWeapon].reloadingAnims.Length)], 0, 0.0f);
      this.CmdReload(true);
      while ((double) this.reloadCooldown > 0.400000005960464)
      {
        if (w != this.curWeapon)
        {
          a.SetBool("Reloading", false);
          this.hub.animationController.gunSource.Stop();
          this.reloadCooldown = 0.0f;
          yield break;
        }
        else
          yield return float.NegativeInfinity;
      }
      a.SetBool("Reloading", false);
      this.CmdReload(false);
    }
  }

  private IEnumerator<float> _ReloadRpc(int weapon)
  {
    this.reloadCooldown = this.weapons[weapon].reloadingTime;
    AudioSource s = this.hub.animationController.gunSource;
    s.maxDistance = 15f;
    s.PlayOneShot(this.weapons[weapon].reloadClip);
    while ((double) this.reloadCooldown > 0.0)
    {
      if (this.curWeapon != weapon)
      {
        s.Stop();
        this.reloadCooldown = 0.0f;
      }
      yield return float.NegativeInfinity;
    }
  }

  public bool GetShootPermission(Team target, bool forceFriendlyFire = false)
  {
    if (this.hub.characterClassManager.CurClass < RoleType.Scp173 || this.hub.characterClassManager.CurClass == RoleType.Spectator || this.hub.characterClassManager.Classes.SafeGet(this.hub.characterClassManager.CurClass).team == target && target == Team.SCP)
      return false;
    if (this.friendlyFire && !forceFriendlyFire)
      return true;
    Team team = this.hub.characterClassManager.Classes.SafeGet(this.hub.characterClassManager.CurClass).team;
    return (team != Team.MTF && team != Team.RSC || target != Team.MTF && target != Team.RSC) && (team != Team.CDP && team != Team.CHI || target != Team.CDP && target != Team.CHI);
  }

  public bool GetShootPermission(CharacterClassManager c, bool forceFriendlyFire = false)
  {
    return c.Classes.CheckBounds(c.CurClass) && this.GetShootPermission(c.Classes.SafeGet(c.CurClass).team, forceFriendlyFire);
  }

  private void PlayMuzzleFlashes(bool firstperson, int gunId)
  {
    GameObject gameObject = firstperson ? this.weapons[gunId].mod_barrels[this.hub.inventory.GetItemInHand().modBarrel].prefab_firstperson : this.weapons[gunId].mod_barrels[this.hub.inventory.GetItemInHand().modBarrel].prefab_thirdperson;
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
      return;
    foreach (ParticleSystem componentsInChild in gameObject.GetComponentsInChildren<ParticleSystem>(true))
      componentsInChild.Play();
  }

  private void MirrorProcessed()
  {
  }

  public bool NetworkfriendlyFire
  {
    get
    {
      return this.friendlyFire;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.friendlyFire, 1UL);
    }
  }

  public int NetworkcurWeapon
  {
    get
    {
      return this.curWeapon;
    }
    [param: In] set
    {
      this.SetSyncVar<int>(value, ref this.curWeapon, 2UL);
    }
  }

  public bool NetworksyncZoomed
  {
    get
    {
      return this.syncZoomed;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.syncZoomed, 4UL);
    }
  }

  public bool NetworksyncFlash
  {
    get
    {
      return this.syncFlash;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.syncFlash, 8UL);
    }
  }

  protected static void InvokeCmdCmdSyncFlash(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSyncFlash called on client.");
    else
      ((WeaponManager) obj).CallCmdSyncFlash(reader.ReadBoolean());
  }

  protected static void InvokeCmdCmdSetZoom(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSetZoom called on client.");
    else
      ((WeaponManager) obj).CallCmdSetZoom(reader.ReadBoolean());
  }

  protected static void InvokeCmdCmdChangeModPreferences(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdChangeModPreferences called on client.");
    else
      ((WeaponManager) obj).CallCmdChangeModPreferences(reader.ReadString());
  }

  protected static void InvokeCmdCmdShoot(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdShoot called on client.");
    else
      ((WeaponManager) obj).CallCmdShoot(reader.ReadGameObject(), reader.ReadString(), reader.ReadVector3(), reader.ReadVector3(), reader.ReadVector3());
  }

  protected static void InvokeCmdCmdReload(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdReload called on client.");
    else
      ((WeaponManager) obj).CallCmdReload(reader.ReadBoolean());
  }

  public void CallCmdSyncFlash(bool b)
  {
    this.NetworksyncFlash = b;
  }

  public void CallCmdSetZoom(bool b)
  {
    this.NetworksyncZoomed = b;
  }

  public void CallCmdChangeModPreferences(string info)
  {
    if (!this._modRateLimit.CanExecute(true) || info == null)
      return;
    if (this.firstSet)
    {
      try
      {
        this.modPreferences = new int[this.weapons.Length, 3];
        this.firstSet = false;
        for (int index1 = 0; index1 < this.weapons.Length; ++index1)
        {
          for (int index2 = 0; index2 < 3; ++index2)
            this.modPreferences[index1, index2] = int.Parse(info.Split('#')[index1].Split(':')[index2]);
        }
      }
      catch
      {
        Debug.Log((object) "Mods unsuccessfully loaded.");
      }
    }
    else
    {
      WorkStation[] objectsOfType = UnityEngine.Object.FindObjectsOfType<WorkStation>();
      try
      {
        foreach (WorkStation workStation in objectsOfType)
        {
          if ((double) Vector3.Distance(workStation.transform.position, this.transform.position) < 5.0 && workStation.isTabletConnected)
          {
            int[] numArray = new int[3];
            for (int index = 0; index < 3; ++index)
              numArray[index] = int.Parse(info.Split(':')[index]);
            this.modPreferences[numArray[0], numArray[1]] = numArray[2];
            if (numArray[1] == 0)
              this.hub.inventory.items.ModifyAttachments(this.hub.inventory.GetItemIndex(), numArray[2], this.hub.inventory.GetItemInHand().modBarrel, this.hub.inventory.GetItemInHand().modOther);
            if (numArray[1] == 1)
              this.hub.inventory.items.ModifyAttachments(this.hub.inventory.GetItemIndex(), this.hub.inventory.GetItemInHand().modSight, numArray[2], this.hub.inventory.GetItemInHand().modOther);
            if (numArray[1] == 2)
              this.hub.inventory.items.ModifyAttachments(this.hub.inventory.GetItemIndex(), this.hub.inventory.GetItemInHand().modSight, this.hub.inventory.GetItemInHand().modBarrel, numArray[2]);
          }
        }
      }
      catch
      {
        Debug.Log((object) "Mods unsuccessfully loaded.");
      }
    }
  }

  public void CallCmdShoot(
    GameObject target,
    string hitboxType,
    Vector3 dir,
    Vector3 sourcePos,
    Vector3 targetPos)
  {
    if (!this._iawRateLimit.CanExecute(true))
      return;
    int itemIndex = this.hub.inventory.GetItemIndex();
    if (itemIndex < 0 || itemIndex >= this.hub.inventory.items.Count || this.curWeapon < 0 || ((double) this.reloadCooldown > 0.0 || (double) this.fireCooldown > 0.0) && !this.isLocalPlayer || (this.hub.inventory.curItem != this.weapons[this.curWeapon].inventoryID || (double) this.hub.inventory.items[itemIndex].durability <= 0.0))
      return;
    if ((double) Vector3.Distance(this.camera.transform.position, sourcePos) > 6.5)
    {
      this.GetComponent<CharacterClassManager>().TargetConsolePrint(this.connectionToClient, "Shot rejected - Code 2.2 (difference between real source position and provided source position is too big)", "gray");
    }
    else
    {
      this.hub.inventory.items.ModifyDuration(itemIndex, this.hub.inventory.items[itemIndex].durability - 1f);
      this.scp268.ServerDisable();
      this.fireCooldown = (float) (1.0 / ((double) this.weapons[this.curWeapon].shotsPerSecond * (double) this.weapons[this.curWeapon].allEffects.firerateMultiplier) * 0.800000011920929);
      float sourceRangeScale = this.weapons[this.curWeapon].allEffects.audioSourceRangeScale;
      this.GetComponent<Scp939_VisionController>().MakeNoise(Mathf.Clamp((float) ((double) sourceRangeScale * (double) sourceRangeScale * 70.0), 5f, 100f));
      bool flag = (UnityEngine.Object) target != (UnityEngine.Object) null;
      if (targetPos == Vector3.zero)
      {
        RaycastHit hitInfo;
        if (Physics.Raycast(sourcePos, dir, out hitInfo, 500f, (int) this.raycastMask))
        {
          HitboxIdentity component = hitInfo.collider.GetComponent<HitboxIdentity>();
          if ((UnityEngine.Object) component != (UnityEngine.Object) null)
          {
            WeaponManager componentInParent = component.GetComponentInParent<WeaponManager>();
            if ((UnityEngine.Object) componentInParent != (UnityEngine.Object) null)
            {
              flag = false;
              target = componentInParent.gameObject;
              hitboxType = component.id;
              targetPos = componentInParent.transform.position;
            }
          }
        }
      }
      else
      {
        RaycastHit hitInfo;
        if (Physics.Linecast(sourcePos, targetPos, out hitInfo, (int) this.raycastMask))
        {
          HitboxIdentity component = hitInfo.collider.GetComponent<HitboxIdentity>();
          if ((UnityEngine.Object) component != (UnityEngine.Object) null)
          {
            WeaponManager componentInParent = component.GetComponentInParent<WeaponManager>();
            if ((UnityEngine.Object) componentInParent != (UnityEngine.Object) null)
            {
              if ((UnityEngine.Object) componentInParent.gameObject == (UnityEngine.Object) target)
                flag = false;
              else if (componentInParent.scp268.Enabled)
              {
                flag = false;
                target = componentInParent.gameObject;
                hitboxType = component.id;
                targetPos = componentInParent.transform.position;
              }
            }
          }
        }
      }
      CharacterClassManager c = (CharacterClassManager) null;
      if ((UnityEngine.Object) target != (UnityEngine.Object) null)
        c = target.GetComponent<CharacterClassManager>();
      if ((UnityEngine.Object) c != (UnityEngine.Object) null && this.GetShootPermission(c, false))
      {
        if ((double) Math.Abs(this.camera.transform.position.y - c.transform.position.y) > 40.0)
          this.GetComponent<CharacterClassManager>().TargetConsolePrint(this.connectionToClient, "Shot rejected - Code 2.1 (too big Y-axis difference between source and target)", "gray");
        else if ((double) Vector3.Distance(c.transform.position, targetPos) > 6.5)
          this.GetComponent<CharacterClassManager>().TargetConsolePrint(this.connectionToClient, "Shot rejected - Code 2.3 (difference between real target position and provided target position is too big)", "gray");
        else if (Physics.Linecast(this.camera.transform.position, sourcePos, (int) this.raycastServerMask))
          this.GetComponent<CharacterClassManager>().TargetConsolePrint(this.connectionToClient, "Shot rejected - Code 2.4 (collision between source positions detected)", "gray");
        else if (flag && Physics.Linecast(sourcePos, targetPos, (int) this.raycastServerMask))
        {
          this.GetComponent<CharacterClassManager>().TargetConsolePrint(this.connectionToClient, "Shot rejected - Code 2.5 (collision on shot line detected)", "gray");
        }
        else
        {
          float num1 = Vector3.Distance(this.camera.transform.position, target.transform.position);
          float num2 = this.weapons[this.curWeapon].damageOverDistance.Evaluate(num1);
          string upper = hitboxType.ToUpper();
          if (!(upper == "HEAD"))
          {
            if (!(upper == "LEG"))
            {
              if (upper == "SCP106")
                num2 /= 10f;
            }
            else
              num2 /= 2f;
          }
          else
            num2 *= 4f;
          this.hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(num2 * this.weapons[this.curWeapon].allEffects.damageMultiplier * this.overallDamagerFactor, this.hub.nicknameSync.MyNick + " (" + this.hub.characterClassManager.UserId + ")", DamageTypes.FromWeaponId(this.curWeapon), this.hub.queryProcessor.PlayerId), c.gameObject);
          this.RpcConfirmShot(true, this.curWeapon);
          this.PlaceDecal(true, new Ray(this.camera.position, dir), (int) c.CurClass, num1);
        }
      }
      else if ((UnityEngine.Object) target != (UnityEngine.Object) null && hitboxType == "window" && (UnityEngine.Object) target.GetComponent<BreakableWindow>() != (UnityEngine.Object) null)
      {
        float damage = this.weapons[this.curWeapon].damageOverDistance.Evaluate(Vector3.Distance(this.camera.transform.position, target.transform.position));
        target.GetComponent<BreakableWindow>().ServerDamageWindow(damage);
        this.RpcConfirmShot(true, this.curWeapon);
      }
      else
      {
        this.PlaceDecal(false, new Ray(this.camera.position, dir), this.curWeapon, 0.0f);
        this.RpcConfirmShot(false, this.curWeapon);
      }
    }
  }

  public void CallCmdReload(bool animationOnly)
  {
    if (!this._iawRateLimit.CanExecute(true))
      return;
    int itemIndex = this.hub.inventory.GetItemIndex();
    if (itemIndex < 0 || itemIndex >= this.hub.inventory.items.Count || (this.curWeapon < 0 || this.hub.inventory.curItem != this.weapons[this.curWeapon].inventoryID) || (double) this.hub.inventory.items[itemIndex].durability >= (double) this.weapons[this.curWeapon].maxAmmo)
      return;
    this.scp268.ServerDisable();
    if (animationOnly)
    {
      this.RpcReload(this.curWeapon);
      this._reloadingWeapon = this.curWeapon;
      if ((double) this.weapons[this._reloadingWeapon].reloadingTime <= (double) this.reloadCooldown)
        return;
      this.reloadCooldown = this.weapons[this._reloadingWeapon].reloadingTime;
    }
    else if (this.curWeapon == this._reloadingWeapon && this._reloadingWeapon != -100)
    {
      this._reloadingWeapon = -100;
      int ammoType = this.weapons[this.curWeapon].ammoType;
      int ammo = this.hub.ammoBox.GetAmmo(ammoType);
      int durability = (int) this.hub.inventory.items[itemIndex].durability;
      for (int maxAmmo = this.weapons[this.curWeapon].maxAmmo; ammo > 0 && durability < maxAmmo; ++durability)
        --ammo;
      this.hub.inventory.items.ModifyDuration(itemIndex, (float) durability);
      this.hub.ammoBox.SetOneAmount(ammoType, ammo.ToString());
    }
    else
      this.GetComponent<CharacterClassManager>().TargetConsolePrint(this.connectionToClient, "Reload rejected - Code 2.6 (not requested reload of that weapon)", "gray");
  }

  protected static void InvokeRpcRpcPlaceDecal(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcPlaceDecal called on server.");
    else
      ((WeaponManager) obj).CallRpcPlaceDecal(reader.ReadBoolean(), reader.ReadPackedInt32(), reader.ReadVector3(), reader.ReadQuaternion());
  }

  protected static void InvokeRpcRpcConfirmShot(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcConfirmShot called on server.");
    else
      ((WeaponManager) obj).CallRpcConfirmShot(reader.ReadBoolean(), reader.ReadPackedInt32());
  }

  protected static void InvokeRpcRpcReload(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcReload called on server.");
    else
      ((WeaponManager) obj).CallRpcReload(reader.ReadPackedInt32());
  }

  public void CallRpcPlaceDecal(bool isBlood, int type, Vector3 pos, Quaternion rot)
  {
    if (!this._placeDecalRateLimit.CanExecute(true))
      return;
    if (isBlood)
    {
      if (!((UnityEngine.Object) this.drawer != (UnityEngine.Object) null))
        return;
      this.drawer.DrawBlood(pos, rot, type);
    }
    else
    {
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.weapons[type].holeEffect);
      gameObject.transform.position = pos;
      gameObject.transform.rotation = rot;
      gameObject.transform.localScale = Vector3.one;
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject, (float) this.decalDuration);
    }
  }

  public void CallRpcConfirmShot(bool hitmarker, int weapon)
  {
  }

  public void CallRpcReload(int weapon)
  {
  }

  static WeaponManager()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (WeaponManager), "CmdSyncFlash", new NetworkBehaviour.CmdDelegate(WeaponManager.InvokeCmdCmdSyncFlash));
    NetworkBehaviour.RegisterCommandDelegate(typeof (WeaponManager), "CmdSetZoom", new NetworkBehaviour.CmdDelegate(WeaponManager.InvokeCmdCmdSetZoom));
    NetworkBehaviour.RegisterCommandDelegate(typeof (WeaponManager), "CmdChangeModPreferences", new NetworkBehaviour.CmdDelegate(WeaponManager.InvokeCmdCmdChangeModPreferences));
    NetworkBehaviour.RegisterCommandDelegate(typeof (WeaponManager), "CmdShoot", new NetworkBehaviour.CmdDelegate(WeaponManager.InvokeCmdCmdShoot));
    NetworkBehaviour.RegisterCommandDelegate(typeof (WeaponManager), "CmdReload", new NetworkBehaviour.CmdDelegate(WeaponManager.InvokeCmdCmdReload));
    NetworkBehaviour.RegisterRpcDelegate(typeof (WeaponManager), "RpcPlaceDecal", new NetworkBehaviour.CmdDelegate(WeaponManager.InvokeRpcRpcPlaceDecal));
    NetworkBehaviour.RegisterRpcDelegate(typeof (WeaponManager), "RpcConfirmShot", new NetworkBehaviour.CmdDelegate(WeaponManager.InvokeRpcRpcConfirmShot));
    NetworkBehaviour.RegisterRpcDelegate(typeof (WeaponManager), "RpcReload", new NetworkBehaviour.CmdDelegate(WeaponManager.InvokeRpcRpcReload));
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteBoolean(this.friendlyFire);
      writer.WritePackedInt32(this.curWeapon);
      writer.WriteBoolean(this.syncZoomed);
      writer.WriteBoolean(this.syncFlash);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteBoolean(this.friendlyFire);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WritePackedInt32(this.curWeapon);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 4L) != 0L)
    {
      writer.WriteBoolean(this.syncZoomed);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 8L) != 0L)
    {
      writer.WriteBoolean(this.syncFlash);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworkfriendlyFire = reader.ReadBoolean();
      this.NetworkcurWeapon = reader.ReadPackedInt32();
      this.NetworksyncZoomed = reader.ReadBoolean();
      this.NetworksyncFlash = reader.ReadBoolean();
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
        this.NetworkfriendlyFire = reader.ReadBoolean();
      if ((num & 2L) != 0L)
        this.NetworkcurWeapon = reader.ReadPackedInt32();
      if ((num & 4L) != 0L)
        this.NetworksyncZoomed = reader.ReadBoolean();
      if ((num & 8L) == 0L)
        return;
      this.NetworksyncFlash = reader.ReadBoolean();
    }
  }

  [Serializable]
  public class Weapon
  {
    public float recoilAnimation = 0.5f;
    public float bobAnimationScale = 1f;
    public float timeToPickup = 0.5f;
    public bool useProceduralPickupAnimation = true;
    public float unfocusedSpread = 5f;
    [Header("Misc properties")]
    public ItemType inventoryID;
    public RecoilProperties recoil;
    public AnimationCurve damageOverDistance;
    public float shotsPerSecond;
    public bool allowFullauto;
    public Vector3 positionOffset;
    public Vector3 additionalUnfocusedOffset;
    public GameObject holeEffect;
    public ParticleSystem husks;
    public string[] customShootAnims;
    public string[] customZoomshotAnims;
    public string[] reloadingAnims;
    public string[] reloadingNoammoAnims;
    public string customShootAnimNoammo;
    public string customZoomshotAnimNoammo;
    public float maxAngleLaser;
    [Header("Ammo & reloading")]
    public AudioClip reloadClip;
    public int maxAmmo;
    public int ammoType;
    public float reloadingTime;
    [Header("Zooming")]
    public bool allowZoom;
    public float zoomingTime;
    [Header("Mods")]
    public WeaponManager.Weapon.WeaponMod[] mod_sights;
    public WeaponManager.Weapon.WeaponMod[] mod_barrels;
    public WeaponManager.Weapon.WeaponMod[] mod_others;
    public WeaponManager.Weapon.WeaponMod.WeaponModEffects allEffects;

    public WeaponManager.Weapon.WeaponMod.WeaponModEffects GetAllEffects(int[] mods)
    {
      this.allEffects = new WeaponManager.Weapon.WeaponMod.WeaponModEffects()
      {
        customProfile = this.mod_sights[mods[0]].effects.customProfile,
        zoomRecoilReduction = this.mod_sights[mods[0]].effects.zoomRecoilReduction,
        zoomFov = this.mod_sights[mods[0]].effects.zoomFov,
        weaponCameraFov = this.mod_sights[mods[0]].effects.weaponCameraFov,
        zoomSlowdown = this.mod_sights[mods[0]].effects.zoomSlowdown,
        zoomSensitivity = this.mod_sights[mods[0]].effects.zoomSensitivity,
        zoomPositionOffset = this.mod_sights[mods[0]].effects.zoomPositionOffset,
        shootSound = this.mod_barrels[mods[1]].effects.shootSound,
        firerateMultiplier = this.mod_barrels[mods[1]].effects.firerateMultiplier,
        zoomRecoilAnimScale = this.mod_sights[mods[0]].effects.zoomRecoilAnimScale,
        damageMultiplier = this.mod_barrels[mods[1]].effects.damageMultiplier,
        overallRecoilReduction = this.mod_sights[mods[0]].effects.overallRecoilReduction * this.mod_barrels[mods[1]].effects.overallRecoilReduction * this.mod_others[mods[2]].effects.overallRecoilReduction,
        unfocusedSpread = this.mod_sights[mods[0]].effects.unfocusedSpread * this.mod_barrels[mods[1]].effects.unfocusedSpread * this.mod_others[mods[2]].effects.unfocusedSpread,
        laserDirection = this.mod_others[mods[2]].effects.laserDirection,
        audioSourceRangeScale = this.mod_barrels[mods[1]].effects.audioSourceRangeScale,
        counterTemplate = this.mod_others[mods[2]].effects.counterTemplate,
        unzoomedShootAnimationScale = this.mod_sights[mods[0]].effects.unzoomedShootAnimationScale * this.mod_barrels[mods[1]].effects.unzoomedShootAnimationScale * this.mod_others[mods[2]].effects.unzoomedShootAnimationScale
      };
      return this.allEffects;
    }

    public void RefreshMods(int[] mods, bool _flashlight)
    {
      for (int index = 0; index < this.mod_sights.Length; ++index)
        this.mod_sights[index].SetVisibility(index == mods[0]);
      for (int index = 0; index < this.mod_barrels.Length; ++index)
        this.mod_barrels[index].SetVisibility(index == mods[1]);
      for (int index = 0; index < this.mod_others.Length; ++index)
      {
        this.mod_others[index].SetVisibility(index == mods[2]);
        if (index == mods[2] && this.mod_others[index].name.Contains("flashlight", StringComparison.OrdinalIgnoreCase))
        {
          if ((UnityEngine.Object) this.mod_others[index].prefab_firstperson != (UnityEngine.Object) null)
          {
            foreach (Behaviour componentsInChild in this.mod_others[index].prefab_firstperson.GetComponentsInChildren<Light>())
              componentsInChild.enabled = _flashlight;
          }
          if (!((UnityEngine.Object) this.mod_others[index].prefab_thirdperson == (UnityEngine.Object) null))
          {
            foreach (Behaviour componentsInChild in this.mod_others[index].prefab_thirdperson.GetComponentsInChildren<Light>())
              componentsInChild.enabled = _flashlight;
          }
        }
      }
      this.allEffects = this.GetAllEffects(mods);
    }

    [Serializable]
    public class WeaponMod
    {
      public string name;
      public GameObject prefab_firstperson;
      public GameObject prefab_thirdperson;
      public WeaponManager.Weapon.WeaponMod.WeaponModEffects effects;
      public Texture icon;
      public bool isActive;

      public void SetVisibility(bool b)
      {
        this.isActive = b;
        if ((UnityEngine.Object) this.prefab_firstperson != (UnityEngine.Object) null)
          this.prefab_firstperson.SetActive(b);
        if (!((UnityEngine.Object) this.prefab_thirdperson != (UnityEngine.Object) null))
          return;
        this.prefab_thirdperson.SetActive(b);
      }

      [Serializable]
      public class WeaponModEffects
      {
        [Tooltip("FOV")]
        public float zoomFov = 70f;
        public float weaponCameraFov = 60f;
        [Tooltip("RECOIL SCALE")]
        public float zoomRecoilReduction = 1f;
        [Tooltip("WALK SLOW")]
        public float zoomSlowdown = 1f;
        [Tooltip("SENSITIVITY")]
        public float zoomSensitivity = 1f;
        [Tooltip("RECOIL ANIMATION SCALE")]
        public float zoomRecoilAnimScale = 1f;
        public Vector3 zoomPositionOffset = Vector3.zero;
        public float damageMultiplier = 1f;
        public float audioSourceRangeScale = 1f;
        public float firerateMultiplier = 1f;
        [Header("Mixed effects")]
        public float overallRecoilReduction = 1f;
        public float unfocusedSpread = 1f;
        public float unzoomedShootAnimationScale = 1f;
        [Header("Sights only effects")]
        public PostProcessingProfile customProfile;
        [Header("Barrels only effects")]
        public AudioClip shootSound;
        [Header("Ammo Counter Effects")]
        public Text counterText;
        public string counterTemplate;
        public GameObject laserDirection;
      }
    }
  }
}
