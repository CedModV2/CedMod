// Decompiled with JetBrains decompiler
// Type: PreauthItem
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

public struct PreauthItem
{
  public string UserId { get; private set; }

  public long Added { get; private set; }

  public PreauthItem(string userId)
  {
    this.UserId = userId;
    this.Added = DateTime.Now.Ticks;
  }

  public void SetUserId(string userId)
  {
    this.UserId = userId;
    this.Added = DateTime.Now.Ticks;
  }
}
