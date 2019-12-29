// Decompiled with JetBrains decompiler
// Type: Dissonance.ImplicitUseTargetFlags
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Dissonance
{
  [Flags]
  internal enum ImplicitUseTargetFlags
  {
    Default = 1,
    Itself = Default, // 0x00000001
    Members = 2,
    WithMembers = Members | Itself, // 0x00000003
  }
}
