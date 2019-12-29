// Decompiled with JetBrains decompiler
// Type: Radio
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Assets._Scripts.Dissonance;
using Dissonance;
using Dissonance.Audio.Playback;
using Dissonance.Integrations.MirrorIgnorance;
using Mirror;
using Security;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Audio;

public class Radio : NetworkBehaviour
{
  private int myRadio = -1;
  private int _lastDebugCode = -1;
  public static Radio localRadio;
  public AudioMixerGroup g_voice;
  public AudioMixerGroup g_radio;
  public AudioMixerGroup g_icom;
  public AudioMixerGroup g_079;
  public AudioClip[] beepStart;
  public AudioClip[] beepStop;
  public AudioSource beepSource;
  [Space]
  public AudioSource playerSource;
  public Radio.RadioPreset[] presets;
  [SyncVar]
  public int curPreset;
  [SyncVar]
  public bool isTransmitting;
  [SyncVar]
  public bool isVoiceChatting;
  private int lastPreset;
  private SpeakerIcon icon;
  public static bool roundStarted;
  public static bool roundEnded;
  private AudioSource noiseSource;
  public float icomNoise;
  public float noiseMultiplier;
  private static float noiseIntensity;
  private Inventory inv;
  private CharacterClassManager ccm;
  private CharacterClassManager localCcm;
  private MirrorIgnorancePlayer mirrorIgnorancePlayer;
  private DissonanceUserSetup _dissonanceSetup;
  private static DissonanceComms comms;
  private VoicePlayerState state;
  private VoicePlayback unityPlayback;
  private RateLimit _interactRateLimit;

  public bool IsOn
  {
    get
    {
      return this.myRadio != -1;
    }
  }

  public void ResetPreset()
  {
    this.NetworkcurPreset = 1;
    this.CmdUpdatePreset(1);
    if (this.myRadio >= 0)
      return;
    for (ushort index = 0; (int) index < this.inv.items.Count; ++index)
    {
      if (this.inv.items[(int) index].id == ItemType.Radio)
        this.myRadio = (int) index;
    }
  }

  public override void OnStartClient()
  {
    base.OnStartClient();
    if (!((UnityEngine.Object) Radio.comms == (UnityEngine.Object) null))
      return;
    Radio.comms = GameObject.FindGameObjectWithTag("Dissonance").GetComponent<DissonanceComms>();
  }

  private void Start()
  {
    this._interactRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[0];
    this.ccm = this.GetComponent<CharacterClassManager>();
    if ((UnityEngine.Object) GameObject.Find("RadioNoiseSound") != (UnityEngine.Object) null)
      this.noiseSource = GameObject.Find("RadioNoiseSound").GetComponent<AudioSource>();
    this.inv = this.GetComponent<Inventory>();
    this._dissonanceSetup = this.GetComponent<DissonanceUserSetup>();
    this.mirrorIgnorancePlayer = this.GetComponent<MirrorIgnorancePlayer>();
    if (this.isLocalPlayer)
    {
      Radio.roundStarted = false;
      Radio.roundEnded = false;
      Radio.localRadio = this;
    }
    if (NetworkServer.active)
      this.InvokeRepeating("UseBattery", 1f, 1f);
    this.icon = this.GetComponentInChildren<SpeakerIcon>();
  }

