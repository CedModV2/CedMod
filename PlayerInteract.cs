// Decompiled with JetBrains decompiler
// Type: PlayerInteract
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using CustomPlayerEffects;
using GameCore;
using Mirror;
using Scp914;
using Security;
using System;
using System.Linq;
using UnityEngine;

public class PlayerInteract : NetworkBehaviour
{
  private string uiToggleKey = "numlock";
  public GameObject playerCamera;
  public LayerMask mask;
  public float raycastMaxDistance;
  private CharacterClassManager _ccm;
  private ServerRoles _sr;
  private Inventory _inv;
  private Handcuffs _hc;
  private bool enableUiToggle;
  private Scp268 scp268;
  private bool _096DestroyLockedDoors;
  private bool CanDisarmedInteract;
  private RateLimit _scp106ContSoundRateLimit;
  private RateLimit _playerInteractRateLimit;
  private bool neonRemoteKeycard;

  private void Start()
  {
    this._scp106ContSoundRateLimit = new RateLimit(1, 8.5f, (NetworkConnection) null);
    this._playerInteractRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[0];
    this._096DestroyLockedDoors = ConfigFile.ServerConfig.GetBool("096_destroy_locked_doors", true);
    this.CanDisarmedInteract = ConfigFile.ServerConfig.GetBool("allow_disarmed_interaction", false);
    this._ccm = this.GetComponent<CharacterClassManager>();
    this._sr = this.GetComponent<ServerRoles>();
    this._inv = this.GetComponent<Inventory>();
    this.scp268 = this.GetComponent<PlayerEffectsController>().GetEffect<Scp268>("SCP-268");
    this._hc = this.GetComponent<Handcuffs>();
    this.neonRemoteKeycard = ConfigFile.ServerConfig.GetBool("neon_remotekeycard", true);
  }

  private void Update()
  {
  }

