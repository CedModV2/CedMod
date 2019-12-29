// Decompiled with JetBrains decompiler
// Type: Assets._Scripts.Dissonance.CivilianVoiceProfile
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Dissonance;

namespace Assets._Scripts.Dissonance
{
  public class CivilianVoiceProfile : VoiceProfile
  {
    public CivilianVoiceProfile(DissonanceUserSetup setup)
      : base(setup)
    {
    }

    public override void Apply()
    {
      this.dissonanceSetup.ResetToDefault();
      this.dissonanceSetup.EnableSpeaking(TriggerType.Proximity, RoleType.Null);
      this.dissonanceSetup.EnableListening(TriggerType.Proximity | TriggerType.Intercom, RoleType.Null);
      if (this.dissonanceSetup.RadioAsHuman)
      {
        BaseCommsTrigger trigger;
        if (this.dissonanceSetup.TryGetVoiceTrigger(TriggerType.Proximity, true, out trigger))
          ((VoiceBroadcastTrigger) trigger).InputName = "Alt Voice Chat";
      }
      else
      {
        BaseCommsTrigger trigger;
        if (this.dissonanceSetup.TryGetVoiceTrigger(TriggerType.Proximity, true, out trigger))
          ((VoiceBroadcastTrigger) trigger).InputName = "Voice Chat";
      }
      if (this.dissonanceSetup.IntercomAsHuman)
        this.dissonanceSetup.EnableSpeaking(TriggerType.Intercom, RoleType.Null);
      else
        this.dissonanceSetup.DisableSpeaking(TriggerType.Intercom, RoleType.Null);
    }
  }
}
