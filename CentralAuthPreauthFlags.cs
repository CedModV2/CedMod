// Decompiled with JetBrains decompiler
// Type: CentralAuthPreauthFlags
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

[Flags]
public enum CentralAuthPreauthFlags : byte
{
  None = 0,
  ReservedSlot = 1,
  IgnoreBans = 2,
  IgnoreWhitelist = 4,
  IgnoreGeoblock = 8,
  GloballyBanned = 16, // 0x10
}
