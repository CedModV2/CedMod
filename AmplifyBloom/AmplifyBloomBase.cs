// Decompiled with JetBrains decompiler
// Type: AmplifyBloom.AmplifyBloomBase
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace AmplifyBloom
{
  [AddComponentMenu("")]
  [Serializable]
  public class AmplifyBloomBase : MonoBehaviour
  {
    [SerializeField]
    private bool m_showDebugMessages = true;
    [SerializeField]
    private int m_softMaxdownscales = 6;
    [SerializeField]
    private Vector4 m_bloomRange = new Vector4(500f, 1f, 0.0f, 0.0f);
    [SerializeField]
    private float m_overallThreshold = 0.53f;
    [SerializeField]
    private Vector4 m_bloomParams = new Vector4(0.8f, 1f, 1f, 1f);
    [SerializeField]
    private float m_temporalFilteringValue = 0.05f;
    [SerializeField]
    private int m_bloomDownsampleCount = 6;
    [SerializeField]
    private float m_featuresThreshold = 0.05f;
    [SerializeField]
    private AmplifyLensFlare m_lensFlare = new AmplifyLensFlare();
    [SerializeField]
    private bool m_applyLensDirt = true;
    [SerializeField]
    private float m_lensDirtStrength = 2f;
    [SerializeField]
    private bool m_applyLensStardurst = true;
    [SerializeField]
    private float m_lensStarburstStrength = 2f;
    [SerializeField]
    private AmplifyGlare m_anamorphicGlare = new AmplifyGlare();
    [SerializeField]
    private AmplifyBokeh m_bokehFilter = new AmplifyBokeh();
    [SerializeField]
    private float[] m_upscaleWeights = new float[6]
    {
      0.0842f,
      0.1282f,
      0.1648f,
      0.2197f,
      0.2197f,
      0.1831f
    };
    [SerializeField]
    private float[] m_gaussianRadius = new float[6]
    {
      1f,
      1f,
      1f,
      1f,
      1f,
      1f
    };
    [SerializeField]
    private int[] m_gaussianSteps = new int[6]
    {
      1,
      1,
      1,
      1,
      1,
      1
    };
    [SerializeField]
    private float[] m_lensDirtWeights = new float[6]
    {
      0.067f,
      0.102f,
      0.1311f,
      0.1749f,
      0.2332f,
      0.3f
    };
    [SerializeField]
    private float[] m_lensStarburstWeights = new float[6]
    {
      0.067f,
      0.102f,
      0.1311f,
      0.1749f,
      0.2332f,
      0.3f
    };
    [SerializeField]
    private bool[] m_downscaleSettingsFoldout = new bool[6];
    private RenderTexture[] m_tempUpscaleRTs = new RenderTexture[6];
    private RenderTexture[] m_tempAuxDownsampleRTs = new RenderTexture[6];
    private Vector2[] m_tempDownsamplesSizes = new Vector2[6];
    public const int MaxGhosts = 5;
    public const int MinDownscales = 1;
    public const int MaxDownscales = 6;
    public const int MaxGaussian = 8;
    private const float MaxDirtIntensity = 1f;
    private const float MaxStarburstIntensity = 1f;
    [SerializeField]
    private Texture m_maskTexture;
    [SerializeField]
    private RenderTexture m_targetTexture;
    [SerializeField]
    private DebugToScreenEnum m_debugToScreen;
    [SerializeField]
    private bool m_highPrecision;
    [SerializeField]
    private bool m_temporalFilteringActive;
    [SerializeField]
    private AnimationCurve m_temporalFilteringCurve;
    [SerializeField]
    private bool m_separateFeaturesThreshold;
    [SerializeField]
    private Texture m_lensDirtTexture;
    [SerializeField]
    private Texture m_lensStardurstTex;
    [SerializeField]
    private int m_featuresSourceId;
    [SerializeField]
    private UpscaleQualityEnum m_upscaleQuality;
    [SerializeField]
    private MainThresholdSizeEnum m_mainThresholdSize;
    private Transform m_cameraTransform;
    private Matrix4x4 m_starburstMat;
    private Shader m_bloomShader;
    private Material m_bloomMaterial;
    private Shader m_finalCompositionShader;
    private Material m_finalCompositionMaterial;
    private RenderTexture m_tempFilterBuffer;
    private Camera m_camera;
    private bool silentError;

    private void Awake()
    {
      if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
      {
        AmplifyUtils.DebugLog("Null graphics device detected. Skipping effect silently.", LogType.Error);
        this.silentError = true;
      }
      else
      {
        if (!AmplifyUtils.IsInitialized)
          AmplifyUtils.InitializeIds();
        this.m_anamorphicGlare.Init();
        this.m_lensFlare.Init();
        for (int index = 0; index < 6; ++index)
          this.m_tempDownsamplesSizes[index] = new Vector2(0.0f, 0.0f);
        this.m_cameraTransform = this.transform;
        this.m_tempFilterBuffer = (RenderTexture) null;
        this.m_starburstMat = Matrix4x4.identity;
        if (this.m_temporalFilteringCurve == null)
          this.m_temporalFilteringCurve = new AnimationCurve(new Keyframe[2]
          {
            new Keyframe(0.0f, 0.0f),
            new Keyframe(1f, 0.999f)
          });
        this.m_bloomShader = Shader.Find("Hidden/AmplifyBloom");
        if ((UnityEngine.Object) this.m_bloomShader != (UnityEngine.Object) null)
        {
          this.m_bloomMaterial = new Material(this.m_bloomShader);
          this.m_bloomMaterial.hideFlags = HideFlags.DontSave;
        }
        else
        {
          AmplifyUtils.DebugLog("Main Bloom shader not found", LogType.Error);
          this.gameObject.SetActive(false);
        }
        this.m_finalCompositionShader = Shader.Find("Hidden/BloomFinal");
        if ((UnityEngine.Object) this.m_finalCompositionShader != (UnityEngine.Object) null)
        {
          this.m_finalCompositionMaterial = new Material(this.m_finalCompositionShader);
          if (!this.m_finalCompositionMaterial.GetTag(AmplifyUtils.ShaderModeTag, false).Equals(AmplifyUtils.ShaderModeValue))
          {
            if (this.m_showDebugMessages)
              AmplifyUtils.DebugLog("Amplify Bloom is running on a limited hardware and may lead to a decrease on its visual quality.", LogType.Warning);
          }
          else
            this.m_softMaxdownscales = 6;
          this.m_finalCompositionMaterial.hideFlags = HideFlags.DontSave;
          if ((UnityEngine.Object) this.m_lensDirtTexture == (UnityEngine.Object) null)
            this.m_lensDirtTexture = this.m_finalCompositionMaterial.GetTexture(AmplifyUtils.LensDirtRTId);
          if ((UnityEngine.Object) this.m_lensStardurstTex == (UnityEngine.Object) null)
            this.m_lensStardurstTex = this.m_finalCompositionMaterial.GetTexture(AmplifyUtils.LensStarburstRTId);
        }
        else
        {
          AmplifyUtils.DebugLog("Bloom Composition shader not found", LogType.Error);
          this.gameObject.SetActive(false);
        }
        this.m_camera = this.GetComponent<Camera>();
        this.m_camera.depthTextureMode |= DepthTextureMode.Depth;
        this.m_lensFlare.CreateLUTexture();
      }
    }

    private void OnDestroy()
    {
      if (this.m_bokehFilter != null)
      {
        this.m_bokehFilter.Destroy();
        this.m_bokehFilter = (AmplifyBokeh) null;
      }
      if (this.m_anamorphicGlare != null)
      {
        this.m_anamorphicGlare.Destroy();
        this.m_anamorphicGlare = (AmplifyGlare) null;
      }
      if (this.m_lensFlare == null)
        return;
      this.m_lensFlare.Destroy();
      this.m_lensFlare = (AmplifyLensFlare) null;
    }

    private void ApplyGaussianBlur(
      RenderTexture renderTexture,
      int amount,
      float radius = 1f,
      bool applyTemporal = false)
    {
      if (amount == 0)
        return;
      this.m_bloomMaterial.SetFloat(AmplifyUtils.BlurRadiusId, radius);
      RenderTexture tempRenderTarget = AmplifyUtils.GetTempRenderTarget(renderTexture.width, renderTexture.height);
      for (int index = 0; index < amount; ++index)
      {
        tempRenderTarget.DiscardContents();
        Graphics.Blit((Texture) renderTexture, tempRenderTarget, this.m_bloomMaterial, 14);
        if (this.m_temporalFilteringActive & applyTemporal && index == amount - 1)
        {
          if ((UnityEngine.Object) this.m_tempFilterBuffer != (UnityEngine.Object) null && this.m_temporalFilteringActive)
          {
            float num = this.m_temporalFilteringCurve.Evaluate(this.m_temporalFilteringValue);
            this.m_bloomMaterial.SetFloat(AmplifyUtils.TempFilterValueId, num);
            this.m_bloomMaterial.SetTexture(AmplifyUtils.AnamorphicRTS[0], (Texture) this.m_tempFilterBuffer);
            renderTexture.DiscardContents();
            Graphics.Blit((Texture) tempRenderTarget, renderTexture, this.m_bloomMaterial, 16);
          }
          else
          {
            renderTexture.DiscardContents();
            Graphics.Blit((Texture) tempRenderTarget, renderTexture, this.m_bloomMaterial, 15);
          }
          bool flag = false;
          if ((UnityEngine.Object) this.m_tempFilterBuffer != (UnityEngine.Object) null)
          {
            if (this.m_tempFilterBuffer.format != renderTexture.format || this.m_tempFilterBuffer.width != renderTexture.width || this.m_tempFilterBuffer.height != renderTexture.height)
            {
              this.CleanTempFilterRT();
              flag = true;
            }
          }
          else
            flag = true;
          if (flag)
            this.CreateTempFilterRT(renderTexture);
          this.m_tempFilterBuffer.DiscardContents();
          Graphics.Blit((Texture) renderTexture, this.m_tempFilterBuffer);
        }
        else
        {
          renderTexture.DiscardContents();
          Graphics.Blit((Texture) tempRenderTarget, renderTexture, this.m_bloomMaterial, 15);
        }
      }
      AmplifyUtils.ReleaseTempRenderTarget(tempRenderTarget);
    }

    private void CreateTempFilterRT(RenderTexture source)
    {
      if ((UnityEngine.Object) this.m_tempFilterBuffer != (UnityEngine.Object) null)
        this.CleanTempFilterRT();
      this.m_tempFilterBuffer = new RenderTexture(source.width, source.height, 0, source.format, AmplifyUtils.CurrentReadWriteMode);
      this.m_tempFilterBuffer.filterMode = AmplifyUtils.CurrentFilterMode;
      this.m_tempFilterBuffer.wrapMode = AmplifyUtils.CurrentWrapMode;
      this.m_tempFilterBuffer.Create();
    }

    private void CleanTempFilterRT()
    {
      if (!((UnityEngine.Object) this.m_tempFilterBuffer != (UnityEngine.Object) null))
        return;
      RenderTexture.active = (RenderTexture) null;
      this.m_tempFilterBuffer.Release();
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object) this.m_tempFilterBuffer);
      this.m_tempFilterBuffer = (RenderTexture) null;
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
      if (this.silentError)
        return;
      if (!AmplifyUtils.IsInitialized)
        AmplifyUtils.InitializeIds();
      if (this.m_highPrecision)
      {
        AmplifyUtils.EnsureKeywordEnabled(this.m_bloomMaterial, AmplifyUtils.HighPrecisionKeyword, true);
        AmplifyUtils.EnsureKeywordEnabled(this.m_finalCompositionMaterial, AmplifyUtils.HighPrecisionKeyword, true);
        AmplifyUtils.CurrentRTFormat = RenderTextureFormat.DefaultHDR;
      }
      else
      {
        AmplifyUtils.EnsureKeywordEnabled(this.m_bloomMaterial, AmplifyUtils.HighPrecisionKeyword, false);
        AmplifyUtils.EnsureKeywordEnabled(this.m_finalCompositionMaterial, AmplifyUtils.HighPrecisionKeyword, false);
        AmplifyUtils.CurrentRTFormat = RenderTextureFormat.Default;
      }
      float num1 = Mathf.Acos(Vector3.Dot(this.m_cameraTransform.right, Vector3.right));
      if ((double) Vector3.Cross(this.m_cameraTransform.right, Vector3.right).y > 0.0)
        num1 = -num1;
      RenderTexture renderTexture1 = (RenderTexture) null;
      RenderTexture dest1 = (RenderTexture) null;
      if (!this.m_highPrecision)
      {
        this.m_bloomRange.y = 1f / this.m_bloomRange.x;
        this.m_bloomMaterial.SetVector(AmplifyUtils.BloomRangeId, this.m_bloomRange);
        this.m_finalCompositionMaterial.SetVector(AmplifyUtils.BloomRangeId, this.m_bloomRange);
      }
      this.m_bloomParams.y = this.m_overallThreshold;
      this.m_bloomMaterial.SetVector(AmplifyUtils.BloomParamsId, this.m_bloomParams);
      this.m_finalCompositionMaterial.SetVector(AmplifyUtils.BloomParamsId, this.m_bloomParams);
      int num2 = 1;
      switch (this.m_mainThresholdSize)
      {
        case MainThresholdSizeEnum.Half:
          num2 = 2;
          break;
        case MainThresholdSizeEnum.Quarter:
          num2 = 4;
          break;
      }
      RenderTexture tempRenderTarget = AmplifyUtils.GetTempRenderTarget(src.width / num2, src.height / num2);
      if ((UnityEngine.Object) this.m_maskTexture != (UnityEngine.Object) null)
      {
        this.m_bloomMaterial.SetTexture(AmplifyUtils.MaskTextureId, this.m_maskTexture);
        Graphics.Blit((Texture) src, tempRenderTarget, this.m_bloomMaterial, 1);
      }
      else
        Graphics.Blit((Texture) src, tempRenderTarget, this.m_bloomMaterial, 0);
      if (this.m_debugToScreen == DebugToScreenEnum.MainThreshold)
      {
        Graphics.Blit((Texture) tempRenderTarget, dest, this.m_bloomMaterial, 33);
        AmplifyUtils.ReleaseAllRT();
      }
      else
      {
        bool flag1 = true;
        RenderTexture renderTexture2 = tempRenderTarget;
        if (this.m_bloomDownsampleCount > 0)
        {
          flag1 = false;
          int width = tempRenderTarget.width;
          int height = tempRenderTarget.height;
          for (int index = 0; index < this.m_bloomDownsampleCount; ++index)
          {
            this.m_tempDownsamplesSizes[index].x = (float) width;
            this.m_tempDownsamplesSizes[index].y = (float) height;
            width = width + 1 >> 1;
            height = height + 1 >> 1;
            this.m_tempAuxDownsampleRTs[index] = AmplifyUtils.GetTempRenderTarget(width, height);
            if (index == 0)
            {
              if (!this.m_temporalFilteringActive || this.m_gaussianSteps[index] != 0)
              {
                if (this.m_upscaleQuality == UpscaleQualityEnum.Realistic)
                  Graphics.Blit((Texture) renderTexture2, this.m_tempAuxDownsampleRTs[index], this.m_bloomMaterial, 10);
                else
                  Graphics.Blit((Texture) renderTexture2, this.m_tempAuxDownsampleRTs[index], this.m_bloomMaterial, 11);
              }
              else
              {
                if ((UnityEngine.Object) this.m_tempFilterBuffer != (UnityEngine.Object) null && this.m_temporalFilteringActive)
                {
                  float num3 = this.m_temporalFilteringCurve.Evaluate(this.m_temporalFilteringValue);
                  this.m_bloomMaterial.SetFloat(AmplifyUtils.TempFilterValueId, num3);
                  this.m_bloomMaterial.SetTexture(AmplifyUtils.AnamorphicRTS[0], (Texture) this.m_tempFilterBuffer);
                  if (this.m_upscaleQuality == UpscaleQualityEnum.Realistic)
                    Graphics.Blit((Texture) renderTexture2, this.m_tempAuxDownsampleRTs[index], this.m_bloomMaterial, 12);
                  else
                    Graphics.Blit((Texture) renderTexture2, this.m_tempAuxDownsampleRTs[index], this.m_bloomMaterial, 13);
                }
                else if (this.m_upscaleQuality == UpscaleQualityEnum.Realistic)
                  Graphics.Blit((Texture) renderTexture2, this.m_tempAuxDownsampleRTs[index], this.m_bloomMaterial, 10);
                else
                  Graphics.Blit((Texture) renderTexture2, this.m_tempAuxDownsampleRTs[index], this.m_bloomMaterial, 11);
                bool flag2 = false;
                if ((UnityEngine.Object) this.m_tempFilterBuffer != (UnityEngine.Object) null)
                {
                  if (this.m_tempFilterBuffer.format != this.m_tempAuxDownsampleRTs[index].format || this.m_tempFilterBuffer.width != this.m_tempAuxDownsampleRTs[index].width || this.m_tempFilterBuffer.height != this.m_tempAuxDownsampleRTs[index].height)
                  {
                    this.CleanTempFilterRT();
                    flag2 = true;
                  }
                }
                else
                  flag2 = true;
                if (flag2)
                  this.CreateTempFilterRT(this.m_tempAuxDownsampleRTs[index]);
                this.m_tempFilterBuffer.DiscardContents();
                Graphics.Blit((Texture) this.m_tempAuxDownsampleRTs[index], this.m_tempFilterBuffer);
                if (this.m_debugToScreen == DebugToScreenEnum.TemporalFilter)
                {
                  Graphics.Blit((Texture) this.m_tempAuxDownsampleRTs[index], dest);
                  AmplifyUtils.ReleaseAllRT();
                  return;
                }
              }
            }
            else
              Graphics.Blit((Texture) this.m_tempAuxDownsampleRTs[index - 1], this.m_tempAuxDownsampleRTs[index], this.m_bloomMaterial, 9);
            if (this.m_gaussianSteps[index] > 0)
            {
              this.ApplyGaussianBlur(this.m_tempAuxDownsampleRTs[index], this.m_gaussianSteps[index], this.m_gaussianRadius[index], index == 0);
              if (this.m_temporalFilteringActive && this.m_debugToScreen == DebugToScreenEnum.TemporalFilter)
              {
                Graphics.Blit((Texture) this.m_tempAuxDownsampleRTs[index], dest);
                AmplifyUtils.ReleaseAllRT();
                return;
              }
            }
          }
          renderTexture2 = this.m_tempAuxDownsampleRTs[this.m_featuresSourceId];
          AmplifyUtils.ReleaseTempRenderTarget(tempRenderTarget);
        }
        if (this.m_bokehFilter.ApplyBokeh && this.m_bokehFilter.ApplyOnBloomSource)
        {
          this.m_bokehFilter.ApplyBokehFilter(renderTexture2, this.m_bloomMaterial);
          if (this.m_debugToScreen == DebugToScreenEnum.BokehFilter)
          {
            Graphics.Blit((Texture) renderTexture2, dest);
            AmplifyUtils.ReleaseAllRT();
            return;
          }
        }
        bool flag3 = false;
        RenderTexture renderTexture3;
        if (this.m_separateFeaturesThreshold)
        {
          this.m_bloomParams.y = this.m_featuresThreshold;
          this.m_bloomMaterial.SetVector(AmplifyUtils.BloomParamsId, this.m_bloomParams);
          this.m_finalCompositionMaterial.SetVector(AmplifyUtils.BloomParamsId, this.m_bloomParams);
          renderTexture3 = AmplifyUtils.GetTempRenderTarget(renderTexture2.width, renderTexture2.height);
          flag3 = true;
          Graphics.Blit((Texture) renderTexture2, renderTexture3, this.m_bloomMaterial, 0);
          if (this.m_debugToScreen == DebugToScreenEnum.FeaturesThreshold)
          {
            Graphics.Blit((Texture) renderTexture3, dest);
            AmplifyUtils.ReleaseAllRT();
            return;
          }
        }
        else
          renderTexture3 = renderTexture2;
        if (this.m_bokehFilter.ApplyBokeh && !this.m_bokehFilter.ApplyOnBloomSource)
        {
          if (!flag3)
          {
            flag3 = true;
            renderTexture3 = AmplifyUtils.GetTempRenderTarget(renderTexture2.width, renderTexture2.height);
            Graphics.Blit((Texture) renderTexture2, renderTexture3);
          }
          this.m_bokehFilter.ApplyBokehFilter(renderTexture3, this.m_bloomMaterial);
          if (this.m_debugToScreen == DebugToScreenEnum.BokehFilter)
          {
            Graphics.Blit((Texture) renderTexture3, dest);
            AmplifyUtils.ReleaseAllRT();
            return;
          }
        }
        if (this.m_lensFlare.ApplyLensFlare && this.m_debugToScreen != DebugToScreenEnum.Bloom)
        {
          renderTexture1 = this.m_lensFlare.ApplyFlare(this.m_bloomMaterial, renderTexture3);
          this.ApplyGaussianBlur(renderTexture1, this.m_lensFlare.LensFlareGaussianBlurAmount, 1f, false);
          if (this.m_debugToScreen == DebugToScreenEnum.LensFlare)
          {
            Graphics.Blit((Texture) renderTexture1, dest);
            AmplifyUtils.ReleaseAllRT();
            return;
          }
        }
        if (this.m_anamorphicGlare.ApplyLensGlare && this.m_debugToScreen != DebugToScreenEnum.Bloom)
        {
          dest1 = AmplifyUtils.GetTempRenderTarget(renderTexture2.width, renderTexture2.height);
          this.m_anamorphicGlare.OnRenderImage(this.m_bloomMaterial, renderTexture3, dest1, num1);
          if (this.m_debugToScreen == DebugToScreenEnum.LensGlare)
          {
            Graphics.Blit((Texture) dest1, dest);
            AmplifyUtils.ReleaseAllRT();
            return;
          }
        }
        if (flag3)
          AmplifyUtils.ReleaseTempRenderTarget(renderTexture3);
        if (flag1)
          this.ApplyGaussianBlur(renderTexture2, this.m_gaussianSteps[0], this.m_gaussianRadius[0], false);
        if (this.m_bloomDownsampleCount > 0)
        {
          if (this.m_bloomDownsampleCount == 1)
          {
            if (this.m_upscaleQuality == UpscaleQualityEnum.Realistic)
            {
              this.ApplyUpscale();
              this.m_finalCompositionMaterial.SetTexture(AmplifyUtils.MipResultsRTS[0], (Texture) this.m_tempUpscaleRTs[0]);
            }
            else
              this.m_finalCompositionMaterial.SetTexture(AmplifyUtils.MipResultsRTS[0], (Texture) this.m_tempAuxDownsampleRTs[0]);
            this.m_finalCompositionMaterial.SetFloat(AmplifyUtils.UpscaleWeightsStr[0], this.m_upscaleWeights[0]);
          }
          else if (this.m_upscaleQuality == UpscaleQualityEnum.Realistic)
          {
            this.ApplyUpscale();
            for (int index1 = 0; index1 < this.m_bloomDownsampleCount; ++index1)
            {
              int index2 = this.m_bloomDownsampleCount - index1 - 1;
              this.m_finalCompositionMaterial.SetTexture(AmplifyUtils.MipResultsRTS[index2], (Texture) this.m_tempUpscaleRTs[index1]);
              this.m_finalCompositionMaterial.SetFloat(AmplifyUtils.UpscaleWeightsStr[index2], this.m_upscaleWeights[index1]);
            }
          }
          else
          {
            for (int index1 = 0; index1 < this.m_bloomDownsampleCount; ++index1)
            {
              int index2 = this.m_bloomDownsampleCount - 1 - index1;
              this.m_finalCompositionMaterial.SetTexture(AmplifyUtils.MipResultsRTS[index2], (Texture) this.m_tempAuxDownsampleRTs[index2]);
              this.m_finalCompositionMaterial.SetFloat(AmplifyUtils.UpscaleWeightsStr[index2], this.m_upscaleWeights[index1]);
            }
          }
        }
        else
        {
          this.m_finalCompositionMaterial.SetTexture(AmplifyUtils.MipResultsRTS[0], (Texture) renderTexture2);
          this.m_finalCompositionMaterial.SetFloat(AmplifyUtils.UpscaleWeightsStr[0], 1f);
        }
        if (this.m_debugToScreen == DebugToScreenEnum.Bloom)
        {
          this.m_finalCompositionMaterial.SetFloat(AmplifyUtils.SourceContributionId, 0.0f);
          this.FinalComposition(0.0f, 1f, src, dest, 0);
        }
        else
        {
          if (this.m_bloomDownsampleCount > 1)
          {
            for (int index = 0; index < this.m_bloomDownsampleCount; ++index)
            {
              this.m_finalCompositionMaterial.SetFloat(AmplifyUtils.LensDirtWeightsStr[this.m_bloomDownsampleCount - index - 1], this.m_lensDirtWeights[index]);
              this.m_finalCompositionMaterial.SetFloat(AmplifyUtils.LensStarburstWeightsStr[this.m_bloomDownsampleCount - index - 1], this.m_lensStarburstWeights[index]);
            }
          }
          else
          {
            this.m_finalCompositionMaterial.SetFloat(AmplifyUtils.LensDirtWeightsStr[0], this.m_lensDirtWeights[0]);
            this.m_finalCompositionMaterial.SetFloat(AmplifyUtils.LensStarburstWeightsStr[0], this.m_lensStarburstWeights[0]);
          }
          if (this.m_lensFlare.ApplyLensFlare)
            this.m_finalCompositionMaterial.SetTexture(AmplifyUtils.LensFlareRTId, (Texture) renderTexture1);
          if (this.m_anamorphicGlare.ApplyLensGlare)
            this.m_finalCompositionMaterial.SetTexture(AmplifyUtils.LensGlareRTId, (Texture) dest1);
          if (this.m_applyLensDirt)
          {
            this.m_finalCompositionMaterial.SetTexture(AmplifyUtils.LensDirtRTId, this.m_lensDirtTexture);
            this.m_finalCompositionMaterial.SetFloat(AmplifyUtils.LensDirtStrengthId, this.m_lensDirtStrength * 1f);
            if (this.m_debugToScreen == DebugToScreenEnum.LensDirt)
            {
              this.FinalComposition(0.0f, 0.0f, src, dest, 2);
              return;
            }
          }
          if (this.m_applyLensStardurst)
          {
            this.m_starburstMat[0, 0] = Mathf.Cos(num1);
            this.m_starburstMat[0, 1] = -Mathf.Sin(num1);
            this.m_starburstMat[1, 0] = Mathf.Sin(num1);
            this.m_starburstMat[1, 1] = Mathf.Cos(num1);
            this.m_finalCompositionMaterial.SetMatrix(AmplifyUtils.LensFlareStarMatrixId, this.m_starburstMat);
            this.m_finalCompositionMaterial.SetFloat(AmplifyUtils.LensFlareStarburstStrengthId, this.m_lensStarburstStrength * 1f);
            this.m_finalCompositionMaterial.SetTexture(AmplifyUtils.LensStarburstRTId, this.m_lensStardurstTex);
            if (this.m_debugToScreen == DebugToScreenEnum.LensStarburst)
            {
              this.FinalComposition(0.0f, 0.0f, src, dest, 1);
              return;
            }
          }
          if ((UnityEngine.Object) this.m_targetTexture != (UnityEngine.Object) null)
          {
            this.m_targetTexture.DiscardContents();
            this.FinalComposition(0.0f, 1f, src, this.m_targetTexture, -1);
            Graphics.Blit((Texture) src, dest);
          }
          else
            this.FinalComposition(1f, 1f, src, dest, -1);
        }
      }
    }

    private void FinalComposition(
      float srcContribution,
      float upscaleContribution,
      RenderTexture src,
      RenderTexture dest,
      int forcePassId)
    {
      this.m_finalCompositionMaterial.SetFloat(AmplifyUtils.SourceContributionId, srcContribution);
      this.m_finalCompositionMaterial.SetFloat(AmplifyUtils.UpscaleContributionId, upscaleContribution);
      int num = 0;
      if (forcePassId > -1)
      {
        num = forcePassId;
      }
      else
      {
        if (this.LensFlareInstance.ApplyLensFlare)
          num |= 8;
        if (this.LensGlareInstance.ApplyLensGlare)
          num |= 4;
        if (this.m_applyLensDirt)
          num |= 2;
        if (this.m_applyLensStardurst)
          num |= 1;
      }
      int pass = num + (this.m_bloomDownsampleCount - 1) * 16;
      Graphics.Blit((Texture) src, dest, this.m_finalCompositionMaterial, pass);
      AmplifyUtils.ReleaseAllRT();
    }

    private void ApplyUpscale()
    {
      int index1 = this.m_bloomDownsampleCount - 1;
      int index2 = 0;
      for (int index3 = index1; index3 > -1; --index3)
      {
        this.m_tempUpscaleRTs[index2] = AmplifyUtils.GetTempRenderTarget((int) this.m_tempDownsamplesSizes[index3].x, (int) this.m_tempDownsamplesSizes[index3].y);
        if (index3 == index1)
        {
          Graphics.Blit((Texture) this.m_tempAuxDownsampleRTs[index1], this.m_tempUpscaleRTs[index2], this.m_bloomMaterial, 17);
        }
        else
        {
          this.m_bloomMaterial.SetTexture(AmplifyUtils.AnamorphicRTS[0], (Texture) this.m_tempUpscaleRTs[index2 - 1]);
          Graphics.Blit((Texture) this.m_tempAuxDownsampleRTs[index3], this.m_tempUpscaleRTs[index2], this.m_bloomMaterial, 18);
        }
        ++index2;
      }
    }

    public AmplifyGlare LensGlareInstance
    {
      get
      {
        return this.m_anamorphicGlare;
      }
    }

    public AmplifyBokeh BokehFilterInstance
    {
      get
      {
        return this.m_bokehFilter;
      }
    }

    public AmplifyLensFlare LensFlareInstance
    {
      get
      {
        return this.m_lensFlare;
      }
    }

    public bool ApplyLensDirt
    {
      get
      {
        return this.m_applyLensDirt;
      }
      set
      {
        this.m_applyLensDirt = value;
      }
    }

    public float LensDirtStrength
    {
      get
      {
        return this.m_lensDirtStrength;
      }
      set
      {
        this.m_lensDirtStrength = (double) value < 0.0 ? 0.0f : value;
      }
    }

    public Texture LensDirtTexture
    {
      get
      {
        return this.m_lensDirtTexture;
      }
      set
      {
        this.m_lensDirtTexture = value;
      }
    }

    public bool ApplyLensStardurst
    {
      get
      {
        return this.m_applyLensStardurst;
      }
      set
      {
        this.m_applyLensStardurst = value;
      }
    }

    public Texture LensStardurstTex
    {
      get
      {
        return this.m_lensStardurstTex;
      }
      set
      {
        this.m_lensStardurstTex = value;
      }
    }

    public float LensStarburstStrength
    {
      get
      {
        return this.m_lensStarburstStrength;
      }
      set
      {
        this.m_lensStarburstStrength = (double) value < 0.0 ? 0.0f : value;
      }
    }

    public PrecisionModes CurrentPrecisionMode
    {
      get
      {
        return this.m_highPrecision ? PrecisionModes.High : PrecisionModes.Low;
      }
      set
      {
        this.HighPrecision = value == PrecisionModes.High;
      }
    }

    public bool HighPrecision
    {
      get
      {
        return this.m_highPrecision;
      }
      set
      {
        if (this.m_highPrecision == value)
          return;
        this.m_highPrecision = value;
        this.CleanTempFilterRT();
      }
    }

    public float BloomRange
    {
      get
      {
        return this.m_bloomRange.x;
      }
      set
      {
        this.m_bloomRange.x = (double) value < 0.0 ? 0.0f : value;
      }
    }

    public float OverallThreshold
    {
      get
      {
        return this.m_overallThreshold;
      }
      set
      {
        this.m_overallThreshold = (double) value < 0.0 ? 0.0f : value;
      }
    }

    public Vector4 BloomParams
    {
      get
      {
        return this.m_bloomParams;
      }
      set
      {
        this.m_bloomParams = value;
      }
    }

    public float OverallIntensity
    {
      get
      {
        return this.m_bloomParams.x;
      }
      set
      {
        this.m_bloomParams.x = (double) value < 0.0 ? 0.0f : value;
      }
    }

    public float BloomScale
    {
      get
      {
        return this.m_bloomParams.w;
      }
      set
      {
        this.m_bloomParams.w = (double) value < 0.0 ? 0.0f : value;
      }
    }

    public float UpscaleBlurRadius
    {
      get
      {
        return this.m_bloomParams.z;
      }
      set
      {
        this.m_bloomParams.z = value;
      }
    }

    public bool TemporalFilteringActive
    {
      get
      {
        return this.m_temporalFilteringActive;
      }
      set
      {
        if (this.m_temporalFilteringActive != value)
          this.CleanTempFilterRT();
        this.m_temporalFilteringActive = value;
      }
    }

    public float TemporalFilteringValue
    {
      get
      {
        return this.m_temporalFilteringValue;
      }
      set
      {
        this.m_temporalFilteringValue = value;
      }
    }

    public int SoftMaxdownscales
    {
      get
      {
        return this.m_softMaxdownscales;
      }
    }

    public int BloomDownsampleCount
    {
      get
      {
        return this.m_bloomDownsampleCount;
      }
      set
      {
        this.m_bloomDownsampleCount = Mathf.Clamp(value, 1, this.m_softMaxdownscales);
      }
    }

    public int FeaturesSourceId
    {
      get
      {
        return this.m_featuresSourceId;
      }
      set
      {
        this.m_featuresSourceId = Mathf.Clamp(value, 0, this.m_bloomDownsampleCount - 1);
      }
    }

    public bool[] DownscaleSettingsFoldout
    {
      get
      {
        return this.m_downscaleSettingsFoldout;
      }
    }

    public float[] UpscaleWeights
    {
      get
      {
        return this.m_upscaleWeights;
      }
    }

    public float[] LensDirtWeights
    {
      get
      {
        return this.m_lensDirtWeights;
      }
    }

    public float[] LensStarburstWeights
    {
      get
      {
        return this.m_lensStarburstWeights;
      }
    }

    public float[] GaussianRadius
    {
      get
      {
        return this.m_gaussianRadius;
      }
    }

    public int[] GaussianSteps
    {
      get
      {
        return this.m_gaussianSteps;
      }
    }

    public AnimationCurve TemporalFilteringCurve
    {
      get
      {
        return this.m_temporalFilteringCurve;
      }
      set
      {
        this.m_temporalFilteringCurve = value;
      }
    }

    public bool SeparateFeaturesThreshold
    {
      get
      {
        return this.m_separateFeaturesThreshold;
      }
      set
      {
        this.m_separateFeaturesThreshold = value;
      }
    }

    public float FeaturesThreshold
    {
      get
      {
        return this.m_featuresThreshold;
      }
      set
      {
        this.m_featuresThreshold = (double) value < 0.0 ? 0.0f : value;
      }
    }

    public DebugToScreenEnum DebugToScreen
    {
      get
      {
        return this.m_debugToScreen;
      }
      set
      {
        this.m_debugToScreen = value;
      }
    }

    public UpscaleQualityEnum UpscaleQuality
    {
      get
      {
        return this.m_upscaleQuality;
      }
      set
      {
        this.m_upscaleQuality = value;
      }
    }

    public bool ShowDebugMessages
    {
      get
      {
        return this.m_showDebugMessages;
      }
      set
      {
        this.m_showDebugMessages = value;
      }
    }

    public MainThresholdSizeEnum MainThresholdSize
    {
      get
      {
        return this.m_mainThresholdSize;
      }
      set
      {
        this.m_mainThresholdSize = value;
      }
    }

    public RenderTexture TargetTexture
    {
      get
      {
        return this.m_targetTexture;
      }
      set
      {
        this.m_targetTexture = value;
      }
    }

    public Texture MaskTexture
    {
      get
      {
        return this.m_maskTexture;
      }
      set
      {
        this.m_maskTexture = value;
      }
    }

    public bool ApplyBokehFilter
    {
      get
      {
        return this.m_bokehFilter.ApplyBokeh;
      }
      set
      {
        this.m_bokehFilter.ApplyBokeh = value;
      }
    }

    public bool ApplyLensFlare
    {
      get
      {
        return this.m_lensFlare.ApplyLensFlare;
      }
      set
      {
        this.m_lensFlare.ApplyLensFlare = value;
      }
    }

    public bool ApplyLensGlare
    {
      get
      {
        return this.m_anamorphicGlare.ApplyLensGlare;
      }
      set
      {
        this.m_anamorphicGlare.ApplyLensGlare = value;
      }
    }
  }
}
