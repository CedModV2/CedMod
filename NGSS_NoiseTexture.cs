// Decompiled with JetBrains decompiler
// Type: NGSS_NoiseTexture
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

[ExecuteInEditMode]
public class NGSS_NoiseTexture : MonoBehaviour
{
  [Range(0.0f, 1f)]
  public float noiseScale = 1f;
  public Texture noiseTex;
  private bool isTextureSet;

  private void Update()
  {
    Shader.SetGlobalFloat("NGSS_NOISE_TEXTURE_SCALE", this.noiseScale);
    if (this.isTextureSet || (Object) this.noiseTex == (Object) null)
      return;
    Shader.SetGlobalTexture("NGSS_NOISE_TEXTURE", this.noiseTex);
    this.isTextureSet = true;
  }
}
