// Decompiled with JetBrains decompiler
// Type: ExplosionPhysicsForce
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Effects;

public class ExplosionPhysicsForce : MonoBehaviour
{
  public float explosionForce = 4f;

  private IEnumerator Start()
  {
    ExplosionPhysicsForce explosionPhysicsForce = this;
    yield return (object) null;
    float num1 = 0.0f;
    if ((Object) explosionPhysicsForce.GetComponent<ParticleSystemMultiplier>() != (Object) null)
      num1 = explosionPhysicsForce.GetComponent<ParticleSystemMultiplier>().multiplier;
    float num2 = 10f * num1;
    Collider[] colliderArray = Physics.OverlapSphere(explosionPhysicsForce.transform.position, num2);
    List<Rigidbody> rigidbodyList = new List<Rigidbody>();
    foreach (Collider collider in colliderArray)
    {
      if ((Object) collider.attachedRigidbody != (Object) null && !rigidbodyList.Contains(collider.attachedRigidbody))
        rigidbodyList.Add(collider.attachedRigidbody);
    }
    foreach (Rigidbody rigidbody in rigidbodyList)
      rigidbody.AddExplosionForce(explosionPhysicsForce.explosionForce * num1, explosionPhysicsForce.transform.position, num2, 1f * num1, ForceMode.Impulse);
  }
}
