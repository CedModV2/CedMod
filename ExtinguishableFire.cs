// Decompiled with JetBrains decompiler
// Type: ExtinguishableFire
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using UnityEngine;

public class ExtinguishableFire : MonoBehaviour
{
  public ParticleSystem fireParticleSystem;
  public ParticleSystem smokeParticleSystem;
  protected bool m_isExtinguished;
  private const float m_FireStartingTime = 2f;

  private void Start()
  {
    this.m_isExtinguished = true;
    this.smokeParticleSystem.Stop();
    this.fireParticleSystem.Stop();
    this.StartCoroutine(this.StartingFire());
  }

  public void Extinguish()
  {
    if (this.m_isExtinguished)
      return;
    this.m_isExtinguished = true;
    this.StartCoroutine(this.Extinguishing());
  }

  private IEnumerator Extinguishing()
  {
    ExtinguishableFire extinguishableFire = this;
    extinguishableFire.fireParticleSystem.Stop();
    extinguishableFire.smokeParticleSystem.time = 0.0f;
    extinguishableFire.smokeParticleSystem.Play();
    for (float elapsedTime = 0.0f; (double) elapsedTime < 2.0; elapsedTime += Time.deltaTime)
    {
      float num = Mathf.Max(0.0f, (float) (1.0 - (double) elapsedTime / 2.0));
      extinguishableFire.fireParticleSystem.transform.localScale = Vector3.one * num;
      yield return (object) null;
    }
    yield return (object) new WaitForSeconds(2f);
    extinguishableFire.smokeParticleSystem.Stop();
    extinguishableFire.fireParticleSystem.transform.localScale = Vector3.one;
    yield return (object) new WaitForSeconds(4f);
    extinguishableFire.StartCoroutine(extinguishableFire.StartingFire());
  }

  private IEnumerator StartingFire()
  {
    this.smokeParticleSystem.Stop();
    this.fireParticleSystem.time = 0.0f;
    this.fireParticleSystem.Play();
    for (float elapsedTime = 0.0f; (double) elapsedTime < 2.0; elapsedTime += Time.deltaTime)
    {
      this.fireParticleSystem.transform.localScale = Vector3.one * Mathf.Min(1f, elapsedTime / 2f);
      yield return (object) null;
    }
    this.fireParticleSystem.transform.localScale = Vector3.one;
    this.m_isExtinguished = false;
  }
}
