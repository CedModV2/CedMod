// Decompiled with JetBrains decompiler
// Type: UnityEngine.PostProcessing.TaaComponent
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace UnityEngine.PostProcessing
{
  public sealed class TaaComponent : PostProcessingComponentRenderTexture<AntialiasingModel>
  {
    private readonly RenderBuffer[] m_MRT = new RenderBuffer[2];
    private bool m_ResetHistory = true;
    private const string k_ShaderString = "Hidden/Post FX/Temporal Anti-aliasing";
    private const int k_SampleCount = 8;
    private RenderTexture m_HistoryTexture;
    private int m_SampleIndex;

    public override bool active
    {
      get
      {
        return this.model.enabled && this.model.settings.method == AntialiasingModel.Method.Taa && (SystemInfo.supportsMotionVectors && SystemInfo.supportedRenderTargetCount >= 2) && !this.context.interrupted;
      }
    }

    public Vector2 jitterVector { get; private set; }

    public override DepthTextureMode GetCameraFlags()
    {
      return DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
    }

    public void ResetHistory()
    {
      this.m_ResetHistory = true;
    }

    public void SetProjectionMatrix(Func<Vector2, Matrix4x4> jitteredFunc)
    {
      Vector2 offset = this.GenerateRandomOffset() * this.model.settings.taaSettings.jitterSpread;
      this.context.camera.nonJitteredProjectionMatrix = this.context.camera.projectionMatrix;
      if (jitteredFunc != null)
        this.context.camera.projectionMatrix = jitteredFunc(offset);
      else
        this.context.camera.projectionMatrix = this.context.camera.orthographic ? this.GetOrthographicProjectionMatrix(offset) : this.GetPerspectiveProjectionMatrix(offset);
      this.context.camera.useJitteredProjectionMatrixForTransparentRendering = false;
      offset.x /= (float) this.context.width;
      offset.y /= (float) this.context.height;
      this.context.materialFactory.Get("Hidden/Post FX/Temporal Anti-aliasing").SetVector(TaaComponent.Uniforms._Jitter, (Vector4) offset);
      this.jitterVector = offset;
    }

    public void Render(RenderTexture source, RenderTexture destination)
    {
      Material material = this.context.materialFactory.Get("Hidden/Post FX/Temporal Anti-aliasing");
      material.shaderKeywords = (string[]) null;
      AntialiasingModel.TaaSettings taaSettings = this.model.settings.taaSettings;
      if (this.m_ResetHistory || (UnityEngine.Object) this.m_HistoryTexture == (UnityEngine.Object) null || (this.m_HistoryTexture.width != source.width || this.m_HistoryTexture.height != source.height))
      {
        if ((bool) (UnityEngine.Object) this.m_HistoryTexture)
          RenderTexture.ReleaseTemporary(this.m_HistoryTexture);
        this.m_HistoryTexture = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        this.m_HistoryTexture.name = "TAA History";
        Graphics.Blit((Texture) source, this.m_HistoryTexture, material, 2);
      }
      material.SetVector(TaaComponent.Uniforms._SharpenParameters, new Vector4(taaSettings.sharpen, 0.0f, 0.0f, 0.0f));
      material.SetVector(TaaComponent.Uniforms._FinalBlendParameters, new Vector4(taaSettings.stationaryBlending, taaSettings.motionBlending, 6000f, 0.0f));
      material.SetTexture(TaaComponent.Uniforms._MainTex, (Texture) source);
      material.SetTexture(TaaComponent.Uniforms._HistoryTex, (Texture) this.m_HistoryTexture);
      RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
      temporary.name = "TAA History";
      this.m_MRT[0] = destination.colorBuffer;
      this.m_MRT[1] = temporary.colorBuffer;
      Graphics.SetRenderTarget(this.m_MRT, source.depthBuffer);
      GraphicsUtils.Blit(material, this.context.camera.orthographic ? 1 : 0);
      RenderTexture.ReleaseTemporary(this.m_HistoryTexture);
      this.m_HistoryTexture = temporary;
      this.m_ResetHistory = false;
    }

    private float GetHaltonValue(int index, int radix)
    {
      float num1 = 0.0f;
      float num2 = 1f / (float) radix;
      while (index > 0)
      {
        num1 += (float) (index % radix) * num2;
        index /= radix;
        num2 /= (float) radix;
      }
      return num1;
    }

    private Vector2 GenerateRandomOffset()
    {
      Vector2 vector2 = new Vector2(this.GetHaltonValue(this.m_SampleIndex & 1023, 2), this.GetHaltonValue(this.m_SampleIndex & 1023, 3));
      if (++this.m_SampleIndex < 8)
        return vector2;
      this.m_SampleIndex = 0;
      return vector2;
    }

    private Matrix4x4 GetPerspectiveProjectionMatrix(Vector2 offset)
    {
      float num1 = Mathf.Tan((float) Math.PI / 360f * this.context.camera.fieldOfView);
      float num2 = num1 * this.context.camera.aspect;
      offset.x *= num2 / (0.5f * (float) this.context.width);
      offset.y *= num1 / (0.5f * (float) this.context.height);
      float num3 = (offset.x - num2) * this.context.camera.nearClipPlane;
      float num4 = (offset.x + num2) * this.context.camera.nearClipPlane;
      float num5 = (offset.y + num1) * this.context.camera.nearClipPlane;
      float num6 = (offset.y - num1) * this.context.camera.nearClipPlane;
      return new Matrix4x4()
      {
        [0, 0] = (float) (2.0 * (double) this.context.camera.nearClipPlane / ((double) num4 - (double) num3)),
        [0, 1] = 0.0f,
        [0, 2] = (float) (((double) num4 + (double) num3) / ((double) num4 - (double) num3)),
        [0, 3] = 0.0f,
        [1, 0] = 0.0f,
        [1, 1] = (float) (2.0 * (double) this.context.camera.nearClipPlane / ((double) num5 - (double) num6)),
        [1, 2] = (float) (((double) num5 + (double) num6) / ((double) num5 - (double) num6)),
        [1, 3] = 0.0f,
        [2, 0] = 0.0f,
        [2, 1] = 0.0f,
        [2, 2] = (float) (-((double) this.context.camera.farClipPlane + (double) this.context.camera.nearClipPlane) / ((double) this.context.camera.farClipPlane - (double) this.context.camera.nearClipPlane)),
        [2, 3] = (float) (-(2.0 * (double) this.context.camera.farClipPlane * (double) this.context.camera.nearClipPlane) / ((double) this.context.camera.farClipPlane - (double) this.context.camera.nearClipPlane)),
        [3, 0] = 0.0f,
        [3, 1] = 0.0f,
        [3, 2] = -1f,
        [3, 3] = 0.0f
      };
    }

    private Matrix4x4 GetOrthographicProjectionMatrix(Vector2 offset)
    {
      float orthographicSize = this.context.camera.orthographicSize;
      float num1 = orthographicSize * this.context.camera.aspect;
      offset.x *= num1 / (0.5f * (float) this.context.width);
      offset.y *= orthographicSize / (0.5f * (float) this.context.height);
      double num2 = (double) offset.x - (double) num1;
      float num3 = offset.x + num1;
      float num4 = offset.y + orthographicSize;
      float num5 = offset.y - orthographicSize;
      double num6 = (double) num3;
      double num7 = (double) num5;
      double num8 = (double) num4;
      double nearClipPlane = (double) this.context.camera.nearClipPlane;
      double farClipPlane = (double) this.context.camera.farClipPlane;
      return Matrix4x4.Ortho((float) num2, (float) num6, (float) num7, (float) num8, (float) nearClipPlane, (float) farClipPlane);
    }

    public override void OnDisable()
    {
      if ((UnityEngine.Object) this.m_HistoryTexture != (UnityEngine.Object) null)
        RenderTexture.ReleaseTemporary(this.m_HistoryTexture);
      this.m_HistoryTexture = (RenderTexture) null;
      this.m_SampleIndex = 0;
      this.ResetHistory();
    }

    private static class Uniforms
    {
      internal static readonly int _Jitter = Shader.PropertyToID(nameof (_Jitter));
      internal static readonly int _SharpenParameters = Shader.PropertyToID(nameof (_SharpenParameters));
      internal static readonly int _FinalBlendParameters = Shader.PropertyToID(nameof (_FinalBlendParameters));
      internal static readonly int _HistoryTex = Shader.PropertyToID(nameof (_HistoryTex));
      internal static readonly int _MainTex = Shader.PropertyToID(nameof (_MainTex));
    }
  }
}
