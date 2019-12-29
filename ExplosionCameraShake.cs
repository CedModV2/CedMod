// Decompiled with JetBrains decompiler
// Type: ExplosionCameraShake
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Kino;
using UnityEngine;

public class ExplosionCameraShake : MonoBehaviour
{
  public float force;
  public float deductSpeed;
  public AnalogGlitch glitch;
  public static ExplosionCameraShake singleton;

  private void Update()
  {
    this.glitch.enabled = (double) this.glitch.horizontalShake > 0.0;
    this.force -= Time.deltaTime / this.deductSpeed;
    this.force = Mathf.Clamp01(this.force);
    this.glitch.scanLineJitter = this.force;
    this.glitch.horizontalShake = this.force;
    this.glitch.colorDrift = this.force;
  }

  private void Awake()
  {
    ExplosionCameraShake.singleton = this;
  }

  public void Shake(float explosionForce)
  {
    if ((double) explosionForce <= (double) this.force)
      return;
    this.force = explosionForce;
  }
}
