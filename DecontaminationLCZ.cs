// Decompiled with JetBrains decompiler
// Type: DecontaminationLCZ
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using CedMod;
using GameCore;
using MEC;
using Mirror;
using RemoteAdmin;
using Security;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DecontaminationLCZ : NetworkBehaviour
{
  public List<DecontaminationLCZ.Announcement> announcements = new List<DecontaminationLCZ.Announcement>();
  public float time;
  private bool smDisableDecontamination;
  private MTFRespawn _mtfrespawn;
  private AlphaWarheadController _alphaController;
  private PlayerStats _ps;
  private CharacterClassManager _ccm;
  private RateLimit _decontLczRateLimit;
  private int _curAnm;

  public int GetCurAnnouncement()
  {
    return this._curAnm;
  }

  private void Start()
  {
    this._decontLczRateLimit = new RateLimit(1, 10f, (NetworkConnection) null);
    this._ccm = this.GetComponent<CharacterClassManager>();
    this._ps = this.GetComponent<PlayerStats>();
    this._mtfrespawn = this.GetComponent<MTFRespawn>();
    this._alphaController = this.GetComponent<AlphaWarheadController>();
    this.smDisableDecontamination = ConfigFile.ServerConfig.GetBool("disable_decontamination", false);
  }

  private void Update()
  {
    if (this.smDisableDecontamination || !this.isLocalPlayer || (!this._ccm.IsHost || ConfigFile.ServerConfig.GetBool("cm_deconrework", false)))
      return;
    this.DoServersideStuff();
  }

  private IEnumerator<float> _KillPlayersInLCZ()
  {
    DecontaminationLCZ decontaminationLcz = this;
    foreach (Lift lift in UnityEngine.Object.FindObjectsOfType<Lift>())
      lift.Lock();
    foreach (Door door in UnityEngine.Object.FindObjectsOfType<Door>())
      door.CloseDecontamination();
    while ((UnityEngine.Object) decontaminationLcz != (UnityEngine.Object) null)
    {
      for (int k = 0; k < 15; ++k)
        yield return 0.0f;
      foreach (GameObject player in PlayerManager.players)
      {
        if (!((UnityEngine.Object) player == (UnityEngine.Object) null))
        {
          float y = player.transform.position.y;
          if ((double) y < 100.0 && (double) y > -100.0)
          {
            PlayerStats component = player.GetComponent<PlayerStats>();
            decontaminationLcz._ps.HurtPlayer(new PlayerStats.HitInfo((float) decontaminationLcz.GetComponent<CharacterClassManager>().Classes.SafeGet(component.ccm.CurClass).maxHP / 50f, "DECONT", DamageTypes.Decont, 0), player);
          }
        }
      }
    }
  }

  [ServerCallback]
  private void DoServersideStuff()
  {
    if (!NetworkServer.active || this._curAnm >= this.announcements.Count || (this._alphaController.inProgress || !this._ccm.RoundStarted) || (double) this._mtfrespawn.respawnCooldown > 0.0)
      return;
    this.time += Time.deltaTime;
    if ((double) this.time / 60.0 <= (double) this.announcements[this._curAnm].startTime)
      return;
    this.RpcPlayAnnouncement(this._curAnm, this.GetOption("global", this._curAnm));
    AlphaWarheadController alphaController = this._alphaController;
    alphaController.NetworktimeToDetonation = alphaController.timeToDetonation + (this.announcements[this._curAnm].clip.length + 10f);
    PlayerManager.localPlayer.GetComponent<MTFRespawn>().SetDecontCooldown(this.announcements[this._curAnm].clip.length + 10f);
    if (this.GetOption("checkpoints", this._curAnm))
      this.Invoke("CallOpenDoors", 10f);
    ++this._curAnm;
  }

  private bool GetOption(string optionName, int curAnm)
  {
    return this.announcements[curAnm].options.Split(',').Contains<string>(optionName);
  }

  private void CallOpenDoors()
  {
    DecontaminationSpeaker.OpenDoors();
  }

  [ClientRpc]
  private void RpcPlayAnnouncement(int id, bool global)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WritePackedInt32(id);
    writer.WriteBoolean(global);
    this.SendRPCInternal(typeof (DecontaminationLCZ), nameof (RpcPlayAnnouncement), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeRpcRpcPlayAnnouncement(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcPlayAnnouncement called on server.");
    else
      ((DecontaminationLCZ) obj).CallRpcPlayAnnouncement(reader.ReadPackedInt32(), reader.ReadBoolean());
  }

  public void CallRpcPlayAnnouncement(int id, bool global)
  {
    if (!this.GetOption("decontstart", id))
      return;
    if (NetworkServer.active)
      Timing.RunCoroutine(this._KillPlayersInLCZ(), Segment.FixedUpdate);
    DecontaminationGas.TurnOn();
  }

  static DecontaminationLCZ()
  {
    NetworkBehaviour.RegisterRpcDelegate(typeof (DecontaminationLCZ), "RpcPlayAnnouncement", new NetworkBehaviour.CmdDelegate(DecontaminationLCZ.InvokeRpcRpcPlayAnnouncement));
    NetworkBehaviour.RegisterRpcDelegate(typeof (DecontaminationLCZ), "RpcCustomOverchargeForOurBeautifulModCreators", new NetworkBehaviour.CmdDelegate(DecontaminationLCZ.InvokeRpcRpcCustomOverchargeForOurBeautifulModCreators));
  }

  public void CallDecon()
  {
    Timing.RunCoroutine(this.StartDeconSystem(), "LCZDecon");
  }

  public void CallDeconStop()
  {
    Timing.KillCoroutines("LCZDecon");
  }

  private IEnumerator<float> StartDeconSystem()
  {
    DecontaminationLCZ decontaminationLcz = this;
    bool OpenAllLightDoors30Seconds = true;
    int Count = 15;
    bool EnableSubtitles = true;
    Initializer.logger.Info("Decon", "started decon system");
    yield return Timing.WaitForSeconds(60f);
    while (Count >= 5)
    {
      PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement("light containment zone decontamination in  t  minus " + (object) Count + "  minutes", false, false);
      if (EnableSubtitles)
      {
        foreach (GameObject player in PlayerManager.players)
        {
          float y = player.transform.position.y;
          if ((double) y < 100.0 && (double) y > -100.0)
            QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(player.GetComponent<NetworkIdentity>().connectionToClient, "light containment zone decontamination in  t  minus " + (object) Count + " minutes", 10U, false);
        }
      }
      Count -= 5;
      if (Count >= 5)
        yield return Timing.WaitForSeconds(300f);
      else
        break;
    }
    yield return Timing.WaitForSeconds(240f);
    PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement(" light containment zone decontamination in  t  minus 1 minute evacuate immediately ", false, false);
    if (EnableSubtitles)
    {
      foreach (GameObject player in PlayerManager.players)
      {
        float y = player.transform.position.y;
        if ((double) y < 100.0 && (double) y > -100.0)
          QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(player.GetComponent<NetworkIdentity>().connectionToClient, "light containment zone decontamination in  t  minus 1 minutes evacuate immediately", 10U, false);
      }
    }
    yield return Timing.WaitForSeconds(30f);
    PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement(" light containment zone decontamination in  t  minus 30 seconds evacuate immediately", false, false);
    if (EnableSubtitles)
    {
      foreach (GameObject player in PlayerManager.players)
      {
        float y = player.transform.position.y;
        if ((double) y < 100.0 && (double) y > -100.0)
          QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(player.GetComponent<NetworkIdentity>().connectionToClient, "Alert light containment zone decontamination in t minus 30 seconds evacuate immediately", 10U, false);
        yield return 1f;
      }
    }
    foreach (Door door in UnityEngine.Object.FindObjectsOfType<Door>())
    {
      if (door.DoorName == "CHECKPOINT_LCZ_A")
        door.OpenWarhead(true, true);
      else if (door.DoorName == "CHECKPOINT_LCZ_B")
        door.OpenWarhead(true, true);
    }
    if (OpenAllLightDoors30Seconds)
    {
      foreach (Door door in UnityEngine.Object.FindObjectsOfType<Door>())
      {
        float y = door.transform.position.y;
        if ((double) y < 100.0 && (double) y > -100.0 && !door.isOpen)
          door.ChangeState(true);
      }
    }
    yield return Timing.WaitForSeconds(27f);
    PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement(" . . . . . 10 . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . decontamination has begun", false, false);
    yield return Timing.WaitForSeconds(1.5f);
    int num1 = 11;
    int minusnumber = 1;
    int finalnumber = num1;
    foreach (GameObject player in PlayerManager.players)
    {
      GameObject player4 = player;
      float y = player4.transform.position.y;
      if ((double) y < 100.0 && (double) y > -100.0)
      {
        while (finalnumber != 0)
        {
          if (finalnumber != 0)
          {
            if (EnableSubtitles)
              QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(player4.GetComponent<NetworkIdentity>().connectionToClient, finalnumber.ToString(), 1U, false);
            finalnumber -= minusnumber;
            yield return Timing.WaitForSeconds(1.1f);
          }
          else if (finalnumber == 0)
            break;
        }
      }
      player4 = (GameObject) null;
    }
    int Remaining = 10;
    Timing.RunCoroutine(decontaminationLcz._KillPlayersInLCZ(), Segment.FixedUpdate);
    foreach (GameObject player in PlayerManager.players)
    {
      if (!((UnityEngine.Object) player == (UnityEngine.Object) null))
      {
        float y = player.transform.position.y;
        if ((double) y < 100.0 && (double) y > -100.0)
        {
          PlayerStats component = player.GetComponent<PlayerStats>();
          decontaminationLcz._ps.HurtPlayer(new PlayerStats.HitInfo((float) decontaminationLcz.GetComponent<CharacterClassManager>().Classes.SafeGet(component.ccm.CurClass).maxHP / 50f, "DECONT", DamageTypes.Decont, 0), player);
          decontaminationLcz._ps.HurtPlayer(new PlayerStats.HitInfo((float) decontaminationLcz.GetComponent<CharacterClassManager>().Classes.SafeGet(component.ccm.CurClass).maxHP / 50f, "DECONT", DamageTypes.Decont, 0), player);
          decontaminationLcz._ps.HurtPlayer(new PlayerStats.HitInfo((float) decontaminationLcz.GetComponent<CharacterClassManager>().Classes.SafeGet(component.ccm.CurClass).maxHP / 50f, "DECONT", DamageTypes.Decont, 0), player);
          decontaminationLcz._ps.HurtPlayer(new PlayerStats.HitInfo((float) decontaminationLcz.GetComponent<CharacterClassManager>().Classes.SafeGet(component.ccm.CurClass).maxHP / 5000000f, "DECONT", DamageTypes.Decont, 0), player);
        }
      }
    }
    while (Remaining != 0)
    {
      int num2 = 0;
      if (Remaining != 0)
        decontaminationLcz.RpcCustomOverchargeForOurBeautifulModCreators(10f, true);
      foreach (GameObject player in PlayerManager.players)
        ++num2;
      if (num2 != 10)
        --Remaining;
      yield return Timing.WaitForSeconds(10.5f);
    }
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

  [ClientRpc]
  public void RpcCustomOverchargeForOurBeautifulModCreators(float forceDuration, bool onlyLight)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteSingle(forceDuration);
    writer.WriteBoolean(onlyLight);
    this.SendRPCInternal(typeof (Generator079), nameof (RpcCustomOverchargeForOurBeautifulModCreators), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  public void CallRpcCustomOverchargeForOurBeautifulModCreators(float forceDuration, bool onlyLight)
  {
    foreach (FlickerableLight flickerableLight in UnityEngine.Object.FindObjectsOfType<FlickerableLight>())
    {
      Scp079Interactable component = flickerableLight.GetComponent<Scp079Interactable>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null || !onlyLight || component.currentZonesAndRooms[0].currentZone == "LightRooms")
        flickerableLight.EnableFlickering(forceDuration);
    }
  }

  [Serializable]
  public class Announcement
  {
    public AudioClip clip;
    public float startTime;
    public string options;
  }
}
