// Decompiled with JetBrains decompiler
// Type: Assets._Scripts.Dissonance.SpectatorVoiceProfile
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;

namespace Assets._Scripts.Dissonance
{
  public class SpectatorVoiceProfile : VoiceProfile
  {
    public SpectatorVoiceProfile(DissonanceUserSetup setup)
      : base(setup)
    {
    }

    public override void Apply()
    {
      this.dissonanceSetup.ResetToDefault();
      Console.AddDebugLog("VC", "ApplySpectatorVoiceProfile\nSpectatorChat:" + this.dissonanceSetup.SpectatorChat.ToString() + "\nroundStarted:" + Radio.roundStarted.ToString() + "\nroundEnded:" + Radio.roundEnded.ToString(), MessageImportance.Normal, false);
      if (this.dissonanceSetup.SpectatorChat || !Radio.roundStarted || Radio.roundEnded)
        this.dissonanceSetup.EnableSpeaking(TriggerType.Role, RoleType.Ghost);
      else
        this.dissonanceSetup.DisableSpeaking(TriggerType.Role, RoleType.Ghost);
      this.dissonanceSetup.EnableListening(TriggerType.Proximity | TriggerType.Role | TriggerType.Intercom, RoleType.Ghost);
    }
  }
}
