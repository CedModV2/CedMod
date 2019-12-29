// Decompiled with JetBrains decompiler
// Type: Waits.UntilWaitManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using System;
using System.Collections.Generic;

namespace Waits
{
  public abstract class UntilWaitManager : WaitManager
  {
    protected Func<bool> allocatedKeepRunning;

    protected override void Awake()
    {
      base.Awake();
      this.allocatedKeepRunning = new Func<bool>(this.KeepRunning);
    }

    protected abstract bool KeepRunning();

    public override IEnumerator<float> _Run()
    {
      UntilWaitManager untilWaitManager = this;
      untilWaitManager.StartAll();
      yield return float.NegativeInfinity;
      yield return Timing.WaitUntilFalse(untilWaitManager.allocatedKeepRunning);
    }
  }
}
