// Decompiled with JetBrains decompiler
// Type: ControlPanelLights
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using System.Collections.Generic;
using UnityEngine;

public class ControlPanelLights : MonoBehaviour
{
  public Texture[] emissions;
  public Material targetMat;

  private void Start()
  {
    Timing.RunCoroutine(this._Animate(), Segment.FixedUpdate);
  }

  private IEnumerator<float> _Animate()
  {
    ControlPanelLights controlPanelLights = this;
    int l = controlPanelLights.emissions.Length;
    while ((Object) controlPanelLights != (Object) null)
    {
      if ((Object) controlPanelLights.targetMat != (Object) null)
        controlPanelLights.targetMat.SetTexture("_EmissionMap", controlPanelLights.emissions[Random.Range(0, l)]);
      yield return Timing.WaitForSeconds(Random.Range(0.2f, 0.8f));
    }
  }
}
