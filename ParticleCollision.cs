// Decompiled with JetBrains decompiler
// Type: ParticleCollision
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

public class ParticleCollision : MonoBehaviour
{
  private List<ParticleCollisionEvent> m_CollisionEvents = new List<ParticleCollisionEvent>();
  private ParticleSystem m_ParticleSystem;

  private void Start()
  {
    this.m_ParticleSystem = this.GetComponent<ParticleSystem>();
  }

  private void OnParticleCollision(GameObject other)
  {
    int collisionEvents = this.m_ParticleSystem.GetCollisionEvents(other, this.m_CollisionEvents);
    for (int index = 0; index < collisionEvents; ++index)
    {
      ExtinguishableFire component = this.m_CollisionEvents[index].colliderComponent.GetComponent<ExtinguishableFire>();
      if ((Object) component != (Object) null)
        component.Extinguish();
    }
  }
}
