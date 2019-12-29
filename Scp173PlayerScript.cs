// Decompiled with JetBrains decompiler
// Type: Scp173PlayerScript
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Dissonance.Integrations.MirrorIgnorance;
using GameCore;
using Grenades;
using Mirror;
using RemoteAdmin;
using Security;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class Scp173PlayerScript : NetworkBehaviour
{
  private bool _allowMove = true;
  private readonly RateLimit _scp173BlinkRateLimit = new RateLimit(2, 0.8f, (NetworkConnection) null);
  private readonly RateLimit _scp173SnapRateLimit = new RateLimit(4, 1f, (NetworkConnection) null);
  [Header("Player Properties")]
  public bool iAm173;
  [Header("Raycasting")]
  public GameObject cam;
  [Header("Blinking")]
  public float minBlinkTime;
  public GameObject weaponCameras;
  public GameObject hitbox;
  public LayerMask layerMask;
  public LayerMask teleportMask;
  public LayerMask hurtLayer;
  public AnimationCurve boost_speed;
  public AudioClip[] necksnaps;
  private VignetteAndChromaticAberration _blinkCtrl;
  private FirstPersonController _fpc;
  private PlyMovementSync _pms;
  private CharacterClassManager _publicCcm;
  private PlayerStats _ps;
  private Inventory _inv;
  private FlashEffect _flash;
  private LocalCurrentRoomEffects _lcre;
  private FootstepSync _footstepSync;
  private RateLimit _interactRateLimit;
  [Header("Boosts")]
  public AnimationCurve boost_teleportDistance;
  public bool SameClass;
  public float maxBlinkTime;
  public float blinkDuration_notsee;
  public float blinkDuration_see;
  public float range;
  private static float _blinkTimeRemaining;
  private static float _antiBlindTime;
  private static float _remainingTime;
  private static bool _localplayerIs173;
  public static bool Blinking;
  private const float PlayerSpottingAngle = 0.4225f;
  private bool neonScp173Rework;
  private static float reworkPlusTime;

  private void Start()
  {
    this._interactRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[0];
    this._lcre = this.GetComponent<LocalCurrentRoomEffects>();
    this._flash = this.GetComponent<FlashEffect>();
    this._ps = this.GetComponent<PlayerStats>();
    this._inv = this.GetComponent<Inventory>();
    this._publicCcm = this.GetComponent<CharacterClassManager>();
    if (!this.isLocalPlayer)
      return;
    this._blinkCtrl = this.GetComponentInChildren<VignetteAndChromaticAberration>();
    this._pms = this.GetComponent<PlyMovementSync>();
    this._footstepSync = this.GetComponent<FootstepSync>();
    Scp173PlayerScript.Blinking = false;
    this.neonScp173Rework = ConfigFile.ServerConfig.GetBool("neon_scp173rework", true);
  }

  public void Init(RoleType classID, Role c)
  {
    this.SameClass = c.team == Team.SCP;
    this.iAm173 = classID == RoleType.Scp173;
    if (this.isLocalPlayer)
      Scp173PlayerScript._localplayerIs173 = this.iAm173;
    this.hitbox.SetActive(!this.isLocalPlayer && Scp173PlayerScript._localplayerIs173);
  }

  private void FixedUpdate()
  {
    this.DoBlinkingSequence();
    if (!this.iAm173 || !this.isLocalPlayer && !NetworkServer.active)
      return;
    if (this._allowMove)
    {
      Scp173PlayerScript.reworkPlusTime -= Time.fixedDeltaTime * 1.25f;
      if ((double) Scp173PlayerScript.reworkPlusTime < 0.0)
        Scp173PlayerScript.reworkPlusTime = 0.0f;
    }
    this._allowMove = true;
    foreach (GameObject player in PlayerManager.players)
    {
      Scp173PlayerScript component = player.GetComponent<Scp173PlayerScript>();
      if (!component.SameClass && component.LookFor173(this.gameObject, true) && this.LookFor173(component.gameObject, false))
      {
        this._allowMove = false;
        break;
      }
    }
  }

  private bool LookFor173(GameObject scp, bool angleCheck)
  {
    if ((UnityEngine.Object) scp == (UnityEngine.Object) null || this._publicCcm.CurClass == RoleType.Spectator || this._flash.blinded)
      return false;
    if (this._lcre.syncFlicker)
    {
      WeaponManager component = this.GetComponent<WeaponManager>();
      bool flag = false;
      if (this._inv.curItem == ItemType.Flashlight)
        flag = true;
      if (component.curWeapon >= 0 && component.curWeapon < component.weapons.Length)
      {
        if (component.syncZoomed)
        {
          foreach (WeaponManager.Weapon.WeaponMod modSight in component.weapons[component.curWeapon].mod_sights)
          {
            if (modSight.isActive && modSight.name == "Night Vision Sight")
              flag = true;
          }
        }
        if (component.syncFlash && ((IEnumerable<WeaponManager.Weapon.WeaponMod>) component.weapons[component.curWeapon].mod_others).Any<WeaponManager.Weapon.WeaponMod>((Func<WeaponManager.Weapon.WeaponMod, bool>) (item => item.isActive && item.name == "Flashlight")))
          flag = true;
      }
      if (!flag & angleCheck)
        return false;
    }
    Vector3 vector3 = scp.transform.position - this.cam.transform.position;
    float num = Vector3.Dot(this.cam.transform.forward, vector3);
    RaycastHit hitInfo;
    return (!angleCheck || (double) num >= 0.0 && (double) num * (double) num / (double) vector3.sqrMagnitude > 0.422500014305115) && Physics.Raycast(this.cam.transform.position, vector3, out hitInfo, this.range, (int) this.layerMask) && hitInfo.transform.name == scp.name;
  }

  public bool CanMove(bool checkBlinking = true)
  {
    if (this._allowMove)
      return true;
    return checkBlinking && (double) Scp173PlayerScript._blinkTimeRemaining > 0.0;
  }

  private void DoBlinkingSequence()
  {
    if (!this.isServer || !this.isLocalPlayer)
      return;
    Scp173PlayerScript._remainingTime -= Time.fixedDeltaTime;
    Scp173PlayerScript._blinkTimeRemaining -= Time.fixedDeltaTime;
    if ((double) Scp173PlayerScript._remainingTime >= 0.0)
      return;
    Scp173PlayerScript._blinkTimeRemaining = (float) ((double) this.blinkDuration_see + 0.5 + (double) Scp173PlayerScript.reworkPlusTime / 3.0);
    Scp173PlayerScript._remainingTime = UnityEngine.Random.Range(this.minBlinkTime, this.maxBlinkTime) - Scp173PlayerScript.reworkPlusTime;
    if (!this._allowMove)
    {
      Scp173PlayerScript.reworkPlusTime += UnityEngine.Random.Range(0.25f, 0.45f);
      if ((double) Scp173PlayerScript.reworkPlusTime > (double) this.minBlinkTime - 1.0)
        Scp173PlayerScript.reworkPlusTime = this.minBlinkTime - 1f;
    }
    foreach (Scp173PlayerScript scp173PlayerScript in UnityEngine.Object.FindObjectsOfType<Scp173PlayerScript>())
      scp173PlayerScript.RpcBlinkTime();
  }

  public void Boost()
  {
    if (!Input.GetKey(NewInput.GetKey("Shoot")))
      return;
    this.Shoot();
  }

  private void Forward()
  {
  }

  public void Blink()
  {
    if (!this.isLocalPlayer)
      return;
    Scp173PlayerScript.Blinking = true;
    foreach (GameObject player in PlayerManager.players)
    {
      if (player.GetComponent<Scp173PlayerScript>().iAm173)
      {
        bool flag = this.LookFor173(player, true);
        if (flag)
        {
          this._blinkCtrl.intensity = 1f;
          this.weaponCameras.SetActive(false);
        }
        this.Invoke("UnBlink", flag ? this.blinkDuration_see : this.blinkDuration_notsee);
      }
    }
  }

  private void UnBlink()
  {
    this._blinkCtrl.intensity = 0.036f;
    Scp173PlayerScript.Blinking = false;
    this.weaponCameras.SetActive(true);
  }

  private void Update()
  {
    if (!this.isLocalPlayer || this.SameClass)
      return;
    this.AntiBlind();
  }

  private void AntiBlind()
  {
    if ((double) this._blinkCtrl.intensity > 0.5)
    {
      Scp173PlayerScript._antiBlindTime += Time.deltaTime;
      if ((double) Scp173PlayerScript._antiBlindTime <= 2.0)
        return;
      this.UnBlink();
    }
    else
      Scp173PlayerScript._antiBlindTime = 0.0f;
  }

  private void Shoot()
  {
    RaycastHit hitInfo;
    if (!Physics.Raycast(this.cam.transform.position, this.cam.transform.forward, out hitInfo, 1.5f, (int) this.hurtLayer))
      return;
    CharacterClassManager component = hitInfo.transform.GetComponent<CharacterClassManager>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null || component.Classes.SafeGet(component.CurClass).team == Team.SCP)
      return;
    this.HurtPlayer(hitInfo.transform.gameObject, this.GetComponent<MirrorIgnorancePlayer>().PlayerId);
  }

  private void HurtPlayer(GameObject go, string plyID)
  {
    this.CmdHurtPlayer(go);
  }

  [TargetRpc]
  private void TargetHitMarker(NetworkConnection connection)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendTargetRPCInternal(connection, typeof (Scp173PlayerScript), nameof (TargetHitMarker), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [Command]
  private void CmdHurtPlayer(GameObject target)
  {
    if (this.isServer)
    {
      this.CallCmdHurtPlayer(target);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(target);
      this.SendCommandInternal(typeof (Scp173PlayerScript), nameof (CmdHurtPlayer), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [ClientRpc]
  private void RpcBlinkTime()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (Scp173PlayerScript), nameof (RpcBlinkTime), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  private void RpcSyncAudio()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (Scp173PlayerScript), nameof (RpcSyncAudio), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeCmdCmdHurtPlayer(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdHurtPlayer called on client.");
    else
      ((Scp173PlayerScript) obj).CallCmdHurtPlayer(reader.ReadGameObject());
  }

  public void CallCmdHurtPlayer(GameObject target)
  {
    if (!this._interactRateLimit.CanExecute(true) || (UnityEngine.Object) target == (UnityEngine.Object) null)
      return;
    CharacterClassManager component = target.GetComponent<CharacterClassManager>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null || this.GetComponent<CharacterClassManager>().CurClass != RoleType.Scp173 || (!this.CanMove(true) || (double) Vector3.Distance(this.GetComponent<PlyMovementSync>().RealModelPosition, target.transform.position) >= 3.0 + (double) this.boost_teleportDistance.Evaluate(this._ps.GetHealthPercent())) || component.Classes.SafeGet(component.CurClass).team == Team.SCP)
      return;
    this.RpcSyncAudio();
    this.GetComponent<CharacterClassManager>().RpcPlaceBlood(target.transform.position, 0, 2.2f);
    this.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(999990f, this.GetComponent<NicknameSync>().MyNick + " (" + this._publicCcm.UserId + ")", DamageTypes.Scp173, this.GetComponent<QueryProcessor>().PlayerId), target);
    this.TargetHitMarker(this.connectionToClient);
  }

  protected static void InvokeRpcRpcBlinkTime(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcBlinkTime called on server.");
    else
      ((Scp173PlayerScript) obj).CallRpcBlinkTime();
  }

  protected static void InvokeRpcRpcSyncAudio(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcSyncAudio called on server.");
    else
      ((Scp173PlayerScript) obj).CallRpcSyncAudio();
  }

  protected static void InvokeRpcTargetHitMarker(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetHitMarker called on server.");
    else
      ((Scp173PlayerScript) obj).CallTargetHitMarker(ClientScene.readyConnection);
  }

  public void CallRpcBlinkTime()
  {
    if (!this._scp173BlinkRateLimit.CanExecute(true))
      return;
    if (this.iAm173)
      this.Boost();
    if (this.SameClass)
      return;
    this.Blink();
  }

  public void CallRpcSyncAudio()
  {
    if (!this._scp173SnapRateLimit.CanExecute(true))
      return;
    this.GetComponent<AnimationController>().gunSource.PlayOneShot(this.necksnaps[UnityEngine.Random.Range(0, this.necksnaps.Length)]);
  }

  public void CallTargetHitMarker(NetworkConnection connection)
  {
  }

  static Scp173PlayerScript()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp173PlayerScript), "CmdHurtPlayer", new NetworkBehaviour.CmdDelegate(Scp173PlayerScript.InvokeCmdCmdHurtPlayer));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Scp173PlayerScript), "RpcBlinkTime", new NetworkBehaviour.CmdDelegate(Scp173PlayerScript.InvokeRpcRpcBlinkTime));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Scp173PlayerScript), "RpcSyncAudio", new NetworkBehaviour.CmdDelegate(Scp173PlayerScript.InvokeRpcRpcSyncAudio));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Scp173PlayerScript), "TargetHitMarker", new NetworkBehaviour.CmdDelegate(Scp173PlayerScript.InvokeRpcTargetHitMarker));
  }
}
