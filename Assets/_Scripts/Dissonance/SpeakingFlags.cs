// Decompiled with JetBrains decompiler
// Type: Assets._Scripts.Dissonance.SpeakingFlags
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Assets._Scripts.Dissonance
{
  [Flags]
  public enum SpeakingFlags : byte
  {
    RadioAsHuman = 1,
    IntercomAsHuman = 2,
    MimicAs939 = 4,
    SpeakerAs079 = 8,
    SpectatorChat = 16, // 0x10
    SCPChat = 32, // 0x20
  }
}
