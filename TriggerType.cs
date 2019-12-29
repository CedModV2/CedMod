// Decompiled with JetBrains decompiler
// Type: TriggerType
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

[Flags]
public enum TriggerType : byte
{
  None = 0,
  Proximity = 1,
  Role = 2,
  Intercom = 4,
}
