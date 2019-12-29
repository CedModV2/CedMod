// Decompiled with JetBrains decompiler
// Type: Waits.ParticleSystemWait
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Waits
{
  public class ParticleSystemWait : Wait
  {
    private Func<bool> isAliveDelegate;
    public ParticleSystem particleSystem;

    protected virtual void Awake()
    {
      this.isAliveDelegate = new Func<bool>(this.particleSystem.IsAlive);
    }

    public override IEnumerator<float> _Run()
    {
      yield return Timing.WaitUntilFalse(this.isAliveDelegate);
    }
  }
}
