// Decompiled with JetBrains decompiler
// Type: NGSS_Directional
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof (Light))]
[ExecuteInEditMode]
public class NGSS_Directional : MonoBehaviour
{
  [Tooltip("Optimize shadows performance by skipping fragments that are 100% lit or 100% shadowed. Some tiny noisy artefacts can be seen if shadows are too soft.")]
  public bool EARLY_BAILOUT_OPTIMIZATION = true;
  [Tooltip("Provides Area Light like soft-shadows. With shadows being harder at close ranges and softer at long ranges.\nDisable it if you are looking for uniformly simple soft-shadows.")]
  public bool PCSS_ENABLED = true;
  private bool PCSS_SWITCH = true;
  [Tooltip("Overall softness for both PCF and PCSS shadows.\nRecommended value: 0.01.")]
  [Range(0.0f, 0.02f)]
  public float PCSS_GLOBAL_SOFTNESS = 0.01f;
  [Tooltip("PCSS softness when shadows is close to caster.\nRecommended value: 0.05.")]
  [Range(0.0f, 1f)]
  public float PCSS_FILTER_DIR_MIN = 0.05f;
  [Tooltip("PCSS softness when shadows is far from caster.\nRecommended value: 0.25.\nIf too high can lead to visible artifacts when early bailout is enabled.")]
  [Range(0.0f, 0.5f)]
  public float PCSS_FILTER_DIR_MAX = 0.25f;
  [Tooltip("Amount of banding or noise. Example: 0.0 gives 100 % Banding and 10.0 gives 100 % Noise.")]
  [Range(0.0f, 10f)]
  public float BANDING_NOISE_AMOUNT = 1f;
  [Tooltip("Recommended values: Mobile = 16, Consoles = 25, Desktop Low = 32, Desktop High = 64")]
  public NGSS_Directional.SAMPLER_COUNT SAMPLERS_COUNT = NGSS_Directional.SAMPLER_COUNT.SAMPLERS_64;
  [Tooltip("Help with bias problems but can leads to some noisy artefacts if Early Bailout Optimization is enabled.\nRequires PCSS in order to work.")]
  public bool USE_BIAS_FADE;
  private bool isInitialized;
  private bool isGraphicSet;
  private Light m_Light;
  private CommandBuffer rawShadowDepthCB;
  private RenderTexture m_ShadowmapCopy;

