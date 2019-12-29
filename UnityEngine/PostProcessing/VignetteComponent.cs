// Decompiled with JetBrains decompiler
// Type: UnityEngine.PostProcessing.VignetteComponent
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace UnityEngine.PostProcessing
{
  public sealed class VignetteComponent : PostProcessingComponentRenderTexture<VignetteModel>
  {
    public override bool active
    {
      get
      {
        return this.model.enabled && !this.context.interrupted;
      }
    }

    public override void Prepare(Material uberMaterial)
    {
      VignetteModel.Settings settings = this.model.settings;
      uberMaterial.SetColor(VignetteComponent.Uniforms._Vignette_Color, settings.color);
      switch (settings.mode)
      {
        case VignetteModel.Mode.Classic:
          uberMaterial.SetVector(VignetteComponent.Uniforms._Vignette_Center, (Vector4) settings.center);
          uberMaterial.EnableKeyword("VIGNETTE_CLASSIC");
          float z = (float) ((1.0 - (double) settings.roundness) * 6.0) + settings.roundness;
          uberMaterial.SetVector(VignetteComponent.Uniforms._Vignette_Settings, new Vector4(settings.intensity * 3f, settings.smoothness * 5f, z, settings.rounded ? 1f : 0.0f));
          break;
        case VignetteModel.Mode.Masked:
          if (!((Object) settings.mask != (Object) null) || (double) settings.opacity <= 0.0)
            break;
          uberMaterial.EnableKeyword("VIGNETTE_MASKED");
          uberMaterial.SetTexture(VignetteComponent.Uniforms._Vignette_Mask, settings.mask);
          uberMaterial.SetFloat(VignetteComponent.Uniforms._Vignette_Opacity, settings.opacity);
          break;
      }
    }

    private static class Uniforms
    {
      internal static readonly int _Vignette_Color = Shader.PropertyToID(nameof (_Vignette_Color));
      internal static readonly int _Vignette_Center = Shader.PropertyToID(nameof (_Vignette_Center));
      internal static readonly int _Vignette_Settings = Shader.PropertyToID(nameof (_Vignette_Settings));
      internal static readonly int _Vignette_Mask = Shader.PropertyToID(nameof (_Vignette_Mask));
      internal static readonly int _Vignette_Opacity = Shader.PropertyToID(nameof (_Vignette_Opacity));
    }
  }
}
