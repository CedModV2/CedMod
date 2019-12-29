// Decompiled with JetBrains decompiler
// Type: LockerChamber
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct LockerChamber : IEquatable<LockerChamber>
{
  public string name;
  public Transform spawnpoint;
  public Animator doorAnimator;
  public AudioClip openClip;
  public AudioClip closeClip;
  public string accessToken;
  public MeshRenderer keycardPad;
  private float lastOpened;
  public string itemTag;
  private bool used;
  private bool enumeratorInUse;

  public void SetCooldown()
  {
    this.lastOpened = Time.realtimeSinceStartup;
  }

  public bool CooldownAtZero()
  {
    return (double) Time.realtimeSinceStartup - (double) this.lastOpened > 2.0;
  }

  public bool IsFree()
  {
    return !this.used;
  }

  public void Obtain()
  {
    this.used = true;
  }

  public void DoMaterial(Material mat, Material defaultMat)
  {
    Timing.RunCoroutine(this._DoMaterial(mat, defaultMat), Segment.FixedUpdate);
  }

  private IEnumerator<float> _DoMaterial(Material mat, Material defaultMat)
  {
    if (!((UnityEngine.Object) this.keycardPad.sharedMaterial == (UnityEngine.Object) mat))
    {
      while (this.enumeratorInUse)
        yield return 0.0f;
      this.enumeratorInUse = true;
      this.keycardPad.sharedMaterial = mat;
      for (int i = 0; i < 30; ++i)
        yield return 0.0f;
      this.keycardPad.sharedMaterial = defaultMat;
      this.enumeratorInUse = false;
    }
  }

  public bool Equals(LockerChamber other)
  {
    return string.Equals(this.name, other.name) && (UnityEngine.Object) this.spawnpoint == (UnityEngine.Object) other.spawnpoint && ((UnityEngine.Object) this.doorAnimator == (UnityEngine.Object) other.doorAnimator && (UnityEngine.Object) this.openClip == (UnityEngine.Object) other.openClip) && ((UnityEngine.Object) this.closeClip == (UnityEngine.Object) other.closeClip && string.Equals(this.accessToken, other.accessToken) && ((UnityEngine.Object) this.keycardPad == (UnityEngine.Object) other.keycardPad && (double) this.lastOpened == (double) other.lastOpened)) && (string.Equals(this.itemTag, other.itemTag) && this.used == other.used) && this.enumeratorInUse == other.enumeratorInUse;
  }

  public override bool Equals(object obj)
  {
    return obj is LockerChamber other && this.Equals(other);
  }

  public override int GetHashCode()
  {
    return ((((((((((this.name != null ? this.name.GetHashCode() : 0) * 397 ^ ((UnityEngine.Object) this.spawnpoint != (UnityEngine.Object) null ? this.spawnpoint.GetHashCode() : 0)) * 397 ^ ((UnityEngine.Object) this.doorAnimator != (UnityEngine.Object) null ? this.doorAnimator.GetHashCode() : 0)) * 397 ^ ((UnityEngine.Object) this.openClip != (UnityEngine.Object) null ? this.openClip.GetHashCode() : 0)) * 397 ^ ((UnityEngine.Object) this.closeClip != (UnityEngine.Object) null ? this.closeClip.GetHashCode() : 0)) * 397 ^ (this.accessToken != null ? this.accessToken.GetHashCode() : 0)) * 397 ^ ((UnityEngine.Object) this.keycardPad != (UnityEngine.Object) null ? this.keycardPad.GetHashCode() : 0)) * 397 ^ this.lastOpened.GetHashCode()) * 397 ^ (this.itemTag != null ? this.itemTag.GetHashCode() : 0)) * 397 ^ this.used.GetHashCode()) * 397 ^ this.enumeratorInUse.GetHashCode();
  }

  public static bool operator ==(LockerChamber left, LockerChamber right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(LockerChamber left, LockerChamber right)
  {
    return !left.Equals(right);
  }
}
