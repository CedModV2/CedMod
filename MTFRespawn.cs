// Decompiled with JetBrains decompiler
// Type: MTFRespawn
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using MEC;
using Mirror;
using Security;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MTFRespawn : NetworkBehaviour
{
  [Range(30f, 1000f)]
  public int minMtfTimeToRespawn = 200;
  [Range(40f, 1200f)]
  public int maxMtfTimeToRespawn = 400;
  public float CI_Time_Multiplier = 2f;
  public float CI_Percent = 20f;
  [Space(10f)]
  [Range(2f, 50f)]
  public int maxMTFRespawnAmount = 15;
  [Range(2f, 50f)]
  public int maxCIRespawnAmount = 15;
  public List<GameObject> playersToNTF = new List<GameObject>();
  public GameObject ciTheme;
  private ChopperAutostart mtf_a;
  private CharacterClassManager _hostCcm;
  private float decontaminationCooldown;
  public float timeToNextRespawn;
  public bool nextWaveIsCI;
  private bool loaded;
  private bool chopperStarted;
  [HideInInspector]
  public float respawnCooldown;
  private RateLimit _mtfCustomRateLimit;
  private RateLimit _ciThemeRateLimit;

  private void Start()
  {
    this._mtfCustomRateLimit = new RateLimit(4, 2.8f, (NetworkConnection) null);
    this._ciThemeRateLimit = new RateLimit(1, 3.5f, (NetworkConnection) null);
    this.maxMTFRespawnAmount = ConfigFile.ServerConfig.GetInt("maximum_MTF_respawn_amount", this.maxMTFRespawnAmount);
    this.maxCIRespawnAmount = ConfigFile.ServerConfig.GetInt("maximum_CI_respawn_amount", this.maxCIRespawnAmount);
    this.minMtfTimeToRespawn = ConfigFile.ServerConfig.GetInt("minimum_MTF_time_to_spawn", 200);
    this.maxMtfTimeToRespawn = ConfigFile.ServerConfig.GetInt("maximum_MTF_time_to_spawn", 400);
    this.CI_Percent = (float) ConfigFile.ServerConfig.GetInt("ci_respawn_percent", 35);
    if (!NetworkServer.active || !this.isServer || (!this.isLocalPlayer || TutorialManager.status))
      return;
    Timing.RunCoroutine(this._Update(), Segment.FixedUpdate);
  }

  public void SetDecontCooldown(float f)
  {
    this.decontaminationCooldown = f;
  }

  private void Update()
  {
    if ((double) this.decontaminationCooldown < 0.0)
      return;
    this.decontaminationCooldown -= Time.deltaTime;
  }

  private IEnumerator<float> _Update()
  {
    MTFRespawn mtfRespawn = this;
    mtfRespawn._hostCcm = mtfRespawn.GetComponent<CharacterClassManager>();
    if (NonFacilityCompatibility.currentSceneSettings.enableRespawning)
    {
      while ((UnityEngine.Object) mtfRespawn != (UnityEngine.Object) null)
      {
        if ((UnityEngine.Object) mtfRespawn.mtf_a == (UnityEngine.Object) null)
          mtfRespawn.mtf_a = UnityEngine.Object.FindObjectOfType<ChopperAutostart>();
        if (!mtfRespawn._hostCcm.RoundStarted)
        {
          yield return float.NegativeInfinity;
        }
        else
        {
          mtfRespawn.timeToNextRespawn -= 0.02f;
          if ((double) mtfRespawn.respawnCooldown >= 0.0)
            mtfRespawn.respawnCooldown -= 0.02f;
          if ((double) mtfRespawn.timeToNextRespawn < (mtfRespawn.nextWaveIsCI ? 13.5 : 18.0) && !mtfRespawn.loaded)
          {
            mtfRespawn.loaded = true;
            if (PlayerManager.players.Any<GameObject>((Func<GameObject, bool>) (ply => ply.GetComponent<CharacterClassManager>().CurClass == RoleType.Spectator)))
            {
              mtfRespawn.chopperStarted = true;
              if (mtfRespawn.nextWaveIsCI && !AlphaWarheadController.Host.detonated)
                mtfRespawn.SummonVan();
              else
                mtfRespawn.SummonChopper(true);
            }
          }
          if ((double) mtfRespawn.timeToNextRespawn < 0.0)
          {
            float maxDelay = 0.0f;
            if (!mtfRespawn.nextWaveIsCI && PlayerManager.players.Any<GameObject>((Func<GameObject, bool>) (item => item.GetComponent<CharacterClassManager>().CurClass == RoleType.Spectator && !item.GetComponent<ServerRoles>().OverwatchEnabled)))
            {
              bool warheadInProgress;
              bool cassieFree;
              do
              {
                warheadInProgress = (UnityEngine.Object) AlphaWarheadController.Host != (UnityEngine.Object) null && AlphaWarheadController.Host.inProgress && !AlphaWarheadController.Host.detonated;
                cassieFree = NineTailedFoxAnnouncer.singleton.Free && (double) mtfRespawn.decontaminationCooldown <= 0.0;
                yield return float.NegativeInfinity;
                maxDelay += 0.02f;
              }
              while ((double) maxDelay <= 70.0 && !cassieFree | warheadInProgress);
            }
            mtfRespawn.loaded = false;
            if (mtfRespawn.GetComponent<CharacterClassManager>().RoundStarted)
              mtfRespawn.SummonChopper(false);
            if (mtfRespawn.chopperStarted)
            {
              mtfRespawn.respawnCooldown = 35f;
              mtfRespawn.RespawnDeadPlayers();
            }
            mtfRespawn.nextWaveIsCI = (double) UnityEngine.Random.Range(0, 100) <= (double) mtfRespawn.CI_Percent;
            mtfRespawn.timeToNextRespawn = (float) UnityEngine.Random.Range(mtfRespawn.minMtfTimeToRespawn, mtfRespawn.maxMtfTimeToRespawn) * (mtfRespawn.nextWaveIsCI ? 1f / mtfRespawn.CI_Time_Multiplier : 1f);
            mtfRespawn.chopperStarted = false;
          }
          yield return float.NegativeInfinity;
        }
      }
    }
  }

  private void RespawnDeadPlayers()
  {
    int num1 = 0;
    List<GameObject> gameObjectList;
    if (ConfigFile.ServerConfig.GetBool("priority_mtf_respawn", true))
    {
      List<GameObject> list = PlayerManager.players.Where<GameObject>((Func<GameObject, bool>) (item => item.GetComponent<CharacterClassManager>().CurClass == RoleType.Spectator && !item.GetComponent<ServerRoles>().OverwatchEnabled)).OrderBy<GameObject, long>((Func<GameObject, long>) (item => item.GetComponent<CharacterClassManager>().DeathTime)).ToList<GameObject>();
      if (this.nextWaveIsCI)
      {
        if (list.Count > this.maxMTFRespawnAmount)
          list.RemoveRange(this.maxMTFRespawnAmount, list.Count - this.maxMTFRespawnAmount);
        else if (list.Count > this.maxCIRespawnAmount)
          list.RemoveRange(this.maxCIRespawnAmount, list.Count - this.maxCIRespawnAmount);
      }
      if (ConfigFile.ServerConfig.GetBool("use_crypto_rng", false))
        list.ShuffleListSecure<GameObject>();
      else
        list.ShuffleList<GameObject>();
      gameObjectList = list;
    }
    else
    {
      List<GameObject> list = PlayerManager.players.Where<GameObject>((Func<GameObject, bool>) (item => item.GetComponent<CharacterClassManager>().CurClass == RoleType.Spectator && !item.GetComponent<ServerRoles>().OverwatchEnabled)).ToList<GameObject>();
      if (ConfigFile.ServerConfig.GetBool("use_crypto_rng", false))
        list.ShuffleListSecure<GameObject>();
      else
        list.ShuffleList<GameObject>();
      if (this.nextWaveIsCI)
      {
        if (list.Count > this.maxMTFRespawnAmount)
          list.RemoveRange(this.maxMTFRespawnAmount, list.Count - this.maxMTFRespawnAmount);
        else if (list.Count > this.maxCIRespawnAmount)
          list.RemoveRange(this.maxCIRespawnAmount, list.Count - this.maxCIRespawnAmount);
      }
      gameObjectList = list;
    }
    this.playersToNTF.Clear();
    if (this.nextWaveIsCI && AlphaWarheadController.Host.detonated)
      this.nextWaveIsCI = false;
    foreach (GameObject ply in gameObjectList)
    {
      if (!((UnityEngine.Object) ply == (UnityEngine.Object) null))
      {
        ++num1;
        if (this.nextWaveIsCI)
        {
          this.GetComponent<CharacterClassManager>().SetPlayersClass(RoleType.ChaosInsurgency, ply, false, false);
          ServerLogs.AddLog(ServerLogs.Modules.ClassChange, ply.GetComponent<NicknameSync>().MyNick + " (" + ply.GetComponent<CharacterClassManager>().UserId + ") respawned as Chaos Insurgency agent.", ServerLogs.ServerLogType.GameEvent);
        }
        else
          this.playersToNTF.Add(ply);
      }
    }
    if (num1 > 0)
    {
      ServerLogs.AddLog(ServerLogs.Modules.ClassChange, (this.nextWaveIsCI ? "Chaos Insurgency" : "MTF") + " respawned!", ServerLogs.ServerLogType.GameEvent);
      if (this.nextWaveIsCI)
      {
        this.Invoke("CmdDelayCIAnnounc", 1f);
        if (ConfigFile.ServerConfig.GetBool("announce_chaos_breaches", false))
        {
          ServerConsole.AddLog("Chaos spawn");
          PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement("alert . all security personnel .g3 chaos JAM_070_3 insurgency has entered the facility at gate a . terminate all chaos insurgency JAM_070_4 immediately this is top priority for all security personnel", false, true);
          float num2 = UnityEngine.Random.Range(0.0f, 2f);
          ServerConsole.AddLog(num2.ToString());
          if ((double) num2 >= 1.0)
          {
            float forceDuration = UnityEngine.Random.Range(7f, 12f);
            ServerConsole.AddLog(forceDuration.ToString());
            Generator079.generators[0].RpcCustomOverchargeForOurBeautifulModCreators(forceDuration, false);
          }
        }
      }
    }
    this.SummonNTF();
  }

  [ServerCallback]
  public void SummonNTF()
  {
    if (!NetworkServer.active || this.playersToNTF.Count <= 0)
      return;
    char letter;
    int number;
    this.SetUnit(this.playersToNTF, out letter, out number);
    NineTailedFoxAnnouncer.singleton.AnnounceNtfEntrance(PlayerManager.players.Count<GameObject>((Func<GameObject, bool>) (item => item.GetComponent<CharacterClassManager>().IsScpButNotZombie())), number, letter);
    for (int index = 0; index < this.playersToNTF.Count; ++index)
    {
      if (index == 0)
      {
        this.GetComponent<CharacterClassManager>().SetPlayersClass(RoleType.NtfCommander, this.playersToNTF[index], false, false);
        ServerLogs.AddLog(ServerLogs.Modules.ClassChange, this.playersToNTF[index].GetComponent<NicknameSync>().MyNick + " (" + this.playersToNTF[index].GetComponent<CharacterClassManager>().UserId + ") respawned as MTF Commander.", ServerLogs.ServerLogType.GameEvent);
      }
      else if (index <= 3)
      {
        this.GetComponent<CharacterClassManager>().SetPlayersClass(RoleType.NtfLieutenant, this.playersToNTF[index], false, false);
        ServerLogs.AddLog(ServerLogs.Modules.ClassChange, this.playersToNTF[index].GetComponent<NicknameSync>().MyNick + " (" + this.playersToNTF[index].GetComponent<CharacterClassManager>().UserId + ") respawned as MTF Lieutenant.", ServerLogs.ServerLogType.GameEvent);
      }
      else
      {
        this.GetComponent<CharacterClassManager>().SetPlayersClass(RoleType.NtfCadet, this.playersToNTF[index], false, false);
        ServerLogs.AddLog(ServerLogs.Modules.ClassChange, this.playersToNTF[index].GetComponent<NicknameSync>().MyNick + " (" + this.playersToNTF[index].GetComponent<CharacterClassManager>().UserId + ") respawned as MTF Guard.", ServerLogs.ServerLogType.GameEvent);
      }
    }
    this.playersToNTF.Clear();
  }

  [ClientRpc]
  public void RpcPlayCustomAnnouncement(string words, bool makeHold, bool makeNoise)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(words);
    writer.WriteBoolean(makeHold);
    writer.WriteBoolean(makeNoise);
    this.SendRPCInternal(typeof (MTFRespawn), nameof (RpcPlayCustomAnnouncement), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ServerCallback]
  private void SetUnit(GameObject[] ply, out char letter, out int number)
  {
    if (!NetworkServer.active)
    {
      // ISSUE: cast to a reference type
      // ISSUE: explicit reference operation
      ^(int&) ref letter = 0;
      number = 0;
    }
    else
    {
      int num = this.GetComponent<NineTailedFoxUnits>().NewName(out number, out letter);
      foreach (GameObject gameObject in ply)
        gameObject.GetComponent<CharacterClassManager>().NetworkNtfUnit = num;
    }
  }

  [ServerCallback]
  private void SetUnit(List<GameObject> ply, out char letter, out int number)
  {
    if (!NetworkServer.active)
    {
      // ISSUE: cast to a reference type
      // ISSUE: explicit reference operation
      ^(int&) ref letter = 0;
      number = 0;
    }
    else
    {
      int num = this.GetComponent<NineTailedFoxUnits>().NewName(out number, out letter);
      foreach (GameObject gameObject in ply)
        gameObject.GetComponent<CharacterClassManager>().NetworkNtfUnit = num;
    }
  }

  [ServerCallback]
  private void SummonChopper(bool state)
  {
    if (!NetworkServer.active || !NonFacilityCompatibility.currentSceneSettings.enableStandardGamplayItems)
      return;
    this.mtf_a.SetState(state);
  }

  [ServerCallback]
  private void SummonVan()
  {
    if (!NetworkServer.active || !NonFacilityCompatibility.currentSceneSettings.enableStandardGamplayItems)
      return;
    this.RpcVan();
  }

  [ClientRpc]
  private void RpcVan()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (MTFRespawn), nameof (RpcVan), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void CmdDelayCIAnnounc()
  {
    this.PlayAnnoncCI();
  }

  [ServerCallback]
  private void PlayAnnoncCI()
  {
    if (!NetworkServer.active)
      return;
    this.RpcAnnouncCI();
  }

  [ClientRpc]
  private void RpcAnnouncCI()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (MTFRespawn), nameof (RpcAnnouncCI), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeRpcRpcPlayCustomAnnouncement(
    NetworkBehaviour obj,
    NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcPlayCustomAnnouncement called on server.");
    else
      ((MTFRespawn) obj).CallRpcPlayCustomAnnouncement(reader.ReadString(), reader.ReadBoolean(), reader.ReadBoolean());
  }

  protected static void InvokeRpcRpcVan(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcVan called on server.");
    else
      ((MTFRespawn) obj).CallRpcVan();
  }

  protected static void InvokeRpcRpcAnnouncCI(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcAnnouncCI called on server.");
    else
      ((MTFRespawn) obj).CallRpcAnnouncCI();
  }

  public void CallRpcPlayCustomAnnouncement(string words, bool makeHold, bool makeNoise)
  {
    if (!this._mtfCustomRateLimit.CanExecute(true) || words == null)
      return;
    NineTailedFoxAnnouncer.singleton.AddPhraseToQueue(words, makeNoise, false, makeHold);
  }

  public void CallRpcVan()
  {
    GameObject.Find("CIVanArrive").GetComponent<Animator>().SetTrigger("Arrive");
  }

  public void CallRpcAnnouncCI()
  {
    if (!this._ciThemeRateLimit.CanExecute(true))
      return;
    foreach (GameObject player in PlayerManager.players)
    {
      CharacterClassManager component = player.GetComponent<CharacterClassManager>();
      if (component.isLocalPlayer)
      {
        switch (component.Classes.SafeGet(component.CurClass).team)
        {
          case Team.CHI:
          case Team.CDP:
            UnityEngine.Object.Instantiate<GameObject>(this.ciTheme);
            continue;
          default:
            if (!component.GetComponent<ServerRoles>().OverwatchEnabled)
              continue;
            goto case Team.CHI;
        }
      }
    }
  }

  static MTFRespawn()
  {
    NetworkBehaviour.RegisterRpcDelegate(typeof (MTFRespawn), "RpcPlayCustomAnnouncement", new NetworkBehaviour.CmdDelegate(MTFRespawn.InvokeRpcRpcPlayCustomAnnouncement));
    NetworkBehaviour.RegisterRpcDelegate(typeof (MTFRespawn), "RpcVan", new NetworkBehaviour.CmdDelegate(MTFRespawn.InvokeRpcRpcVan));
    NetworkBehaviour.RegisterRpcDelegate(typeof (MTFRespawn), "RpcAnnouncCI", new NetworkBehaviour.CmdDelegate(MTFRespawn.InvokeRpcRpcAnnouncCI));
  }
}
