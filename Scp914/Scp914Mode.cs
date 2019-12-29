// Decompiled with JetBrains decompiler
// Type: Scp914.Scp914Mode
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Scp914
{
  [Flags]
  public enum Scp914Mode
  {
    Dropped = 1,
    Inventory = 2,
    DroppedAndInventory = Inventory | Dropped, // 0x00000003
    DroppedAndPlayerTeleport = 5,
    Held = 6,
    DroppedAndHeld = Held | Dropped, // 0x00000007
  }
}
