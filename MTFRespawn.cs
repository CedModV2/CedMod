using System;
using System.Collections.Generic;
using System.Linq;
using GameCore;
using MEC;
using Mirror;
using Security;
using UnityEngine;

// Token: 0x020002A0 RID: 672
public class MTFRespawn : NetworkBehaviour
{
    // Token: 0x06000F79 RID: 3961 RVA: 0x0005DAE4 File Offset: 0x0005BCE4
    private void Start()
    {
        this._mtfCustomRateLimit = new RateLimit(4, 2.8f, null);
        this._ciThemeRateLimit = new RateLimit(1, 3.5f, null);
        this.maxMTFRespawnAmount = ConfigFile.ServerConfig.GetInt("maximum_MTF_respawn_amount", this.maxMTFRespawnAmount);
        this.maxCIRespawnAmount = ConfigFile.ServerConfig.GetInt("maximum_CI_respawn_amount", this.maxCIRespawnAmount);
        this.minMtfTimeToRespawn = ConfigFile.ServerConfig.GetInt("minimum_MTF_time_to_spawn", 200);
        this.maxMtfTimeToRespawn = ConfigFile.ServerConfig.GetInt("maximum_MTF_time_to_spawn", 400);
        this.CI_Percent = (float)ConfigFile.ServerConfig.GetInt("ci_respawn_percent", 35);
        if (NetworkServer.active && base.isServer && base.isLocalPlayer && !TutorialManager.status)
        {
            Timing.RunCoroutine(this._Update(), Segment.FixedUpdate);
        }
    }

    // Token: 0x06000F7A RID: 3962 RVA: 0x00013329 File Offset: 0x00011529
    public void SetDecontCooldown(float f)
    {
        this.decontaminationCooldown = f;
    }

    // Token: 0x06000F7B RID: 3963 RVA: 0x00013332 File Offset: 0x00011532
    private void Update()
    {
        if (this.decontaminationCooldown >= 0f)
        {
            this.decontaminationCooldown -= Time.deltaTime;
        }
    }

    // Token: 0x06000F7C RID: 3964 RVA: 0x00013353 File Offset: 0x00011553
    private IEnumerator<float> _Update()
    {
        this._hostCcm = base.GetComponent<CharacterClassManager>();
        if (!NonFacilityCompatibility.currentSceneSettings.enableRespawning)
        {
            yield break;
        }
        while (this != null)
        {
            if (this.mtf_a == null)
            {
                this.mtf_a = UnityEngine.Object.FindObjectOfType<ChopperAutostart>();
            }
            if (!this._hostCcm.RoundStarted)
            {
                yield return float.NegativeInfinity;
            }
            else
            {
                this.timeToNextRespawn -= 0.02f;
                if (this.respawnCooldown >= 0f)
                {
                    this.respawnCooldown -= 0.02f;
                }
                if (this.timeToNextRespawn < (this.nextWaveIsCI ? 13.5f : 18f) && !this.loaded)
                {
                    this.loaded = true;
                    if (PlayerManager.players.Any((GameObject ply) => ply.GetComponent<CharacterClassManager>().CurClass == RoleType.Spectator))
                    {
                        this.chopperStarted = true;
                        if (this.nextWaveIsCI && !AlphaWarheadController.Host.detonated)
                        {
                            this.SummonVan();
                        }
                        else
                        {
                            this.SummonChopper(true);
                        }
                    }
                }
                if (this.timeToNextRespawn < 0f)
                {
                    float maxDelay = 0f;
                    if (!this.nextWaveIsCI)
                    {
                        if (PlayerManager.players.Any((GameObject item) => item.GetComponent<CharacterClassManager>().CurClass == RoleType.Spectator && !item.GetComponent<ServerRoles>().OverwatchEnabled))
                        {
                            bool warheadInProgress;
                            bool cassieFree;
                            do
                            {
                                warheadInProgress = (AlphaWarheadController.Host != null && AlphaWarheadController.Host.inProgress && !AlphaWarheadController.Host.detonated);
                                cassieFree = (NineTailedFoxAnnouncer.singleton.Free && this.decontaminationCooldown <= 0f);
                                yield return float.NegativeInfinity;
                                maxDelay += 0.02f;
                            }
                            while (maxDelay <= 70f && (!cassieFree || warheadInProgress));
                        }
                    }
                    this.loaded = false;
                    if (base.GetComponent<CharacterClassManager>().RoundStarted)
                    {
                        this.SummonChopper(false);
                    }
                    if (this.chopperStarted)
                    {
                        this.respawnCooldown = 35f;
                        this.RespawnDeadPlayers();
                    }
                    this.nextWaveIsCI = ((float)UnityEngine.Random.Range(0, 100) <= this.CI_Percent);
                    this.timeToNextRespawn = (float)UnityEngine.Random.Range(this.minMtfTimeToRespawn, this.maxMtfTimeToRespawn) * (this.nextWaveIsCI ? (1f / this.CI_Time_Multiplier) : 1f);
                    this.chopperStarted = false;
                }
                yield return float.NegativeInfinity;
            }
        }
        yield break;
    }

