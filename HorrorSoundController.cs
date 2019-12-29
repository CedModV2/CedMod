// Decompiled with JetBrains decompiler
// Type: HorrorSoundController
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HorrorSoundController : NetworkBehaviour
{
  private float cooldown = 20f;
  private float horrorSoundTriggerRange = 35f;
  private List<GameObject> scpsThatICanSee = new List<GameObject>();
  private CharacterClassManager cmng;
  public LayerMask mask;
  [SerializeField]
  private Transform plyCamera;
  public AudioSource horrorSoundSource;
  [SerializeField]
  private HorrorSoundController.DistanceSound[] sounds;
  public AudioClip blindedSoundClip;

  private void Start()
  {
    this.cmng = this.GetComponent<CharacterClassManager>();
  }

  public void BlindSFX()
  {
    this.horrorSoundSource.PlayOneShot(this.blindedSoundClip);
  }

  private void Update()
  {
  }

  private void MirrorProcessed()
  {
  }

  [Serializable]
  public struct DistanceSound : IEquatable<HorrorSoundController.DistanceSound>
  {
    public float distance;
    public AudioClip clip;

    public bool Equals(HorrorSoundController.DistanceSound other)
    {
      return (double) this.distance == (double) other.distance && (UnityEngine.Object) this.clip == (UnityEngine.Object) other.clip;
    }

    public override bool Equals(object obj)
    {
      return obj is HorrorSoundController.DistanceSound other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return this.distance.GetHashCode() * 397 ^ ((UnityEngine.Object) this.clip != (UnityEngine.Object) null ? this.clip.GetHashCode() : 0);
    }

    public static bool operator ==(
      HorrorSoundController.DistanceSound left,
      HorrorSoundController.DistanceSound right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(
      HorrorSoundController.DistanceSound left,
      HorrorSoundController.DistanceSound right)
    {
      return !left.Equals(right);
    }
  }
}
