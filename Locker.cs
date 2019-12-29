// Decompiled with JetBrains decompiler
// Type: Locker
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Locker
{
  public string name;
  public string lockerTag;
  public Transform gameObject;
  public bool supportsStandarizedAnimation;
  public LockerChamber[] chambers;
  [Range(0.0f, 100f)]
  public int chanceOfSpawn;
  public float sortingDuration;
  public GameObject sortingTarget;
  public float sortingForce;
  public Vector3 sortingTorque;
  private List<Pickup> assignedPickups;

  public void LockPickups(bool state)
  {
    if (this.assignedPickups == null)
      return;
    foreach (Pickup assignedPickup in this.assignedPickups)
    {
      if ((UnityEngine.Object) assignedPickup != (UnityEngine.Object) null)
        assignedPickup.info.locked = state;
    }
  }

  public void AssignPickup(Pickup p)
  {
    if (this.assignedPickups == null)
      this.assignedPickups = new List<Pickup>();
    this.assignedPickups.Add(p);
  }
}
