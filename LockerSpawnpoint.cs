// Decompiled with JetBrains decompiler
// Type: LockerSpawnpoint
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

[Serializable]
public class LockerSpawnpoint
{
  public string name;
  public Transform spawnpoint;
  public string lockerTag;
  private bool used;

  public bool IsFree()
  {
    return !this.used;
  }

  public void Obtain()
  {
    this.used = true;
  }
}
