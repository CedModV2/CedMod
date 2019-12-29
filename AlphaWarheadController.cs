// Decompiled with JetBrains decompiler
// Type: AlphaWarheadController
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using MEC;
using Mirror;
using Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class AlphaWarheadController : NetworkBehaviour
{
  public int cooldown = 30;
  [SyncVar]
  public int sync_resumeScenario = -1;
  public AlphaWarheadController.DetonationScenario[] scenarios_start;
  public AlphaWarheadController.DetonationScenario[] scenarios_resume;
  public AudioClip sound_canceled;
  internal BlastDoor[] blastDoors;
  private CharacterClassManager _ccm;
  public bool doorsClosed;
  public bool doorsOpen;
  public bool detonated;
  public int warheadKills;
  private static int _startScenario;
  private static int _resumeScenario;
  private float _shake;
  public static AudioSource alarmSource;
  public static AlphaWarheadController Host;
  [SyncVar]
  public float timeToDetonation;
  [SyncVar]
  public int sync_startScenario;
  [SyncVar]
  public bool inProgress;
  private string file;
  private RateLimit _detonationRateLimit;

  private void Start()
  {
    this._detonationRateLimit = new RateLimit(4, 6.5f, (NetworkConnection) null);
    this._ccm = this.GetComponent<CharacterClassManager>();
    if (!this.isLocalPlayer || TutorialManager.status)
      return;
    Timing.RunCoroutine(this._ReadCustomTranslations(), Segment.FixedUpdate);
    AlphaWarheadController.alarmSource = GameObject.Find("GameManager").GetComponent<AudioSource>();
    this.blastDoors = UnityEngine.Object.FindObjectsOfType<BlastDoor>();
    if (!this.isServer)
      return;
    int num = Mathf.RoundToInt((float) Mathf.Clamp(ConfigFile.ServerConfig.GetInt("warhead_tminus_start_duration", 90), 80, 120) / 10f) * 10;
    this.Networksync_startScenario = 3;
    for (int index = 0; index < this.scenarios_start.Length; ++index)
    {
      if (this.scenarios_start[index].tMinusTime == num)
        this.Networksync_startScenario = index;
    }
  }

  public void StartDetonation()
  {
    if (Recontainer079.isLocked)
      return;
    this.doorsOpen = false;
    ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Countdown started.", ServerLogs.ServerLogType.GameEvent);
    if ((AlphaWarheadController._resumeScenario != -1 || (double) this.scenarios_start[AlphaWarheadController._startScenario].SumTime() != (double) this.timeToDetonation) && (AlphaWarheadController._resumeScenario == -1 || (double) this.scenarios_resume[AlphaWarheadController._resumeScenario].SumTime() != (double) this.timeToDetonation))
      return;
    this.NetworkinProgress = true;
  }

  public void InstantPrepare()
  {
    this.NetworktimeToDetonation = AlphaWarheadController._resumeScenario == -1 ? this.scenarios_start[AlphaWarheadController._startScenario].SumTime() : this.scenarios_resume[AlphaWarheadController._resumeScenario].SumTime();
  }

  private IEnumerator<float> _ReadCustomTranslations()
  {
    AlphaWarheadController.DetonationScenario[] detonationScenarioArray = this.scenarios_resume;
    int index;
    AlphaWarheadController.DetonationScenario asource;
    string path;
    UnityWebRequest www;
    for (index = 0; index < detonationScenarioArray.Length; ++index)
    {
      asource = detonationScenarioArray[index];
      path = TranslationReader.path + "/Custom Audio/" + asource.clip.name + ".ogg";
      if (!File.Exists(path))
      {
        yield break;
      }
      else
      {
        this.file = Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor ? "file:///" : "file://";
        www = UnityWebRequestMultimedia.GetAudioClip(this.file + path, Misc.GetAudioType(this.file + path));
        try
        {
          yield return Timing.WaitUntilDone((AsyncOperation) www.SendWebRequest());
          asource.clip = DownloadHandlerAudioClip.GetContent(www);
        }
        finally
        {
          www?.Dispose();
        }
        www = (UnityWebRequest) null;
        asource.clip.name = Path.GetFileName(path);
        path = (string) null;
        asource = (AlphaWarheadController.DetonationScenario) null;
      }
    }
    detonationScenarioArray = (AlphaWarheadController.DetonationScenario[]) null;
    detonationScenarioArray = this.scenarios_start;
    for (index = 0; index < detonationScenarioArray.Length; ++index)
    {
      asource = detonationScenarioArray[index];
      path = TranslationReader.path + "/Custom Audio/" + asource.clip.name + ".ogg";
      if (!File.Exists(path))
      {
        yield break;
      }
      else
      {
        this.file = Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor ? "file:///" : "file://";
        www = UnityWebRequestMultimedia.GetAudioClip(this.file + path, Misc.GetAudioType(path));
        try
        {
          yield return Timing.WaitUntilDone((AsyncOperation) www.SendWebRequest());
          asource.clip = DownloadHandlerAudioClip.GetContent(www);
        }
        finally
        {
          www?.Dispose();
        }
        www = (UnityWebRequest) null;
        asource.clip.name = Path.GetFileName(path);
        path = (string) null;
        asource = (AlphaWarheadController.DetonationScenario) null;
      }
    }
    detonationScenarioArray = (AlphaWarheadController.DetonationScenario[]) null;
  }

  public void CancelDetonation()
  {
    this.CancelDetonation((GameObject) null);
  }

  public void CancelDetonation(GameObject disabler)
  {
    ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Detonation cancelled.", ServerLogs.ServerLogType.GameEvent);
    if (!this.inProgress || (double) this.timeToDetonation <= 10.0)
      return;
    if ((double) this.timeToDetonation <= 15.0 && (UnityEngine.Object) disabler != (UnityEngine.Object) null)
      this.GetComponent<PlayerStats>().TargetAchieve(disabler.GetComponent<NetworkIdentity>().connectionToClient, "thatwasclose");
    for (int index = 0; index < this.scenarios_resume.Length; ++index)
    {
      if ((double) this.scenarios_resume[index].SumTime() > (double) this.timeToDetonation && (double) this.scenarios_resume[index].SumTime() < (double) this.scenarios_start[AlphaWarheadController._startScenario].SumTime())
        this.Networksync_resumeScenario = index;
    }
    this.NetworktimeToDetonation = (AlphaWarheadController._resumeScenario < 0 ? this.scenarios_start[AlphaWarheadController._startScenario].SumTime() : this.scenarios_resume[AlphaWarheadController._resumeScenario].SumTime()) + (float) this.cooldown;
    this.NetworkinProgress = false;
    foreach (Door door in UnityEngine.Object.FindObjectsOfType<Door>())
    {
      door.warheadlock = false;
      door.UpdateLock();
    }
  }

  internal void Detonate()
  {
    ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Warhead detonated.", ServerLogs.ServerLogType.GameEvent);
    this.detonated = true;
    this.RpcShake(true);
    GameObject[] gameObjectsWithTag = GameObject.FindGameObjectsWithTag("LiftTarget");
    foreach (GameObject player in PlayerManager.players)
    {
      foreach (GameObject gameObject in gameObjectsWithTag)
      {
        if (player.GetComponent<PlayerStats>().Explode((double) Vector3.Distance(gameObject.transform.position, player.transform.position) < 3.5))
          ++this.warheadKills;
      }
    }
    foreach (Door door in UnityEngine.Object.FindObjectsOfType<Door>())
    {
      if (door.blockAfterDetonation)
        door.OpenWarhead(true, true);
    }
  }

  [ClientRpc]
  private void RpcShake(bool achieve)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteBoolean(achieve);
    this.SendRPCInternal(typeof (AlphaWarheadController), nameof (RpcShake), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void FixedUpdate()
  {
    if (this._ccm.IsHost)
    {
      AlphaWarheadController.Host = this;
      AlphaWarheadController._startScenario = this.sync_startScenario;
      AlphaWarheadController._resumeScenario = this.sync_resumeScenario;
    }
    if (!this.isLocalPlayer)
      return;
    this.UpdateSourceState();
    if (!this.isServer)
      return;
    this.ServerCountdown();
  }

  private void UpdateSourceState()
  {
    if (TutorialManager.status || (UnityEngine.Object) AlphaWarheadController.Host == (UnityEngine.Object) null)
      return;
    if (AlphaWarheadController.Host.inProgress)
    {
      if ((double) Math.Abs(AlphaWarheadController.Host.timeToDetonation) > 1.0 / 1000.0)
      {
        if (!AlphaWarheadController.alarmSource.isPlaying)
        {
          AlphaWarheadController.alarmSource.volume = 1f;
          AlphaWarheadController.alarmSource.clip = AlphaWarheadController._resumeScenario < 0 ? this.scenarios_start[AlphaWarheadController._startScenario].clip : this.scenarios_resume[AlphaWarheadController._resumeScenario].clip;
          AlphaWarheadController.alarmSource.Play();
          return;
        }
        float max = this.RealDetonationTime();
        float num = max - AlphaWarheadController.Host.timeToDetonation;
        if ((double) Mathf.Abs(AlphaWarheadController.alarmSource.time - num) > 0.5)
          AlphaWarheadController.alarmSource.time = Mathf.Clamp(num, 0.0f, max);
      }
      if ((double) AlphaWarheadController.Host.timeToDetonation >= 5.0 || (double) AlphaWarheadController.Host.timeToDetonation == 0.0)
        return;
      this._shake += Time.fixedDeltaTime / 20f;
      this._shake = Mathf.Clamp(this._shake, 0.0f, 0.5f);
      if ((double) Vector3.Distance(this.transform.position, AlphaWarheadOutsitePanel.nukeside.transform.position) >= 100.0)
        return;
      ExplosionCameraShake.singleton.Shake(this._shake);
    }
    else
    {
      if (!AlphaWarheadController.alarmSource.isPlaying || !((UnityEngine.Object) AlphaWarheadController.alarmSource.clip != (UnityEngine.Object) null))
        return;
      AlphaWarheadController.alarmSource.Stop();
      AlphaWarheadController.alarmSource.clip = (AudioClip) null;
      AlphaWarheadController.alarmSource.PlayOneShot(this.sound_canceled);
    }
  }

  public float RealDetonationTime()
  {
    return AlphaWarheadController._resumeScenario < 0 ? this.scenarios_start[AlphaWarheadController._startScenario].SumTime() : this.scenarios_resume[AlphaWarheadController._resumeScenario].SumTime();
  }

  [ServerCallback]
  private void ServerCountdown()
  {
    if (!NetworkServer.active)
      return;
    float num1 = this.RealDetonationTime();
    float num2 = this.timeToDetonation;
    if ((double) this.timeToDetonation > 0.0)
    {
      if (this.inProgress)
      {
        float num3 = num2 - Time.fixedDeltaTime;
        if ((double) num3 < 2.0 && !this.doorsClosed)
        {
          this.doorsClosed = true;
          foreach (BlastDoor blastDoor in this.blastDoors)
            blastDoor.SetClosed(true);
        }
        if (ConfigFile.ServerConfig.GetBool("open_doors_on_countdown", true) && !this.doorsOpen && (double) num3 < (double) num1 - (AlphaWarheadController._resumeScenario >= 0 ? (double) this.scenarios_resume[AlphaWarheadController._resumeScenario].additionalTime : (double) this.scenarios_start[AlphaWarheadController._startScenario].additionalTime))
        {
          this.doorsOpen = true;
          bool flag1 = ConfigFile.ServerConfig.GetBool("lock_gates_on_countdown", true);
          bool flag2 = ConfigFile.ServerConfig.GetBool("isolate_zones_on_countdown", false);
          foreach (Door door in UnityEngine.Object.FindObjectsOfType<Door>())
          {
            if (flag2 && door.DoorName.Contains("CHECKPOINT"))
            {
              door.warheadlock = true;
              door.UpdateLock();
              door.SetStateWithSound(false);
            }
            else
              door.OpenWarhead(false, flag1 || !door.DoorName.Contains("GATE"));
          }
        }
        if ((double) num3 <= 0.0)
          this.Detonate();
        num2 = Mathf.Clamp(num3, 0.0f, num1);
      }
      else
      {
        if ((double) num2 > (double) num1)
          num2 -= Time.fixedDeltaTime;
        num2 = Mathf.Clamp(num2, num1, (float) this.cooldown + num1);
      }
    }
    this.NetworktimeToDetonation = num2;
  }

  private void MirrorProcessed()
  {
  }

  public float NetworktimeToDetonation
  {
    get
    {
      return this.timeToDetonation;
    }
    [param: In] set
    {
      this.SetSyncVar<float>(value, ref this.timeToDetonation, 1UL);
    }
  }

  public int Networksync_startScenario
  {
    get
    {
      return this.sync_startScenario;
    }
    [param: In] set
    {
      this.SetSyncVar<int>(value, ref this.sync_startScenario, 2UL);
    }
  }

  public int Networksync_resumeScenario
  {
    get
    {
      return this.sync_resumeScenario;
    }
    [param: In] set
    {
      this.SetSyncVar<int>(value, ref this.sync_resumeScenario, 4UL);
    }
  }

  public bool NetworkinProgress
  {
    get
    {
      return this.inProgress;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.inProgress, 8UL);
    }
  }

  protected static void InvokeRpcRpcShake(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcShake called on server.");
    else
      ((AlphaWarheadController) obj).CallRpcShake(reader.ReadBoolean());
  }

  public void CallRpcShake(bool achieve)
  {
  }

  static AlphaWarheadController()
  {
    NetworkBehaviour.RegisterRpcDelegate(typeof (AlphaWarheadController), "RpcShake", new NetworkBehaviour.CmdDelegate(AlphaWarheadController.InvokeRpcRpcShake));
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteSingle(this.timeToDetonation);
      writer.WritePackedInt32(this.sync_startScenario);
      writer.WritePackedInt32(this.sync_resumeScenario);
      writer.WriteBoolean(this.inProgress);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteSingle(this.timeToDetonation);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WritePackedInt32(this.sync_startScenario);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 4L) != 0L)
    {
      writer.WritePackedInt32(this.sync_resumeScenario);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 8L) != 0L)
    {
      writer.WriteBoolean(this.inProgress);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworktimeToDetonation = reader.ReadSingle();
      this.Networksync_startScenario = reader.ReadPackedInt32();
      this.Networksync_resumeScenario = reader.ReadPackedInt32();
      this.NetworkinProgress = reader.ReadBoolean();
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
        this.NetworktimeToDetonation = reader.ReadSingle();
      if ((num & 2L) != 0L)
        this.Networksync_startScenario = reader.ReadPackedInt32();
      if ((num & 4L) != 0L)
        this.Networksync_resumeScenario = reader.ReadPackedInt32();
      if ((num & 8L) == 0L)
        return;
      this.NetworkinProgress = reader.ReadBoolean();
    }
  }

  [Serializable]
  public class DetonationScenario
  {
    public AudioClip clip;
    public int tMinusTime;
    public float additionalTime;

    public float SumTime()
    {
      return (float) this.tMinusTime + this.additionalTime;
    }
  }
}
