// Decompiled with JetBrains decompiler
// Type: Waits.OrWaitManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Waits
{
  public class OrWaitManager : UntilWaitManager
  {
    protected override bool KeepRunning()
    {
      return ((IEnumerable<CoroutineHandle>) this.waitHandles).Any<CoroutineHandle>((Func<CoroutineHandle, bool>) (x => x.IsRunning));
    }
  }
}
