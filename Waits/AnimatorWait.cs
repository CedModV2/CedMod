using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;

namespace Waits
{
    // Token: 0x020003BD RID: 957
    public class AnimatorWait : Wait
    {
        // Token: 0x060017F5 RID: 6133 RVA: 0x00018F11 File Offset: 0x00017111
        public override IEnumerator<float> _Run()
        {
            yield return Timing.WaitUntilFalse(() => this.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f);
            yield break;
        }

        // Token: 0x0400194D RID: 6477
        public Animator animator;
    }
}
