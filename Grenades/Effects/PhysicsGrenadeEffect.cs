// Decompiled with JetBrains decompiler
// Type: Grenades.Effects.PhysicsGrenadeEffect
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Random;

namespace Grenades.Effects
{
  public class PhysicsGrenadeEffect : GrenadeEffect
  {
    public float force = 4f;
    public float radius = 10.8f;
    public float lift = 10.8f;
    [NonSerialized]
    public List<Rigidbody> ignoredRigidbodies;

    protected override void Awake()
    {
      base.Awake();
      this.playSegment = Segment.FixedUpdate;
      this.ignoredRigidbodies = new List<Rigidbody>();
    }

    protected override IEnumerator<float> _Play()
    {
      // ISSUE: reference to a compiler-generated field
      int num = this.<1__>state;
      PhysicsGrenadeEffect physicsGrenadeEffect = this;
      if (num != 0)
        return false;
      // ISSUE: reference to a compiler-generated field
      this.<1__>State = -1;
      Vector3 position = physicsGrenadeEffect.transform.position;
      foreach (Collider collider in Physics.OverlapSphere(position, physicsGrenadeEffect.radius))
      {
        Rigidbody attachedRigidbody = collider.attachedRigidbody;
        if (!((UnityEngine.Object) attachedRigidbody == (UnityEngine.Object) null) && !physicsGrenadeEffect.ignoredRigidbodies.Contains(attachedRigidbody))
        {
          physicsGrenadeEffect.ignoredRigidbodies.Add(attachedRigidbody);
          attachedRigidbody.AddExplosionForce(physicsGrenadeEffect.force, position, physicsGrenadeEffect.radius, physicsGrenadeEffect.lift, ForceMode.Impulse);
        }
      }
      return false;
    }
  }
}
