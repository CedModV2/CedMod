// Decompiled with JetBrains decompiler
// Type: Grenades.Effects.GrenadeEffect
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using System.Collections.Generic;
using UnityEngine;
using Waits;

namespace Grenades.Effects
{
  public class GrenadeEffect : MonoBehaviour
  {
    private WaitManager wait;
    protected Segment playSegment;

    protected virtual void Awake()
    {
      this.wait = this.GetComponent<WaitManager>();
      this.playSegment = Segment.Update;
    }

    protected virtual void Start()
    {
      IEnumerator<float> coroutine = this._Play();
      Timing.RunCoroutine(this._Destroy(coroutine == null ? new CoroutineHandle() : Timing.RunCoroutine(coroutine, this.playSegment)));
    }

    protected virtual IEnumerator<float> _Play()
    {
      return (IEnumerator<float>) null;
    }

    private IEnumerator<float> _Destroy(CoroutineHandle playHandle)
    {
      GrenadeEffect grenadeEffect = this;
      if (playHandle.IsValid)
        yield return Timing.WaitUntilDone(playHandle);
      if ((Object) grenadeEffect.wait != (Object) null)
        yield return Timing.WaitUntilDone(grenadeEffect.wait._Run());
      Object.Destroy((Object) grenadeEffect.gameObject);
    }
  }
}
