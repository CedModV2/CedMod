using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Grenades.Effects
{
    // Token: 0x0200049F RID: 1183
    public class PhysicsGrenadeEffect : GrenadeEffect
    {
        // Token: 0x06001B7E RID: 7038 RVA: 0x0001AE85 File Offset: 0x00019085
        protected override void Awake()
        {
            base.Awake();
            this.playSegment = Segment.FixedUpdate;
            this.ignoredRigidbodies = new List<Rigidbody>();
        }

        // Token: 0x06001B7F RID: 7039 RVA: 0x0001AE9F File Offset: 0x0001909F
        protected override IEnumerator<float> _Play()
        {
            Vector3 position = base.transform.position;
            Collider[] array = Physics.OverlapSphere(position, this.radius);
            for (int i = 0; i < array.Length; i++)
            {
                Rigidbody attachedRigidbody = array[i].attachedRigidbody;
                if (!(attachedRigidbody == null) && !this.ignoredRigidbodies.Contains(attachedRigidbody))
                {
                    this.ignoredRigidbodies.Add(attachedRigidbody);
                    attachedRigidbody.AddExplosionForce(this.force, position, this.radius, this.lift, ForceMode.Impulse);
                }
            }
            yield break;
        }

        // Token: 0x04001CE0 RID: 7392
        public float force = 4f;

        // Token: 0x04001CE1 RID: 7393
        public float radius = 10.8f;

        // Token: 0x04001CE2 RID: 7394
        public float lift = 10.8f;

        // Token: 0x04001CE3 RID: 7395
        [NonSerialized]
        public List<Rigidbody> ignoredRigidbodies;
    }
}