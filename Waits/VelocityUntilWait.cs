// Decompiled with JetBrains decompiler
// Type: Waits.VelocityUntilWait
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

namespace Waits
{
  public class VelocityUntilWait : UntilWait
  {
    public float threshold = 0.05f;
    [NonSerialized]
    private float sqrThreshold;
    public Rigidbody rigidbody;

    protected override void Awake()
    {
      base.Awake();
      this.sqrThreshold = this.threshold * this.threshold;
    }

    protected override bool Predicate()
    {
      return (double) this.rigidbody.velocity.sqrMagnitude < (double) this.sqrThreshold;
    }
  }
}
