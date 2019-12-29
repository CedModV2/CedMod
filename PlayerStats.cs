// Decompiled with JetBrains decompiler
// Type: PlayerStats
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using CedMod;
using Dissonance.Integrations.MirrorIgnorance;
using GameCore;
using Mirror;
using RemoteAdmin;
using Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
  private static Lift[] _lifts = new Lift[0];
  public PlayerStats.HitInfo lastHitInfo = new PlayerStats.HitInfo(0.0f, "NONE", DamageTypes.None, 0);
  internal List<string> badguylist = new List<string>();
  public Transform[] grenadePoints;
  public CharacterClassManager ccm;
  private UserMainInterface _ui;
  private static DateTime rrTime;
  public bool used914;
  private bool _pocketCleanup;
  private bool _allowSPDmg;
  private int _maxHP;
  private float _health;
  private bool _hpDirty;
  [SyncVar]
  public byte syncArtificialHealth;
  public float unsyncedArtificialHealth;
  public float artificialNormalRatio;
  public int maxArtificialHealth;
  private RateLimit _interactRateLimit;
  private float killstreak_time;
  private int killstreak;
  private readonly Scp079Interactable.InteractableType[] _filters;

  public int maxHP
  {
    get
    {
      return this._maxHP;
    }
    set
    {
      this._maxHP = value;
    }
  }

  public float health
  {
    get
    {
      return this._health;
    }
    set
    {
      this._health = value;
      this._hpDirty = true;
    }
  }

  public void MakeHpDirty()
  {
    this._hpDirty = true;
  }

  private void Start()
  {
    this._interactRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[0];
    this._pocketCleanup = ConfigFile.ServerConfig.GetBool("SCP106_CLEANUP", false);
    this._allowSPDmg = ConfigFile.ServerConfig.GetBool("spawn_protect_allow_dmg", true);
    this.ccm = this.GetComponent<CharacterClassManager>();
    this._ui = UserMainInterface.singleton;
    if (PlayerStats._lifts.Length == 0)
      PlayerStats._lifts = UnityEngine.Object.FindObjectsOfType<Lift>();
    if (!NetworkServer.active)
      return;
    if (!PlayerPrefsSl.HasKey("LastRoundrestartTime", PlayerPrefsSl.DataType.Int))
      PlayerPrefsSl.Set("LastRoundrestartTime", 5000);
    TimeSpan timeSpan = DateTime.Now - PlayerStats.rrTime;
    if (timeSpan.TotalSeconds > 20.0)
      Debug.Log((object) "Restart too long or the server has just started.");
    else
      PlayerPrefsSl.Set("LastRoundrestartTime", (PlayerPrefsSl.Get("LastRoundrestartTime", 5000) + (int) timeSpan.TotalMilliseconds) / 2);
  }

  private void Update()
  {
    if (!this._hpDirty)
      return;
    this._hpDirty = false;
    if (NetworkServer.active)
      this.TargetSyncHp(this.connectionToClient, this._health);
    foreach (GameObject player in PlayerManager.players)
    {
      CharacterClassManager component = player.GetComponent<CharacterClassManager>();
      if (component.CurClass == RoleType.Spectator && component.IsVerified)
        this.TargetSyncHp(component.connectionToClient, this._health);
    }
  }

  [TargetRpc]
  public void TargetSyncHp(NetworkConnection conn, float hp)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteSingle(hp);
    this.SendTargetRPCInternal(conn, typeof (PlayerStats), nameof (TargetSyncHp), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  public float GetHealthPercent()
  {
    return this.ccm.CurClass < RoleType.Scp173 ? 0.0f : Mathf.Clamp01((float) (1.0 - (double) this.health / (double) this.ccm.Classes.SafeGet(this.ccm.CurClass).maxHP));
  }

  [Command]
  public void CmdSelfDeduct(PlayerStats.HitInfo info)
  {
    if (this.isServer)
    {
      this.CallCmdSelfDeduct(info);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      GeneratedNetworkCode._WriteHitInfo_PlayerStats(writer, info);
      this.SendCommandInternal(typeof (PlayerStats), nameof (CmdSelfDeduct), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  public bool Explode(bool inElevator)
  {
    bool flag = (double) this.health > 0.0 && (inElevator || (double) this.transform.position.y < 900.0);
    switch (this.ccm.CurClass)
    {
      case RoleType.Scp106:
        Scp106PlayerScript component = this.GetComponent<Scp106PlayerScript>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          component.DeletePortal();
          if (component.goingViaThePortal)
          {
            flag = true;
            break;
          }
          break;
        }
        break;
      case RoleType.Scp079:
        flag = true;
        break;
    }
    return flag && this.HurtPlayer(new PlayerStats.HitInfo(-1f, "WORLD", DamageTypes.Nuke, 0), this.gameObject);
  }

  [Command]
  public void CmdTesla()
  {
    if (this.isServer)
    {
      this.CallCmdTesla();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (PlayerStats), nameof (CmdTesla), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  public void SetHPAmount(int hp)
  {
    this.health = (float) hp;
  }

  public bool HealHPAmount(float hp)
  {
    float num = Mathf.Clamp(hp, 0.0f, (float) this.maxHP - this.health);
    this.health += num;
    return (double) num > 0.0;
  }

  public bool HurtPlayer(PlayerStats.HitInfo info, GameObject go)
  {
    bool flag1 = false;
    bool flag2 = (UnityEngine.Object) go == (UnityEngine.Object) null;
    if ((double) info.Amount < 0.0)
    {
      if (flag2)
      {
        info.Amount = Mathf.Abs(999999f);
      }
      else
      {
        PlayerStats component = go.GetComponent<PlayerStats>();
        info.Amount = (UnityEngine.Object) component != (UnityEngine.Object) null ? Mathf.Abs((float) ((double) component.health + (double) component.syncArtificialHealth + 10.0)) : Mathf.Abs(999999f);
      }
    }
    if ((double) info.Amount > 2147483648.0)
      info.Amount = (float) int.MaxValue;
    if (flag2)
      return flag1;
    PlayerStats component1 = go.GetComponent<PlayerStats>();
    CharacterClassManager component2 = go.GetComponent<CharacterClassManager>();
    if ((UnityEngine.Object) component1 == (UnityEngine.Object) null || (UnityEngine.Object) component2 == (UnityEngine.Object) null || component2.GodMode || this.ccm.Classes.SafeGet(this.ccm.CurClass).team == Team.SCP && this.ccm.Classes.SafeGet(component2.CurClass).team == Team.SCP && (UnityEngine.Object) this.ccm != (UnityEngine.Object) component2 || component2.SpawnProtected && !this._allowSPDmg)
      return false;
    if (this.isLocalPlayer && info.PlyId != go.GetComponent<QueryProcessor>().PlayerId)
      RoundSummary.Damages += (double) component1.health < (double) info.Amount ? component1.health : info.Amount;
    if (this.lastHitInfo.Attacker == "ARTIFICIALDEGEN")
    {
      component1.unsyncedArtificialHealth -= info.Amount;
      if ((double) component1.unsyncedArtificialHealth < 0.0)
        component1.unsyncedArtificialHealth = 0.0f;
    }
    else
    {
      if ((double) component1.unsyncedArtificialHealth > 0.0)
      {
        float num1 = info.Amount * this.artificialNormalRatio;
        float num2 = info.Amount - num1;
        component1.unsyncedArtificialHealth -= num1;
        if ((double) component1.unsyncedArtificialHealth < 0.0)
        {
          num2 += Mathf.Abs(component1.unsyncedArtificialHealth);
          component1.unsyncedArtificialHealth = 0.0f;
        }
        component1.health -= num2;
        if ((double) component1.health > 0.0 && (double) component1.health - (double) num1 <= 0.0)
          this.TargetAchieve(this.connectionToClient, "didntevenfeelthat");
      }
      else
        component1.health -= info.Amount;
      if ((double) component1.health < 0.0)
        component1.health = 0.0f;
      component1.lastHitInfo = info;
    }
    if ((double) component1.health < 1.0 && component2.CurClass != RoleType.Spectator)
    {
      foreach (Scp079PlayerScript instance in Scp079PlayerScript.instances)
      {
        Scp079Interactable.ZoneAndRoom otherRoom = go.GetComponent<Scp079PlayerScript>().GetOtherRoom();
        bool flag3 = false;
        foreach (Scp079Interaction scp079Interaction in instance.ReturnRecentHistory(12f, this._filters))
        {
          foreach (Scp079Interactable.ZoneAndRoom currentZonesAndRoom in scp079Interaction.interactable.currentZonesAndRooms)
          {
            if (currentZonesAndRoom.currentZone == otherRoom.currentZone && currentZonesAndRoom.currentRoom == otherRoom.currentRoom)
              flag3 = true;
          }
        }
        if (flag3)
          instance.RpcGainExp(ExpGainType.KillAssist, component2.CurClass);
      }
      if (RoundSummary.RoundInProgress() && RoundSummary.roundTime < 60)
        this.TargetAchieve(component2.connectionToClient, "wowreally");
      if (this.isLocalPlayer && info.PlyId != go.GetComponent<QueryProcessor>().PlayerId)
        ++RoundSummary.Kills;
      flag1 = true;
      foreach (Scp049PlayerScript scp049PlayerScript in UnityEngine.Object.FindObjectsOfType<Scp049PlayerScript>())
        scp049PlayerScript.RpcSetDeathTime(go);
      if (component2.CurClass == RoleType.Scp096 && go.GetComponent<Scp096PlayerScript>().enraged == Scp096PlayerScript.RageState.Panic)
        this.TargetAchieve(component2.connectionToClient, "unvoluntaryragequit");
      else if (info.GetDamageType() == DamageTypes.Pocket)
        this.TargetAchieve(component2.connectionToClient, "newb");
      else if (info.GetDamageType() == DamageTypes.Scp173)
        this.TargetAchieve(component2.connectionToClient, "firsttime");
      else if (info.GetDamageType() == DamageTypes.Grenade && info.PlyId == go.GetComponent<QueryProcessor>().PlayerId)
        this.TargetAchieve(component2.connectionToClient, "iwanttobearocket");
      else if (info.GetDamageType().isWeapon)
      {
        Inventory component3 = component2.GetComponent<Inventory>();
        if (component2.CurClass == RoleType.Scientist)
        {
          Item itemById = component3.GetItemByID(component3.curItem);
          if (itemById != null && itemById.itemCategory == ItemCategory.Keycard && this.GetComponent<CharacterClassManager>().CurClass == RoleType.ClassD)
            this.TargetAchieve(this.connectionToClient, "betrayal");
        }
        if ((double) Time.realtimeSinceStartup - (double) this.killstreak_time > 30.0 || this.killstreak == 0)
        {
          this.killstreak = 0;
          this.killstreak_time = Time.realtimeSinceStartup;
        }
        if (this.GetComponent<WeaponManager>().GetShootPermission(component2, true))
          ++this.killstreak;
        if (this.killstreak > 5)
          this.TargetAchieve(this.connectionToClient, "pewpew");
        if ((this.ccm.Classes.SafeGet(this.ccm.CurClass).team == Team.MTF || this.ccm.Classes.SafeGet(this.ccm.CurClass).team == Team.RSC) && component2.CurClass == RoleType.ClassD)
          this.TargetStats(this.connectionToClient, "dboys_killed", "justresources", 50);
        if (this.ccm.Classes.SafeGet(this.ccm.CurClass).team == Team.RSC && this.ccm.Classes.SafeGet(component2.CurClass).team == Team.SCP)
          this.TargetAchieve(this.connectionToClient, "timetodoitmyself");
      }
      else if (this.ccm.Classes.SafeGet(this.ccm.CurClass).team == Team.SCP && go.GetComponent<MicroHID>().CurrentHidState != MicroHID.MicroHidState.Idle)
        this.TargetAchieve(this.connectionToClient, "illpassthanks");
      ServerLogs.AddLog(ServerLogs.Modules.ClassChange, go.GetComponent<NicknameSync>().MyNick + " (" + go.GetComponent<CharacterClassManager>().UserId + ") killed by " + info.Attacker + " using " + info.GetDamageName() + ".", ServerLogs.ServerLogType.KillLog);
      this.FFA(component2, this.ccm);
      if (!this._pocketCleanup || info.GetDamageType() != DamageTypes.Pocket)
      {
        go.GetComponent<Inventory>().ServerDropAll();
        if (component2.Classes.CheckBounds(component2.CurClass) && info.GetDamageType() != DamageTypes.RagdollLess)
          this.GetComponent<RagdollManager>().SpawnRagdoll(go.transform.position, go.transform.rotation, (int) component2.CurClass, info, component2.Classes.SafeGet(component2.CurClass).team > Team.SCP, go.GetComponent<MirrorIgnorancePlayer>().PlayerId, go.GetComponent<NicknameSync>().MyNick, go.GetComponent<QueryProcessor>().PlayerId);
      }
      else
        go.GetComponent<Inventory>().Clear();
      component2.NetworkDeathPosition = go.transform.position;
      if (component2.Classes.SafeGet(component2.CurClass).team == Team.SCP)
      {
        if (component2.CurClass == RoleType.Scp0492)
        {
          NineTailedFoxAnnouncer.CheckForZombies(go);
        }
        else
        {
          GameObject gameObject = (GameObject) null;
          foreach (GameObject player in PlayerManager.players)
          {
            if (player.GetComponent<QueryProcessor>().PlayerId == info.PlyId)
              gameObject = player;
          }
          if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
          {
            NineTailedFoxAnnouncer.AnnounceScpTermination(component2.Classes.SafeGet(component2.CurClass), info, "");
          }
          else
          {
            DamageTypes.DamageType damageType = info.GetDamageType();
            if (damageType == DamageTypes.Tesla)
              NineTailedFoxAnnouncer.AnnounceScpTermination(component2.Classes.SafeGet(component2.CurClass), info, "TESLA");
            else if (damageType == DamageTypes.Nuke)
              NineTailedFoxAnnouncer.AnnounceScpTermination(component2.Classes.SafeGet(component2.CurClass), info, "WARHEAD");
            else if (damageType == DamageTypes.Decont)
              NineTailedFoxAnnouncer.AnnounceScpTermination(component2.Classes.SafeGet(component2.CurClass), info, "DECONTAMINATION");
            else if (component2.CurClass != RoleType.Scp079)
              NineTailedFoxAnnouncer.AnnounceScpTermination(component2.Classes.SafeGet(component2.CurClass), info, "UNKNOWN");
          }
        }
      }
      component1.SetHPAmount(100);
      component2.SetClassID(RoleType.Spectator);
    }
    else
    {
      Vector3 pos = Vector3.zero;
      float num = 40f;
      if (info.GetDamageType().isWeapon)
      {
        GameObject playerOfId = this.GetPlayerOfID(info.PlyId);
        if ((UnityEngine.Object) playerOfId != (UnityEngine.Object) null)
        {
          pos = go.transform.InverseTransformPoint(playerOfId.transform.position).normalized;
          num = 100f;
        }
      }
      else if (info.GetDamageType() == DamageTypes.Pocket)
      {
        PlyMovementSync component3 = this.ccm.GetComponent<PlyMovementSync>();
        if ((double) component3.RealModelPosition.y > -1900.0)
          component3.OverridePosition(Vector3.down * 1998.5f, 0.0f, true);
      }
      this.TargetOofEffect(go.GetComponent<NetworkIdentity>().connectionToClient, pos, Mathf.Clamp01(info.Amount / num));
    }
    return flag1;
  }

  [TargetRpc]
  public void TargetAchieve(NetworkConnection conn, string key)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(key);
    this.SendTargetRPCInternal(conn, typeof (PlayerStats), nameof (TargetAchieve), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [TargetRpc]
  public void TargetStats(
    NetworkConnection conn,
    string key,
    string targetAchievement,
    int maxValue)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(key);
    writer.WriteString(targetAchievement);
    writer.WritePackedInt32(maxValue);
    this.SendTargetRPCInternal(conn, typeof (PlayerStats), nameof (TargetStats), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private GameObject GetPlayerOfID(int id)
  {
    return PlayerManager.players.FirstOrDefault<GameObject>((Func<GameObject, bool>) (ply => ply.GetComponent<QueryProcessor>().PlayerId == id));
  }

  [TargetRpc]
  private void TargetOofEffect(NetworkConnection conn, Vector3 pos, float overall)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteVector3(pos);
    writer.WriteSingle(overall);
    this.SendTargetRPCInternal(conn, typeof (PlayerStats), nameof (TargetOofEffect), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  private void RpcRoundrestart(float timeOffset)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteSingle(timeOffset);
    this.SendRPCInternal(typeof (PlayerStats), nameof (RpcRoundrestart), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  public void Roundrestart()
  {
    foreach (MirrorIgnorancePlayer mirrorIgnorancePlayer in UnityEngine.Object.FindObjectsOfType<MirrorIgnorancePlayer>())
      mirrorIgnorancePlayer.OnDisable();
    PlayerManager.localPlayer.GetComponent<PlayerStats>().badguylist.Clear();
    this.GetComponent<DecontaminationLCZ>().CallDeconStop();
    this.RpcRoundrestart((float) (PlayerPrefsSl.Get("LastRoundrestartTime", 5000) / 1000));
    this.Invoke("ChangeLevel", 2.5f);
  }

  private void ChangeLevel()
  {
    if (!NetworkServer.active)
      NetworkManager.singleton.StopClient();
    else if (ServerStatic.StopNextRound)
    {
      ServerConsole.AddLog("Stopping the server (StopNextRound command was used)...");
      Application.Quit();
    }
    else
    {
      GC.Collect();
      PlayerStats.rrTime = DateTime.Now;
      NetworkManager.singleton.ServerChangeScene(NetworkManager.singleton.onlineScene);
    }
  }

  public string HealthToString()
  {
    float num1 = Mathf.Round(this.health);
    double num2 = (double) num1 / (double) this.maxHP * 100.0;
    return num1.ToString((IFormatProvider) CultureInfo.InvariantCulture) + "/" + (object) this.maxHP + " (" + num2.ToString("####0.##", (IFormatProvider) CultureInfo.InvariantCulture) + "%)";
  }

  static PlayerStats()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlayerStats), "CmdSelfDeduct", new NetworkBehaviour.CmdDelegate(PlayerStats.InvokeCmdCmdSelfDeduct));
    NetworkBehaviour.RegisterCommandDelegate(typeof (PlayerStats), "CmdTesla", new NetworkBehaviour.CmdDelegate(PlayerStats.InvokeCmdCmdTesla));
    NetworkBehaviour.RegisterRpcDelegate(typeof (PlayerStats), "RpcRoundrestart", new NetworkBehaviour.CmdDelegate(PlayerStats.InvokeRpcRpcRoundrestart));
    NetworkBehaviour.RegisterRpcDelegate(typeof (PlayerStats), "TargetSyncHp", new NetworkBehaviour.CmdDelegate(PlayerStats.InvokeRpcTargetSyncHp));
    NetworkBehaviour.RegisterRpcDelegate(typeof (PlayerStats), "TargetAchieve", new NetworkBehaviour.CmdDelegate(PlayerStats.InvokeRpcTargetAchieve));
    NetworkBehaviour.RegisterRpcDelegate(typeof (PlayerStats), "TargetStats", new NetworkBehaviour.CmdDelegate(PlayerStats.InvokeRpcTargetStats));
    NetworkBehaviour.RegisterRpcDelegate(typeof (PlayerStats), "TargetOofEffect", new NetworkBehaviour.CmdDelegate(PlayerStats.InvokeRpcTargetOofEffect));
  }

  private void MirrorProcessed()
  {
  }

  public byte NetworksyncArtificialHealth
  {
    get
    {
      return this.syncArtificialHealth;
    }
    [param: In] set
    {
      this.SetSyncVar<byte>(value, ref this.syncArtificialHealth, 1UL);
    }
  }

  protected static void InvokeCmdCmdSelfDeduct(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSelfDeduct called on client.");
    else
      ((PlayerStats) obj).CallCmdSelfDeduct(GeneratedNetworkCode._ReadHitInfo_PlayerStats(reader));
  }

  protected static void InvokeCmdCmdTesla(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdTesla called on client.");
    else
      ((PlayerStats) obj).CallCmdTesla();
  }

  public void CallCmdSelfDeduct(PlayerStats.HitInfo info)
  {
    if (!this._interactRateLimit.CanExecute(true))
      return;
    this.HurtPlayer(info, this.gameObject);
  }

  public void CallCmdTesla()
  {
    if (!this._interactRateLimit.CanExecute(true))
      return;
    this.HurtPlayer(new PlayerStats.HitInfo((float) UnityEngine.Random.Range(100, 200), this.GetComponent<MirrorIgnorancePlayer>().PlayerId, DamageTypes.Tesla, 0), this.gameObject);
  }

  protected static void InvokeRpcRpcRoundrestart(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcRoundrestart called on server.");
    else
      ((PlayerStats) obj).CallRpcRoundrestart(reader.ReadSingle());
  }

  protected static void InvokeRpcTargetSyncHp(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetSyncHp called on server.");
    else
      ((PlayerStats) obj).CallTargetSyncHp(ClientScene.readyConnection, reader.ReadSingle());
  }

  protected static void InvokeRpcTargetAchieve(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetAchieve called on server.");
    else
      ((PlayerStats) obj).CallTargetAchieve(ClientScene.readyConnection, reader.ReadString());
  }

  protected static void InvokeRpcTargetStats(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetStats called on server.");
    else
      ((PlayerStats) obj).CallTargetStats(ClientScene.readyConnection, reader.ReadString(), reader.ReadString(), reader.ReadPackedInt32());
  }

  protected static void InvokeRpcTargetOofEffect(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetOofEffect called on server.");
    else
      ((PlayerStats) obj).CallTargetOofEffect(ClientScene.readyConnection, reader.ReadVector3(), reader.ReadSingle());
  }

  public void CallRpcRoundrestart(float timeOffset)
  {
    if (this.isServer)
      return;
    UnityEngine.Object.FindObjectOfType<CustomNetworkManager>().reconnectTime = timeOffset;
    this.Invoke("ChangeLevel", 0.5f);
  }

  public void CallTargetSyncHp(NetworkConnection conn, float hp)
  {
    this._health = hp;
  }

  public void CallTargetAchieve(NetworkConnection conn, string key)
  {
  }

  public void CallTargetStats(
    NetworkConnection conn,
    string key,
    string targetAchievement,
    int maxValue)
  {
  }

  public void CallTargetOofEffect(NetworkConnection conn, Vector3 pos, float overall)
  {
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteByte(this.syncArtificialHealth);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteByte(this.syncArtificialHealth);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworksyncArtificialHealth = reader.ReadByte();
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      this.NetworksyncArtificialHealth = reader.ReadByte();
    }
  }

  public void FFA(CharacterClassManager victim, CharacterClassManager killer)
  {
    if (!ConfigFile.ServerConfig.GetBool("ffa_enable", false) || !RoundSummary.RoundInProgress())
      return;
    bool flag = false;
    Team team1 = victim.Classes.SafeGet(victim.CurClass).team;
    Team team2 = killer.Classes.SafeGet(killer.CurClass).team;
    Initializer.logger.Debug(nameof (FFA), "VictimDetails: " + victim.gameObject.GetComponent<CharacterClassManager>().UserId + " " + victim.gameObject.GetComponent<NicknameSync>().MyNick + " " + victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() + " " + (object) team1 + " " + (object) victim.CurClass);
    Initializer.logger.Debug(nameof (FFA), "KillerDetails: " + killer.gameObject.GetComponent<CharacterClassManager>().UserId + " " + killer.gameObject.GetComponent<NicknameSync>().MyNick + " " + killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() + " " + (object) team2 + " " + (object) killer.CurClass);
    if (killer.CurClass != RoleType.Tutorial)
    {
      if (victim.CurClass == killer.CurClass && victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString())
      {
        flag = true;
        Initializer.logger.Debug(nameof (FFA), "Teamkill1");
        Initializer.logger.Debug(nameof (FFA), "VictimDetails: " + victim.gameObject.GetComponent<CharacterClassManager>().UserId + " " + victim.gameObject.GetComponent<NicknameSync>().MyNick + " " + victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() + " " + (object) team1 + " " + (object) victim.CurClass);
        Initializer.logger.Debug(nameof (FFA), "Killer: " + killer.gameObject.GetComponent<CharacterClassManager>().UserId + " " + killer.gameObject.GetComponent<NicknameSync>().MyNick + " " + killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() + " " + (object) team2 + " " + (object) killer.CurClass);
      }
      else if (victim.CurClass == RoleType.ClassD && killer.CurClass == RoleType.ChaosInsurgency && victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString())
      {
        flag = true;
        Initializer.logger.Debug(nameof (FFA), "Teamkill2");
        Initializer.logger.Debug(nameof (FFA), "VictimDetails: " + victim.gameObject.GetComponent<CharacterClassManager>().UserId + " " + victim.gameObject.GetComponent<NicknameSync>().MyNick + " " + victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() + " " + (object) team1 + " " + (object) victim.CurClass);
        Initializer.logger.Debug(nameof (FFA), "Killer: " + killer.gameObject.GetComponent<CharacterClassManager>().UserId + " " + killer.gameObject.GetComponent<NicknameSync>().MyNick + " " + killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() + " " + (object) team2 + " " + (object) killer.CurClass);
      }
      else if (victim.CurClass == RoleType.ChaosInsurgency && killer.CurClass == RoleType.ClassD && victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString())
      {
        flag = true;
        Initializer.logger.Debug(nameof (FFA), "Teamkill3");
        Initializer.logger.Debug(nameof (FFA), "VictimDetails: " + victim.gameObject.GetComponent<CharacterClassManager>().UserId + " " + victim.gameObject.GetComponent<NicknameSync>().MyNick + " " + victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() + " " + (object) team1 + " " + (object) victim.CurClass);
        Initializer.logger.Debug(nameof (FFA), "Killer: " + killer.gameObject.GetComponent<CharacterClassManager>().UserId + " " + killer.gameObject.GetComponent<NicknameSync>().MyNick + " " + killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() + " " + (object) team2 + " " + (object) killer.CurClass);
      }
      else if (team1 == team2 && victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString())
      {
        flag = true;
        Initializer.logger.Debug(nameof (FFA), "Teamkill4");
        Initializer.logger.Debug(nameof (FFA), "VictimDetails: " + victim.gameObject.GetComponent<CharacterClassManager>().UserId + " " + victim.gameObject.GetComponent<NicknameSync>().MyNick + " " + victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() + " " + (object) team1 + " " + (object) victim.CurClass);
        Initializer.logger.Debug(nameof (FFA), "Killer: " + killer.gameObject.GetComponent<CharacterClassManager>().UserId + " " + killer.gameObject.GetComponent<NicknameSync>().MyNick + " " + killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() + " " + (object) team2 + " " + (object) killer.CurClass);
      }
      else if (team2 == Team.MTF && victim.CurClass == RoleType.Scientist && victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString())
      {
        flag = true;
        Initializer.logger.Debug(nameof (FFA), "Teamkill5");
        Initializer.logger.Debug(nameof (FFA), "VictimDetails: " + victim.gameObject.GetComponent<CharacterClassManager>().UserId + " " + victim.gameObject.GetComponent<NicknameSync>().MyNick + " " + victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() + " " + (object) team1 + " " + (object) victim.CurClass);
        Initializer.logger.Debug(nameof (FFA), "Killer: " + killer.gameObject.GetComponent<CharacterClassManager>().UserId + " " + killer.gameObject.GetComponent<NicknameSync>().MyNick + " " + killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() + " " + (object) team2 + " " + (object) killer.CurClass);
      }
      else if (team1 == Team.MTF && killer.CurClass == RoleType.Scientist && victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString())
      {
        flag = true;
        Initializer.logger.Debug(nameof (FFA), "Teamkill6");
        Initializer.logger.Debug(nameof (FFA), "VictimDetails: " + victim.gameObject.GetComponent<CharacterClassManager>().UserId + " " + victim.gameObject.GetComponent<NicknameSync>().MyNick + " " + victim.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() + " " + (object) team1 + " " + (object) victim.CurClass);
        Initializer.logger.Debug(nameof (FFA), "Killer: " + killer.gameObject.GetComponent<CharacterClassManager>().UserId + " " + killer.gameObject.GetComponent<NicknameSync>().MyNick + " " + killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() + " " + (object) team2 + " " + (object) killer.CurClass);
      }
    }
    if (!flag)
      return;
    if (killer.GetComponent<NetworkIdentity>().connectionToClient != null)
      QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(killer.gameObject.GetComponent<NetworkIdentity>().connectionToClient, "<size=25><b><color=yellow>You teamkilled: </color></b><color=red>" + victim.gameObject.GetComponent<NicknameSync>().MyNick + "</color><color=yellow><b> If you continue teamkilling it will result in a ban</b></color></size>", 20U, false);
    if (victim.GetComponent<NetworkIdentity>().connectionToClient != null)
      QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(victim.gameObject.GetComponent<NetworkIdentity>().connectionToClient, "<size=35><b><color=yellow>You have been teamkilled by: </color></b></size>" + "<color=red><size=35>" + killer.gameObject.GetComponent<NicknameSync>().MyNick + " (" + killer.gameObject.GetComponent<CharacterClassManager>().UserId + ") </size></color>" + "<size=35><b><color=yellow> Use this as a screenshot as evidence for a report</color></b></size>" + "<size=35><i><color=yellow> Note: if he continues to teamkill the server will ban him</color></i></size>", 20U, false);
    this.badguylist.Add(killer.gameObject.GetComponent<CharacterClassManager>().UserId);
    int num = 0;
    foreach (string str in this.badguylist)
    {
      if (str == killer.gameObject.GetComponent<CharacterClassManager>().UserId.ToString())
      {
        ++num;
        if (num >= ConfigFile.ServerConfig.GetInt("ffa_ammountoftkbeforeban", 3))
        {
          Initializer.logger.Info(nameof (FFA), "Player: " + killer.gameObject.GetComponent<NicknameSync>().MyNick + " " + killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString() + " " + killer.gameObject.GetComponent<CharacterClassManager>().UserId + " exeeded teamkill limit");
          QueryProcessor.Localplayer.GetComponent<BanPlayer>().BanUser(killer.gameObject, ConfigFile.ServerConfig.GetInt("ffa_banduration", 4320), ConfigFile.ServerConfig.GetString("ffa_banreason", "You have teamkilled too many people"), "Server.Module.FriendlyFireAutoban");
        }
      }
    }
  }

  [Serializable]
  public struct HitInfo : IEquatable<PlayerStats.HitInfo>
  {
    public float Amount;
    public readonly int Tool;
    public readonly int Time;
    public readonly string Attacker;
    public readonly int PlyId;

    public HitInfo(float amnt, string attackerName, DamageTypes.DamageType weapon, int attackerId)
    {
      this.Amount = amnt;
      this.Tool = DamageTypes.ToIndex(weapon);
      this.Attacker = attackerName;
      this.PlyId = attackerId;
      this.Time = ServerTime.time;
    }

    public GameObject GetPlayerObject()
    {
      foreach (GameObject player in PlayerManager.players)
      {
        if (player.GetComponent<QueryProcessor>().PlayerId == this.PlyId)
          return player;
      }
      return (GameObject) null;
    }

    public DamageTypes.DamageType GetDamageType()
    {
      return DamageTypes.FromIndex(this.Tool);
    }

    public string GetDamageName()
    {
      return DamageTypes.FromIndex(this.Tool).name;
    }

    public bool Equals(PlayerStats.HitInfo other)
    {
      return (double) Math.Abs(this.Amount - other.Amount) < 0.00499999988824129 && this.Tool == other.Tool && (this.Time == other.Time && string.Equals(this.Attacker, other.Attacker)) && this.PlyId == other.PlyId;
    }

    public override bool Equals(object obj)
    {
      return obj is PlayerStats.HitInfo other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return (((this.Amount.GetHashCode() * 397 ^ this.Tool) * 397 ^ this.Time) * 397 ^ (this.Attacker != null ? this.Attacker.GetHashCode() : 0)) * 397 ^ this.PlyId;
    }

    public static bool operator ==(PlayerStats.HitInfo left, PlayerStats.HitInfo right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(PlayerStats.HitInfo left, PlayerStats.HitInfo right)
    {
      return !left.Equals(right);
    }
  }
}
