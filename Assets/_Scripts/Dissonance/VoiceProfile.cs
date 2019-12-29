// Decompiled with JetBrains decompiler
// Type: Assets._Scripts.Dissonance.VoiceProfile
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Assets._Scripts.Dissonance
{
  [Serializable]
  public abstract class VoiceProfile
  {
    protected DissonanceUserSetup dissonanceSetup;

    public VoiceProfile(DissonanceUserSetup dissonanceSetup)
    {
      this.dissonanceSetup = dissonanceSetup;
    }

    public abstract void Apply();
  }
}
