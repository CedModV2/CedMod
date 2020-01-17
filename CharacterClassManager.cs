// Decompiled with JetBrains decompiler
// Type: CharacterClassManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Cryptography;
using GameCore;
using MEC;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using RemoteAdmin;
using Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

public class CharacterClassManager : NetworkBehaviour
{
  public List<Team> ClassTeamQueue = new List<Team>();
  private readonly List<int> _ids = new List<int>();
  public RoleType ForceClass = RoleType.None;
  private int _seed = -1;
  private RoleType _prevId = RoleType.None;
  private readonly RateLimit _deathScreenRateLimit = new RateLimit(2, 4f, (NetworkConnection) null);
  public GameObject UnfocusedCamera;
  public Role[] Classes;
  private GameObject _plyCam;
  private CentralAuthInterface _centralAuthInt;
  private PlyMovementSync _pms;
  private Searching _searching;
  private static GameObject _host;
  public static SpawnpointManager SpawnpointManager;
  private static Role[] _staticClasses;
  public float CiPercentage;
  [NonSerialized]
  public float AliveTime;
  [NonSerialized]
  public long DeathTime;
  [NonSerialized]
  public string UserId2;
  public static bool OnlineMode;
  [NonSerialized]
  public bool GodMode;
  [NonSerialized]
  public bool IsHost;
  private bool _wasAnytimeAlive;
  private bool _commandtokensent;
  private bool _enableSyncServerCmdBinding;
  [NonSerialized]
  internal string AuthToken;
  [NonSerialized]
  internal string AuthTokenSerial;
  [NonSerialized]
  public string RequestIp;
  [NonSerialized]
  public string Asn;
  public bool LaterJoinEnabled;
  public float LaterJoinTime;
  public bool SpawnProtected;
  public float ProtectedTime;
  public bool EnableSP;
  public float SProtectedDuration;
  public List<int> SProtectedTeam;
  public bool KeepItemsAfterEscaping;
  public bool PutItemsInInvAfterEscaping;
  [NonSerialized]
  public int EscapeStartTime;
  [NonSerialized]
  public bool CheatReported;
  [SerializeField]
  public AudioClip Bell;
  [SerializeField]
  public AudioClip BellDead;
  [HideInInspector]
  public GameObject MyModel;
  [SyncVar(hook = "SetMuted")]
  public bool Muted;
  [SyncVar]
  public bool IntercomMuted;
  [SyncVar]
  public bool NoclipEnabled;
  [HideInInspector]
  public Scp049PlayerScript Scp049;
  [HideInInspector]
  public Scp049_2PlayerScript Scp0492;
  [HideInInspector]
  public Scp079PlayerScript Scp079;
  [HideInInspector]
  public Scp096PlayerScript Scp096;
  [HideInInspector]
  public Scp106PlayerScript Scp106;
  [HideInInspector]
  public Scp173PlayerScript Scp173;
  [HideInInspector]
  public Scp939PlayerScript Scp939;
  private LureSubjectContainer _lureSpj;
  [SyncVar(hook = "SetClassID")]
  public RoleType CurClass;
  [SyncVar]
  public Vector3 DeathPosition;
  [SyncVar]
  public int NtfUnit;
  [SyncVar]
  public bool RoundStarted;
  [SyncVar]
  public bool IsVerified;
  [SyncVar(hook = "UserIdHook")]
  public string SyncedUserId;
  private string _privUserId;
  private static bool _hostFound;
  private RateLimit _interactRateLimit;
  private RateLimit _commandRateLimit;
  private int laterJoinNextIndex;

  internal ServerRoles SrvRoles { get; private set; }

  internal NetworkConnection Connection { get; private set; }

  public string VacSession { get; internal set; }

  public string UserId
  {
    get
    {
      if (!NetworkServer.active)
        return this.SyncedUserId;
      if (this._privUserId == null)
        return (string) null;
      return this._privUserId.Contains("$") ? this._privUserId.Substring(0, this._privUserId.IndexOf("$", StringComparison.Ordinal)) : this._privUserId;
    }
    set
    {
      if (!NetworkServer.active)
        return;
      this._privUserId = value;
      this.RefreshSyncedId();
    }
  }

  public string SaltedUserId
  {
    get
    {
      return !NetworkServer.active ? this.SyncedUserId : this._privUserId;
    }
  }

  [Server]
  public void RefreshSyncedId()
  {
    if (!NetworkServer.active)
      Debug.LogWarning((object) "[Server] function 'System.Void CharacterClassManager::RefreshSyncedId()' called on client");
    else if (this._privUserId == null)
      this.NetworkSyncedUserId = (string) null;
    else
      this.NetworkSyncedUserId = !this._privUserId.EndsWith("@steam") || this.SrvRoles.DoNotTrack ? Sha.HashToString(Sha.Sha512(this._privUserId)) : this._privUserId;
  }

  public bool InWorld
  {
    get
    {
      return this.CurClass != RoleType.Spectator && this.CurClass != RoleType.Scp079;
    }
  }

  private void Awake()
  {
    this.SrvRoles = this.GetComponent<ServerRoles>();
    if (!this.isServer)
      return;
    CharacterClassManager.ResetHostFound();
  }

  private void Start()
  {
    this._interactRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[0];
    this._commandRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[2];
    if ((UnityEngine.Object) CharacterClassManager.SpawnpointManager == (UnityEngine.Object) null)
      CharacterClassManager.SpawnpointManager = UnityEngine.Object.FindObjectOfType<SpawnpointManager>();
    if (this.isServer)
    {
      CharacterClassManager.OnlineMode = ConfigFile.ServerConfig.GetBool("online_mode", true);
      ReservedSlot.Reload();
      MuteHandler.Reload();
      WhiteList.Reload();
    }
    this._centralAuthInt = new CentralAuthInterface(this, this.isServer);
    this._pms = this.GetComponent<PlyMovementSync>();
    this._searching = this.GetComponent<Searching>();
    this._lureSpj = UnityEngine.Object.FindObjectOfType<LureSubjectContainer>();
    this.Connection = this.connectionToClient;
    this.Scp049 = this.GetComponent<Scp049PlayerScript>();
    this.Scp079 = this.GetComponent<Scp079PlayerScript>();
    this.Scp0492 = this.GetComponent<Scp049_2PlayerScript>();
    this.Scp106 = this.GetComponent<Scp106PlayerScript>();
    this.Scp173 = this.GetComponent<Scp173PlayerScript>();
    this.Scp096 = this.GetComponent<Scp096PlayerScript>();
    this.Scp939 = this.GetComponent<Scp939PlayerScript>();
    this.ForceClass = (RoleType) ConfigFile.ServerConfig.GetInt("server_forced_class", -1);
    this.CiPercentage = (float) ConfigFile.ServerConfig.GetInt("ci_on_start_percent", 10);
    this._enableSyncServerCmdBinding = ConfigFile.ServerConfig.GetBool("enable_sync_command_binding", false);
    this.EnableSP = !ConfigFile.ServerConfig.GetBool("spawn_protect_disable", true);
    this.SProtectedDuration = (float) ConfigFile.ServerConfig.GetInt("spawn_protect_time", 30);
    this.SProtectedTeam = new List<int>((IEnumerable<int>) ConfigFile.ServerConfig.GetIntList("spawn_protect_team"));
    if (this.SProtectedTeam.Count <= 0)
      this.SProtectedTeam = new List<int>() { 1, 2 };
    this.KeepItemsAfterEscaping = ConfigFile.ServerConfig.GetBool("keep_items_after_escaping", true);
    this.PutItemsInInvAfterEscaping = ConfigFile.ServerConfig.GetBool("items_in_inv_after_escaping", false);
    this.LaterJoinEnabled = ConfigFile.ServerConfig.GetBool("later_join_enabled", true);
    this.LaterJoinTime = ConfigFile.ServerConfig.GetFloat("later_join_time", 30f);
    this.StartCoroutine(this.Init());
    string str = ConfigFile.ServerConfig.GetString("team_respawn_queue", "401431403144144");
    this.ClassTeamQueue.Clear();
    foreach (char ch in str)
    {
      int result = 4;
      if (!int.TryParse(ch.ToString(), out result))
        result = 4;
      this.ClassTeamQueue.Add((Team) result);
    }
    while (this.ClassTeamQueue.Count < NetworkManager.singleton.maxConnections)
      this.ClassTeamQueue.Add(Team.CDP);
    if (!this.isLocalPlayer && TutorialManager.status)
      this.ApplyProperties(false, false);
    Regex regex = new Regex("[^\\w\\d]+");
    for (int index = 0; index <= this.Classes.Length - 1; ++index)
    {
      if (this.Classes[index].team != Team.SCP && this.Classes[index].team != Team.RIP)
      {
        List<int> intList = ConfigFile.ServerConfig.GetIntList(regex.Replace(this.Classes[index].fullName, "_").ToLower() + "_defaultammo");
        if (intList.Count == 3)
          this.Classes[index].ammoTypes = intList.ToArray();
      }
    }
    if (this.isLocalPlayer)
    {
      for (byte index = 0; (int) index < this.Classes.Length; ++index)
      {
        try
        {
          RoleExtensionMethods.Get(this.Classes, (int) index).fullName = TranslationReader.Get("Class_Names", (int) index, "NO_TRANSLATION");
          RoleExtensionMethods.Get(this.Classes, (int) index).nickname = TranslationReader.Get("Class_Nicknames", (int) index, "-");
          if (RoleExtensionMethods.Get(this.Classes, (int) index).nickname.Equals("-"))
            RoleExtensionMethods.Get(this.Classes, (int) index).nickname = string.Empty;
          RoleExtensionMethods.Get(this.Classes, (int) index).description = TranslationReader.Get("Class_Descriptions", (int) index, "NO_TRANSLATION");
          RoleExtensionMethods.Get(this.Classes, (int) index).bio = TranslationReader.Get("Class_Bio", (int) index, "NO_TRANSLATION");
          foreach (Ability ability in RoleExtensionMethods.Get(this.Classes, (int) index).abilities)
            ability.LoadNameAndDescriptionFromTranslation();
        }
        catch
        {
          Debug.Log((object) "Error when reading class translations...");
        }
      }
      CharacterClassManager._staticClasses = this.Classes;
      if (ServerStatic.IsDedicated)
        return;
      CentralAuth.singleton.GenerateToken((ICentralAuth) this._centralAuthInt);
    }
    else if (CharacterClassManager._staticClasses == null || CharacterClassManager._staticClasses.Length == 0)
    {
      for (byte index = 0; (int) index < this.Classes.Length; ++index)
      {
        try
        {
          RoleExtensionMethods.Get(this.Classes, (int) index).description = TranslationReader.Get("Class_Descriptions", (int) index, "NO_TRANSLATION");
          RoleExtensionMethods.Get(this.Classes, (int) index).bio = TranslationReader.Get("Class_Bio", (int) index, "NO_TRANSLATION");
          if (RoleExtensionMethods.Get(this.Classes, (int) index).team != Team.SCP)
            RoleExtensionMethods.Get(this.Classes, (int) index).fullName = TranslationReader.Get("Class_Names", (int) index, "NO_TRANSLATION");
          RoleExtensionMethods.Get(this.Classes, (int) index).nickname = TranslationReader.Get("Class_Nicknames", (int) index, "NO_TRANSLATION");
          if (RoleExtensionMethods.Get(this.Classes, (int) index).nickname.Equals("-"))
            RoleExtensionMethods.Get(this.Classes, (int) index).nickname = string.Empty;
          foreach (Ability ability in RoleExtensionMethods.Get(this.Classes, (int) index).abilities)
            ability.LoadNameAndDescriptionFromTranslation();
        }
        catch
        {
          Debug.Log((object) "Error when reading class translations...");
        }
      }
    }
    else
      this.Classes = CharacterClassManager._staticClasses;
  }

