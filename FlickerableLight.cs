// Decompiled with JetBrains decompiler
// Type: FlickerableLight
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

public class FlickerableLight : MonoBehaviour
{
  private bool _enabled;
  private float remainingFlicker;
  private float curAnimationProgress;

  private void Start()
  {
  }

  private void Update()
  {
    if ((double) this.remainingFlicker > 0.0)
    {
      this._enabled = true;
      this.curAnimationProgress += Time.deltaTime;
      this.curAnimationProgress = Mathf.Clamp01(this.curAnimationProgress);
      this.remainingFlicker -= Time.deltaTime;
      if ((double) this.remainingFlicker > 0.0)
        return;
      this.curAnimationProgress = 0.0f;
    }
    else
    {
      if (!this._enabled)
        return;
      this.curAnimationProgress += Time.deltaTime;
      this.curAnimationProgress = Mathf.Clamp01(this.curAnimationProgress);
      if ((double) Math.Abs(this.curAnimationProgress - 1f) >= 0.00499999988824129)
        return;
      this._enabled = false;
    }
  }

  public bool IsDisabled()
  {
    return (double) Math.Abs(this.curAnimationProgress - 1f) < 0.00499999988824129 && this._enabled;
  }

  public bool EnableFlickering(float dur)
  {
    if ((double) this.remainingFlicker > 0.0)
      return false;
    this.remainingFlicker = dur;
    this.curAnimationProgress = 0.0f;
    return true;
  }
}