  private void OnDestroy()
  {
    if (this.isGraphicSet)
    {
      this.isGraphicSet = false;
      GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows, Shader.Find("Hidden/Internal-ScreenSpaceShadows"));
      GraphicsSettings.SetShaderMode(BuiltinShaderType.ScreenSpaceShadows, BuiltinShaderMode.UseBuiltin);
    }
    this.RemoveCommandBuffers();
  }

  private void OnDisable()
  {
    if (this.isGraphicSet)
    {
      this.isGraphicSet = false;
      GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows, Shader.Find("Hidden/Internal-ScreenSpaceShadows"));
      GraphicsSettings.SetShaderMode(BuiltinShaderType.ScreenSpaceShadows, BuiltinShaderMode.UseBuiltin);
    }
    this.RemoveCommandBuffers();
  }

  private void OnApplicationQuit()
  {
    if (this.isGraphicSet)
    {
      this.isGraphicSet = false;
      GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows, Shader.Find("Hidden/Internal-ScreenSpaceShadows"));
      GraphicsSettings.SetShaderMode(BuiltinShaderType.ScreenSpaceShadows, BuiltinShaderMode.UseBuiltin);
    }
    this.RemoveCommandBuffers();
  }

  private void RemoveCommandBuffers()
  {
    if (!this.isInitialized)
      return;
    this.m_Light.RemoveCommandBuffer(LightEvent.AfterShadowMap, this.rawShadowDepthCB);
    this.m_ShadowmapCopy = (RenderTexture) null;
    this.isInitialized = false;
  }

  private void OnEnable()
  {
    this.Init();
  }

  private void Init()
  {
    if (this.isInitialized)
      return;
    if (!this.isGraphicSet)
    {
      this.isGraphicSet = true;
      GraphicsSettings.SetShaderMode(BuiltinShaderType.ScreenSpaceShadows, BuiltinShaderMode.UseCustom);
      GraphicsSettings.SetCustomShader(BuiltinShaderType.ScreenSpaceShadows, Shader.Find("Hidden/NGSS_Directional"));
    }
    if (!this.PCSS_ENABLED)
      return;
    this.m_Light = this.GetComponent<Light>();
    int num1;
    switch (QualitySettings.shadowResolution)
    {
      case ShadowResolution.Medium:
        num1 = 1024;
        break;
      case ShadowResolution.High:
        num1 = 2048;
        break;
      case ShadowResolution.VeryHigh:
        num1 = 4096;
        break;
      default:
        num1 = 512;
        break;
    }
    int num2 = num1;
    this.m_ShadowmapCopy = (RenderTexture) null;
    this.m_ShadowmapCopy = new RenderTexture(num2, num2, 0, RenderTextureFormat.RFloat);
    this.m_ShadowmapCopy.filterMode = FilterMode.Bilinear;
    this.m_ShadowmapCopy.useMipMap = false;
    this.rawShadowDepthCB = new CommandBuffer()
    {
      name = "NGSS Directional PCSS buffer"
    };
    this.rawShadowDepthCB.Clear();
    this.rawShadowDepthCB.SetShadowSamplingMode((RenderTargetIdentifier) BuiltinRenderTextureType.CurrentActive, ShadowSamplingMode.RawDepth);
    this.rawShadowDepthCB.Blit((RenderTargetIdentifier) BuiltinRenderTextureType.CurrentActive, (RenderTargetIdentifier) (Texture) this.m_ShadowmapCopy);
    this.rawShadowDepthCB.SetGlobalTexture("NGSS_DirectionalRawDepth", (RenderTargetIdentifier) (Texture) this.m_ShadowmapCopy);
    foreach (CommandBuffer commandBuffer in this.m_Light.GetCommandBuffers(LightEvent.AfterShadowMap))
    {
      if (commandBuffer.name == this.rawShadowDepthCB.name)
      {
        this.isInitialized = true;
        return;
      }
    }
    this.m_Light.AddCommandBuffer(LightEvent.AfterShadowMap, this.rawShadowDepthCB);
    this.isInitialized = true;
  }

  private void Update()
  {
    if (this.PCSS_ENABLED != this.PCSS_SWITCH)
    {
      this.PCSS_SWITCH = !this.PCSS_SWITCH;
      if (this.PCSS_ENABLED)
      {
        this.isInitialized = false;
        this.Init();
      }
      else
        this.RemoveCommandBuffers();
    }
    this.SetGlobalSettings();
  }

  private void SetGlobalSettings()
  {
    Shader.SetGlobalFloat("NGSS_PCSS_GLOBAL_SOFTNESS", this.PCSS_GLOBAL_SOFTNESS);
    Shader.SetGlobalFloat("NGSS_PCSS_FILTER_DIR_MIN", (double) this.PCSS_FILTER_DIR_MIN > (double) this.PCSS_FILTER_DIR_MAX ? this.PCSS_FILTER_DIR_MAX : this.PCSS_FILTER_DIR_MIN);
    Shader.SetGlobalFloat("NGSS_PCSS_FILTER_DIR_MAX", (double) this.PCSS_FILTER_DIR_MAX < (double) this.PCSS_FILTER_DIR_MIN ? this.PCSS_FILTER_DIR_MIN : this.PCSS_FILTER_DIR_MAX);
    Shader.SetGlobalFloat("NGSS_POISSON_SAMPLING_NOISE_DIR", this.BANDING_NOISE_AMOUNT);
    if (this.PCSS_ENABLED)
      Shader.EnableKeyword("NGSS_PCSS_FILTER_DIR");
    else
      Shader.DisableKeyword("NGSS_PCSS_FILTER_DIR");
    if (this.EARLY_BAILOUT_OPTIMIZATION)
      Shader.EnableKeyword("NGSS_USE_EARLY_BAILOUT_OPTIMIZATION_DIR");
    else
      Shader.DisableKeyword("NGSS_USE_EARLY_BAILOUT_OPTIMIZATION_DIR");
    if (this.USE_BIAS_FADE)
      Shader.EnableKeyword("NGSS_USE_BIAS_FADE_DIR");
    else
      Shader.DisableKeyword("NGSS_USE_BIAS_FADE_DIR");
    Shader.DisableKeyword("DIR_POISSON_64");
    Shader.DisableKeyword("DIR_POISSON_32");
    Shader.DisableKeyword("DIR_POISSON_25");
    Shader.DisableKeyword("DIR_POISSON_16");
    Shader.EnableKeyword(this.SAMPLERS_COUNT == NGSS_Directional.SAMPLER_COUNT.SAMPLERS_64 ? "DIR_POISSON_64" : (this.SAMPLERS_COUNT == NGSS_Directional.SAMPLER_COUNT.SAMPLERS_32 ? "DIR_POISSON_32" : (this.SAMPLERS_COUNT == NGSS_Directional.SAMPLER_COUNT.SAMPLERS_25 ? "DIR_POISSON_25" : "DIR_POISSON_16")));
  }

  public enum SAMPLER_COUNT
  {
    SAMPLERS_16,
    SAMPLERS_25,
    SAMPLERS_32,
    SAMPLERS_64,
  }
}