  [Command]
  public void CmdUpdateClass()
  {
    if (this.isServer)
    {
      this.CallCmdUpdateClass();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (Radio), nameof (CmdUpdateClass), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  private void Update()
  {
    if ((UnityEngine.Object) Radio.comms != (UnityEngine.Object) null && (UnityEngine.Object) this.unityPlayback == (UnityEngine.Object) null && !string.IsNullOrEmpty(this.mirrorIgnorancePlayer.PlayerId))
    {
      this.state = Radio.comms.FindPlayer(this.mirrorIgnorancePlayer.PlayerId);
      if (this.state?.Playback != null)
        this.unityPlayback = (VoicePlayback) this.state.Playback;
    }
    if ((UnityEngine.Object) this.inv == (UnityEngine.Object) null)
      return;
    this.myRadio = -1;
    for (ushort index = 0; (int) index < this.inv.items.Count; ++index)
    {
      if (this.inv.items[(int) index].id == ItemType.Radio)
        this.myRadio = (int) index;
    }
    if ((UnityEngine.Object) this.localCcm == (UnityEngine.Object) null && (UnityEngine.Object) PlayerManager.localPlayer != (UnityEngine.Object) null && PlayerManager.LocalPlayerSet)
      this.localCcm = PlayerManager.localPlayer.GetComponent<CharacterClassManager>();
    if (!this.isLocalPlayer)
      return;
    if ((UnityEngine.Object) this.noiseSource == (UnityEngine.Object) null)
    {
      if ((UnityEngine.Object) GameObject.Find("RadioNoiseSound") != (UnityEngine.Object) null)
        this.noiseSource = GameObject.Find("RadioNoiseSound").GetComponent<AudioSource>();
      else
        Debug.LogError((object) "Fatal: RadioNoiseSound is missing!");
    }
    else
    {
      if ((double) this.noiseSource.volume > 0.0 != (double) Radio.noiseIntensity > 0.0 && Radio.localRadio.CheckRadio())
        this.PlayBeepSound((double) Radio.noiseIntensity > 0.0);
      this.noiseSource.volume = Radio.noiseIntensity * this.noiseMultiplier;
      Radio.noiseIntensity = 0.0f;
      this.GetInput();
      if (this.myRadio == -1 || this.presets[this.curPreset] == null)
        return;
      RadioDisplay.battery = Mathf.Clamp(Mathf.CeilToInt(this.inv.items[this.myRadio].durability), 0, 100).ToString();
      RadioDisplay.power = this.presets[this.curPreset].powerText;
      RadioDisplay.label = this.presets[this.curPreset].label;
    }
  }

  private void UseBattery()
  {
    if (!this.CheckRadio() || this.inv.items[this.myRadio].id != ItemType.Radio)
      return;
    float num = this.inv.items[this.myRadio].durability - (float) (1.66999995708466 * (1.0 / (double) this.presets[this.curPreset].powerTime) * (this.isTransmitting ? 3.0 : 1.0));
    if ((double) num <= -1.0 || (double) num >= 101.0)
      return;
    this.inv.items.ModifyDuration(this.myRadio, num);
  }

  private void GetInput()
  {
  }

  private void SetSpatialization(bool spatialized, int code = -1)
  {
    if (code >= 0 && code != this._lastDebugCode)
    {
      if (!spatialized)
        GameCore.Console.AddDebugLog("VC", "SetSpatializationFalse:" + (object) code, MessageImportance.LeastImportant, false);
      else
        GameCore.Console.AddDebugLog("VC", "SetSpatializationTrue:" + (object) code, MessageImportance.LeastImportant, false);
      this._lastDebugCode = code;
    }
    if (code == 5)
      return;
    this.playerSource.volume = 1f;
    this.playerSource.spatialBlend = spatialized ? 1f : 0.0f;
  }

  public void ApplySpatialization()
  {
    if (this.isLocalPlayer || (UnityEngine.Object) this.unityPlayback == (UnityEngine.Object) null)
      return;
    this.playerSource = this.unityPlayback.AudioSource;
    if (!Radio.roundStarted || Radio.roundEnded)
    {
      this.icon.state = SpeakerIcon.SpeakerIconState.Local;
      this.SetSpatialization(false, -1);
      this.playerSource.outputAudioMixerGroup = this.g_voice;
    }
    else if ((double) this.unityPlayback.Amplitude > 0.0)
    {
      if (this.ccm.Classes.SafeGet(this.ccm.CurClass).team == Team.SCP)
      {
        if (this._dissonanceSetup.SpeakerAs079 && this.isVoiceChatting && this.unityPlayback.IsSpeaking)
        {
          this.icon.state = SpeakerIcon.SpeakerIconState.Local;
          this.SetSpatialization(true, 0);
          this.playerSource.outputAudioMixerGroup = this.g_079;
        }
        else if (this._dissonanceSetup.MimicAs939 && this.isTransmitting && this.unityPlayback.IsSpeaking)
        {
          this.icon.state = SpeakerIcon.SpeakerIconState.Local;
          this.SetSpatialization(true, 1);
          this.playerSource.outputAudioMixerGroup = this.g_voice;
        }
        else
        {
          if (!this._dissonanceSetup.SCPChat || !this.isVoiceChatting || this.localCcm.Classes.SafeGet(this.localCcm.CurClass).team != Team.SCP)
            return;
          this.icon.state = SpeakerIcon.SpeakerIconState.Local;
          this.SetSpatialization(false, 2);
          this.playerSource.outputAudioMixerGroup = this.g_voice;
        }
      }
      else if (this._dissonanceSetup.SpectatorChat && this.ccm.Classes.SafeGet(this.ccm.CurClass).team == Team.RIP && (this.isVoiceChatting && this.localCcm.CurClass == RoleType.Spectator))
      {
        this.icon.state = SpeakerIcon.SpeakerIconState.Local;
        this.SetSpatialization(false, 3);
        this.playerSource.outputAudioMixerGroup = this.g_voice;
      }
      else if (this._dissonanceSetup.IntercomAsHuman && this.isVoiceChatting)
      {
        this.icon.state = SpeakerIcon.SpeakerIconState.Radio;
        this.playerSource.outputAudioMixerGroup = this.g_icom;
        if ((double) this.icomNoise > (double) Radio.noiseIntensity)
          Radio.noiseIntensity = this.icomNoise;
        this.SetSpatialization(false, 4);
      }
      else if (this._dissonanceSetup.RadioAsHuman && this.isTransmitting && (this.unityPlayback.IsSpeaking && this.CheckRadio()))
      {
        if (Radio.localRadio.CheckRadio())
        {
          this.icon.state = SpeakerIcon.SpeakerIconState.Radio;
          this.playerSource.outputAudioMixerGroup = this.g_radio;
          int lowerPresetId = this.GetLowerPresetID();
          float time = Vector3.Distance(Radio.localRadio.transform.position, this.transform.position);
          this.playerSource.spatialBlend = 0.0f;
          this.playerSource.volume = this.presets[lowerPresetId].volume.Evaluate(time);
          float num = this.presets[lowerPresetId].nosie.Evaluate(time);
          if ((double) num <= (double) Radio.noiseIntensity || this.isLocalPlayer)
            return;
          Radio.noiseIntensity = num;
          this.SetSpatialization(true, 5);
        }
        else
        {
          this.icon.state = SpeakerIcon.SpeakerIconState.Local;
          this.SetSpatialization(true, 6);
          this.playerSource.outputAudioMixerGroup = this.g_voice;
        }
      }
      else
      {
        if (!this.IsUsingVoiceChat() || !this.unityPlayback.IsSpeaking || this.ccm.Classes.SafeGet(this.ccm.CurClass).team == Team.RIP)
          return;
        this.icon.state = SpeakerIcon.SpeakerIconState.Local;
        this.SetSpatialization(true, 7);
        this.playerSource.outputAudioMixerGroup = this.g_voice;
      }
    }
    else
    {
      this.icon.state = SpeakerIcon.SpeakerIconState.Off;
      this.playerSource.volume = 0.0f;
      this.playerSource.spatialBlend = 1f;
    }
  }

  public int GetLowerPresetID()
  {
    return this.curPreset >= Radio.localRadio.curPreset ? Radio.localRadio.curPreset : this.curPreset;
  }

  public bool CheckRadio()
  {
    return this.myRadio != -1 && this.myRadio < this.inv.items.Count && ((double) this.inv.items[this.myRadio].durability > 0.0 && this.curPreset > 0) && (uint) this.ccm.Classes.SafeGet(this.ccm.CurClass).team > 0U;
  }

  [Command]
  private void CmdSyncTransmissionStatus(bool b)
  {
    if (this.isServer)
    {
      this.CallCmdSyncTransmissionStatus(b);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBoolean(b);
      this.SendCommandInternal(typeof (Radio), nameof (CmdSyncTransmissionStatus), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdSyncVoiceChatStatus(bool b)
  {
    if (this.isServer)
    {
      this.CallCmdSyncVoiceChatStatus(b);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBoolean(b);
      this.SendCommandInternal(typeof (Radio), nameof (CmdSyncVoiceChatStatus), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  private bool IsUsingVoiceChat()
  {
    return this.isVoiceChatting || this.isTransmitting;
  }

  public void PlayBeepSound(bool b)
  {
    this.beepSource.PlayOneShot(b ? this.beepStart[UnityEngine.Random.Range(0, this.beepStart.Length)] : this.beepStop[UnityEngine.Random.Range(0, this.beepStop.Length)]);
  }

  private float Distance(Vector3 a, Vector3 b)
  {
    return Vector3.Distance(new Vector3(a.x, a.y / 4f, a.z), new Vector3(b.x, b.y / 4f, b.z));
  }

  public bool ShouldBeVisible(GameObject localplayer)
  {
    return this.isTransmitting && (double) this.presets[this.GetLowerPresetID()].beepRange > (double) this.Distance(this.transform.position, localplayer.transform.position);
  }

  [Command]
  public void CmdUpdatePreset(int preset)
  {
    if (this.isServer)
    {
      this.CallCmdUpdatePreset(preset);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WritePackedInt32(preset);
      this.SendCommandInternal(typeof (Radio), nameof (CmdUpdatePreset), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  private void MirrorProcessed()
  {
  }

  public int NetworkcurPreset
  {
    get
    {
      return this.curPreset;
    }
    [param: In] set
    {
      this.SetSyncVar<int>(value, ref this.curPreset, 1UL);
    }
  }

  public bool NetworkisTransmitting
  {
    get
    {
      return this.isTransmitting;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.isTransmitting, 2UL);
    }
  }

  public bool NetworkisVoiceChatting
  {
    get
    {
      return this.isVoiceChatting;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.isVoiceChatting, 4UL);
    }
  }

  protected static void InvokeCmdCmdUpdateClass(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdUpdateClass called on client.");
    else
      ((Radio) obj).CallCmdUpdateClass();
  }

  protected static void InvokeCmdCmdSyncTransmissionStatus(
    NetworkBehaviour obj,
    NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSyncTransmissionStatus called on client.");
    else
      ((Radio) obj).CallCmdSyncTransmissionStatus(reader.ReadBoolean());
  }

  protected static void InvokeCmdCmdSyncVoiceChatStatus(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSyncVoiceChatStatus called on client.");
    else
      ((Radio) obj).CallCmdSyncVoiceChatStatus(reader.ReadBoolean());
  }

  protected static void InvokeCmdCmdUpdatePreset(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdUpdatePreset called on client.");
    else
      ((Radio) obj).CallCmdUpdatePreset(reader.ReadPackedInt32());
  }

  public void CallCmdUpdateClass()
  {
    this._dissonanceSetup.TargetUpdateForTeam(this.ccm.Classes.SafeGet(this.ccm.CurClass).team);
  }

  public void CallCmdSyncTransmissionStatus(bool b)
  {
    if (!this._interactRateLimit.CanExecute(true))
      return;
    this.NetworkisTransmitting = b;
    this._dissonanceSetup.RadioAsHuman = this.CheckRadio() & b;
  }

  public void CallCmdSyncVoiceChatStatus(bool b)
  {
    if (!this._interactRateLimit.CanExecute(true))
      return;
    this.NetworkisVoiceChatting = b;
    this._dissonanceSetup.SpectatorChat = this.ccm.CurClass == RoleType.Spectator & b;
    this._dissonanceSetup.SCPChat = this.ccm.Classes.SafeGet(this.ccm.CurClass).team == Team.SCP & b;
  }

  public void CallCmdUpdatePreset(int preset)
  {
    if (!this._interactRateLimit.CanExecute(true))
      return;
    this.NetworkcurPreset = preset;
  }

  static Radio()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (Radio), "CmdUpdateClass", new NetworkBehaviour.CmdDelegate(Radio.InvokeCmdCmdUpdateClass));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Radio), "CmdSyncTransmissionStatus", new NetworkBehaviour.CmdDelegate(Radio.InvokeCmdCmdSyncTransmissionStatus));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Radio), "CmdSyncVoiceChatStatus", new NetworkBehaviour.CmdDelegate(Radio.InvokeCmdCmdSyncVoiceChatStatus));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Radio), "CmdUpdatePreset", new NetworkBehaviour.CmdDelegate(Radio.InvokeCmdCmdUpdatePreset));
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WritePackedInt32(this.curPreset);
      writer.WriteBoolean(this.isTransmitting);
      writer.WriteBoolean(this.isVoiceChatting);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WritePackedInt32(this.curPreset);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WriteBoolean(this.isTransmitting);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 4L) != 0L)
    {
      writer.WriteBoolean(this.isVoiceChatting);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworkcurPreset = reader.ReadPackedInt32();
      this.NetworkisTransmitting = reader.ReadBoolean();
      this.NetworkisVoiceChatting = reader.ReadBoolean();
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
        this.NetworkcurPreset = reader.ReadPackedInt32();
      if ((num & 2L) != 0L)
        this.NetworkisTransmitting = reader.ReadBoolean();
      if ((num & 4L) == 0L)
        return;
      this.NetworkisVoiceChatting = reader.ReadBoolean();
    }
  }

  [Serializable]
  public class RadioPreset
  {
    public string label;
    public string powerText;
    public float powerTime;
    public AnimationCurve nosie;
    public AnimationCurve volume;
    public float beepRange;
  }
}
