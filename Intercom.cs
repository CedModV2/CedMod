// Decompiled with JetBrains decompiler
// Type: Intercom
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Assets._Scripts.Dissonance;
using GameCore;
using MEC;
using Mirror;
using RemoteAdmin;
using Security;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class Intercom : NetworkBehaviour
{
  private string _contentSet = "";
  private bool intercomSupported = true;
  private CharacterClassManager ccm;
  private Transform area;
  public float triggerDistance;
  private float speechTime;
  private float cooldownAfter;
  public float speechRemainingTime;
  public float remainingCooldown;
  public bool speaking;
  public Text txt;
  [SyncVar]
  public GameObject speaker;
  [SyncVar(hook = "UpdateIntercomText")]
  public string intercomText;
  public bool Muted;
  public static bool AdminSpeaking;
  public static bool LastState;
  public static Intercom host;
  public GameObject start_sound;
  public GameObject stop_sound;
  private bool _inUse;
  private bool _isTransmitting;
  private bool _contentDirty;
  private bool _textSetPending;
  private bool _textSetPending2;
  private RateLimit _intercomSoundRateLimit;
  private RateLimit _interactRateLimit;
  private uint ___speakerNetId;

  private string _content
  {
    get
    {
      return this._contentSet;
    }
    set
    {
      this._contentSet = value;
      this._contentDirty = true;
    }
  }

  private IEnumerator<float> _StartTransmitting(GameObject sp)
  {
    Intercom intercom = this;
    if (intercom.intercomSupported)
    {
      intercom.speaking = true;
      if (sp.GetComponent<CharacterClassManager>().IntercomMuted || sp.GetComponent<CharacterClassManager>().Muted || MuteHandler.QueryPersistantMute(sp.GetComponent<CharacterClassManager>().UserId))
      {
        intercom.Muted = true;
        intercom.remainingCooldown = 3f;
        while ((double) intercom.remainingCooldown >= 0.0)
        {
          intercom.remainingCooldown -= Time.deltaTime;
          yield return float.NegativeInfinity;
        }
        intercom.Muted = false;
        intercom.speaking = false;
        intercom._inUse = false;
      }
      else
      {
        intercom.RpcPlaySound(true, sp.GetComponent<QueryProcessor>().PlayerId);
        for (byte i = 0; i < (byte) 100; ++i)
          yield return 0.0f;
        intercom.Networkspeaker = sp;
        DissonanceUserSetup uservc = sp.GetComponentInChildren<DissonanceUserSetup>();
        if ((Object) uservc != (Object) null)
          uservc.IntercomAsHuman = true;
        bool wasAdmin = Intercom.AdminSpeaking;
        if (Intercom.AdminSpeaking)
        {
          while ((Object) intercom.Networkspeaker != (Object) null)
            yield return float.NegativeInfinity;
        }
        else if (sp.GetComponent<ServerRoles>().BypassMode)
        {
          intercom.speechRemainingTime = -77f;
          while ((Object) intercom.Networkspeaker != (Object) null && sp.GetComponent<Intercom>().ServerAllowToSpeak())
            yield return float.NegativeInfinity;
        }
        else
        {
          intercom.speechRemainingTime = intercom.speechTime;
          while ((double) intercom.speechRemainingTime > 0.0 && (Object) intercom.Networkspeaker != (Object) null && sp.GetComponent<Intercom>().ServerAllowToSpeak())
          {
            intercom.speechRemainingTime -= Timing.DeltaTime;
            yield return float.NegativeInfinity;
          }
        }
        if (intercom.isLocalPlayer && (Object) uservc != (Object) null)
          uservc.IntercomAsHuman = false;
        intercom.Networkspeaker = (GameObject) null;
        intercom.RpcPlaySound(false, 0);
        intercom.speaking = false;
        if (!wasAdmin)
        {
          intercom.remainingCooldown = intercom.cooldownAfter;
          while ((double) intercom.remainingCooldown >= 0.0)
          {
            intercom.remainingCooldown -= Time.deltaTime;
            yield return float.NegativeInfinity;
          }
        }
        if (!intercom.speaking)
          intercom._inUse = false;
      }
    }
  }

  private void Start()
  {
    this._intercomSoundRateLimit = new RateLimit(4, 3f, (NetworkConnection) null);
    this._interactRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[0];
    if (NonFacilityCompatibility.currentSceneSettings.voiceChatSupport != NonFacilityCompatibility.SceneDescription.VoiceChatSupportMode.FullySupported)
    {
      this.intercomSupported = false;
    }
    else
    {
      this.ccm = this.GetComponent<CharacterClassManager>();
      this.area = GameObject.Find("IntercomSpeakingZone").transform;
      this.speechTime = (float) ConfigFile.ServerConfig.GetInt("intercom_max_speech_time", 20);
      this.cooldownAfter = (float) ConfigFile.ServerConfig.GetInt("intercom_cooldown", 180);
      Timing.RunCoroutine(this._FindHost());
      if (!this.isLocalPlayer || !this.isServer)
        return;
      this.InvokeRepeating("RefreshText", 5f, 7f);
    }
  }

  private void RefreshText()
  {
    this.NetworkintercomText = this._content;
  }

  private IEnumerator<float> _FindHost()
  {
    while ((Object) Intercom.host == (Object) null)
    {
      GameObject gameObject = GameObject.Find("Host");
      if ((Object) gameObject != (Object) null)
        Intercom.host = gameObject.GetComponent<Intercom>();
      yield return float.NegativeInfinity;
    }
  }

  [ClientRpc]
  public void RpcPlaySound(bool start, int transmitterID)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteBoolean(start);
    writer.WritePackedInt32(transmitterID);
    this.SendRPCInternal(typeof (Intercom), nameof (RpcPlaySound), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void Update()
  {
    if (!this.intercomSupported || !this.isLocalPlayer || !this.isServer)
      return;
    this.UpdateText();
  }

  private void UpdateText()
  {
    this._content = !this.Muted ? (!Intercom.AdminSpeaking ? ((double) this.remainingCooldown <= 0.0 ? (!((Object) this.Networkspeaker != (Object) null) ? "READY" : ((double) this.speechRemainingTime != -77.0 ? "TRANSMITTING...\nTIME LEFT - " + (object) Mathf.CeilToInt(this.speechRemainingTime) : "TRANSMITTING...\nBYPASS MODE")) : "RESTARTING\n" + (object) Mathf.CeilToInt(this.remainingCooldown)) : "ADMIN IS USING\nTHE INTERCOM NOW") : "YOU ARE MUTED BY ADMIN";
    if (this._contentDirty)
    {
      this.NetworkintercomText = this._content;
      this._contentDirty = false;
    }
    if (Intercom.AdminSpeaking == Intercom.LastState)
      return;
    Intercom.LastState = Intercom.AdminSpeaking;
    this.RpcUpdateAdminStatus(Intercom.AdminSpeaking);
  }

  public void UpdateIntercomText(string t)
  {
  }

  [ClientRpc]
  private void RpcUpdateAdminStatus(bool status)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteBoolean(status);
    this.SendRPCInternal(typeof (Intercom), nameof (RpcUpdateAdminStatus), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  public void RequestTransmission(GameObject spk)
  {
    if ((Object) spk == (Object) null)
    {
      this.Networkspeaker = (GameObject) null;
    }
    else
    {
      if (((double) this.remainingCooldown > 0.0 || this._inUse) && (!spk.GetComponent<ServerRoles>().BypassMode || this.speaking))
        return;
      this.speaking = true;
      this.remainingCooldown = -1f;
      this._inUse = true;
      Timing.RunCoroutine(this._StartTransmitting(spk), Segment.FixedUpdate);
    }
  }

  private bool ServerAllowToSpeak()
  {
    return (double) Vector3.Distance(this.transform.position, this.area.position) < (double) this.triggerDistance && this.ccm.Classes.SafeGet(this.ccm.CurClass).team != Team.SCP && this.ccm.Classes.SafeGet(this.ccm.CurClass).team != Team.RIP;
  }

  [Command]
  private void CmdSetTransmit(bool player)
  {
    if (this.isServer)
    {
      this.CallCmdSetTransmit(player);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBoolean(player);
      this.SendCommandInternal(typeof (Intercom), nameof (CmdSetTransmit), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  private void MirrorProcessed()
  {
  }

  public GameObject Networkspeaker
  {
    get
    {
      return this.GetSyncVarGameObject(this.___speakerNetId, ref this.speaker);
    }
    [param: In] set
    {
      this.SetSyncVarGameObject(value, ref this.speaker, 1UL, ref this.___speakerNetId);
    }
  }

  public string NetworkintercomText
  {
    get
    {
      return this.intercomText;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(2UL))
      {
        this.setSyncVarHookGuard(2UL, true);
        this.UpdateIntercomText(value);
        this.setSyncVarHookGuard(2UL, false);
      }
      this.SetSyncVar<string>(value, ref this.intercomText, 2UL);
    }
  }

  protected static void InvokeCmdCmdSetTransmit(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSetTransmit called on client.");
    else
      ((Intercom) obj).CallCmdSetTransmit(reader.ReadBoolean());
  }

  public void CallCmdSetTransmit(bool player)
  {
    if (!this._interactRateLimit.CanExecute(true) || Intercom.AdminSpeaking)
      return;
    if (player)
    {
      if (!this.ServerAllowToSpeak())
        return;
      Intercom.host.RequestTransmission(this.gameObject);
    }
    else
    {
      if (!((Object) Intercom.host.Networkspeaker == (Object) this.gameObject))
        return;
      Intercom.host.RequestTransmission((GameObject) null);
    }
  }

  protected static void InvokeRpcRpcPlaySound(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcPlaySound called on server.");
    else
      ((Intercom) obj).CallRpcPlaySound(reader.ReadBoolean(), reader.ReadPackedInt32());
  }

  protected static void InvokeRpcRpcUpdateAdminStatus(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcUpdateAdminStatus called on server.");
    else
      ((Intercom) obj).CallRpcUpdateAdminStatus(reader.ReadBoolean());
  }

  public void CallRpcPlaySound(bool start, int transmitterID)
  {
    if (!this._intercomSoundRateLimit.CanExecute(true))
      return;
    Object.Destroy((Object) Object.Instantiate<GameObject>(start ? this.start_sound : this.stop_sound), 10f);
  }

  public void CallRpcUpdateAdminStatus(bool status)
  {
  }

  static Intercom()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (Intercom), "CmdSetTransmit", new NetworkBehaviour.CmdDelegate(Intercom.InvokeCmdCmdSetTransmit));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Intercom), "RpcPlaySound", new NetworkBehaviour.CmdDelegate(Intercom.InvokeRpcRpcPlaySound));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Intercom), "RpcUpdateAdminStatus", new NetworkBehaviour.CmdDelegate(Intercom.InvokeRpcRpcUpdateAdminStatus));
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteGameObject(this.Networkspeaker);
      writer.WriteString(this.intercomText);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteGameObject(this.Networkspeaker);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WriteString(this.intercomText);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.___speakerNetId = reader.ReadPackedUInt32();
      string t = reader.ReadString();
      this.UpdateIntercomText(t);
      this.NetworkintercomText = t;
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
        this.___speakerNetId = reader.ReadPackedUInt32();
      if ((num & 2L) == 0L)
        return;
      string t = reader.ReadString();
      this.UpdateIntercomText(t);
      this.NetworkintercomText = t;
    }
  }
}
