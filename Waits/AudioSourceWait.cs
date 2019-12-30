using System;
using System.Collections.Generic;
using MEC;
using UnityEngine;

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