  private void Update()
  {
    if (!CharacterClassManager._hostFound && this.name == "Host")
    {
      CharacterClassManager._hostFound = true;
      this.IsHost = true;
    }
    if (this.CurClass == RoleType.Spectator)
      this.AliveTime = 0.0f;
    else
      this.AliveTime += Time.deltaTime;
    if (this.isLocalPlayer && this.isServer)
      this.AllowContain();
    if (this._prevId != this.CurClass)
    {
      this.RefreshPlyModel(RoleType.None);
      this._prevId = this.CurClass;
    }
    if (!this.IsHost)
      return;
    Radio.roundStarted = this.RoundStarted;
  }

  public bool LaterJoinPossible()
  {
    return this.LaterJoinEnabled && (Radio.roundStarted || Radio.roundEnded) && (double) this.LaterJoinTime >= (double) Time.realtimeSinceStartup - (double) RoundSummary.singleton.classlistStart.time;
  }

  private void FixedUpdate()
  {
    if (NetworkServer.active && this.CurClass < RoleType.Scp173 && (this.IsVerified && RoundStart.AntiNoclass))
    {
      this.SetPlayersClass(RoleType.Spectator, this.gameObject, false, false);
      if (this.LaterJoinPossible())
        Timing.RunCoroutine(this._LaterJoin(), Segment.FixedUpdate);
      else if (RoundStart.singleton.Timer == (short) -1)
        this.TargetRawDeathScreen(this.connectionToClient);
    }
    if (!this.isServer || !this.SpawnProtected || (double) Time.time - (double) this.ProtectedTime <= (double) this.SProtectedDuration && this.CurClass >= RoleType.Scp173 && this.SProtectedTeam.Contains((int) this.Classes.SafeGet(this.CurClass).team))
      return;
    this.SpawnProtected = false;
    this.GodMode = false;
  }

  private IEnumerator<float> _LaterJoin()
  {
    CharacterClassManager characterClassManager = this;
    RoleType role = (RoleType) characterClassManager.FindRandomIdUsingDefinedTeam(characterClassManager.ClassTeamQueue[characterClassManager.laterJoinNextIndex]);
    ++characterClassManager.laterJoinNextIndex;
    for (byte i = 0; i < (byte) 50; ++i)
      yield return 0.0f;
    characterClassManager.SetPlayersClass(role, characterClassManager.gameObject, false, false);
  }

  internal static void ResetHostFound()
  {
    CharacterClassManager._hostFound = false;
  }

  public void UserIdHook(string i)
  {
  }

  public void SetMuted(bool i)
  {
    this.NetworkMuted = i;
  }

  [ServerCallback]
  public void AllowContain()
  {
    if (!NetworkServer.active || !NonFacilityCompatibility.currentSceneSettings.enableStandardGamplayItems)
      return;
    foreach (GameObject player in PlayerManager.players)
    {
      if ((double) Vector3.Distance(player.transform.position, this._lureSpj.transform.position) < 1.97000002861023)
      {
        CharacterClassManager component1 = player.GetComponent<CharacterClassManager>();
        PlayerStats component2 = player.GetComponent<PlayerStats>();
        if (component1.Classes.SafeGet(component1.CurClass).team != Team.SCP && component1.CurClass != RoleType.Spectator && !component1.GodMode)
        {
          component2.HurtPlayer(new PlayerStats.HitInfo(10000f, "WORLD", DamageTypes.Lure, 0), player);
          this._lureSpj.SetState(true);
        }
      }
    }
  }

