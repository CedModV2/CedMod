// Decompiled with JetBrains decompiler
// Type: UnityEngine.PostProcessing.BloomComponent
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace UnityEngine.PostProcessing
{
  public sealed class BloomComponent : PostProcessingComponentRenderTexture<BloomModel>
  {
    private readonly RenderTexture[] m_BlurBuffer1 = new RenderTexture[16];
    private readonly RenderTexture[] m_BlurBuffer2 = new RenderTexture[16];
    private const int k_MaxPyramidBlurLevel = 16;

    public override bool active
    {
      get
      {
        return this.model.enabled && (double) this.model.settings.bloom.intensity > 0.0 && !this.context.interrupted;
      }
    }

    public void Prepare(RenderTexture source, Material uberMaterial, Texture autoExposure)
    {
      BloomModel.BloomSettings bloom = this.model.settings.bloom;
      BloomModel.LensDirtSettings lensDirt = this.model.settings.lensDirt;
      Material mat = this.context.materialFactory.Get("Hidden/Post FX/Bloom");
      mat.shaderKeywords = (string[]) null;
      mat.SetTexture(BloomComponent.Uniforms._AutoExposure, autoExposure);
      int width = this.context.width / 2;
      int height = this.context.height / 2;
      RenderTextureFormat format = Application.isMobilePlatform ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR;
      float num1 = (float) ((double) Mathf.Log((float) height, 2f) + (double) bloom.radius - 8.0);
      int num2 = (int) num1;
      int num3 = Mathf.Clamp(num2, 1, 16);
      float thresholdLinear = bloom.thresholdLinear;
      mat.SetFloat(BloomComponent.Uniforms._Threshold, thresholdLinear);
      float num4 = (float) ((double) thresholdLinear * (double) bloom.softKnee + 9.99999974737875E-06);
      Vector3 vector3 = new Vector3(thresholdLinear - num4, num4 * 2f, 0.25f / num4);
      mat.SetVector(BloomComponent.Uniforms._Curve, (Vector4) vector3);
      mat.SetFloat(BloomComponent.Uniforms._PrefilterOffs, bloom.antiFlicker ? -0.5f : 0.0f);
      float x = 0.5f + num1 - (float) num2;
      mat.SetFloat(BloomComponent.Uniforms._SampleScale, x);
      if (bloom.antiFlicker)
        mat.EnableKeyword("ANTI_FLICKER");
      RenderTexture renderTexture1 = this.context.renderTextureFactory.Get(width, height, 0, format, RenderTextureReadWrite.Default, FilterMode.Bilinear, TextureWrapMode.Clamp, "FactoryTempTexture");
      Graphics.Blit((Texture) source, renderTexture1, mat, 0);
      RenderTexture renderTexture2 = renderTexture1;
      for (int index = 0; index < num3; ++index)
      {
        this.m_BlurBuffer1[index] = this.context.renderTextureFactory.Get(renderTexture2.width / 2, renderTexture2.height / 2, 0, format, RenderTextureReadWrite.Default, FilterMode.Bilinear, TextureWrapMode.Clamp, "FactoryTempTexture");
        int pass = index == 0 ? 1 : 2;
        Graphics.Blit((Texture) renderTexture2, this.m_BlurBuffer1[index], mat, pass);
        renderTexture2 = this.m_BlurBuffer1[index];
      }
      for (int index = num3 - 2; index >= 0; --index)
      {
        RenderTexture renderTexture3 = this.m_BlurBuffer1[index];
        mat.SetTexture(BloomComponent.Uniforms._BaseTex, (Texture) renderTexture3);
        this.m_BlurBuffer2[index] = this.context.renderTextureFactory.Get(renderTexture3.width, renderTexture3.height, 0, format, RenderTextureReadWrite.Default, FilterMode.Bilinear, TextureWrapMode.Clamp, "FactoryTempTexture");
        Graphics.Blit((Texture) renderTexture2, this.m_BlurBuffer2[index], mat, 3);
        renderTexture2 = this.m_BlurBuffer2[index];
      }
      RenderTexture renderTexture4 = renderTexture2;
      for (int index = 0; index < 16; ++index)
      {
        if ((Object) this.m_BlurBuffer1[index] != (Object) null)
          this.context.renderTextureFactory.Release(this.m_BlurBuffer1[index]);
        if ((Object) this.m_BlurBuffer2[index] != (Object) null && (Object) this.m_BlurBuffer2[index] != (Object) renderTexture4)
          this.context.renderTextureFactory.Release(this.m_BlurBuffer2[index]);
        this.m_BlurBuffer1[index] = (RenderTexture) null;
        this.m_BlurBuffer2[index] = (RenderTexture) null;
      }
      this.context.renderTextureFactory.Release(renderTexture1);
      uberMaterial.SetTexture(BloomComponent.Uniforms._BloomTex, (Texture) renderTexture4);
      uberMaterial.SetVector(BloomComponent.Uniforms._Bloom_Settings, (Vector4) new Vector2(x, bloom.intensity));
      if ((double) lensDirt.intensity > 0.0 && (Object) lensDirt.texture != (Object) null)
      {
        uberMaterial.SetTexture(BloomComponent.Uniforms._Bloom_DirtTex, lensDirt.texture);
        uberMaterial.SetFloat(BloomComponent.Uniforms._Bloom_DirtIntensity, lensDirt.intensity);
        uberMaterial.EnableKeyword("BLOOM_LENS_DIRT");
      }
      else
        uberMaterial.EnableKeyword("BLOOM");
    }

    private static class Uniforms
    {
      internal static readonly int _AutoExposure = Shader.PropertyToID(nameof (_AutoExposure));
      internal static readonly int _Threshold = Shader.PropertyToID(nameof (_Threshold));
      internal static readonly int _Curve = Shader.PropertyToID(nameof (_Curve));
      internal static readonly int _PrefilterOffs = Shader.PropertyToID(nameof (_PrefilterOffs));
      internal static readonly int _SampleScale = Shader.PropertyToID(nameof (_SampleScale));
      internal static readonly int _BaseTex = Shader.PropertyToID(nameof (_BaseTex));
      internal static readonly int _BloomTex = Shader.PropertyToID(nameof (_BloomTex));
      internal static readonly int _Bloom_Settings = Shader.PropertyToID(nameof (_Bloom_Settings));
      internal static readonly int _Bloom_DirtTex = Shader.PropertyToID(nameof (_Bloom_DirtTex));
      internal static readonly int _Bloom_DirtIntensity = Shader.PropertyToID(nameof (_Bloom_DirtIntensity));
    }
  }
}
