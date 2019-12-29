// Decompiled with JetBrains decompiler
// Type: TextureAnimator
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using System.Collections.Generic;
using UnityEngine;

public class TextureAnimator : MonoBehaviour
{
  public Material[] textures;
  public Renderer targetRenderer;
  public float cooldown;
  public Light optionalLight;
  public int lightRange;

  private void Start()
  {
    Timing.RunCoroutine(this._Animate(), Segment.FixedUpdate);
  }

  private IEnumerator<float> _Animate()
  {
    TextureAnimator textureAnimator = this;
    while ((Object) textureAnimator != (Object) null)
    {
      for (int i = 0; i < textureAnimator.textures.Length; ++i)
      {
        textureAnimator.optionalLight.enabled = i < textureAnimator.lightRange;
        textureAnimator.targetRenderer.material = textureAnimator.textures[i];
        for (int x = 0; (double) x < 50.0 * (double) textureAnimator.cooldown; ++x)
          yield return 0.0f;
      }
    }
  }
}