  [ClientRpc]
  public void RpcPlaceBlood(Vector3 pos, int type, float f)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteVector3(pos);
    writer.WritePackedInt32(type);
    writer.WriteSingle(f);
    this.SendRPCInternal(typeof (CharacterClassManager), nameof (RpcPlaceBlood), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  public void SyncServerCmdBinding()
  {
    if (!this.isServer || !this._enableSyncServerCmdBinding)
      return;
    foreach (CmdBinding.Bind binding in CmdBinding.Bindings)
    {
      if (binding.command.StartsWith(".") || binding.command.StartsWith("/"))
        this.TargetChangeCmdBinding(this.connectionToClient, binding.key, binding.command);
    }
  }

  [TargetRpc]
  public void TargetChangeCmdBinding(NetworkConnection connection, KeyCode code, string cmd)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WritePackedInt32((int) code);
    writer.WriteString(cmd);
    this.SendTargetRPCInternal(connection, typeof (CharacterClassManager), nameof (TargetChangeCmdBinding), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  public void TargetConsolePrint(NetworkConnection connection, string text, string color)
  {
    this.GetComponent<GameConsoleTransmission>().SendToClient(connection, text, color);
  }

  public bool IsHuman()
  {
    return this.Classes.SafeGet(this.CurClass).team != Team.SCP && this.Classes.SafeGet(this.CurClass).team != Team.RIP;
  }

  public bool IsTargetForSCPs()
  {
    return this.Classes.SafeGet(this.CurClass).team != Team.SCP && this.Classes.SafeGet(this.CurClass).team != Team.RIP && this.Classes.SafeGet(this.CurClass).team != Team.CHI;
  }

  private IEnumerator Init()
  {
    CharacterClassManager characterClassManager = this;
    if (NetworkServer.active)
    {
      if (CharacterClassManager.OnlineMode && !characterClassManager.isLocalPlayer)
      {
        float timeout = 0.0f;
        do
        {
          timeout += Timing.DeltaTime;
          yield return (object) null;
          if (!string.IsNullOrEmpty(characterClassManager.UserId))
          {
            characterClassManager.NetworkIsVerified = true;
            goto label_9;
          }
        }
        while ((double) timeout < 45.0);
        ServerConsole.Disconnect(characterClassManager.connectionToClient, "Your client has failed to authenticate in time.");
        yield break;
      }
      else
        characterClassManager.NetworkIsVerified = true;
    }
label_9:
    while ((UnityEngine.Object) CharacterClassManager._host == (UnityEngine.Object) null)
    {
      CharacterClassManager._host = GameObject.Find("Host");
      yield return (object) null;
    }
    if (characterClassManager.isLocalPlayer)
    {
      while (characterClassManager._seed == 0)
        characterClassManager._seed = CharacterClassManager._host.GetComponent<RandomSeedSync>().seed;
      if (NetworkServer.active)
      {
        if (ServerStatic.IsDedicated)
          ServerConsole.AddLog("Waiting for players..");
        if (NonFacilityCompatibility.currentSceneSettings.roundAutostart)
        {
          CharacterClassManager.ForceRoundStart();
        }
        else
        {
          short originalTimeLeft = ConfigFile.ServerConfig.GetShort("lobby_waiting_time", (short) 20);
          short timeLeft = originalTimeLeft;
          int topPlayers = 2;
          while (RoundStart.singleton.Timer != (short) -1)
          {
            if (timeLeft == (short) -2)
              timeLeft = originalTimeLeft;
            int count = PlayerManager.players.Count;
            if (!RoundStart.LobbyLock && count > 1)
            {
              if (count > topPlayers)
              {
                topPlayers = count;
                if ((int) timeLeft < (int) originalTimeLeft)
                {
                  do
                  {
                    ++timeLeft;
                  }
                  while ((int) timeLeft % 5 == 0 && (int) timeLeft < (int) originalTimeLeft);
                }
              }
              else
                --timeLeft;
              if (count >= ((CustomNetworkManager) NetworkManager.singleton).ReservedMaxPlayers)
                timeLeft = (short) -1;
              if (timeLeft == (short) -1)
                CharacterClassManager.ForceRoundStart();
            }
            else
              timeLeft = (short) -2;
            if (RoundStart.singleton.Timer != (short) -1)
              RoundStart.singleton.NetworkTimer = timeLeft;
            yield return (object) new WaitForSeconds(1f);
          }
        }
        characterClassManager.CmdStartRound();
        characterClassManager.NetworkRoundStarted = true;
        bool flag;
        do
        {
          characterClassManager.SetRandomRoles();
          yield return (object) new WaitForSeconds(0.5f);
          flag = true;
          foreach (GameObject player in PlayerManager.players)
          {
            switch (player.GetComponent<CharacterClassManager>().CurClass)
            {
              case RoleType.None:
                flag = false;
                continue;
              case RoleType.Spectator:
                if (player.GetComponent<ServerRoles>().OverwatchEnabled)
                  continue;
                goto case RoleType.None;
              default:
                continue;
            }
          }
        }
        while (!flag);
      }
      while (!CharacterClassManager._host.GetComponent<CharacterClassManager>().RoundStarted)
        yield return (object) null;
      yield return (object) new WaitForSeconds(2f);
      while (characterClassManager.CurClass < RoleType.Scp173)
      {
        characterClassManager.CmdSuicide(new PlayerStats.HitInfo());
        yield return (object) new WaitForSeconds(1f);
      }
    }
    if (characterClassManager.isLocalPlayer)
    {
      int iteration = 0;
      while (true)
      {
        List<GameObject> plys = PlayerManager.players;
        if (iteration >= plys.Count)
        {
          yield return (object) new WaitForSeconds(3f);
          iteration = 0;
        }
        try
        {
          plys[iteration].GetComponent<CharacterClassManager>().InitSCPs();
        }
        catch
        {
        }
        ++iteration;
        yield return (object) null;
        plys = (List<GameObject>) null;
      }
    }
  }

  [Command]
  public void CmdSendToken(string token)
  {
    if (this.isServer)
    {
      this.CallCmdSendToken(token);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(token);
      this.SendCommandInternal(typeof (CharacterClassManager), nameof (CmdSendToken), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  public void CmdSetNoclip(bool state)
  {
    if (this.isServer)
    {
      this.CallCmdSetNoclip(state);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBoolean(state);
      this.SendCommandInternal(typeof (CharacterClassManager), nameof (CmdSetNoclip), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  public void CmdToggleNoclip()
  {
    if (this.isServer)
    {
      this.CallCmdToggleNoclip();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (CharacterClassManager), nameof (CmdToggleNoclip), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [ServerCallback]
  private bool NoclipCmdsAllowed()
  {
    if (!NetworkServer.active || !this._commandRateLimit.CanExecute(true))
      return false;
    if (this.SrvRoles.NoclipReady)
      return true;
    this.TargetConsolePrint(this.connectionToClient, "You don't have permissions to execute this command.", "red");
    return false;
  }

  [ServerCallback]
  public void SetNoclip(bool state)
  {
    if (!NetworkServer.active)
      return;
    if (state && !this.SrvRoles.NoclipReady)
      this.SrvRoles.NoclipReady = true;
    this.NetworkNoclipEnabled = state;
    this._pms.NoclipWhitelisted = state;
  }

  [Command]
  public void CmdRequestContactEmail()
  {
    if (this.isServer)
    {
      this.CallCmdRequestContactEmail();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (CharacterClassManager), nameof (CmdRequestContactEmail), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  public void CmdRequestServerConfig()
  {
    if (this.isServer)
    {
      this.CallCmdRequestServerConfig();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (CharacterClassManager), nameof (CmdRequestServerConfig), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  public void CmdRequestServerGroups()
  {
    if (this.isServer)
    {
      this.CallCmdRequestServerGroups();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (CharacterClassManager), nameof (CmdRequestServerGroups), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  public void CmdRequestHideTag()
  {
    if (this.isServer)
    {
      this.CallCmdRequestHideTag();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (CharacterClassManager), nameof (CmdRequestHideTag), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  public void CmdRequestShowTag(bool global)
  {
    if (this.isServer)
    {
      this.CallCmdRequestShowTag(global);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBoolean(global);
      this.SendCommandInternal(typeof (CharacterClassManager), nameof (CmdRequestShowTag), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  public void CmdSuicide(PlayerStats.HitInfo hitInfo)
  {
    if (this.isServer)
    {
      this.CallCmdSuicide(hitInfo);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      GeneratedNetworkCode._WriteHitInfo_PlayerStats(writer, hitInfo);
      this.SendCommandInternal(typeof (CharacterClassManager), nameof (CmdSuicide), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  public static bool ForceRoundStart()
  {
    if (!NetworkServer.active)
      return false;
    ServerLogs.AddLog(ServerLogs.Modules.Logger, "Round has been started.", ServerLogs.ServerLogType.GameEvent);
    ServerConsole.AddLog("New round has been started.");
    RoundStart.singleton.NetworkTimer = (short) -1;
    RoundStart.RoundStartTimer.Restart();
    return true;
  }

  [TargetRpc]
  private void TargetSetDisconnectError(NetworkConnection conn, string message)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(message);
    this.SendTargetRPCInternal(conn, typeof (CharacterClassManager), nameof (TargetSetDisconnectError), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [Command]
  private void CmdConfirmDisconnect()
  {
    if (this.isServer)
    {
      this.CallCmdConfirmDisconnect();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (CharacterClassManager), nameof (CmdConfirmDisconnect), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  public void DisconnectClient(NetworkConnection conn, string message)
  {
    this.TargetSetDisconnectError(conn, message);
    Timing.RunCoroutine(this._DisconnectAfterTimeout(conn), Segment.FixedUpdate);
  }

  private IEnumerator<float> _DisconnectAfterTimeout(NetworkConnection conn)
  {
    for (int i = 0; i < 150; ++i)
      yield return 0.0f;
    if (conn != null)
    {
      conn.Disconnect();
      conn.Dispose();
    }
  }

  public void InitSCPs()
  {
    if (this.CurClass == RoleType.None || TutorialManager.status)
      return;
    Role c = this.Classes.Get(this.CurClass);
    this.Scp049.Init(this.CurClass, c);
    this.Scp0492.Init(this.CurClass, c);
    this.Scp079.Init(this.CurClass, c);
    this.Scp106.Init(this.CurClass, c);
    this.Scp173.Init(this.CurClass, c);
    this.Scp096.Init(this.CurClass, c);
    this.Scp939.Init(this.CurClass, c);
  }

  [Command]
  private void CmdRegisterEscape()
  {
    if (this.isServer)
    {
      this.CallCmdRegisterEscape();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (CharacterClassManager), nameof (CmdRegisterEscape), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  public bool IsScpButNotZombie()
  {
    return this.CurClass != RoleType.Scp0492 && this.Classes.SafeGet(this.CurClass).team == Team.SCP;
  }

  public bool IsAnyScp()
  {
    return this.Classes.SafeGet(this.CurClass).team == Team.SCP;
  }

  public void ApplyProperties(bool lite = false, bool escape = false)
  {
    Role role = this.Classes.SafeGet(this.CurClass);
    if (!this._wasAnytimeAlive && this.CurClass != RoleType.Spectator && this.CurClass != RoleType.None)
      this._wasAnytimeAlive = true;
    this.InitSCPs();
    this.AliveTime = 0.0f;
    switch (role.team)
    {
      case Team.MTF:
        AchievementManager.Achieve("arescue", true);
        break;
      case Team.CHI:
        AchievementManager.Achieve("chaos", true);
        break;
      case Team.RSC:
      case Team.CDP:
        this.EscapeStartTime = (int) Time.realtimeSinceStartup;
        break;
    }
    this.GetComponent<Inventory>();
    try
    {
      this.GetComponent<FootstepSync>().SetLoudness(role.team, role.roleId.Is939());
    }
    catch
    {
    }
    if (NetworkServer.active)
    {
      Handcuffs component = this.GetComponent<Handcuffs>();
      component.ClearTarget();
      component.NetworkCufferId = -1;
    }
    if (role.team != Team.RIP)
    {
      if (NetworkServer.active && !lite)
      {
        Vector3 constantRespawnPoint = NonFacilityCompatibility.currentSceneSettings.constantRespawnPoint;
        if (constantRespawnPoint != Vector3.zero)
        {
          this._pms.OnPlayerClassChange(constantRespawnPoint, 0.0f);
        }
        else
        {
          GameObject randomPosition = CharacterClassManager.SpawnpointManager.GetRandomPosition(this.CurClass);
          if ((UnityEngine.Object) randomPosition != (UnityEngine.Object) null)
          {
            this._pms.OnPlayerClassChange(randomPosition.transform.position, randomPosition.transform.rotation.eulerAngles.y);
            AmmoBox component1 = this.GetComponent<AmmoBox>();
            if (escape && this.KeepItemsAfterEscaping)
            {
              Inventory component2 = PlayerManager.localPlayer.GetComponent<Inventory>();
              for (ushort index = 0; index < (ushort) 3; ++index)
              {
                if (component1.GetAmmo((int) index) >= 15)
                  component2.SetPickup(component1.types[(int) index].inventoryID, (float) component1.GetAmmo((int) index), randomPosition.transform.position, randomPosition.transform.rotation, 0, 0, 0);
              }
            }
            component1.SetAmmoAmount();
          }
          else
            this._pms.OnPlayerClassChange(this.DeathPosition, 0.0f);
        }
        if (!this.SpawnProtected && this.EnableSP && this.SProtectedTeam.Contains((int) role.team))
        {
          this.GodMode = true;
          this.SpawnProtected = true;
          this.ProtectedTime = Time.time;
        }
      }
      if (!this.isLocalPlayer)
        this.GetComponent<PlayerStats>().maxHP = role.maxHP;
    }
    this.Scp049.iAm049 = this.CurClass == RoleType.Scp049;
    this.Scp0492.iAm049_2 = this.CurClass == RoleType.Scp0492;
    this.Scp096.iAm096 = this.CurClass == RoleType.Scp096;
    this.Scp106.iAm106 = this.CurClass == RoleType.Scp106;
    this.Scp173.iAm173 = this.CurClass == RoleType.Scp173;
    this.Scp939.iAm939 = this.CurClass.Is939();
    this.RefreshPlyModel(RoleType.None);
  }

  public void RefreshPlyModel(RoleType classId = RoleType.None)
  {
    this.GetComponent<AnimationController>().OnChangeClass();
    if ((UnityEngine.Object) this.MyModel != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.MyModel);
    Role role = this.Classes.SafeGet(classId < RoleType.Scp173 ? this.CurClass : classId);
    if (role.team != Team.RIP)
    {
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(role.model_player, this.gameObject.transform, true);
      gameObject.transform.localPosition = role.model_offset.position;
      gameObject.transform.localRotation = Quaternion.Euler(role.model_offset.rotation);
      gameObject.transform.localScale = role.model_offset.scale;
      this.MyModel = gameObject;
      AnimationController component1 = this.GetComponent<AnimationController>();
      if ((UnityEngine.Object) this.MyModel.GetComponent<Animator>() != (UnityEngine.Object) null)
        component1.animator = this.MyModel.GetComponent<Animator>();
      FootstepSync component2 = this.GetComponent<FootstepSync>();
      FootstepHandler component3 = this.MyModel.GetComponent<FootstepHandler>();
      if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
        component2.FootstepHandler = component3;
      if ((UnityEngine.Object) component3 != (UnityEngine.Object) null)
      {
        component3.FootstepSync = component2;
        component3.AnimationController = component1;
      }
      if (this.isLocalPlayer)
      {
        if ((UnityEngine.Object) this.MyModel.GetComponent<Renderer>() != (UnityEngine.Object) null)
          this.MyModel.GetComponent<Renderer>().enabled = false;
        foreach (Renderer componentsInChild in this.MyModel.GetComponentsInChildren<Renderer>())
          componentsInChild.enabled = false;
        foreach (Collider componentsInChild in this.MyModel.GetComponentsInChildren<Collider>())
        {
          if (componentsInChild.name != "LookingTarget")
            componentsInChild.enabled = false;
        }
      }
    }
    this.GetComponent<CapsuleCollider>().enabled = role.team != Team.RIP;
    if (!((UnityEngine.Object) this.MyModel != (UnityEngine.Object) null))
      return;
    this.GetComponent<WeaponManager>().hitboxes = this.MyModel.GetComponentsInChildren<HitboxIdentity>(true);
  }

  public void SetClassID(RoleType id)
  {
    this.SetClassIDAdv(id, false, false);
  }

  public void SetClassIDAdv(RoleType id, bool lite, bool escape = false)
  {
    if (NetworkServer.active && (UnityEngine.Object) this._pms != (UnityEngine.Object) null)
    {
      if (id == RoleType.Spectator || (UnityEngine.Object) this.SrvRoles == (UnityEngine.Object) null || PermissionsHandler.IsPermitted(this.SrvRoles.Permissions, PlayerPermissions.AFKImmunity))
        this.GetComponent<PlyMovementSync>().IsAFK = false;
      else
        this.GetComponent<PlyMovementSync>().IsAFK = true;
    }
    if (!this.IsVerified && id != RoleType.Spectator || id == RoleType.Tutorial && ServerStatic.IsDedicated && !ConfigFile.ServerConfig.GetBool("allow_playing_as_tutorial", true))
      return;
    if (this.SrvRoles.OverwatchEnabled && id != RoleType.Spectator)
    {
      if (this.CurClass == RoleType.Spectator)
        return;
      id = RoleType.Spectator;
    }
    this.DeathTime = id == RoleType.Spectator ? DateTime.UtcNow.Ticks : 0L;
    this.NetworkCurClass = id;
    bool flag = id == RoleType.Spectator;
    if (NetworkServer.active)
    {
      if (flag)
      {
        foreach (GameObject player in PlayerManager.players)
        {
          PlayerStats component = player.GetComponent<PlayerStats>();
          component.TargetSyncHp(this.connectionToClient, component.health);
        }
      }
      else
      {
        foreach (GameObject player in PlayerManager.players)
        {
          if ((UnityEngine.Object) player.GetComponent<CharacterClassManager>() != (UnityEngine.Object) this)
            player.GetComponent<PlayerStats>().TargetSyncHp(this.connectionToClient, -1f);
        }
      }
      this.GetComponent<PlayerStats>().MakeHpDirty();
      this.GetComponent<PlayerStats>().unsyncedArtificialHealth = 0.0f;
    }
    if (!flag || this.isLocalPlayer)
    {
      this.AliveTime = 0.0f;
      this.ApplyProperties(lite, escape);
    }
  }

  public void InstantiateRagdoll(int id)
  {
    if (id < 0)
      return;
    Role role = this.Classes.SafeGet(this.CurClass);
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(role.model_ragdoll);
    gameObject.transform.position = this.transform.position + role.ragdoll_offset.position;
    gameObject.transform.rotation = Quaternion.Euler(this.transform.rotation.eulerAngles + role.ragdoll_offset.rotation);
    gameObject.transform.localScale = role.ragdoll_offset.scale;
  }

  public void SetRandomRoles()
  {
    RoleType forcedClass = NonFacilityCompatibility.currentSceneSettings.forcedClass;
    if (this.isLocalPlayer && this.isServer)
    {
      GameObject[] array = this.GetShuffledPlayerList().ToArray();
      RoundSummary.SumInfo_ClassList info = new RoundSummary.SumInfo_ClassList();
      bool flag = false;
      int num = 0;
      float[] numArray = new float[4]
      {
        0.0f,
        0.4f,
        0.6f,
        0.5f
      };
      this.laterJoinNextIndex = 0;
      for (int index = 0; index < array.Length; ++index)
      {
        RoleType roleType = this.ForceClass < RoleType.Scp173 ? (RoleType) this.FindRandomIdUsingDefinedTeam(this.ClassTeamQueue[index]) : this.ForceClass;
        ++this.laterJoinNextIndex;
        if (this.Classes.CheckBounds(forcedClass))
          roleType = forcedClass;
        switch (this.Classes.SafeGet(roleType).team)
        {
          case Team.SCP:
            ++info.scps_except_zombies;
            break;
          case Team.MTF:
            ++info.mtf_and_guards;
            break;
          case Team.CHI:
            ++info.chaos_insurgents;
            break;
          case Team.RSC:
            ++info.scientists;
            break;
          case Team.CDP:
            ++info.class_ds;
            break;
        }
        if (this.Classes.SafeGet(roleType).team == Team.SCP && !flag)
        {
          if ((double) numArray[Mathf.Clamp(num, 0, numArray.Length)] > (double) UnityEngine.Random.value)
          {
            flag = true;
            this.Classes.Get(roleType).banClass = false;
            roleType = RoleType.Scp079;
          }
          ++num;
        }
        if (TutorialManager.status)
        {
          this.SetPlayersClass(RoleType.Tutorial, this.gameObject, false, false);
        }
        else
        {
          this.SetPlayersClass(roleType, array[index], false, false);
          ServerLogs.AddLog(ServerLogs.Modules.ClassChange, array[index].GetComponent<NicknameSync>().MyNick + " (" + array[index].GetComponent<CharacterClassManager>().UserId + ") spawned as " + this.Classes.SafeGet(roleType).fullName.Replace("\n", "") + ".", ServerLogs.ServerLogType.GameEvent);
        }
      }
      UnityEngine.Object.FindObjectOfType<PlayerList>().NetworkRoundStartTime = (int) Time.realtimeSinceStartup;
      info.time = (int) Time.realtimeSinceStartup;
      info.warhead_kills = -1;
      UnityEngine.Object.FindObjectOfType<RoundSummary>().SetStartClassList(info);
      if (ConfigFile.ServerConfig.GetBool("cm_deconrework", false))
        PlayerManager.localPlayer.GetComponent<DecontaminationLCZ>().CallDecon();
      if (ConfigFile.ServerConfig.GetBool("smart_class_picker", true))
        this.RunSmartClassPicker();
    }
    if (NetworkServer.active)
      Timing.RunCoroutine(this.MakeSureToSetHP(), Segment.FixedUpdate);
    Scp106PlayerScript.neonStalky106 = ConfigFile.ServerConfig.GetBool("neon_stalky106", true);
    if (!Scp106PlayerScript.neonStalky106)
      return;
    Scp106PlayerScript.InitializeStalky106();
  }

  private List<GameObject> GetShuffledPlayerList()
  {
    List<GameObject> list = new List<GameObject>((IEnumerable<GameObject>) PlayerManager.players);
    if (ConfigFile.ServerConfig.GetBool("use_crypto_rng", false))
      list.ShuffleListSecure<GameObject>();
    else
      list.ShuffleList<GameObject>();
    return list;
  }

  [Command]
  private void CmdRequestDeathScreen()
  {
    if (this.isServer)
    {
      this.CallCmdRequestDeathScreen();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (CharacterClassManager), nameof (CmdRequestDeathScreen), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [TargetRpc]
  private void TargetDeathScreen(NetworkConnection conn, PlayerStats.HitInfo hitinfo)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._WriteHitInfo_PlayerStats(writer, hitinfo);
    this.SendTargetRPCInternal(conn, typeof (CharacterClassManager), nameof (TargetDeathScreen), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [TargetRpc]
  public void TargetRawDeathScreen(NetworkConnection conn)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendTargetRPCInternal(conn, typeof (CharacterClassManager), nameof (TargetRawDeathScreen), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void RunSmartClassPicker()
  {
    string str = "Before Starting";
    try
    {
      str = "Setting Initial Value";
      if (ConfigFile.smBalancedPicker == null)
        ConfigFile.smBalancedPicker = new Dictionary<string, int[]>();
      str = "Valid Players List Error";
      List<GameObject> shuffledPlayerList = this.GetShuffledPlayerList();
      str = "Copying Balanced Picker List";
      Dictionary<string, int[]> dictionary = new Dictionary<string, int[]>((IDictionary<string, int[]>) ConfigFile.smBalancedPicker);
      str = "Clearing Balanced Picker List";
      ConfigFile.smBalancedPicker.Clear();
      str = "Re-building Balanced Picker List";
      foreach (GameObject gameObject in shuffledPlayerList)
      {
        if (!((UnityEngine.Object) gameObject == (UnityEngine.Object) null))
        {
          CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
          NetworkConnection networkConnection = (NetworkConnection) null;
          if ((UnityEngine.Object) component != (UnityEngine.Object) null)
            networkConnection = component.connectionToClient ?? component.connectionToServer;
          str = "Getting Player ID";
          if (networkConnection == null && (UnityEngine.Object) component == (UnityEngine.Object) null)
          {
            shuffledPlayerList.Remove(gameObject);
            break;
          }
          if (this.SrvRoles.DoNotTrack)
          {
            shuffledPlayerList.Remove(gameObject);
          }
          else
          {
            string key = (networkConnection != null ? networkConnection.address : "") + ((UnityEngine.Object) component != (UnityEngine.Object) null ? component.UserId : "");
            str = "Setting up Player \"" + key + "\"";
            if (!dictionary.ContainsKey(key))
            {
              str = "Adding Player \"" + key + "\" to smBalancedPicker";
              int[] numArray = new int[this.Classes.Length];
              for (int index = 0; index < numArray.Length; ++index)
                numArray[index] = ConfigFile.ServerConfig.GetInt("smart_cp_starting_weight", 6);
              ConfigFile.smBalancedPicker.Add(key, numArray);
            }
            else
            {
              str = "Updating Player \"" + key + "\" in smBalancedPicker";
              int[] numArray;
              if (dictionary.TryGetValue(key, out numArray))
                ConfigFile.smBalancedPicker.Add(key, numArray);
            }
          }
        }
      }
      str = "Clearing Copied Balanced Picker List";
      dictionary.Clear();
      List<RoleType> availableClasses = new List<RoleType>();
      str = "Getting Available Roles";
      if (shuffledPlayerList.Contains((GameObject) null))
        shuffledPlayerList.Remove((GameObject) null);
      foreach (GameObject gameObject in shuffledPlayerList)
      {
        if (!((UnityEngine.Object) gameObject == (UnityEngine.Object) null))
        {
          CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
          if ((UnityEngine.Object) component != (UnityEngine.Object) null)
            availableClasses.Add(component.CurClass);
          else
            shuffledPlayerList.Remove(gameObject);
        }
      }
      List<GameObject> gameObjectList = new List<GameObject>();
      str = "Setting Roles";
      foreach (GameObject ply in shuffledPlayerList)
      {
        if (!((UnityEngine.Object) ply == (UnityEngine.Object) null))
        {
          CharacterClassManager component = ply.GetComponent<CharacterClassManager>();
          NetworkConnection networkConnection = (NetworkConnection) null;
          if ((UnityEngine.Object) component != (UnityEngine.Object) null)
            networkConnection = component.connectionToClient ?? component.connectionToServer;
          if (networkConnection == null && (UnityEngine.Object) component == (UnityEngine.Object) null)
          {
            shuffledPlayerList.Remove(ply);
            break;
          }
          string playerUuid = (networkConnection != null ? networkConnection.address : "") + ((UnityEngine.Object) component != (UnityEngine.Object) null ? component.UserId : "");
          str = "Setting Player \"" + playerUuid + "\"'s Class";
          RoleType mostLikelyClass = this.GetMostLikelyClass(playerUuid, availableClasses);
          if (mostLikelyClass != RoleType.None)
          {
            this.SetPlayersClass(mostLikelyClass, ply, false, false);
            ServerLogs.AddLog(ServerLogs.Modules.ClassChange, ply.GetComponent<NicknameSync>().MyNick + " (" + ply.GetComponent<CharacterClassManager>().UserId + ") class set to " + this.Classes.SafeGet(mostLikelyClass).fullName.Replace("\n", "") + " by Smart Class Picker.", ServerLogs.ServerLogType.GameEvent);
            availableClasses.Remove(mostLikelyClass);
          }
          else
            gameObjectList.Add(ply);
        }
      }
      str = "Reversing Additional Classes List";
      availableClasses.Reverse();
      str = "Setting Unknown Players Classes";
      foreach (GameObject ply in gameObjectList)
      {
        if (!((UnityEngine.Object) ply == (UnityEngine.Object) null))
        {
          if (availableClasses.Count > 0)
          {
            RoleType roleType = availableClasses[0];
            this.SetPlayersClass(roleType, ply, false, false);
            ServerLogs.AddLog(ServerLogs.Modules.ClassChange, ply.GetComponent<NicknameSync>().MyNick + " (" + ply.GetComponent<CharacterClassManager>().UserId + ") class set to " + this.Classes.SafeGet(roleType).fullName.Replace("\n", "") + " by Smart Class Picker.", ServerLogs.ServerLogType.GameEvent);
            availableClasses.Remove(roleType);
          }
          else
          {
            this.SetPlayersClass(RoleType.Spectator, ply, false, false);
            ServerLogs.AddLog(ServerLogs.Modules.ClassChange, ply.GetComponent<NicknameSync>().MyNick + " (" + ply.GetComponent<CharacterClassManager>().UserId + ") class set to SPECTATOR by Smart Class Picker.", ServerLogs.ServerLogType.GameEvent);
          }
        }
      }
      str = "Clearing Unknown Players List";
      gameObjectList.Clear();
      str = "Clearing Available Classes List";
      availableClasses.Clear();
    }
    catch (Exception ex)
    {
      GameCore.Console.AddLog("Smart Class Picker Failed: " + str + ", " + ex.Message, (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 0, byte.MaxValue), false);
    }
  }

  private RoleType GetMostLikelyClass(string playerUuid, List<RoleType> availableClasses)
  {
    int[] classChances = (int[]) null;
    RoleType roleType = RoleType.None;
    if (availableClasses.Count <= 0 || !ConfigFile.smBalancedPicker.TryGetValue(playerUuid, out classChances) || (classChances == null || classChances.Length != this.Classes.Length) || !this.ContainsPossibleClass(classChances, availableClasses))
      return roleType;
    int max = 0;
    int[] numArray = (int[]) classChances.Clone();
    for (int index = 0; index < numArray.Length; ++index)
    {
      max += numArray[index];
      numArray[index] = max;
    }
    while (!availableClasses.Contains(roleType))
    {
      int num = UnityEngine.Random.Range(0, max);
      for (int index = 0; index < numArray.Length; ++index)
      {
        if (num < numArray[index])
        {
          roleType = (RoleType) index;
          break;
        }
      }
    }
    if (roleType < RoleType.Scp173 || roleType >= (RoleType) this.Classes.Length)
      return RoleType.None;
    this.UpdateClassChances((int) roleType, classChances);
    return roleType;
  }

  private bool ContainsPossibleClass(int[] classChances, List<RoleType> availableClasses)
  {
    foreach (int availableClass in availableClasses)
    {
      int index;
      if ((index = availableClass) >= 0 && index < classChances.Length && classChances[index] > 0)
        return true;
    }
    return false;
  }

  private void UpdateClassChances(int classChoice, int[] classChances)
  {
    int num1 = ConfigFile.ServerConfig.GetInt("smart_cp_weight_min", 1);
    int min = num1 < 0 ? 1 : num1;
    int num2 = ConfigFile.ServerConfig.GetInt("smart_cp_weight_max", 11);
    int max = num2 < min ? min + 10 : num2;
    for (int rtId = 0; rtId < classChances.Length; ++rtId)
    {
      bool flag1 = false;
      bool flag2 = false;
      if (ConfigFile.ServerConfig.GetInt("smart_cp_team_" + (object) RoleExtensionMethods.Get(this.Classes, rtId).team + "_weight_decrease", -99) != -99 && RoleExtensionMethods.Get(this.Classes, rtId).team == RoleExtensionMethods.Get(this.Classes, classChoice).team)
      {
        classChances[rtId] -= ConfigFile.ServerConfig.GetInt("smart_cp_team_" + (object) RoleExtensionMethods.Get(this.Classes, rtId).team + "_weight_decrease", 0);
        flag2 = true;
      }
      else if (ConfigFile.ServerConfig.GetInt("smart_cp_team_" + (object) RoleExtensionMethods.Get(this.Classes, rtId).team + "_weight_increase", -99) != -99 && RoleExtensionMethods.Get(this.Classes, rtId).team != RoleExtensionMethods.Get(this.Classes, classChoice).team)
      {
        classChances[rtId] += ConfigFile.ServerConfig.GetInt("smart_cp_team_" + (object) RoleExtensionMethods.Get(this.Classes, rtId).team + "_weight_increase", 0);
        flag1 = true;
      }
      if (ConfigFile.ServerConfig.GetInt("smart_cp_class_" + (object) rtId + "_weight_decrease", -99) != -99 && rtId == classChoice && !flag1)
        classChances[rtId] -= ConfigFile.ServerConfig.GetInt("smart_cp_class_" + (object) rtId + "_weight_decrease", 3);
      else if (ConfigFile.ServerConfig.GetInt("smart_cp_class_" + (object) rtId + "_weight_increase", -99) != -99 && rtId != classChoice && !flag2)
        classChances[rtId] += ConfigFile.ServerConfig.GetInt("smart_cp_class_" + (object) rtId + "_weight_increase", 1);
      else if (!flag1 && !flag2)
      {
        if (RoleExtensionMethods.Get(this.Classes, classChoice).team == Team.MTF && RoleExtensionMethods.Get(this.Classes, classChoice).team == RoleExtensionMethods.Get(this.Classes, rtId).team)
        {
          classChances[rtId] -= 2;
          if (rtId == classChoice)
            classChances[rtId] -= 2;
        }
        else if (RoleExtensionMethods.Get(this.Classes, classChoice).team == Team.CDP && RoleExtensionMethods.Get(this.Classes, classChoice).team == RoleExtensionMethods.Get(this.Classes, rtId).team)
          classChances[rtId] -= 3;
        else if (RoleExtensionMethods.Get(this.Classes, classChoice).team == Team.SCP && RoleExtensionMethods.Get(this.Classes, classChoice).team == RoleExtensionMethods.Get(this.Classes, rtId).team)
        {
          classChances[rtId] -= 2;
          if (rtId == classChoice)
            --classChances[rtId];
        }
        else if (rtId == classChoice)
          classChances[rtId] -= 2;
        else
          ++classChances[rtId];
      }
      classChances[rtId] = Mathf.Clamp(classChances[rtId], min, max);
    }
  }

  [ServerCallback]
  private void CmdStartRound()
  {
    if (!NetworkServer.active)
      return;
    if (!TutorialManager.status)
    {
      try
      {
        GameObject.Find("MeshDoor173").GetComponentInChildren<Door>().ForceCooldown(25f);
        UnityEngine.Object.FindObjectOfType<ChopperAutostart>().SetState(false);
      }
      catch
      {
      }
    }
    this.NetworkRoundStarted = true;
  }

  [ServerCallback]
  public void SetPlayersClass(RoleType classid, GameObject ply, bool lite = false, bool escape = false)
  {
    if (!NetworkServer.active || !ply.GetComponent<CharacterClassManager>().IsVerified)
      return;
    ply.GetComponent<CharacterClassManager>().SetClassIDAdv(classid, lite, escape);
    ply.GetComponent<PlayerStats>().SetHPAmount(this.Classes.SafeGet(classid).maxHP);
    if (lite)
      return;
    Inventory component = ply.GetComponent<Inventory>();
    List<Inventory.SyncItemInfo> syncItemInfoList = new List<Inventory.SyncItemInfo>();
    if (escape && this.KeepItemsAfterEscaping)
    {
      foreach (Inventory.SyncItemInfo syncItemInfo in (SyncList<Inventory.SyncItemInfo>) component.items)
        syncItemInfoList.Add(syncItemInfo);
    }
    component.items.Clear();
    foreach (ItemType startItem in this.Classes.SafeGet(classid).startItems)
      component.AddNewItem(startItem, -4.656647E+11f, 0, 0, 0);
    if (!escape || !this.KeepItemsAfterEscaping)
      return;
    foreach (Inventory.SyncItemInfo syncItemInfo1 in syncItemInfoList)
    {
      if (this.PutItemsInInvAfterEscaping)
      {
        Item itemById = component.GetItemByID(syncItemInfo1.id);
        bool flag = false;
        foreach (InventoryCategory category in this._searching.categories)
        {
          if (category.itemType == itemById.itemCategory && (itemById.itemCategory != ItemCategory.None || itemById.itemCategory != ItemCategory.NoCategory))
          {
            int num = 0;
            foreach (Inventory.SyncItemInfo syncItemInfo2 in (SyncList<Inventory.SyncItemInfo>) component.items)
            {
              if (component.GetItemByID(syncItemInfo2.id).itemCategory == itemById.itemCategory)
                ++num;
            }
            if (num >= category.maxItems)
            {
              flag = true;
              break;
            }
            break;
          }
        }
        if (component.items.Count >= 8 | flag)
          component.SetPickup(syncItemInfo1.id, syncItemInfo1.durability, this._pms.RealModelPosition, Quaternion.Euler(this._pms.Rotations.x, this._pms.Rotations.y, 0.0f), syncItemInfo1.modSight, syncItemInfo1.modBarrel, syncItemInfo1.modOther);
        else
          component.AddNewItem(syncItemInfo1.id, syncItemInfo1.durability, syncItemInfo1.modSight, syncItemInfo1.modBarrel, syncItemInfo1.modOther);
      }
      else
        component.SetPickup(syncItemInfo1.id, syncItemInfo1.durability, this._pms.RealModelPosition, Quaternion.Euler(this._pms.Rotations.x, this._pms.Rotations.y, 0.0f), syncItemInfo1.modSight, syncItemInfo1.modBarrel, syncItemInfo1.modOther);
    }
  }

  private IEnumerator<float> MakeSureToSetHP()
  {
    for (byte i = 0; i < (byte) 7; ++i)
    {
      foreach (GameObject player in PlayerManager.players)
      {
        if (!((UnityEngine.Object) player == (UnityEngine.Object) null))
        {
          CharacterClassManager component1 = player.GetComponent<CharacterClassManager>();
          PlayerStats component2 = player.GetComponent<PlayerStats>();
          if ((double) component2.health <= (double) this.Classes.SafeGet(component1.CurClass).maxHP)
            component2.SetHPAmount(this.Classes.SafeGet(component1.CurClass).maxHP);
        }
      }
      for (byte x = 0; x < (byte) 50; ++x)
        yield return 0.0f;
    }
  }

  private int FindRandomIdUsingDefinedTeam(Team team)
  {
    this._ids.Clear();
    for (int rtId = 0; rtId < this.Classes.Length; ++rtId)
    {
      if (this.Classes.SafeGet(rtId).team == team && !this.Classes.SafeGet(rtId).banClass)
        this._ids.Add(rtId);
    }
    if (this._ids.Count == 0)
      return 1;
    int index = UnityEngine.Random.Range(0, this._ids.Count);
    if (this.Classes.SafeGet(this._ids[index]).team == Team.SCP)
      this.Classes.SafeGet(this._ids[index]).banClass = true;
    return this._ids[index];
  }

  private void MirrorProcessed()
  {
  }

  public bool NetworkMuted
  {
    get
    {
      return this.Muted;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(1UL))
      {
        this.setSyncVarHookGuard(1UL, true);
        this.SetMuted(value);
        this.setSyncVarHookGuard(1UL, false);
      }
      this.SetSyncVar<bool>(value, ref this.Muted, 1UL);
    }
  }

  public bool NetworkIntercomMuted
  {
    get
    {
      return this.IntercomMuted;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.IntercomMuted, 2UL);
    }
  }

  public bool NetworkNoclipEnabled
  {
    get
    {
      return this.NoclipEnabled;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.NoclipEnabled, 4UL);
    }
  }

  public RoleType NetworkCurClass
  {
    get
    {
      return this.CurClass;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(8UL))
      {
        this.setSyncVarHookGuard(8UL, true);
        this.SetClassID(value);
        this.setSyncVarHookGuard(8UL, false);
      }
      this.SetSyncVar<RoleType>(value, ref this.CurClass, 8UL);
    }
  }

  public Vector3 NetworkDeathPosition
  {
    get
    {
      return this.DeathPosition;
    }
    [param: In] set
    {
      this.SetSyncVar<Vector3>(value, ref this.DeathPosition, 16UL);
    }
  }

  public int NetworkNtfUnit
  {
    get
    {
      return this.NtfUnit;
    }
    [param: In] set
    {
      this.SetSyncVar<int>(value, ref this.NtfUnit, 32UL);
    }
  }

  public bool NetworkRoundStarted
  {
    get
    {
      return this.RoundStarted;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.RoundStarted, 64UL);
    }
  }

  public bool NetworkIsVerified
  {
    get
    {
      return this.IsVerified;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.IsVerified, 128UL);
    }
  }

  public string NetworkSyncedUserId
  {
    get
    {
      return this.SyncedUserId;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(256UL))
      {
        this.setSyncVarHookGuard(256UL, true);
        this.UserIdHook(value);
        this.setSyncVarHookGuard(256UL, false);
      }
      this.SetSyncVar<string>(value, ref this.SyncedUserId, 256UL);
    }
  }

  protected static void InvokeCmdCmdSendToken(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSendToken called on client.");
    else
      ((CharacterClassManager) obj).CallCmdSendToken(reader.ReadString());
  }

  protected static void InvokeCmdCmdSetNoclip(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSetNoclip called on client.");
    else
      ((CharacterClassManager) obj).CallCmdSetNoclip(reader.ReadBoolean());
  }

  protected static void InvokeCmdCmdToggleNoclip(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdToggleNoclip called on client.");
    else
      ((CharacterClassManager) obj).CallCmdToggleNoclip();
  }

  protected static void InvokeCmdCmdRequestContactEmail(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdRequestContactEmail called on client.");
    else
      ((CharacterClassManager) obj).CallCmdRequestContactEmail();
  }

  protected static void InvokeCmdCmdRequestServerConfig(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdRequestServerConfig called on client.");
    else
      ((CharacterClassManager) obj).CallCmdRequestServerConfig();
  }

  protected static void InvokeCmdCmdRequestServerGroups(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdRequestServerGroups called on client.");
    else
      ((CharacterClassManager) obj).CallCmdRequestServerGroups();
  }

  protected static void InvokeCmdCmdRequestHideTag(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdRequestHideTag called on client.");
    else
      ((CharacterClassManager) obj).CallCmdRequestHideTag();
  }

  protected static void InvokeCmdCmdRequestShowTag(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdRequestShowTag called on client.");
    else
      ((CharacterClassManager) obj).CallCmdRequestShowTag(reader.ReadBoolean());
  }

  protected static void InvokeCmdCmdSuicide(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSuicide called on client.");
    else
      ((CharacterClassManager) obj).CallCmdSuicide(GeneratedNetworkCode._ReadHitInfo_PlayerStats(reader));
  }

  protected static void InvokeCmdCmdConfirmDisconnect(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdConfirmDisconnect called on client.");
    else
      ((CharacterClassManager) obj).CallCmdConfirmDisconnect();
  }

  protected static void InvokeCmdCmdRegisterEscape(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdRegisterEscape called on client.");
    else
      ((CharacterClassManager) obj).CallCmdRegisterEscape();
  }

  protected static void InvokeCmdCmdRequestDeathScreen(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdRequestDeathScreen called on client.");
    else
      ((CharacterClassManager) obj).CallCmdRequestDeathScreen();
  }

  public void CallCmdSendToken(string token)
  {
    if (this._commandtokensent && !this.isLocalPlayer)
    {
      ServerConsole.Disconnect(this.connectionToClient, "Your client sent second authentication token.");
    }
    else
    {
      if (ConfigFile.ServerConfig.GetBool("online_mode", true))
      {
        if (string.IsNullOrEmpty(token) || this._commandtokensent)
        {
          if (!this.isLocalPlayer || !this.isServer)
          {
            ServerConsole.Disconnect(this.connectionToClient, "Your client sent an empty authentication token. Make sure you are running the game by steam.");
            return;
          }
          this.NetworkIsVerified = true;
        }
        else if (!this.isLocalPlayer || !this.isServer)
        {
          if (token.StartsWith("ERROR: "))
          {
            ServerConsole.AddLog("Player from IP " + this.connectionToClient.address + " kicked due to authentication error: " + token.Substring(7));
            ServerLogs.AddLog(ServerLogs.Modules.Networking, "Player from IP " + this.connectionToClient.address + " kicked due to authentication error: " + token.Substring(7), ServerLogs.ServerLogType.ConnectionUpdate);
            ServerConsole.Disconnect(this.connectionToClient, "Error during authentication: " + token.Substring(7));
          }
          else
          {
            CentralAuth.singleton.StartValidateToken((ICentralAuth) this._centralAuthInt, token, LiteNetLib4MirrorServer.Peers[this.connectionToClient.connectionId].EndPoint);
            this.AuthToken = token;
          }
        }
        else
        {
          this.NetworkIsVerified = true;
          if (token.StartsWith("ERROR: "))
            GameCore.Console.AddLog("Error during authentication: " + token.Substring(7), Color.red, false);
          else
            CentralAuth.singleton.StartValidateToken((ICentralAuth) this._centralAuthInt, token, (IPEndPoint) null);
          this.AuthToken = token;
        }
      }
      this._commandtokensent = true;
    }
  }

  public void CallCmdSetNoclip(bool state)
  {
    if (!this.NoclipCmdsAllowed())
      return;
    this.SetNoclip(state);
  }

  public void CallCmdToggleNoclip()
  {
    if (!this.NoclipCmdsAllowed())
      return;
    this.SetNoclip(!this.NoclipEnabled);
  }

  public void CallCmdRequestContactEmail()
  {
    if (!this._commandRateLimit.CanExecute(true))
      return;
    if (this.SrvRoles.RemoteAdmin || this.SrvRoles.Staff)
      this.TargetConsolePrint(this.connectionToClient, "Contact email address: " + ConfigFile.ServerConfig.GetString("contact_email", ""), "green");
    else
      this.TargetConsolePrint(this.connectionToClient, "You don't have permissions to execute this command.", "red");
  }

  public void CallCmdRequestServerConfig()
  {
    if (!this._commandRateLimit.CanExecute(true))
      return;
    YamlConfig serverConfig = ConfigFile.ServerConfig;
    if (!this.SrvRoles.Staff)
    {
      if (this.SrvRoles.RemoteAdmin)
      {
        if (ServerStatic.PermissionsHandler.IsPermitted(this.SrvRoles.Permissions, new PlayerPermissions[5]
        {
          PlayerPermissions.BanningUpToDay,
          PlayerPermissions.LongTermBanning,
          PlayerPermissions.PermissionsManagement,
          PlayerPermissions.SetGroup,
          PlayerPermissions.ForceclassWithoutRestrictions
        }))
          goto label_4;
      }
      NetworkConnection connectionToClient = this.connectionToClient;
      object[] objArray = new object[18]
      {
        (object) "Basic server configuration:\nServer name: ",
        (object) serverConfig.GetString("server_name", ""),
        (object) "\nServer IP: ",
        (object) serverConfig.GetString("server_ip", ""),
        (object) "\nServer pastebin ID: ",
        (object) serverConfig.GetString("serverinfo_pastebin_id", ""),
        (object) "\nServer max players: ",
        (object) serverConfig.GetInt("max_players", 0),
        (object) "\nRA password authentication: ",
        (object) this.GetComponent<QueryProcessor>().OverridePasswordEnabled.ToString(),
        (object) "\nOnline mode: ",
        null,
        null,
        null,
        null,
        null,
        null,
        null
      };
      bool flag = serverConfig.GetBool("online_mode", false);
      objArray[11] = (object) flag.ToString();
      objArray[12] = (object) "\nWhitelist: ";
      flag = serverConfig.GetBool("enable_whitelist", false);
      objArray[13] = (object) flag.ToString();
      objArray[14] = (object) "\nFriendly fire: ";
      objArray[15] = (object) ServerConsole.FriendlyFire.ToString();
      objArray[16] = (object) "\nMap seed: ";
      objArray[17] = (object) serverConfig.GetInt("map_seed", 0);
      string text = string.Concat(objArray);
      this.TargetConsolePrint(connectionToClient, text, "green");
      return;
    }
label_4:
    this.TargetConsolePrint(this.connectionToClient, "Extended server configuration:\nServer name: " + serverConfig.GetString("server_name", "") + "\nServer IP: " + serverConfig.GetString("server_ip", "") + "\nCurrent Server IP: " + CustomNetworkManager.Ip + "\nServer pastebin ID: " + serverConfig.GetString("serverinfo_pastebin_id", "") + "\nServer max players: " + (object) serverConfig.GetInt("max_players", 0) + "\nOnline mode: " + serverConfig.GetBool("online_mode", false).ToString() + "\nRA password authentication: " + this.GetComponent<QueryProcessor>().OverridePasswordEnabled.ToString() + "\nIP banning: " + serverConfig.GetBool("ip_banning", false).ToString() + "\nWhitelist: " + serverConfig.GetBool("enable_whitelist", false).ToString() + "\nQuery status: " + serverConfig.GetBool("enable_query", false).ToString() + " with port shift " + (object) serverConfig.GetInt("query_port_shift", 0) + "\nFriendly fire: " + ServerConsole.FriendlyFire.ToString() + "\nMap seed: " + (object) serverConfig.GetInt("map_seed", 0), "green");
  }

  public void CallCmdRequestServerGroups()
  {
    if (!this._commandRateLimit.CanExecute(true))
      return;
    string str = "Groups defined on this server:";
    Dictionary<string, UserGroup> allGroups = ServerStatic.PermissionsHandler.GetAllGroups();
    ServerRoles.NamedColor[] namedColors = this.SrvRoles.NamedColors;
    foreach (KeyValuePair<string, UserGroup> keyValuePair in allGroups)
    {
      KeyValuePair<string, UserGroup> permentry = keyValuePair;
      try
      {
        if (namedColors != null)
          str = str + "\n" + permentry.Key + " (" + (object) permentry.Value.Permissions + ") - <color=#" + ((IEnumerable<ServerRoles.NamedColor>) namedColors).FirstOrDefault<ServerRoles.NamedColor>((Func<ServerRoles.NamedColor, bool>) (x => x.Name == permentry.Value.BadgeColor))?.ColorHex + ">" + permentry.Value.BadgeText + "</color> in color " + permentry.Value.BadgeColor;
      }
      catch
      {
        str = str + "\n" + permentry.Key + " (" + (object) permentry.Value.Permissions + ") - " + permentry.Value.BadgeText + " in color " + permentry.Value.BadgeColor;
      }
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.KickingAndShortTermBanning))
        str += " BN1";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.BanningUpToDay))
        str += " BN2";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.LongTermBanning))
        str += " BN3";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassSelf))
        str += " FSE";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassToSpectator))
        str += " FSP";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassWithoutRestrictions))
        str += " FWR";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.GivingItems))
        str += " GIV";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.WarheadEvents))
        str += " EWA";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.RespawnEvents))
        str += " ERE";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.RoundEvents))
        str += " ERO";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.SetGroup))
        str += " SGR";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.GameplayData))
        str += " GMD";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Overwatch))
        str += " OVR";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.FacilityManagement))
        str += " FCM";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PlayersManagement))
        str += " PLM";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PermissionsManagement))
        str += " PRM";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ServerConsoleCommands))
        str += " SCC";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ViewHiddenBadges))
        str += " VHB";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ServerConfigs))
        str += " CFG";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Broadcasting))
        str += " BRC";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PlayerSensitiveDataAccess))
        str += " CDA";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Noclip))
        str += " NCP";
      if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.AFKImmunity))
        str += " AFK";
    }
    this.TargetConsolePrint(this.connectionToClient, "Defined groups on server " + str, "grey");
  }

  public void CallCmdRequestHideTag()
  {
    if (!this._commandRateLimit.CanExecute(true))
      return;
    this.SrvRoles.HiddenBadge = this.SrvRoles.MyText;
    this.SrvRoles.NetworkGlobalBadge = (string) null;
    this.SrvRoles.SetText((string) null);
    this.SrvRoles.SetColor((string) null);
    this.SrvRoles.GlobalSet = false;
    this.SrvRoles.RefreshHiddenTag();
    this.TargetConsolePrint(this.connectionToClient, "Badge hidden.", "green");
  }

  public void CallCmdRequestShowTag(bool global)
  {
    if (!this._commandRateLimit.CanExecute(true))
      return;
    if (global)
    {
      if (string.IsNullOrEmpty(this.SrvRoles.PrevBadge))
        this.TargetConsolePrint(this.connectionToClient, "You don't have global tag.", "magenta");
      else if ((string.IsNullOrEmpty(this.SrvRoles.MyText) || !this.SrvRoles.RemoteAdmin) && ((this.SrvRoles.GlobalBadgeType == 3 || this.SrvRoles.GlobalBadgeType == 4) && (ConfigFile.ServerConfig.GetBool("block_gtag_banteam_badges", false) && !ServerStatic.PermissionsHandler.IsVerified) || this.SrvRoles.GlobalBadgeType == 1 && ConfigFile.ServerConfig.GetBool("block_gtag_staff_badges", false) || (this.SrvRoles.GlobalBadgeType == 2 && ConfigFile.ServerConfig.GetBool("block_gtag_management_badges", false) && !ServerStatic.PermissionsHandler.IsVerified || this.SrvRoles.GlobalBadgeType == 0 && ConfigFile.ServerConfig.GetBool("block_gtag_patreon_badges", false) && !ServerStatic.PermissionsHandler.IsVerified)))
      {
        this.TargetConsolePrint(this.connectionToClient, "You can't show this type of global badge on this server. Try joining server with global badges allowed.", "red");
      }
      else
      {
        this.SrvRoles.NetworkGlobalBadge = this.SrvRoles.PrevBadge;
        this.SrvRoles.GlobalSet = true;
        this.SrvRoles.HiddenBadge = (string) null;
        this.SrvRoles.RpcResetFixed();
        this.TargetConsolePrint(this.connectionToClient, "Global tag refreshed.", "green");
      }
    }
    else
    {
      this.SrvRoles.NetworkGlobalBadge = (string) null;
      this.SrvRoles.HiddenBadge = (string) null;
      this.SrvRoles.RpcResetFixed();
      this.SrvRoles.RefreshPermissions(true);
      this.TargetConsolePrint(this.connectionToClient, "Local tag refreshed.", "green");
    }
  }

  public void CallCmdSuicide(PlayerStats.HitInfo hitInfo)
  {
    if (!this._commandRateLimit.CanExecute(true))
      return;
    hitInfo.Amount = (double) Math.Abs(hitInfo.Amount) < 0.00999999977648258 ? 999799f : hitInfo.Amount;
    this.GetComponent<PlayerStats>().HurtPlayer(hitInfo, this.gameObject);
  }

  public void CallCmdConfirmDisconnect()
  {
    if (this.connectionToClient == null)
      return;
    this.connectionToClient.Disconnect();
    this.connectionToClient.Dispose();
  }

  public void CallCmdRegisterEscape()
  {
    if (!this._interactRateLimit.CanExecute(true) || (double) Vector3.Distance(this.transform.position, this.GetComponent<Escape>().worldPosition) >= (double) (Escape.radius * 2))
      return;
    bool flag = false;
    Handcuffs component1 = this.GetComponent<Handcuffs>();
    if (component1.CufferId >= 0 && ConfigFile.ServerConfig.GetBool("cuffed_escapee_change_team", true))
    {
      CharacterClassManager component2 = component1.GetCuffer(component1.CufferId).GetComponent<CharacterClassManager>();
      if (this.CurClass == RoleType.Scientist && (component2.CurClass == RoleType.ChaosInsurgency || component2.CurClass == RoleType.ClassD))
        flag = true;
      if (this.CurClass == RoleType.ClassD && (component2.Classes.SafeGet(component2.CurClass).team == Team.MTF || component2.CurClass == RoleType.Scientist))
        flag = true;
    }
    switch (this.Classes.SafeGet(this.CurClass).team)
    {
      case Team.RSC:
        if (flag)
        {
          this.SetPlayersClass(RoleType.ChaosInsurgency, this.gameObject, false, true);
          ++RoundSummary.escaped_ds;
          break;
        }
        this.SetPlayersClass(RoleType.NtfScientist, this.gameObject, false, true);
        ++RoundSummary.escaped_scientists;
        break;
      case Team.CDP:
        if (flag)
        {
          this.SetPlayersClass(RoleType.NtfCadet, this.gameObject, false, true);
          ++RoundSummary.escaped_scientists;
          break;
        }
        this.SetPlayersClass(RoleType.ChaosInsurgency, this.gameObject, false, true);
        ++RoundSummary.escaped_ds;
        break;
    }
  }

  public void CallCmdRequestDeathScreen()
  {
    if (!this._commandRateLimit.CanExecute(true))
      return;
    this.TargetDeathScreen(this.connectionToClient, this.GetComponent<PlayerStats>().lastHitInfo);
  }

  protected static void InvokeRpcRpcPlaceBlood(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcPlaceBlood called on server.");
    else
      ((CharacterClassManager) obj).CallRpcPlaceBlood(reader.ReadVector3(), reader.ReadPackedInt32(), reader.ReadSingle());
  }

  protected static void InvokeRpcTargetChangeCmdBinding(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetChangeCmdBinding called on server.");
    else
      ((CharacterClassManager) obj).CallTargetChangeCmdBinding(ClientScene.readyConnection, (KeyCode) reader.ReadPackedInt32(), reader.ReadString());
  }

  protected static void InvokeRpcTargetSetDisconnectError(
    NetworkBehaviour obj,
    NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetSetDisconnectError called on server.");
    else
      ((CharacterClassManager) obj).CallTargetSetDisconnectError(ClientScene.readyConnection, reader.ReadString());
  }

  protected static void InvokeRpcTargetDeathScreen(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetDeathScreen called on server.");
    else
      ((CharacterClassManager) obj).CallTargetDeathScreen(ClientScene.readyConnection, GeneratedNetworkCode._ReadHitInfo_PlayerStats(reader));
  }

  protected static void InvokeRpcTargetRawDeathScreen(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetRawDeathScreen called on server.");
    else
      ((CharacterClassManager) obj).CallTargetRawDeathScreen(ClientScene.readyConnection);
  }

  public void CallRpcPlaceBlood(Vector3 pos, int type, float f)
  {
    this.GetComponent<BloodDrawer>().PlaceUnderneath(pos, type, f);
  }

  public void CallTargetChangeCmdBinding(NetworkConnection connection, KeyCode code, string cmd)
  {
  }

  public void CallTargetSetDisconnectError(NetworkConnection conn, string message)
  {
    ((CustomNetworkManager) LiteNetLib4MirrorNetworkManager.singleton).disconnectMessage = message;
    this.CmdConfirmDisconnect();
  }

  public void CallTargetDeathScreen(NetworkConnection conn, PlayerStats.HitInfo hitinfo)
  {
  }

  public void CallTargetRawDeathScreen(NetworkConnection conn)
  {
  }

  static CharacterClassManager()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (CharacterClassManager), "CmdSendToken", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeCmdCmdSendToken));
    NetworkBehaviour.RegisterCommandDelegate(typeof (CharacterClassManager), "CmdSetNoclip", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeCmdCmdSetNoclip));
    NetworkBehaviour.RegisterCommandDelegate(typeof (CharacterClassManager), "CmdToggleNoclip", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeCmdCmdToggleNoclip));
    NetworkBehaviour.RegisterCommandDelegate(typeof (CharacterClassManager), "CmdRequestContactEmail", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeCmdCmdRequestContactEmail));
    NetworkBehaviour.RegisterCommandDelegate(typeof (CharacterClassManager), "CmdRequestServerConfig", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeCmdCmdRequestServerConfig));
    NetworkBehaviour.RegisterCommandDelegate(typeof (CharacterClassManager), "CmdRequestServerGroups", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeCmdCmdRequestServerGroups));
    NetworkBehaviour.RegisterCommandDelegate(typeof (CharacterClassManager), "CmdRequestHideTag", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeCmdCmdRequestHideTag));
    NetworkBehaviour.RegisterCommandDelegate(typeof (CharacterClassManager), "CmdRequestShowTag", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeCmdCmdRequestShowTag));
    NetworkBehaviour.RegisterCommandDelegate(typeof (CharacterClassManager), "CmdSuicide", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeCmdCmdSuicide));
    NetworkBehaviour.RegisterCommandDelegate(typeof (CharacterClassManager), "CmdConfirmDisconnect", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeCmdCmdConfirmDisconnect));
    NetworkBehaviour.RegisterCommandDelegate(typeof (CharacterClassManager), "CmdRegisterEscape", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeCmdCmdRegisterEscape));
    NetworkBehaviour.RegisterCommandDelegate(typeof (CharacterClassManager), "CmdRequestDeathScreen", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeCmdCmdRequestDeathScreen));
    NetworkBehaviour.RegisterRpcDelegate(typeof (CharacterClassManager), "RpcPlaceBlood", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeRpcRpcPlaceBlood));
    NetworkBehaviour.RegisterRpcDelegate(typeof (CharacterClassManager), "TargetChangeCmdBinding", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeRpcTargetChangeCmdBinding));
    NetworkBehaviour.RegisterRpcDelegate(typeof (CharacterClassManager), "TargetSetDisconnectError", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeRpcTargetSetDisconnectError));
    NetworkBehaviour.RegisterRpcDelegate(typeof (CharacterClassManager), "TargetDeathScreen", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeRpcTargetDeathScreen));
    NetworkBehaviour.RegisterRpcDelegate(typeof (CharacterClassManager), "TargetRawDeathScreen", new NetworkBehaviour.CmdDelegate(CharacterClassManager.InvokeRpcTargetRawDeathScreen));
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteBoolean(this.Muted);
      writer.WriteBoolean(this.IntercomMuted);
      writer.WriteBoolean(this.NoclipEnabled);
      writer.WritePackedInt32((int) this.CurClass);
      writer.WriteVector3(this.DeathPosition);
      writer.WritePackedInt32(this.NtfUnit);
      writer.WriteBoolean(this.RoundStarted);
      writer.WriteBoolean(this.IsVerified);
      writer.WriteString(this.SyncedUserId);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteBoolean(this.Muted);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WriteBoolean(this.IntercomMuted);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 4L) != 0L)
    {
      writer.WriteBoolean(this.NoclipEnabled);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 8L) != 0L)
    {
      writer.WritePackedInt32((int) this.CurClass);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 16L) != 0L)
    {
      writer.WriteVector3(this.DeathPosition);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 32L) != 0L)
    {
      writer.WritePackedInt32(this.NtfUnit);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 64L) != 0L)
    {
      writer.WriteBoolean(this.RoundStarted);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 128L) != 0L)
    {
      writer.WriteBoolean(this.IsVerified);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 256L) != 0L)
    {
      writer.WriteString(this.SyncedUserId);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      bool i1 = reader.ReadBoolean();
      this.SetMuted(i1);
      this.NetworkMuted = i1;
      this.NetworkIntercomMuted = reader.ReadBoolean();
      this.NetworkNoclipEnabled = reader.ReadBoolean();
      RoleType id = (RoleType) reader.ReadPackedInt32();
      this.SetClassID(id);
      this.NetworkCurClass = id;
      this.NetworkDeathPosition = reader.ReadVector3();
      this.NetworkNtfUnit = reader.ReadPackedInt32();
      this.NetworkRoundStarted = reader.ReadBoolean();
      this.NetworkIsVerified = reader.ReadBoolean();
      string i2 = reader.ReadString();
      this.UserIdHook(i2);
      this.NetworkSyncedUserId = i2;
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
      {
        bool i = reader.ReadBoolean();
        this.SetMuted(i);
        this.NetworkMuted = i;
      }
      if ((num & 2L) != 0L)
        this.NetworkIntercomMuted = reader.ReadBoolean();
      if ((num & 4L) != 0L)
        this.NetworkNoclipEnabled = reader.ReadBoolean();
      if ((num & 8L) != 0L)
      {
        RoleType id = (RoleType) reader.ReadPackedInt32();
        this.SetClassID(id);
        this.NetworkCurClass = id;
      }
      if ((num & 16L) != 0L)
        this.NetworkDeathPosition = reader.ReadVector3();
      if ((num & 32L) != 0L)
        this.NetworkNtfUnit = reader.ReadPackedInt32();
      if ((num & 64L) != 0L)
        this.NetworkRoundStarted = reader.ReadBoolean();
      if ((num & 128L) != 0L)
        this.NetworkIsVerified = reader.ReadBoolean();
      if ((num & 256L) == 0L)
        return;
      string i1 = reader.ReadString();
      this.UserIdHook(i1);
      this.NetworkSyncedUserId = i1;
    }
  }
}
