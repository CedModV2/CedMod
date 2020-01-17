// Decompiled with JetBrains decompiler
// Type: Scp106PlayerScript
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using CustomPlayerEffects;
using GameCore;
using MEC;
using Mirror;
using RemoteAdmin;
using Security;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Scp106PlayerScript : NetworkBehaviour
{
    private const int V = 0;
    [Header("Balance")]
  [SerializeField]
  private float captureCooldown = 2f;
  [Header("Player Properties")]
  public Transform plyCam;
  public bool iAm106;
  public bool sameClass;
  [SyncVar]
  private float remainingPoints;
  public float teleportSpeed;
  public GameObject screamsPrefab;
  [Header("Portal")]
  [SyncVar(hook = "SetPortalPosition")]
  public Vector3 portalPosition;
  public GameObject portalPrefab;
  private Vector3 previousPortalPosition;
  private Offset modelOffset;
  private CharacterClassManager ccm;
  private FirstPersonController fpc;
  private GameObject popup106;
  private string highlightedString;
  public int highlightID;
  private static BlastDoor blastDoor;
  private float remainingCooldown;
  public bool goingViaThePortal;
  private bool isCollidingDoorOpen;
  private Door doorCurrentlyIn;
  private RateLimit _interactRateLimit;
  private RateLimit _iawRateLimit;
  public LayerMask teleportPlacementMask;
  private static float stalky106LastTick;
  private static float disableFor;
  private static float stalkyCd;
  private static string[] parser;
  public static bool neonStalky106;

  private void Start()
  {
    this._interactRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[0];
    this._iawRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[3];
    if ((UnityEngine.Object) Scp106PlayerScript.blastDoor == (UnityEngine.Object) null)
      Scp106PlayerScript.blastDoor = UnityEngine.Object.FindObjectOfType<BlastDoor>();
    this.ccm = this.GetComponent<CharacterClassManager>();
    this.fpc = this.GetComponent<FirstPersonController>();
    this.InvokeRepeating("ExitDoor", 1f, 2f);
    this.modelOffset = this.ccm.Classes.Get(RoleType.Scp106).model_offset;
    Scp106PlayerScript.neonStalky106 = ConfigFile.ServerConfig.GetBool("neon_stalky106", true);
  }

  public void Init(RoleType classID, Role c)
  {
    if (!this.iAm106 && classID == RoleType.Scp106)
    {
      NetworkConnection connectionToClient = this.connectionToClient;
      Broadcast component = PlayerManager.localPlayer.GetComponent<Broadcast>();
      if (ConfigFile.ServerConfig.GetBool("neon_stalky106", true))
      {
        component.TargetClearElements(connectionToClient);
        component.TargetAddElement(connectionToClient, "<size=80><color=#0020ed><b>Stalk</b></color></size>" + Environment.NewLine + "In this server, you can <color=#0020ed><b>stalk</b></color> humans by double-clicking the portal creation button in the <b>[TAB]</b> menu.", 12U, false);
      }
    }
    this.iAm106 = classID == RoleType.Scp106;
    this.sameClass = c.team == Team.SCP;
    if ((double) Scp106PlayerScript.stalkyCd < (double) Time.time + 120.0)
      return;
    Scp106PlayerScript.stalkyCd = Time.time + 120f;
  }

  private void Update()
  {
    this.DoorCollisionCheck();
  }

  public void SetDoors()
  {
    if (!this.isLocalPlayer)
      return;
    foreach (Door door in UnityEngine.Object.FindObjectsOfType<Door>())
    {
      if (door.permissionLevel != "UNACCESSIBLE" && !door.locked)
      {
        foreach (Collider componentsInChild in door.GetComponentsInChildren<Collider>())
        {
          if (!componentsInChild.CompareTag("DoorButton"))
          {
            try
            {
              componentsInChild.isTrigger = this.iAm106;
            }
            catch
            {
            }
          }
        }
      }
    }
  }

  [Server]
  public void Contain(CharacterClassManager ccm)
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Void Scp106PlayerScript::Contain(CharacterClassManager)' called on client");
    }
    else
    {
      this.NetworkremainingPoints = 0.0f;
      Timing.RunCoroutine(this._ContainAnimation(ccm), Segment.FixedUpdate);
    }
  }

  public void DeletePortal()
  {
    if ((double) this.portalPosition.y >= 900.0)
      return;
    this.portalPrefab = (GameObject) null;
    this.NetworkportalPosition = Vector3.zero;
  }

  public void UseTeleport()
  {
    if (!this.GetComponent<FallDamage>().isGrounded || !((UnityEngine.Object) this.portalPrefab != (UnityEngine.Object) null) || !(this.portalPosition != Vector3.zero))
      return;
    this.CmdUsePortal();
  }

  private void SetPortalPosition(Vector3 pos)
  {
    this.NetworkportalPosition = pos;
    Timing.RunCoroutine(this._DoPortalSetupAnimation(), Segment.Update);
  }

  public void CreatePortalInCurrentPosition()
  {
    if (!this.GetComponent<FallDamage>().isGrounded || !this.isLocalPlayer)
      return;
    this.CmdMakePortal();
  }

    [Server]
    private IEnumerator<float> _ContainAnimation(CharacterClassManager ccm)
    {
        RpcContainAnimation();
        for (int i = 0; i < 900; i++)
        { yield return 0f; }
        goingViaThePortal = true;
        for (int i = 0; (float)i < 175f; i++)
        { yield return 0f; } Kill(ccm);
        goingViaThePortal = false;
    }

    [ClientRpc]
  private void RpcContainAnimation()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (Scp106PlayerScript), nameof (RpcContainAnimation), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void LateUpdate()
  {
    Animator animator = this.GetComponent<AnimationController>().animator;
    if ((UnityEngine.Object) animator == (UnityEngine.Object) null || !this.iAm106 || this.isLocalPlayer)
      return;
    AnimationFloatValue component = this.ccm.MyModel.GetComponent<AnimationFloatValue>();
    Offset modelOffset = this.modelOffset;
    modelOffset.position -= component.v3_value * component.f_value;
    animator.transform.localPosition = modelOffset.position;
    animator.transform.localRotation = Quaternion.Euler(modelOffset.rotation);
  }

  [Server]
  public void Kill(CharacterClassManager ccm)
  {
    if (!NetworkServer.active)
      Debug.LogWarning((object) "[Server] function 'System.Void Scp106PlayerScript::Kill(CharacterClassManager)' called on client");
    else
      this.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(999799f, "", DamageTypes.RagdollLess, ccm.GetComponent<QueryProcessor>().PlayerId), this.gameObject);
  }

  private IEnumerator<float> _DoPortalSetupAnimation()
  {
    while ((UnityEngine.Object) this.portalPrefab == (UnityEngine.Object) null)
    {
      this.portalPrefab = GameObject.Find("SCP106_PORTAL");
      yield return float.NegativeInfinity;
    }
    Animator portalAnim = this.portalPrefab.GetComponent<Animator>();
    portalAnim.SetBool("activated", false);
    yield return Timing.WaitForSeconds(1f);
    this.portalPrefab.transform.position = this.portalPosition;
    portalAnim.SetBool("activated", true);
  }

  [Server]
  private IEnumerator<float> _DoTeleportAnimation()
  {
      if (!(portalPrefab == null) && goingViaThePortal)
      {
          Vector3 pos = portalPrefab.transform.position + Vector3.up * 1.5f;
          RpcTeleportAnimation();
          goingViaThePortal = true;
          PlyMovementSync pms = GetComponent<PlyMovementSync>();
          for (int i = 0; (float) i < 175f; i++)
          {
              yield return 0f;
          }
          pms.OverridePosition(pos, 0f);
          for (int i = 0; (float) i < 175f; i++);
          {
              yield return 0f;
          }
          if (AlphaWarheadController.Host.detonated && base.transform.position.y < 800f)
          {
              GetComponent<PlayerStats>().HurtPlayer(new PlayerStats().LastHitInfo(9000f, "WORLD", DamageTypes.Nuke, 0),
                  base.gameObject);
          }
          goingViaThePortal = false;
      }
  }

  [ClientRpc]
  public void RpcTeleportAnimation()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (Scp106PlayerScript), nameof (RpcTeleportAnimation), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [Command]
  private void CmdMakePortal()
  {
    if (this.isServer)
    {
      this.CallCmdMakePortal();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (Scp106PlayerScript), nameof (CmdMakePortal), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  public void CmdUsePortal()
  {
    if (this.isServer)
    {
      this.CallCmdUsePortal();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (Scp106PlayerScript), nameof (CmdUsePortal), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdMovePlayer(GameObject ply, int t)
  {
    if (this.isServer)
    {
      this.CallCmdMovePlayer(ply, t);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(ply);
      writer.WritePackedInt32(t);
      this.SendCommandInternal(typeof (Scp106PlayerScript), nameof (CmdMovePlayer), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  private void OnTriggerEnter(Collider other)
  {
  }

  private void ExitDoor()
  {
  }

  private void OnTriggerExit(Collider other)
  {
  }

  private void DoorCollisionCheck()
  {
  }

  private void MirrorProcessed()
  {
  }

  public float NetworkremainingPoints
  {
    get
    {
      return this.remainingPoints;
    }
    [param: In] set
    {
      this.SetSyncVar<float>(value, ref this.remainingPoints, 1UL);
    }
  }

  public Vector3 NetworkportalPosition
  {
    get
    {
      return this.portalPosition;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(2UL))
      {
        this.setSyncVarHookGuard(2UL, true);
        this.SetPortalPosition(value);
        this.setSyncVarHookGuard(2UL, false);
      }
      this.SetSyncVar<Vector3>(value, ref this.portalPosition, 2UL);
    }
  }

  protected static void InvokeCmdCmdMakePortal(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdMakePortal called on client.");
    else
      ((Scp106PlayerScript) obj).CallCmdMakePortal();
  }

  protected static void InvokeCmdCmdUsePortal(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdUsePortal called on client.");
    else
      ((Scp106PlayerScript) obj).CallCmdUsePortal();
  }

  protected static void InvokeCmdCmdMovePlayer(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdMovePlayer called on client.");
    else
      ((Scp106PlayerScript) obj).CallCmdMovePlayer(reader.ReadGameObject(), reader.ReadPackedInt32());
  }

  public void CallCmdMakePortal()
  {
    if (!this._interactRateLimit.CanExecute(true) || !this.GetComponent<FallDamage>().isGrounded)
      return;
    Debug.DrawRay(this.transform.position, -this.transform.up, Color.red, 10f);
    RaycastHit hitInfo;
    if (!this.iAm106 || this.goingViaThePortal || !Physics.Raycast(new Ray(this.transform.position, -this.transform.up), out hitInfo, 10f, (int) this.teleportPlacementMask))
      return;
    if (!Scp106PlayerScript.neonStalky106)
    {
      this.SetPortalPosition(hitInfo.point - Vector3.up);
    }
    else
    {
      if ((double) Scp106PlayerScript.disableFor > (double) Time.time)
        return;
      double num1 = (double) Time.time - (double) Scp106PlayerScript.stalky106LastTick;
      float num2 = Scp106PlayerScript.stalkyCd - Time.time;
      Broadcast component = PlayerManager.localPlayer.GetComponent<Broadcast>();
      if (num1 > 6.0)
      {
        Scp106PlayerScript.stalky106LastTick = Time.time;
        if ((double) num2 < 0.0)
        {
          component.TargetClearElements(this.connectionToClient);
          component.TargetAddElement(this.connectionToClient, Environment.NewLine + "Press the portal creation button again to <color=#ff0955><b>Stalk</b></color> a random player.", 6U, false);
        }
        this.SetPortalPosition(hitInfo.point - Vector3.up);
      }
      else
      {
        component.TargetClearElements(this.connectionToClient);
        if ((double) num2 > 0.0)
        {
          Scp106PlayerScript.stalky106LastTick = Time.time;
          int num3;
          for (num3 = 0; num3 < 5 && (double) num2 > (double) num3; ++num3)
            component.TargetAddElement(this.connectionToClient, Environment.NewLine + "You have to wait $time seconds to use <color=#0020ed><b>Stalk</b></color>.".Replace("$time", (num2 - (float) num3).ToString("00")), 1U, false);
          Scp106PlayerScript.disableFor = (float) ((double) Time.time + (double) num3 + 1.0);
        }
        else
        {
          Scp106PlayerScript.disableFor = Time.time + 4f;
          Timing.RunCoroutine(this.StalkCoroutine(component), 0);
        }
      }
    }
  }

  public void CallCmdUsePortal()
  {
    if (!this._interactRateLimit.CanExecute(true) || !this.GetComponent<FallDamage>().isGrounded || (!this.iAm106 || !(this.portalPosition != Vector3.zero)) || this.goingViaThePortal)
      return;
    Timing.RunCoroutine(this._DoTeleportAnimation(), Segment.Update);
  }

  public void CallCmdMovePlayer(GameObject ply, int t)
  {
    if (!this._iawRateLimit.CanExecute(true) || (UnityEngine.Object) ply == (UnityEngine.Object) null)
      return;
    CharacterClassManager component1 = ply.GetComponent<CharacterClassManager>();
    if ((UnityEngine.Object) component1 == (UnityEngine.Object) null || !ServerTime.CheckSynchronization(t) || (!this.iAm106 || (double) Vector3.Distance(this.GetComponent<PlyMovementSync>().RealModelPosition, ply.transform.position) >= 3.0) || !component1.IsHuman())
      return;
    CharacterClassManager component2 = ply.GetComponent<CharacterClassManager>();
    if (component2.GodMode || component2.Classes.SafeGet(component2.CurClass).team == Team.SCP)
      return;
    this.GetComponent<CharacterClassManager>().RpcPlaceBlood(ply.transform.position, 1, 2f);
    if (Scp106PlayerScript.blastDoor.isClosed)
    {
      this.GetComponent<CharacterClassManager>().RpcPlaceBlood(ply.transform.position, 1, 2f);
      this.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(500f, this.GetComponent<NicknameSync>().MyNick + " (" + this.GetComponent<CharacterClassManager>().UserId + ")", DamageTypes.Scp106, this.GetComponent<QueryProcessor>().PlayerId), ply);
    }
    else
    {
      this.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(40f, this.GetComponent<NicknameSync>().MyNick + " (" + this.GetComponent<CharacterClassManager>().UserId + ")", DamageTypes.Scp106, this.GetComponent<QueryProcessor>().PlayerId), ply);
      ply.GetComponent<PlyMovementSync>().OverridePosition(Vector3.down * 1998.5f, 0.0f, true);
      CharacterClassManager component3 = ply.GetComponent<CharacterClassManager>();
      foreach (Scp079PlayerScript instance in Scp079PlayerScript.instances)
      {
        Scp079Interactable.ZoneAndRoom otherRoom = ply.GetComponent<Scp079PlayerScript>().GetOtherRoom();
        Scp079Interactable.InteractableType[] filter = new Scp079Interactable.InteractableType[5]
        {
          Scp079Interactable.InteractableType.Door,
          Scp079Interactable.InteractableType.Light,
          Scp079Interactable.InteractableType.Lockdown,
          Scp079Interactable.InteractableType.Tesla,
          Scp079Interactable.InteractableType.ElevatorUse
        };
        bool flag = false;
        foreach (Scp079Interaction scp079Interaction in instance.ReturnRecentHistory(12f, filter))
        {
          foreach (Scp079Interactable.ZoneAndRoom currentZonesAndRoom in scp079Interaction.interactable.currentZonesAndRooms)
          {
            if (currentZonesAndRoom.currentZone == otherRoom.currentZone && currentZonesAndRoom.currentRoom == otherRoom.currentRoom)
              flag = true;
          }
        }
        if (flag)
          instance.RpcGainExp(ExpGainType.PocketAssist, component3.CurClass);
      }
    }
    PlayerEffectsController componentInParent = ply.GetComponentInParent<PlayerEffectsController>();
    componentInParent.GetEffect<Corroding>("Corroding").isInPd = true;
    componentInParent.EnableEffect("Corroding");
  }

  protected static void InvokeRpcRpcContainAnimation(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcContainAnimation called on server.");
    else
      ((Scp106PlayerScript) obj).CallRpcContainAnimation();
  }

  protected static void InvokeRpcRpcTeleportAnimation(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcTeleportAnimation called on server.");
    else
      ((Scp106PlayerScript) obj).CallRpcTeleportAnimation();
  }

  public void CallRpcContainAnimation()
  {
  }

  public void CallRpcTeleportAnimation()
  {
  }

  static Scp106PlayerScript()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp106PlayerScript), "CmdMakePortal", new NetworkBehaviour.CmdDelegate(Scp106PlayerScript.InvokeCmdCmdMakePortal));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp106PlayerScript), "CmdUsePortal", new NetworkBehaviour.CmdDelegate(Scp106PlayerScript.InvokeCmdCmdUsePortal));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp106PlayerScript), "CmdMovePlayer", new NetworkBehaviour.CmdDelegate(Scp106PlayerScript.InvokeCmdCmdMovePlayer));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Scp106PlayerScript), "RpcContainAnimation", new NetworkBehaviour.CmdDelegate(Scp106PlayerScript.InvokeRpcRpcContainAnimation));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Scp106PlayerScript), "RpcTeleportAnimation", new NetworkBehaviour.CmdDelegate(Scp106PlayerScript.InvokeRpcRpcTeleportAnimation));
    Scp106PlayerScript.parser = new string[18]
    {
      "<color=#F00>SCP-173</color>",
      "<color=#FF8E00>Class D</color>",
      "spec",
      "<color=#F00>SCP-106</color>",
      "<color=#0096FF>NTF Scientist</color>",
      "<color=#F00>SCP-049</color>",
      "<color=#FFFF7CFF>Scientist</color>",
      "pc",
      "<color=#008f1e>Chaos Insurgent</color>",
      "<color=#f00>SCP-096</color>",
      "<color=#f00>Zombie</color>",
      "<color=#0096FF>NTF Lieutenant</color>",
      "<color=#0096FF>NTF Commander</color>",
      "<color=#0096FF>NTF Cadet</color>",
      "Tutorial",
      "<color=#59636f>Facility Guard</color>",
      "<color=#f00>SCP-939-53</color>",
      "<color=#f00>SCP-939-89</color>"
    };
    Scp106PlayerScript.stalky106LastTick = 0.0f;
    Scp106PlayerScript.stalkyCd = Time.time + 120f;
    Scp106PlayerScript.disableFor = 0.0f;
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteSingle(this.remainingPoints);
      writer.WriteVector3(this.portalPosition);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteSingle(this.remainingPoints);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WriteVector3(this.portalPosition);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworkremainingPoints = reader.ReadSingle();
      Vector3 pos = reader.ReadVector3();
      this.SetPortalPosition(pos);
      this.NetworkportalPosition = pos;
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
        this.NetworkremainingPoints = reader.ReadSingle();
      if ((num & 2L) == 0L)
        return;
      Vector3 pos = reader.ReadVector3();
      this.SetPortalPosition(pos);
      this.NetworkportalPosition = pos;
    }
  }

    private IEnumerator<float> StalkCoroutine(Broadcast bc)
    {
        List<GameObject> list = new List<GameObject>();
        foreach (GameObject gameObject in PlayerManager.players)
        {
            if (gameObject.GetComponent<CharacterClassManager>().CurClass != RoleType.ChaosInsurgency && gameObject.GetComponent<CharacterClassManager>().CurClass != RoleType.Spectator && gameObject.GetComponent<CharacterClassManager>().CurClass != RoleType.Tutorial && gameObject.GetComponent<CharacterClassManager>().IsHuman())
            {
                list.Add(gameObject);
            }
        }
        if (list.Count < 1)
        {
            bc.TargetAddElement(base.connectionToClient, "No valid player found.", 4U, false);
            yield break;
        }
        Scp106PlayerScript.stalky106LastTick = Time.time;
        GameObject gameObject2;
        RaycastHit raycastHit;
        do
        {
            int index = UnityEngine.Random.Range(0, list.Count);
            gameObject2 = list[index];
            Physics.Raycast(new Ray(gameObject2.transform.position, -Vector3.up), out raycastHit, 10f, this.teleportPlacementMask);
            if (Vector3.Distance(gameObject2.transform.position, new Vector3(0f, -1998f, 0f)) < 40f)
            {
                gameObject2 = null;
                raycastHit.point = Vector3.zero;
            }
            list.RemoveAt(index);
        }
        while (raycastHit.point.Equals(Vector3.zero) && list.Count > 0);
        if (gameObject2 == null)
        {
            bc.TargetAddElement(base.connectionToClient, "No valid player found.", 4U, false);
            yield break;
        }
        if (raycastHit.point.Equals(Vector3.zero))
        {
            bc.TargetAddElement(base.connectionToClient, "An error has ocurred. Try it again in a few seconds.", 4U, false);
            yield break;
        }
        this.MovePortal(raycastHit.point - Vector3.up);
        Scp106PlayerScript.stalkyCd = Time.time + 240f;
        Timing.RunCoroutine(Scp106PlayerScript.StalkyCooldownAnnounce(240f), 1);
        Scp106PlayerScript.stalky106LastTick = Time.time;
        Scp106PlayerScript.disableFor = Time.time + 10f;
        string data = string.Concat(new string[]
        {
            Environment.NewLine,
            "<i>You will <color=#0020ed><b>stalk</b></color><b>",
            gameObject2.GetComponent<NicknameSync>().MyNick,
            "</b>, who is a ",
            Scp106PlayerScript.parser[(int)gameObject2.GetComponent<CharacterClassManager>().CurClass],
            "</i>\n<size=30><color=#FFFFFF66>Cooldown: 6</color></size>"
        });
        bc.TargetAddElement(base.connectionToClient, data, 5U, false);
        yield break;
    }

    private void MovePortal(Vector3 pos)
  {
    Timing.RunCoroutine(this.PortalProcedure(pos), Segment.FixedUpdate);
  }

  private IEnumerator<float> PortalProcedure(Vector3 pos)
  {
    yield return 0.0f;
    Scp106PlayerScript component = PlayerManager.localPlayer.GetComponent<Scp106PlayerScript>();
    component.NetworkportalPosition = pos;
    Animator anim = component.portalPrefab.GetComponent<Animator>();
    anim.SetBool("activated", false);
    component.portalPrefab.transform.position = pos;
    Timing.RunCoroutine(this.ForceTeleportLarry(), 1);
    yield return Timing.WaitForSeconds(1f);
    anim.SetBool("activated", true);
  }

  private IEnumerator<float> ForceTeleportLarry()
  {
    yield return Timing.WaitForSeconds(0.1f);
    do
    {
      this.CallCmdUsePortal();
      yield return 0.0f;
    }
    while (!this.goingViaThePortal);
  }

  public static IEnumerator<float> StalkyCooldownAnnounce(float duration)
  {
    yield return Timing.WaitForSeconds(duration);
    Broadcast component = PlayerManager.localPlayer.GetComponent<Broadcast>();
    foreach (GameObject player in PlayerManager.players)
    {
      if (player.GetComponent<CharacterClassManager>().CurClass == RoleType.Scp106)
        component.TargetAddElement(player.GetComponent<NetworkIdentity>().connectionToClient, Environment.NewLine + "<b><color=#0020ed><b>Stalk</b></color> <color=#00e861>ready</color></b>." + Environment.NewLine + "<size=30>Press the portal creation button twice to use it.</size>", 5U, false);
    }
  }

  public static void InitializeStalky106()
  {
    Scp106PlayerScript.stalky106LastTick = 0.0f;
    Scp106PlayerScript.stalkyCd = Time.time + 80f;
    Scp106PlayerScript.disableFor = Time.time + 12f;
    Timing.RunCoroutine(Scp106PlayerScript.StalkyCooldownAnnounce(80f), 1);
  }
}
