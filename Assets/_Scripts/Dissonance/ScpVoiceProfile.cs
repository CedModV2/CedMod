// Decompiled with JetBrains decompiler
// Type: Assets._Scripts.Dissonance.ScpVoiceProfile
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Dissonance;

namespace Assets._Scripts.Dissonance
{
  public class ScpVoiceProfile : VoiceProfile
  {
    public ScpVoiceProfile(DissonanceUserSetup setup)
      : base(setup)
    {
    }

    public override void Apply()
    {
      this.dissonanceSetup.ResetToDefault();
      this.dissonanceSetup.EnableListening(TriggerType.Proximity | TriggerType.Role | TriggerType.Intercom, RoleType.SCP);
      if (this.dissonanceSetup.SpeakerAs079)
        this.dissonanceSetup.EnableSpeaking(TriggerType.Proximity, RoleType.Null);
      else if (this.dissonanceSetup.MimicAs939)
      {
        BaseCommsTrigger trigger;
        if (this.dissonanceSetup.TryGetVoiceTrigger(TriggerType.Proximity, true, out trigger))
          (trigger as VoiceBroadcastTrigger).InputName = "Alt Voice Chat";
        this.dissonanceSetup.EnableSpeaking(TriggerType.Proximity, RoleType.Null);
      }
      else if (this.dissonanceSetup.SCPChat)
      {
        this.dissonanceSetup.EnableSpeaking(TriggerType.Role, RoleType.SCP);
      }
      else
      {
        BaseCommsTrigger trigger;
        if (this.dissonanceSetup.TryGetVoiceTrigger(TriggerType.Proximity, true, out trigger))
        {
          VoiceBroadcastTrigger broadcastTrigger = trigger as VoiceBroadcastTrigger;
          if (broadcastTrigger.InputName == "Alt Voice Chat")
            broadcastTrigger.InputName = "Voice Chat";
        }
        this.dissonanceSetup.DisableSpeaking(TriggerType.Role, RoleType.Null);
        this.dissonanceSetup.DisableSpeaking(TriggerType.Proximity, RoleType.Null);
      }
    }
  }
}
