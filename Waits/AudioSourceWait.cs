// Decompiled with JetBrains decompiler
// Type: Waits.AudioSourceWait
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Random;

namespace Waits
{
    // Token: 0x020003C0 RID: 960
    public class AudioSourceWait : Wait
    {
        // Token: 0x06001800 RID: 6144 RVA: 0x00018F5C File Offset: 0x0001715C
        public override IEnumerator<float> _Run()
        {
            yield return Timing.WaitUntilFalse(() => this.audioSource.isPlaying);
            yield break;
        }

        // Token: 0x04001952 RID: 6482
        public AudioSource audioSource;
    }
}

namespace Waits
{
  public class AudioSourceWait : Wait
  {
    public AudioSource audioSource;

    public override IEnumerator<float> _Run()
    {
      // ISSUE: reference to a compiler-generated field
      int num = this.<1__>State;
      AudioSourceWait audioSourceWait = this;
      if (num != 0)
      {
        if (num != 1)
          return false;
        // ISSUE: reference to a compiler-generated field
        this.<1__>State = -1;
        return false;
      }
      // ISSUE: reference to a compiler-generated field
      this.<1__>State = -1;
            int __1_0 = 0;
            bool current = false;
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated method
            this.<2__>current = Timing.WaitUntilFalse(new Func<bool>(audioSourceWait.<_Run\> __1_0));
      // ISSUE: reference to a compiler-generated field
      this.<1__>State = 1;
      return true;
    }
  }
}