    // Token: 0x06000F7D RID: 3965 RVA: 0x0005DBC4 File Offset: 0x0005BDC4
    private void RespawnDeadPlayers()
    {
        int num = 0;
        List<GameObject> list2;
        if (ConfigFile.ServerConfig.GetBool("priority_mtf_respawn", true))
        {
            List<GameObject> list = (from item in PlayerManager.players
                                     where item.GetComponent<CharacterClassManager>().CurClass == RoleType.Spectator && !item.GetComponent<ServerRoles>().OverwatchEnabled
                                     orderby item.GetComponent<CharacterClassManager>().DeathTime
                                     select item).ToList<GameObject>();
            if (this.nextWaveIsCI)
            {
                if (list.Count > this.maxMTFRespawnAmount)
                {
                    list.RemoveRange(this.maxMTFRespawnAmount, list.Count - this.maxMTFRespawnAmount);
                }
                else if (list.Count > this.maxCIRespawnAmount)
                {
                    list.RemoveRange(this.maxCIRespawnAmount, list.Count - this.maxCIRespawnAmount);
                }
            }
            if (ConfigFile.ServerConfig.GetBool("use_crypto_rng", false))
            {
                list.ShuffleListSecure<GameObject>();
            }
            else
            {
                list.ShuffleList<GameObject>();
            }
            list2 = list;
        }
        else
        {
            List<GameObject> list3 = (from item in PlayerManager.players
                                      where item.GetComponent<CharacterClassManager>().CurClass == RoleType.Spectator && !item.GetComponent<ServerRoles>().OverwatchEnabled
                                      select item).ToList<GameObject>();
            if (ConfigFile.ServerConfig.GetBool("use_crypto_rng", false))
            {
                list3.ShuffleListSecure<GameObject>();
            }
            else
            {
                list3.ShuffleList<GameObject>();
            }
            if (this.nextWaveIsCI)
            {
                if (list3.Count > this.maxMTFRespawnAmount)
                {
                    list3.RemoveRange(this.maxMTFRespawnAmount, list3.Count - this.maxMTFRespawnAmount);
                }
                else if (list3.Count > this.maxCIRespawnAmount)
                {
                    list3.RemoveRange(this.maxCIRespawnAmount, list3.Count - this.maxCIRespawnAmount);
                }
            }
            list2 = list3;
        }
        this.playersToNTF.Clear();
        if (this.nextWaveIsCI && AlphaWarheadController.Host.detonated)
        {
            this.nextWaveIsCI = false;
        }
        foreach (GameObject gameObject in list2)
        {
            if (!(gameObject == null))
            {
                num++;
                if (this.nextWaveIsCI)
                {
                    base.GetComponent<CharacterClassManager>().SetPlayersClass(RoleType.ChaosInsurgency, gameObject, false, false);
                    ServerLogs.AddLog(ServerLogs.Modules.ClassChange, gameObject.GetComponent<NicknameSync>().MyNick + " (" + gameObject.GetComponent<CharacterClassManager>().UserId + ") respawned as Chaos Insurgency agent.", ServerLogs.ServerLogType.GameEvent);
                }
                else
                {
                    this.playersToNTF.Add(gameObject);
                }
            }
        }
        if (num > 0)
        {
            ServerLogs.AddLog(ServerLogs.Modules.ClassChange, (this.nextWaveIsCI ? "Chaos Insurgency" : "MTF") + " respawned!", ServerLogs.ServerLogType.GameEvent);
            if (this.nextWaveIsCI)
            {
                base.Invoke("CmdDelayCIAnnounc", 1f);
                if (ConfigFile.ServerConfig.GetBool("announce_chaos_breaches", false))
                {
                    ServerConsole.AddLog("Chaos spawn");
                    PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement("alert . all security personnel .g3 chaos JAM_070_3 insurgency has entered the facility at gate a . terminate all chaos insurgency JAM_070_4 immediately this is top priority for all security personnel", false, true);
                    float num2 = UnityEngine.Random.Range(0f, 2f);
                    ServerConsole.AddLog(num2.ToString());
                    if (num2 >= 1f)
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

    // Token: 0x06000F7E RID: 3966 RVA: 0x0005DF00 File Offset: 0x0005C100
    [ServerCallback]
    public void SummonNTF()
    {
        if (!NetworkServer.active)
        {
            return;
        }
        if (this.playersToNTF.Count <= 0)
        {
            return;
        }
        char mtfLetter;
        int mtfNumber;
        this.SetUnit(this.playersToNTF, out mtfLetter, out mtfNumber);
        NineTailedFoxAnnouncer.singleton.AnnounceNtfEntrance(PlayerManager.players.Count((GameObject item) => item.GetComponent<CharacterClassManager>().IsScpButNotZombie()), mtfNumber, mtfLetter);
        for (int i = 0; i < this.playersToNTF.Count; i++)
        {
            if (i == 0)
            {
                base.GetComponent<CharacterClassManager>().SetPlayersClass(RoleType.NtfCommander, this.playersToNTF[i], false, false);
                ServerLogs.AddLog(ServerLogs.Modules.ClassChange, this.playersToNTF[i].GetComponent<NicknameSync>().MyNick + " (" + this.playersToNTF[i].GetComponent<CharacterClassManager>().UserId + ") respawned as MTF Commander.", ServerLogs.ServerLogType.GameEvent);
            }
            else if (i <= 3)
            {
                base.GetComponent<CharacterClassManager>().SetPlayersClass(RoleType.NtfLieutenant, this.playersToNTF[i], false, false);
                ServerLogs.AddLog(ServerLogs.Modules.ClassChange, this.playersToNTF[i].GetComponent<NicknameSync>().MyNick + " (" + this.playersToNTF[i].GetComponent<CharacterClassManager>().UserId + ") respawned as MTF Lieutenant.", ServerLogs.ServerLogType.GameEvent);
            }
            else
            {
                base.GetComponent<CharacterClassManager>().SetPlayersClass(RoleType.NtfCadet, this.playersToNTF[i], false, false);
                ServerLogs.AddLog(ServerLogs.Modules.ClassChange, this.playersToNTF[i].GetComponent<NicknameSync>().MyNick + " (" + this.playersToNTF[i].GetComponent<CharacterClassManager>().UserId + ") respawned as MTF Guard.", ServerLogs.ServerLogType.GameEvent);
            }
        }
        this.playersToNTF.Clear();
    }

    // Token: 0x06000F7F RID: 3967 RVA: 0x0005E0B8 File Offset: 0x0005C2B8
    [ClientRpc]
    public void RpcPlayCustomAnnouncement(string words, bool makeHold, bool makeNoise)
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteString(words);
        writer.WriteBoolean(makeHold);
        writer.WriteBoolean(makeNoise);
        this.SendRPCInternal(typeof(MTFRespawn), "RpcPlayCustomAnnouncement", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x06000F80 RID: 3968 RVA: 0x0005E10C File Offset: 0x0005C30C
    [ServerCallback]
    private void SetUnit(GameObject[] ply, out char letter, out int number)
    {
        if (!NetworkServer.active)
        {
            letter = Convert.ToChar(0);
            number = Convert.ToChar(0);
            return;
        }
        int networkNtfUnit = base.GetComponent<NineTailedFoxUnits>().NewName(out number, out letter);
        for (int i = 0; i < ply.Length; i++)
        {
            ply[i].GetComponent<CharacterClassManager>().NetworkNtfUnit = networkNtfUnit;
        }
    }

    // Token: 0x06000F81 RID: 3969 RVA: 0x0005E15C File Offset: 0x0005C35C
    [ServerCallback]
    private void SetUnit(List<GameObject> ply, out char letter, out int number)
    {
        if (!NetworkServer.active)
        {
            letter = Convert.ToChar(0);
            number = Convert.ToChar(0);
            return;
        }
        int networkNtfUnit = base.GetComponent<NineTailedFoxUnits>().NewName(out number, out letter);
        foreach (GameObject gameObject in ply)
        {
            gameObject.GetComponent<CharacterClassManager>().NetworkNtfUnit = networkNtfUnit;
        }
    }

    // Token: 0x06000F82 RID: 3970 RVA: 0x00013362 File Offset: 0x00011562
    [ServerCallback]
    private void SummonChopper(bool state)
    {
        if (!NetworkServer.active)
        {
            return;
        }
        if (NonFacilityCompatibility.currentSceneSettings.enableStandardGamplayItems)
        {
            this.mtf_a.SetState(state);
        }
    }

    // Token: 0x06000F83 RID: 3971 RVA: 0x00013387 File Offset: 0x00011587
    [ServerCallback]
    private void SummonVan()
    {
        if (!NetworkServer.active)
        {
            return;
        }
        if (NonFacilityCompatibility.currentSceneSettings.enableStandardGamplayItems)
        {
            this.RpcVan();
        }
    }

    // Token: 0x06000F84 RID: 3972 RVA: 0x0005E1D4 File Offset: 0x0005C3D4
    [ClientRpc]
    private void RpcVan()
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        this.SendRPCInternal(typeof(MTFRespawn), "RpcVan", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x06000F85 RID: 3973 RVA: 0x000133A6 File Offset: 0x000115A6
    private void CmdDelayCIAnnounc()
    {
        this.PlayAnnoncCI();
    }

    // Token: 0x06000F86 RID: 3974 RVA: 0x000133AE File Offset: 0x000115AE
    [ServerCallback]
    private void PlayAnnoncCI()
    {
        if (!NetworkServer.active)
        {
            return;
        }
        this.RpcAnnouncCI();
    }

    // Token: 0x06000F87 RID: 3975 RVA: 0x0005E208 File Offset: 0x0005C408
    [ClientRpc]
    private void RpcAnnouncCI()
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        this.SendRPCInternal(typeof(MTFRespawn), "RpcAnnouncCI", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x06000F89 RID: 3977 RVA: 0x00002FD1 File Offset: 0x000011D1
    private void MirrorProcessed()
    {
    }

    // Token: 0x06000F8A RID: 3978 RVA: 0x000133C1 File Offset: 0x000115C1
    protected static void InvokeRpcRpcPlayCustomAnnouncement(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("RPC RpcPlayCustomAnnouncement called on server.");
            return;
        }
        ((MTFRespawn)obj).CallRpcPlayCustomAnnouncement(reader.ReadString(), reader.ReadBoolean(), reader.ReadBoolean());
    }

    // Token: 0x06000F8B RID: 3979 RVA: 0x000133F6 File Offset: 0x000115F6
    protected static void InvokeRpcRpcVan(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("RPC RpcVan called on server.");
            return;
        }
        ((MTFRespawn)obj).CallRpcVan();
    }

    // Token: 0x06000F8C RID: 3980 RVA: 0x00013419 File Offset: 0x00011619
    protected static void InvokeRpcRpcAnnouncCI(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("RPC RpcAnnouncCI called on server.");
            return;
        }
        ((MTFRespawn)obj).CallRpcAnnouncCI();
    }

    // Token: 0x06000F8D RID: 3981 RVA: 0x0001343C File Offset: 0x0001163C
    public void CallRpcPlayCustomAnnouncement(string words, bool makeHold, bool makeNoise)
    {
        if (!this._mtfCustomRateLimit.CanExecute(true))
        {
            return;
        }
        if (words == null)
        {
            return;
        }
        NineTailedFoxAnnouncer.singleton.AddPhraseToQueue(words, makeNoise, false, makeHold);
    }

    // Token: 0x06000F8E RID: 3982 RVA: 0x0001345F File Offset: 0x0001165F
    public void CallRpcVan()
    {
        GameObject.Find("CIVanArrive").GetComponent<Animator>().SetTrigger("Arrive");
    }

    // Token: 0x06000F8F RID: 3983 RVA: 0x0005E298 File Offset: 0x0005C498
    public void CallRpcAnnouncCI()
    {
        if (!this._ciThemeRateLimit.CanExecute(true))
        {
            return;
        }
        foreach (GameObject gameObject in PlayerManager.players)
        {
            CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
            if (component.isLocalPlayer)
            {
                Team team = component.Classes.SafeGet(component.CurClass).team;
                if (team == Team.CDP || team == Team.CHI || component.GetComponent<ServerRoles>().OverwatchEnabled)
                {
                    UnityEngine.Object.Instantiate<GameObject>(this.ciTheme);
                }
            }
        }
    }

    // Token: 0x06000F90 RID: 3984 RVA: 0x0005E338 File Offset: 0x0005C538
    static MTFRespawn()
    {
        NetworkBehaviour.RegisterRpcDelegate(typeof(MTFRespawn), "RpcPlayCustomAnnouncement", new NetworkBehaviour.CmdDelegate(MTFRespawn.InvokeRpcRpcPlayCustomAnnouncement));
        NetworkBehaviour.RegisterRpcDelegate(typeof(MTFRespawn), "RpcVan", new NetworkBehaviour.CmdDelegate(MTFRespawn.InvokeRpcRpcVan));
        NetworkBehaviour.RegisterRpcDelegate(typeof(MTFRespawn), "RpcAnnouncCI", new NetworkBehaviour.CmdDelegate(MTFRespawn.InvokeRpcRpcAnnouncCI));
    }

    // Token: 0x04001189 RID: 4489
    public GameObject ciTheme;

    // Token: 0x0400118A RID: 4490
    private ChopperAutostart mtf_a;

    // Token: 0x0400118B RID: 4491
    private CharacterClassManager _hostCcm;

    // Token: 0x0400118C RID: 4492
    [Range(30f, 1000f)]
    public int minMtfTimeToRespawn = 200;

    // Token: 0x0400118D RID: 4493
    [Range(40f, 1200f)]
    public int maxMtfTimeToRespawn = 400;

    // Token: 0x0400118E RID: 4494
    public float CI_Time_Multiplier = 2f;

    // Token: 0x0400118F RID: 4495
    public float CI_Percent = 20f;

    // Token: 0x04001190 RID: 4496
    private float decontaminationCooldown;

    // Token: 0x04001191 RID: 4497
    [Space(10f)]
    [Range(2f, 50f)]
    public int maxMTFRespawnAmount = 15;

    // Token: 0x04001192 RID: 4498
    [Range(2f, 50f)]
    public int maxCIRespawnAmount = 15;

    // Token: 0x04001193 RID: 4499
    public float timeToNextRespawn;

    // Token: 0x04001194 RID: 4500
    public bool nextWaveIsCI;

    // Token: 0x04001195 RID: 4501
    public List<GameObject> playersToNTF = new List<GameObject>();

    // Token: 0x04001196 RID: 4502
    private bool loaded;

    // Token: 0x04001197 RID: 4503
    private bool chopperStarted;

    // Token: 0x04001198 RID: 4504
    [HideInInspector]
    public float respawnCooldown;

    // Token: 0x04001199 RID: 4505
    private RateLimit _mtfCustomRateLimit;

    // Token: 0x0400119A RID: 4506
    private RateLimit _ciThemeRateLimit;
}