  [Command]
  private void CmdUse914()
  {
    if (this.isServer)
    {
      this.CallCmdUse914();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (PlayerInteract), nameof (CmdUse914), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdUseLocker(int lockerId, int chamberNumber)
  {
    if (this.isServer)
    {
      this.CallCmdUseLocker(lockerId, chamberNumber);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WritePackedInt32(lockerId);
      writer.WritePackedInt32(chamberNumber);
      this.SendCommandInternal(typeof (PlayerInteract), nameof (CmdUseLocker), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdUseGenerator(string command, GameObject go)
  {
    if (this.isServer)
    {
      this.CallCmdUseGenerator(command, go);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(command);
      writer.WriteGameObject(go);
      this.SendCommandInternal(typeof (PlayerInteract), nameof (CmdUseGenerator), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdChange914Knob()
  {
    if (this.isServer)
    {
      this.CallCmdChange914Knob();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (PlayerInteract), nameof (CmdChange914Knob), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  public void CmdUseWorkStation_Place(GameObject station)
  {
    if (this.isServer)
    {
      this.CallCmdUseWorkStation_Place(station);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(station);
      this.SendCommandInternal(typeof (PlayerInteract), nameof (CmdUseWorkStation_Place), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  public void CmdUseWorkStation_Take(GameObject station)
  {
    if (this.isServer)
    {
      this.CallCmdUseWorkStation_Take(station);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(station);
      this.SendCommandInternal(typeof (PlayerInteract), nameof (CmdUseWorkStation_Take), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdUsePanel(string n)
  {
    if (this.isServer)
    {
      this.CallCmdUsePanel(n);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(n);
      this.SendCommandInternal(typeof (PlayerInteract), nameof (CmdUsePanel), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [ClientRpc]
  private void RpcLeverSound()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (PlayerInteract), nameof (RpcLeverSound), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [Command]
  private void CmdUseElevator(GameObject elevator)
  {
    if (this.isServer)
    {
      this.CallCmdUseElevator(elevator);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(elevator);
      this.SendCommandInternal(typeof (PlayerInteract), nameof (CmdUseElevator), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdSwitchAWButton()
  {
    if (this.isServer)
    {
      this.CallCmdSwitchAWButton();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (PlayerInteract), nameof (CmdSwitchAWButton), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdDetonateWarhead()
  {
    if (this.isServer)
    {
      this.CallCmdDetonateWarhead();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (PlayerInteract), nameof (CmdDetonateWarhead), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command(channel = 5)]
  private void CmdOpenDoor(GameObject doorId)
  {
    if (this.isServer)
    {
      this.CallCmdOpenDoor(doorId);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(doorId);
      this.SendCommandInternal(typeof (PlayerInteract), nameof (CmdOpenDoor), writer, 5);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [ClientRpc(channel = 5)]
  private void RpcDenied(GameObject door)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteGameObject(door);
    this.SendRPCInternal(typeof (PlayerInteract), nameof (RpcDenied), writer, 5);
    NetworkWriterPool.Recycle(writer);
  }

  private bool ChckDis(Vector3 pos)
  {
    return TutorialManager.status || (double) Vector3.Distance(this.GetComponent<PlyMovementSync>().RealModelPosition, pos) < (double) this.raycastMaxDistance * 1.5;
  }

  [Command]
  private void CmdContain106()
  {
    if (this.isServer)
    {
      this.CallCmdContain106();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (PlayerInteract), nameof (CmdContain106), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [ClientRpc]
  private void RpcContain106(GameObject executor)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteGameObject(executor);
    this.SendRPCInternal(typeof (PlayerInteract), nameof (RpcContain106), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  public void OnInteract()
  {
    this.scp268.ServerDisable();
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeCmdCmdUse914(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdUse914 called on client.");
    else
      ((PlayerInteract) obj).CallCmdUse914();
  }

  protected static void InvokeCmdCmdUseLocker(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdUseLocker called on client.");
    else
      ((PlayerInteract) obj).CallCmdUseLocker(reader.ReadPackedInt32(), reader.ReadPackedInt32());
  }

  protected static void InvokeCmdCmdUseGenerator(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdUseGenerator called on client.");
    else
      ((PlayerInteract) obj).CallCmdUseGenerator(reader.ReadString(), reader.ReadGameObject());
  }

  protected static void InvokeCmdCmdChange914Knob(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdChange914Knob called on client.");
    else
      ((PlayerInteract) obj).CallCmdChange914Knob();
  }

  protected static void InvokeCmdCmdUseWorkStation_Place(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdUseWorkStation_Place called on client.");
    else
      ((PlayerInteract) obj).CallCmdUseWorkStation_Place(reader.ReadGameObject());
  }

  protected static void InvokeCmdCmdUseWorkStation_Take(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdUseWorkStation_Take called on client.");
    else
      ((PlayerInteract) obj).CallCmdUseWorkStation_Take(reader.ReadGameObject());
  }

  protected static void InvokeCmdCmdUsePanel(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdUsePanel called on client.");
    else
      ((PlayerInteract) obj).CallCmdUsePanel(reader.ReadString());
  }

  protected static void InvokeCmdCmdUseElevator(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdUseElevator called on client.");
    else
      ((PlayerInteract) obj).CallCmdUseElevator(reader.ReadGameObject());
  }

  protected static void InvokeCmdCmdSwitchAWButton(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSwitchAWButton called on client.");
    else
      ((PlayerInteract) obj).CallCmdSwitchAWButton();
  }

  protected static void InvokeCmdCmdDetonateWarhead(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdDetonateWarhead called on client.");
    else
      ((PlayerInteract) obj).CallCmdDetonateWarhead();
  }

  protected static void InvokeCmdCmdOpenDoor(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdOpenDoor called on client.");
    else
      ((PlayerInteract) obj).CallCmdOpenDoor(reader.ReadGameObject());
  }

  protected static void InvokeCmdCmdContain106(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdContain106 called on client.");
    else
      ((PlayerInteract) obj).CallCmdContain106();
  }

  public void CallCmdUse914()
  {
    if (!this._playerInteractRateLimit.CanExecute(true) || this._hc.CufferId > 0 && !this.CanDisarmedInteract || (Scp914Machine.singleton.working || !this.ChckDis(Scp914Machine.singleton.button.position)))
      return;
    Scp914Machine.singleton.RpcActivate(NetworkTime.time);
    this.OnInteract();
  }

  public void CallCmdUseLocker(int lockerId, int chamberNumber)
  {
    if (!this._playerInteractRateLimit.CanExecute(true) || this._hc.CufferId > 0 && !this.CanDisarmedInteract)
      return;
    LockerManager singleton = LockerManager.singleton;
    if (lockerId < 0 || lockerId >= singleton.lockers.Length || (!this.ChckDis(singleton.lockers[lockerId].gameObject.position) || !singleton.lockers[lockerId].supportsStandarizedAnimation) || (chamberNumber < 0 || chamberNumber >= singleton.lockers[lockerId].chambers.Length || ((UnityEngine.Object) singleton.lockers[lockerId].chambers[chamberNumber].doorAnimator == (UnityEngine.Object) null || !singleton.lockers[lockerId].chambers[chamberNumber].CooldownAtZero())))
      return;
    singleton.lockers[lockerId].chambers[chamberNumber].SetCooldown();
    string accessToken = singleton.lockers[lockerId].chambers[chamberNumber].accessToken;
    Item itemById = this._inv.GetItemByID(this._inv.curItem);
    if (this._sr.BypassMode || string.IsNullOrEmpty(accessToken) || itemById != null && itemById.permissions.Contains<string>(accessToken))
    {
      bool flag = ((int) singleton.openLockers[lockerId] & 1 << chamberNumber) != 1 << chamberNumber;
      singleton.ModifyOpen(lockerId, chamberNumber, flag);
      singleton.RpcDoSound(lockerId, chamberNumber, flag);
      bool state = true;
      for (int index = 0; index < singleton.lockers[lockerId].chambers.Length; ++index)
      {
        if (((int) singleton.openLockers[lockerId] & 1 << index) == 1 << index)
        {
          state = false;
          break;
        }
      }
      singleton.lockers[lockerId].LockPickups(state);
      if (!string.IsNullOrEmpty(accessToken))
        singleton.RpcChangeMaterial(lockerId, chamberNumber, false);
    }
    else
      singleton.RpcChangeMaterial(lockerId, chamberNumber, true);
    this.OnInteract();
  }

  public void CallCmdUseGenerator(string command, GameObject go)
  {
    if (!this._playerInteractRateLimit.CanExecute(true) || this._hc.CufferId > 0 && !this.CanDisarmedInteract || (UnityEngine.Object) go == (UnityEngine.Object) null)
      return;
    Generator079 component = go.GetComponent<Generator079>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      return;
    if (this.ChckDis(go.transform.position))
    {
      component.Interact(this.gameObject, command);
      this.OnInteract();
    }
    else
      Debug.Log((object) "Command aborted");
  }

  public void CallCmdChange914Knob()
  {
    if (!this._playerInteractRateLimit.CanExecute(true) || this._hc.CufferId > 0 && !this.CanDisarmedInteract || (Scp914Machine.singleton.working || !this.ChckDis(Scp914Machine.singleton.knob.position)))
      return;
    Scp914Machine.singleton.ChangeKnobStatus();
    this.OnInteract();
  }

  public void CallCmdUseWorkStation_Place(GameObject station)
  {
    if (!this._playerInteractRateLimit.CanExecute(true) || this._hc.CufferId > 0 && !this.CanDisarmedInteract || ((UnityEngine.Object) station == (UnityEngine.Object) null || !this.ChckDis(station.transform.position)))
      return;
    WorkStation component = station.GetComponent<WorkStation>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      return;
    component.ConnectTablet(this.gameObject);
    this.OnInteract();
  }

  public void CallCmdUseWorkStation_Take(GameObject station)
  {
    if (!this._playerInteractRateLimit.CanExecute(true) || this._hc.CufferId > 0 && !this.CanDisarmedInteract || ((UnityEngine.Object) station == (UnityEngine.Object) null || !this.ChckDis(station.transform.position)))
      return;
    WorkStation component = station.GetComponent<WorkStation>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      return;
    component.UnconnectTablet(this.gameObject);
    this.OnInteract();
  }

  public void CallCmdUsePanel(string n)
  {
    if (!this._playerInteractRateLimit.CanExecute(true) || this._hc.CufferId > 0 && !this.CanDisarmedInteract || n == null)
      return;
    AlphaWarheadNukesitePanel nukeside = AlphaWarheadOutsitePanel.nukeside;
    if (!this.ChckDis(nukeside.transform.position))
      return;
    if (n.Contains("cancel"))
    {
      this.OnInteract();
      AlphaWarheadController.Host.CancelDetonation(this.gameObject);
      ServerLogs.AddLog(ServerLogs.Modules.Warhead, this.GetComponent<NicknameSync>().MyNick + " (" + this.GetComponent<CharacterClassManager>().UserId + ") cancelled the Alpha Warhead detonation.", ServerLogs.ServerLogType.GameEvent);
    }
    else
    {
      if (!n.Contains("lever"))
        return;
      this.OnInteract();
      if (!nukeside.AllowChangeLevelState())
        return;
      nukeside.Networkenabled = !nukeside.enabled;
      this.RpcLeverSound();
      ServerLogs.AddLog(ServerLogs.Modules.Warhead, this.GetComponent<NicknameSync>().MyNick + " (" + this.GetComponent<CharacterClassManager>().UserId + ") set the Alpha Warhead status to " + nukeside.enabled.ToString() + ".", ServerLogs.ServerLogType.GameEvent);
    }
  }

  public void CallCmdUseElevator(GameObject elevator)
  {
    if (!this._playerInteractRateLimit.CanExecute(true) || this._hc.CufferId > 0 && !this.CanDisarmedInteract || (UnityEngine.Object) elevator == (UnityEngine.Object) null)
      return;
    Lift component = elevator.GetComponent<Lift>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
      return;
    foreach (Lift.Elevator elevator1 in component.elevators)
    {
      if (this.ChckDis(elevator1.door.transform.position))
      {
        elevator.GetComponent<Lift>().UseLift();
        this.OnInteract();
      }
    }
  }

  public void CallCmdSwitchAWButton()
  {
    if (!this._playerInteractRateLimit.CanExecute(true) || this._hc.CufferId > 0 && !this.CanDisarmedInteract)
      return;
    GameObject gameObject = GameObject.Find("OutsitePanelScript");
    if (!this.ChckDis(gameObject.transform.position) || !this._inv.GetItemByID(this._inv.curItem).permissions.Contains<string>("CONT_LVL_3"))
      return;
    gameObject.GetComponentInParent<AlphaWarheadOutsitePanel>().NetworkkeycardEntered = true;
    this.OnInteract();
  }

  public void CallCmdDetonateWarhead()
  {
    if (!this._playerInteractRateLimit.CanExecute(true) || this._hc.CufferId > 0 && !this.CanDisarmedInteract || !this._playerInteractRateLimit.CanExecute(true))
      return;
    GameObject gameObject = GameObject.Find("OutsitePanelScript");
    if (!this.ChckDis(gameObject.transform.position) || !AlphaWarheadOutsitePanel.nukeside.enabled || !gameObject.GetComponent<AlphaWarheadOutsitePanel>().keycardEntered)
      return;
    AlphaWarheadController.Host.StartDetonation();
    ServerLogs.AddLog(ServerLogs.Modules.Warhead, this.GetComponent<NicknameSync>().MyNick + " (" + this.GetComponent<CharacterClassManager>().UserId + ") started the Alpha Warhead detonation.", ServerLogs.ServerLogType.GameEvent);
    this.OnInteract();
  }

  public void CallCmdOpenDoor(GameObject doorId)
  {
    if (!this._playerInteractRateLimit.CanExecute(true) || this._hc.CufferId > 0 && !this.CanDisarmedInteract || ((UnityEngine.Object) doorId == (UnityEngine.Object) null || this._ccm.CurClass == RoleType.None || this._ccm.CurClass == RoleType.Spectator))
      return;
    Door component1 = doorId.GetComponent<Door>();
    if ((UnityEngine.Object) component1 == (UnityEngine.Object) null || (component1.buttons.Count == 0 ? (this.ChckDis(doorId.transform.position) ? 1 : 0) : (component1.buttons.Any<GameObject>((Func<GameObject, bool>) (item => this.ChckDis(item.transform.position))) ? 1 : 0)) == 0)
      return;
    Scp096PlayerScript component2 = this.GetComponent<Scp096PlayerScript>();
    if ((UnityEngine.Object) component1.destroyedPrefab != (UnityEngine.Object) null && (!component1.isOpen || (double) component1.curCooldown > 0.0) && (component2.iAm096 && component2.enraged == Scp096PlayerScript.RageState.Enraged))
    {
      if (!this._096DestroyLockedDoors && component1.locked && !this._sr.BypassMode)
        return;
      component1.DestroyDoor(true);
    }
    else
    {
      this.OnInteract();
      if (this._sr.BypassMode)
      {
        component1.ChangeState(true);
      }
      else
      {
        if (this.neonRemoteKeycard)
        {
          try
          {
            Inventory.SyncListItemInfo items = this._inv.items;
            for (int index = 0; index < items.Count; ++index)
            {
              if (this._inv.GetItemByID(items[index].id).permissions.Contains<string>(component1.permissionLevel))
              {
                if (!component1.locked)
                {
                  component1.ChangeState(false);
                  return;
                }
                this.RpcDenied(doorId);
                return;
              }
            }
          }
          catch
          {
            this.RpcDenied(doorId);
          }
        }
        if (string.Equals(component1.permissionLevel, "CHCKPOINT_ACC", StringComparison.OrdinalIgnoreCase) && this.GetComponent<CharacterClassManager>().Classes.SafeGet(this.GetComponent<CharacterClassManager>().CurClass).team == Team.SCP)
        {
          component1.ChangeState(false);
        }
        else
        {
          try
          {
            if (string.IsNullOrEmpty(component1.permissionLevel))
            {
              if (component1.locked)
                return;
              component1.ChangeState(false);
            }
            else if (this._inv.GetItemByID(this._inv.curItem).permissions.Contains<string>(component1.permissionLevel))
            {
              if (!component1.locked)
                component1.ChangeState(false);
              else
                this.RpcDenied(doorId);
            }
            else
              this.RpcDenied(doorId);
          }
          catch
          {
            this.RpcDenied(doorId);
          }
        }
      }
    }
  }

  public void CallCmdContain106()
  {
    if (!this._playerInteractRateLimit.CanExecute(true) || this._hc.CufferId > 0 && !this.CanDisarmedInteract || (!UnityEngine.Object.FindObjectOfType<LureSubjectContainer>().allowContain || this._ccm.Classes.SafeGet(this._ccm.CurClass).team == Team.SCP && this._ccm.CurClass != RoleType.Scp106) || (!this.ChckDis(GameObject.FindGameObjectWithTag("FemurBreaker").transform.position) || UnityEngine.Object.FindObjectOfType<OneOhSixContainer>().used || this._ccm.Classes.SafeGet(this._ccm.CurClass).team == Team.RIP))
      return;
    bool flag = false;
    foreach (GameObject player in PlayerManager.players)
    {
      if (player.GetComponent<CharacterClassManager>().GodMode && player.GetComponent<CharacterClassManager>().CurClass == RoleType.Scp106)
        flag = true;
    }
    if (!flag)
    {
      foreach (GameObject player in PlayerManager.players)
      {
        if (player.GetComponent<CharacterClassManager>().CurClass == RoleType.Scp106)
          player.GetComponent<Scp106PlayerScript>().Contain(this._ccm);
      }
      this.RpcContain106(this.gameObject);
      UnityEngine.Object.FindObjectOfType<OneOhSixContainer>().Networkused = true;
    }
    this.OnInteract();
  }

  protected static void InvokeRpcRpcLeverSound(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcLeverSound called on server.");
    else
      ((PlayerInteract) obj).CallRpcLeverSound();
  }

  protected static void InvokeRpcRpcDenied(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcDenied called on server.");
    else
      ((PlayerInteract) obj).CallRpcDenied(reader.ReadGameObject());
  }

  protected static void InvokeRpcRpcContain106(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcContain106 called on server.");
    else
      ((PlayerInteract) obj).CallRpcContain106(reader.ReadGameObject());
  }

  public void CallRpcLeverSound()
  {
    AlphaWarheadOutsitePanel.nukeside.lever.GetComponent<AudioSource>().Play();
  }

  public void CallRpcDenied(GameObject door)
  {
  }

  public void CallRpcContain106(GameObject executor)
  {
    if (!this._scp106ContSoundRateLimit.CanExecute(true))
      return;
    UnityEngine.Object.Instantiate<GameObject>(this.GetComponent<Scp106PlayerScript>().screamsPrefab);
    if ((UnityEngine.Object) executor != (UnityEngine.Object) this.gameObject)
      return;
    foreach (GameObject player in PlayerManager.players)
    {
      if (player.GetComponent<CharacterClassManager>().CurClass == RoleType.Scp106)
        AchievementManager.Achieve("securecontainprotect", true);
    }
  }

  static PlayerInteract()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlayerInteract), "CmdUse914", new NetworkBehaviour.CmdDelegate(PlayerInteract.InvokeCmdCmdUse914));
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlayerInteract), "CmdUseLocker", new NetworkBehaviour.CmdDelegate(PlayerInteract.InvokeCmdCmdUseLocker));
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlayerInteract), "CmdUseGenerator", new NetworkBehaviour.CmdDelegate(PlayerInteract.InvokeCmdCmdUseGenerator));
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlayerInteract), "CmdChange914Knob", new NetworkBehaviour.CmdDelegate(PlayerInteract.InvokeCmdCmdChange914Knob));
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlayerInteract), "CmdUseWorkStation_Place", new NetworkBehaviour.CmdDelegate(PlayerInteract.InvokeCmdCmdUseWorkStation_Place));
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlayerInteract), "CmdUseWorkStation_Take", new NetworkBehaviour.CmdDelegate(PlayerInteract.InvokeCmdCmdUseWorkStation_Take));
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlayerInteract), "CmdUsePanel", new NetworkBehaviour.CmdDelegate(PlayerInteract.InvokeCmdCmdUsePanel));
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlayerInteract), "CmdUseElevator", new NetworkBehaviour.CmdDelegate(PlayerInteract.InvokeCmdCmdUseElevator));
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlayerInteract), "CmdSwitchAWButton", new NetworkBehaviour.CmdDelegate(PlayerInteract.InvokeCmdCmdSwitchAWButton));
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlayerInteract), "CmdDetonateWarhead", new NetworkBehaviour.CmdDelegate(PlayerInteract.InvokeCmdCmdDetonateWarhead));
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlayerInteract), "CmdOpenDoor", new NetworkBehaviour.CmdDelegate(PlayerInteract.InvokeCmdCmdOpenDoor));
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlayerInteract), "CmdContain106", new NetworkBehaviour.CmdDelegate(PlayerInteract.InvokeCmdCmdContain106));
    NetworkBehaviour.RegisterRpcDelegate(typeof (PlayerInteract), "RpcLeverSound", new NetworkBehaviour.CmdDelegate(PlayerInteract.InvokeRpcRpcLeverSound));
    NetworkBehaviour.RegisterRpcDelegate(typeof (PlayerInteract), "RpcDenied", new NetworkBehaviour.CmdDelegate(PlayerInteract.InvokeRpcRpcDenied));
    NetworkBehaviour.RegisterRpcDelegate(typeof (PlayerInteract), "RpcContain106", new NetworkBehaviour.CmdDelegate(PlayerInteract.InvokeRpcRpcContain106));
  }
}
