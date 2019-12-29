// Decompiled with JetBrains decompiler
// Type: Assets._Scripts.Dissonance.DissonanceUserSetup
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Dissonance;
using Mirror;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Assets._Scripts.Dissonance
{
  public class DissonanceUserSetup : NetworkBehaviour
  {
    private static readonly TriggerType[] TriggerTypes = new TriggerType[4]
    {
      TriggerType.Intercom,
      TriggerType.Proximity,
      TriggerType.Role,
      TriggerType.None
    };
    [SerializeField]
    private DissonanceUserSetup.VoiceTriggerEntry[] triggers = new DissonanceUserSetup.VoiceTriggerEntry[6];
    private readonly Dictionary<Team, VoiceProfile> voice = new Dictionary<Team, VoiceProfile>();
    [SerializeField]
    [SyncVar(hook = "SetSpeakingFlags")]
    private SpeakingFlags speakingFlags;
    [SerializeField]
    private VoiceProfile currentProfile;
    public const string inputName = "Voice Chat";
    public const string altInputName = "Alt Voice Chat";
    [SyncVar]
    public bool altIsActive;

    public bool RadioAsHuman
    {
      get
      {
        return this.speakingFlags.HasFlagNonAlloc(SpeakingFlags.RadioAsHuman);
      }
      set
      {
        this.SpeakingFlagsSetter(value, SpeakingFlags.RadioAsHuman);
      }
    }

    public bool IntercomAsHuman
    {
      get
      {
        return this.speakingFlags.HasFlagNonAlloc(SpeakingFlags.IntercomAsHuman);
      }
      set
      {
        this.SpeakingFlagsSetter(value, SpeakingFlags.IntercomAsHuman);
      }
    }

    public bool MimicAs939
    {
      get
      {
        return this.speakingFlags.HasFlagNonAlloc(SpeakingFlags.MimicAs939);
      }
      set
      {
        this.SpeakingFlagsSetter(value, SpeakingFlags.MimicAs939);
      }
    }

    public bool SpeakerAs079
    {
      get
      {
        return this.speakingFlags.HasFlagNonAlloc(SpeakingFlags.SpeakerAs079);
      }
      set
      {
        this.SpeakingFlagsSetter(value, SpeakingFlags.SpeakerAs079);
      }
    }

    public bool SpectatorChat
    {
      get
      {
        return this.speakingFlags.HasFlagNonAlloc(SpeakingFlags.SpectatorChat);
      }
      set
      {
        this.SpeakingFlagsSetter(value, SpeakingFlags.SpectatorChat);
      }
    }

    public bool SCPChat
    {
      get
      {
        return this.speakingFlags.HasFlagNonAlloc(SpeakingFlags.SCPChat);
      }
      set
      {
        this.SpeakingFlagsSetter(value, SpeakingFlags.SCPChat);
      }
    }

    private void Start()
    {
      if (!this.isLocalPlayer)
        this.ResetToDefault();
      VoiceProfile voiceProfile = (VoiceProfile) new CivilianVoiceProfile(this);
      this.voice[Team.SCP] = (VoiceProfile) new ScpVoiceProfile(this);
      this.voice[Team.RIP] = (VoiceProfile) new SpectatorVoiceProfile(this);
      this.voice[Team.CDP] = voiceProfile;
      this.voice[Team.CHI] = voiceProfile;
      this.voice[Team.MTF] = voiceProfile;
      this.voice[Team.RSC] = voiceProfile;
      this.voice[Team.TUT] = voiceProfile;
      if (!this.isLocalPlayer)
        return;
      this.voice[Team.RIP].Apply();
    }

    private void Update()
    {
      if (!this.isLocalPlayer)
        return;
      this.CmdAltIsActive(Input.GetKey(NewInput.GetKey("Alt Voice Chat")));
    }

    [Command]
    public void CmdAltIsActive(bool value)
    {
      if (this.isServer)
      {
        this.CallCmdAltIsActive(value);
      }
      else
      {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteBoolean(value);
        this.SendCommandInternal(typeof (DissonanceUserSetup), nameof (CmdAltIsActive), writer, 0);
        NetworkWriterPool.Recycle(writer);
      }
    }

    private void SpeakingFlagsSetter(bool value, SpeakingFlags flag)
    {
      if (value == this.speakingFlags.HasFlagNonAlloc(flag))
        return;
      if (value)
        this.NetworkspeakingFlags = this.speakingFlags | flag;
      else
        this.NetworkspeakingFlags = this.speakingFlags & ~flag;
    }

    private void SetSpeakingFlags(SpeakingFlags newFlags)
    {
      this.NetworkspeakingFlags = newFlags;
      this.currentProfile?.Apply();
    }

    public bool TryGetVoiceTrigger(
      TriggerType triggerType,
      bool isBroadcast,
      out BaseCommsTrigger trigger)
    {
      foreach (DissonanceUserSetup.VoiceTriggerEntry trigger1 in this.triggers)
      {
        if (trigger1.type == triggerType && (isBroadcast ? (trigger1.trigger is CustomBroadcastTrigger ? 1 : 0) : (trigger1.trigger is VoiceReceiptTrigger ? 1 : 0)) != 0)
        {
          trigger = trigger1.trigger;
          return true;
        }
      }
      trigger = (BaseCommsTrigger) null;
      return false;
    }

    public bool IsTransmittingOnAny()
    {
      foreach (DissonanceUserSetup.VoiceTriggerEntry trigger1 in this.triggers)
      {
        if (trigger1.trigger is CustomBroadcastTrigger trigger && trigger.IsTransmitting)
          return true;
      }
      return false;
    }

    [TargetRpc]
    public void TargetUpdateForTeam(Team team)
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      NetworkWriterExtensions.WriteByte(writer, (byte) team);
      this.SendTargetRPCInternal((NetworkConnection) null, typeof (DissonanceUserSetup), nameof (TargetUpdateForTeam), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }

    public void EnableSpeaking(TriggerType triggerType, RoleType roleType = RoleType.Null)
    {
      foreach (TriggerType triggerType1 in DissonanceUserSetup.TriggerTypes)
      {
        BaseCommsTrigger trigger;
        if ((triggerType & triggerType1) == triggerType1 && this.TryGetVoiceTrigger(triggerType1, true, out trigger))
        {
          CustomBroadcastTrigger broadcastTrigger = (CustomBroadcastTrigger) trigger;
          string str = triggerType1 != TriggerType.Role || roleType == RoleType.Null ? triggerType1.TriggerTypeToString() : roleType.RoleTypeToString();
          if (!string.IsNullOrEmpty(str))
          {
            broadcastTrigger.enabled = true;
            broadcastTrigger.RoomName = str;
          }
        }
      }
    }

    public void DisableSpeaking(TriggerType triggerType, RoleType roleType = RoleType.Null)
    {
      foreach (TriggerType triggerType1 in DissonanceUserSetup.TriggerTypes)
      {
        BaseCommsTrigger trigger;
        if ((triggerType & triggerType1) == triggerType1 && this.TryGetVoiceTrigger(triggerType1, true, out trigger))
        {
          CustomBroadcastTrigger broadcastTrigger = (CustomBroadcastTrigger) trigger;
          if (!string.IsNullOrEmpty(triggerType1 != TriggerType.Role || roleType == RoleType.Null ? triggerType1.TriggerTypeToString() : roleType.RoleTypeToString()))
          {
            broadcastTrigger.enabled = true;
            broadcastTrigger.RoomName = (string) null;
            broadcastTrigger.enabled = false;
          }
        }
      }
    }

    public void EnableListening(TriggerType triggerType, RoleType roleType = RoleType.Null)
    {
      foreach (TriggerType triggerType1 in DissonanceUserSetup.TriggerTypes)
      {
        BaseCommsTrigger trigger;
        if ((triggerType & triggerType1) == triggerType1 && this.TryGetVoiceTrigger(triggerType1, false, out trigger))
        {
          VoiceReceiptTrigger voiceReceiptTrigger = (VoiceReceiptTrigger) trigger;
          string str = triggerType1 != TriggerType.Role || roleType == RoleType.Null ? triggerType1.TriggerTypeToString() : roleType.RoleTypeToString();
          if (!string.IsNullOrEmpty(str))
          {
            voiceReceiptTrigger.enabled = true;
            voiceReceiptTrigger.RoomName = str;
          }
        }
      }
    }

    public void DisableListening(TriggerType triggerType, RoleType roleType = RoleType.Null)
    {
      foreach (TriggerType triggerType1 in DissonanceUserSetup.TriggerTypes)
      {
        BaseCommsTrigger trigger;
        if ((triggerType & triggerType1) == triggerType1 && this.TryGetVoiceTrigger(triggerType1, false, out trigger))
        {
          VoiceReceiptTrigger voiceReceiptTrigger = (VoiceReceiptTrigger) trigger;
          if (!string.IsNullOrEmpty(triggerType1 != TriggerType.Role || roleType == RoleType.Null ? triggerType1.TriggerTypeToString() : roleType.RoleTypeToString()))
          {
            voiceReceiptTrigger.enabled = true;
            voiceReceiptTrigger.RoomName = (string) null;
            voiceReceiptTrigger.enabled = false;
          }
        }
      }
    }

    public void ResetToDefault()
    {
      this.DisableSpeaking(TriggerType.Proximity | TriggerType.Intercom, RoleType.Null);
      this.DisableListening(TriggerType.Proximity | TriggerType.Intercom, RoleType.Null);
      this.DisableListening(TriggerType.Role, RoleType.Ghost);
      this.DisableListening(TriggerType.Role, RoleType.SCP);
      this.DisableSpeaking(TriggerType.Role, RoleType.Ghost);
      this.DisableSpeaking(TriggerType.Role, RoleType.SCP);
      BaseCommsTrigger trigger1;
      if (this.TryGetVoiceTrigger(TriggerType.Role, true, out trigger1))
        ((VoiceBroadcastTrigger) trigger1).InputName = "Voice Chat";
      BaseCommsTrigger trigger2;
      if (!this.TryGetVoiceTrigger(TriggerType.Proximity, true, out trigger2))
        return;
      ((VoiceBroadcastTrigger) trigger2).InputName = "Voice Chat";
    }

    static DissonanceUserSetup()
    {
      NetworkBehaviour.RegisterCommandDelegate(typeof (DissonanceUserSetup), "CmdAltIsActive", new NetworkBehaviour.CmdDelegate(DissonanceUserSetup.InvokeCmdCmdAltIsActive));
      NetworkBehaviour.RegisterRpcDelegate(typeof (DissonanceUserSetup), "TargetUpdateForTeam", new NetworkBehaviour.CmdDelegate(DissonanceUserSetup.InvokeRpcTargetUpdateForTeam));
    }

    private void MirrorProcessed()
    {
    }

    public SpeakingFlags NetworkspeakingFlags
    {
      get
      {
        return this.speakingFlags;
      }
      [param: In] set
      {
        if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(1UL))
        {
          this.setSyncVarHookGuard(1UL, true);
          this.SetSpeakingFlags(value);
          this.setSyncVarHookGuard(1UL, false);
        }
        this.SetSyncVar<SpeakingFlags>(value, ref this.speakingFlags, 1UL);
      }
    }

    public bool NetworkaltIsActive
    {
      get
      {
        return this.altIsActive;
      }
      [param: In] set
      {
        this.SetSyncVar<bool>(value, ref this.altIsActive, 2UL);
      }
    }

    protected static void InvokeCmdCmdAltIsActive(NetworkBehaviour obj, NetworkReader reader)
    {
      if (!NetworkServer.active)
        Debug.LogError((object) "Command CmdAltIsActive called on client.");
      else
        ((DissonanceUserSetup) obj).CallCmdAltIsActive(reader.ReadBoolean());
    }

    public void CallCmdAltIsActive(bool value)
    {
      this.NetworkaltIsActive = value;
    }

    protected static void InvokeRpcTargetUpdateForTeam(NetworkBehaviour obj, NetworkReader reader)
    {
      if (!NetworkClient.active)
        Debug.LogError((object) "TargetRPC TargetUpdateForTeam called on server.");
      else
        ((DissonanceUserSetup) obj).CallTargetUpdateForTeam((Team) NetworkReaderExtensions.ReadByte(reader));
    }

    public void CallTargetUpdateForTeam(Team team)
    {
      VoiceProfile voiceProfile;
      if (this.voice.TryGetValue(team, out voiceProfile))
      {
        if (voiceProfile == this.currentProfile)
          return;
        this.currentProfile = voiceProfile;
        voiceProfile.Apply();
      }
      else
        Debug.LogWarning((object) ("Tried to load a voice profile for unknown team: " + (object) team));
    }

    public override bool OnSerialize(NetworkWriter writer, bool forceAll)
    {
      bool flag = base.OnSerialize(writer, forceAll);
      if (forceAll)
      {
        NetworkWriterExtensions.WriteByte(writer, (byte) this.speakingFlags);
        writer.WriteBoolean(this.altIsActive);
        return true;
      }
      writer.WritePackedUInt64(this.syncVarDirtyBits);
      if (((long) this.syncVarDirtyBits & 1L) != 0L)
      {
        NetworkWriterExtensions.WriteByte(writer, (byte) this.speakingFlags);
        flag = true;
      }
      if (((long) this.syncVarDirtyBits & 2L) != 0L)
      {
        writer.WriteBoolean(this.altIsActive);
        flag = true;
      }
      return flag;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
      base.OnDeserialize(reader, initialState);
      if (initialState)
      {
        SpeakingFlags newFlags = (SpeakingFlags) NetworkReaderExtensions.ReadByte(reader);
        this.SetSpeakingFlags(newFlags);
        this.NetworkspeakingFlags = newFlags;
        this.NetworkaltIsActive = reader.ReadBoolean();
      }
      else
      {
        long num = (long) reader.ReadPackedUInt64();
        if ((num & 1L) != 0L)
        {
          SpeakingFlags newFlags = (SpeakingFlags) NetworkReaderExtensions.ReadByte(reader);
          this.SetSpeakingFlags(newFlags);
          this.NetworkspeakingFlags = newFlags;
        }
        if ((num & 2L) == 0L)
          return;
        this.NetworkaltIsActive = reader.ReadBoolean();
      }
    }

    [Serializable]
    private struct VoiceTriggerEntry
    {
      public TriggerType type;
      public BaseCommsTrigger trigger;
    }
  }
}
