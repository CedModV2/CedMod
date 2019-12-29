// Decompiled with JetBrains decompiler
// Type: UnityEngine.PostProcessing.DitheringComponent
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace UnityEngine.PostProcessing
{
  public sealed class DitheringComponent : PostProcessingComponentRenderTexture<DitheringModel>
  {
    private const int k_TextureCount = 64;
    private Texture2D[] noiseTextures;
    private int textureIndex;

    public override bool active
    {
      get
      {
        return this.model.enabled && !this.context.interrupted;
      }
    }

    public override void OnDisable()
    {
      this.noiseTextures = (Texture2D[]) null;
    }

    private void LoadNoiseTextures()
    {
      this.noiseTextures = new Texture2D[64];
      for (int index = 0; index < 64; ++index)
        this.noiseTextures[index] = Resources.Load<Texture2D>("Bluenoise64/LDR_LLL1_" + (object) index);
    }

    public override void Prepare(Material uberMaterial)
    {
      if (++this.textureIndex >= 64)
        this.textureIndex = 0;
      float z = Random.value;
      float w = Random.value;
      if (this.noiseTextures == null)
        this.LoadNoiseTextures();
      Texture2D noiseTexture = this.noiseTextures[this.textureIndex];
      uberMaterial.EnableKeyword("DITHERING");
      uberMaterial.SetTexture(DitheringComponent.Uniforms._DitheringTex, (Texture) noiseTexture);
      uberMaterial.SetVector(DitheringComponent.Uniforms._DitheringCoords, new Vector4((float) this.context.width / (float) noiseTexture.width, (float) this.context.height / (float) noiseTexture.height, z, w));
    }

    private static class Uniforms
    {
      internal static readonly int _DitheringTex = Shader.PropertyToID(nameof (_DitheringTex));
      internal static readonly int _DitheringCoords = Shader.PropertyToID(nameof (_DitheringCoords));
    }
  }
}
