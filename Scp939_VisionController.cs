// Decompiled with JetBrains decompiler
// Type: Scp939_VisionController
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Scp939_VisionController : NetworkBehaviour
{
  public float minimumSilenceTime = 2.5f;
  public float minimumNoiseLevel = 2f;
  public List<Scp939_VisionController.Scp939_Vision> seeingSCPs = new List<Scp939_VisionController.Scp939_Vision>();
  public float noise;
  private CharacterClassManager _ccm;

  private void Start()
  {
    this._ccm = this.GetComponent<CharacterClassManager>();
  }

  public bool CanSee(Scp939PlayerScript scp939)
  {
    if (!scp939.iAm939)
      return false;
    foreach (Scp939_VisionController.Scp939_Vision seeingScP in this.seeingSCPs)
    {
      if ((UnityEngine.Object) seeingScP.scp == (UnityEngine.Object) scp939)
        return true;
    }
    return false;
  }

  private void FixedUpdate()
  {
    if (!NetworkServer.active)
      return;
    foreach (Scp939PlayerScript instance in Scp939PlayerScript.instances)
    {
      if ((UnityEngine.Object) instance != (UnityEngine.Object) null && (double) Vector3.Distance(this.transform.position, instance.transform.position) < (double) this.noise)
        this.AddVision(instance);
    }
    this.noise = this.minimumNoiseLevel;
    this.UpdateVisions();
  }

  private void AddVision(Scp939PlayerScript scp939)
  {
    for (int index = 0; index < this.seeingSCPs.Count; ++index)
    {
      if ((UnityEngine.Object) this.seeingSCPs[index].scp == (UnityEngine.Object) scp939)
      {
        this.seeingSCPs[index].remainingTime = this.minimumSilenceTime;
        return;
      }
    }
    this.seeingSCPs.Add(new Scp939_VisionController.Scp939_Vision()
    {
      scp = scp939,
      remainingTime = this.minimumSilenceTime
    });
  }

  private void UpdateVisions()
  {
    for (int index = 0; index < this.seeingSCPs.Count; ++index)
    {
      this.seeingSCPs[index].remainingTime -= 0.02f;
      if ((UnityEngine.Object) this.seeingSCPs[index].scp == (UnityEngine.Object) null || !this.seeingSCPs[index].scp.iAm939 || (double) this.seeingSCPs[index].remainingTime <= 0.0)
      {
        this.seeingSCPs.RemoveAt(index);
        break;
      }
    }
  }

  [Server]
  public void MakeNoise(float distanceIntensity)
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Void Scp939_VisionController::MakeNoise(System.Single)' called on client");
    }
    else
    {
      if (!this._ccm.IsHuman() || (double) this.noise >= (double) distanceIntensity)
        return;
      this.noise = distanceIntensity;
    }
  }

  private void MirrorProcessed()
  {
  }

  [Serializable]
  public class Scp939_Vision
  {
    public Scp939PlayerScript scp;
    public float remainingTime;
  }
}
