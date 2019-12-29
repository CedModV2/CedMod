// Decompiled with JetBrains decompiler
// Type: NGSS_ContactShadows
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.Rendering;

[ImageEffectAllowedInSceneView]
[ExecuteInEditMode]
public class NGSS_ContactShadows : MonoBehaviour
{
  [Range(0.0f, 3f)]
  public float shadowsSoftness = 1f;
  [Range(1f, 4f)]
  public float shadowsDistance = 2f;
  [Range(0.1f, 4f)]
  public float shadowsFade = 1f;
  [Range(0.0f, 0.02f)]
  public float shadowsBias = 0.0065f;
  [Range(0.0f, 1f)]
  public float rayWidth = 0.1f;
  [Range(16f, 128f)]
  public int raySamples = 64;
  public Light mainDirectionalLight;
  public Shader contactShadowsShader;
  public bool noiseFilter;
  private CommandBuffer blendShadowsCB;
  private CommandBuffer computeShadowsCB;
  private bool isInitialized;
  private Camera _mCamera;
  private Material _mMaterial;

  private Camera mCamera
  {
    get
    {
      if ((Object) this._mCamera == (Object) null)
      {
        this._mCamera = this.GetComponent<Camera>();
        if ((Object) this._mCamera == (Object) null)
          this._mCamera = Camera.main;
        if ((Object) this._mCamera == (Object) null)
          Debug.LogError((object) "NGSS Error: No MainCamera found, please provide one.", (Object) this);
        else
          this._mCamera.depthTextureMode |= DepthTextureMode.Depth;
      }
      return this._mCamera;
    }
  }

  private Material mMaterial
  {
    get
    {
      if ((Object) this._mMaterial == (Object) null)
      {
        if ((Object) this.contactShadowsShader == (Object) null)
          Shader.Find("Hidden/NGSS_ContactShadows");
        this._mMaterial = new Material(this.contactShadowsShader);
        if ((Object) this._mMaterial == (Object) null)
        {
          Debug.LogWarning((object) "NGSS Warning: can't find NGSS_ContactShadows shader, make sure it's on your project.", (Object) this);
          this.enabled = false;
          return (Material) null;
        }
      }
      return this._mMaterial;
    }
  }

  private void AddCommandBuffers()
  {
    this.computeShadowsCB = new CommandBuffer()
    {
      name = "NGSS ContactShadows: Compute"
    };
    this.blendShadowsCB = new CommandBuffer()
    {
      name = "NGSS ContactShadows: Mix"
    };
    bool flag = this.mCamera.actualRenderingPath == RenderingPath.Forward;
    if ((bool) (Object) this.mCamera)
    {
      foreach (CommandBuffer commandBuffer in this.mCamera.GetCommandBuffers(flag ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeLighting))
      {
        if (commandBuffer.name == this.computeShadowsCB.name)
          return;
      }
      this.mCamera.AddCommandBuffer(flag ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeLighting, this.computeShadowsCB);
    }
    if (!(bool) (Object) this.mainDirectionalLight)
      return;
    foreach (CommandBuffer commandBuffer in this.mainDirectionalLight.GetCommandBuffers(LightEvent.AfterScreenspaceMask))
    {
      if (commandBuffer.name == this.blendShadowsCB.name)
        return;
    }
    this.mainDirectionalLight.AddCommandBuffer(LightEvent.AfterScreenspaceMask, this.blendShadowsCB);
  }

  private void RemoveCommandBuffers()
  {
    this._mMaterial = (Material) null;
    bool flag = this.mCamera.actualRenderingPath == RenderingPath.Forward;
    if ((bool) (Object) this.mCamera)
      this.mCamera.RemoveCommandBuffer(flag ? CameraEvent.AfterDepthTexture : CameraEvent.BeforeLighting, this.computeShadowsCB);
    if ((bool) (Object) this.mainDirectionalLight)
      this.mainDirectionalLight.RemoveCommandBuffer(LightEvent.AfterScreenspaceMask, this.blendShadowsCB);
    this.isInitialized = false;
  }

  private void Init()
  {
    if (this.isInitialized || (Object) this.mainDirectionalLight == (Object) null)
      return;
    if (this.mCamera.renderingPath == RenderingPath.UsePlayerSettings || this.mCamera.renderingPath == RenderingPath.VertexLit)
    {
      Debug.LogWarning((object) "Please set your camera rendering path to either Forward or Deferred and re-enable this component.", (Object) this);
      this.enabled = false;
    }
    else
    {
      this.AddCommandBuffers();
      int id1 = Shader.PropertyToID("NGSS_ContactShadowRT");
      int id2 = Shader.PropertyToID("NGSS_DepthSourceRT");
      this.computeShadowsCB.GetTemporaryRT(id1, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.R8);
      this.computeShadowsCB.GetTemporaryRT(id2, -1, -1, 0, FilterMode.Point, RenderTextureFormat.RFloat);
      this.computeShadowsCB.Blit((RenderTargetIdentifier) id1, (RenderTargetIdentifier) id2, this.mMaterial, 0);
      this.computeShadowsCB.Blit((RenderTargetIdentifier) id2, (RenderTargetIdentifier) id1, this.mMaterial, 1);
      this.computeShadowsCB.Blit((RenderTargetIdentifier) id1, (RenderTargetIdentifier) id2, this.mMaterial, 2);
      this.blendShadowsCB.Blit((RenderTargetIdentifier) BuiltinRenderTextureType.None, (RenderTargetIdentifier) BuiltinRenderTextureType.CurrentActive, this.mMaterial, 3);
      this.computeShadowsCB.SetGlobalTexture("NGSS_ContactShadowsTexture", (RenderTargetIdentifier) id2);
      this.isInitialized = true;
    }
  }

  private void OnEnable()
  {
    this.Init();
  }

  private void OnDisable()
  {
    if (!this.isInitialized)
      return;
    this.RemoveCommandBuffers();
  }

  private void OnApplicationQuit()
  {
    if (!this.isInitialized)
      return;
    this.RemoveCommandBuffers();
  }

  private void OnPreRender()
  {
    this.Init();
    if (!this.isInitialized || (Object) this.mainDirectionalLight == (Object) null)
      return;
    this.mMaterial.SetVector("LightDir", (Vector4) this.mCamera.transform.InverseTransformDirection(this.mainDirectionalLight.transform.forward));
    this.mMaterial.SetFloat("ShadowsSoftness", this.shadowsSoftness);
    this.mMaterial.SetFloat("ShadowsDistance", this.shadowsDistance);
    this.mMaterial.SetFloat("ShadowsFade", this.shadowsFade);
    this.mMaterial.SetFloat("ShadowsBias", this.shadowsBias);
    this.mMaterial.SetFloat("RayWidth", this.rayWidth);
    this.mMaterial.SetInt("RaySamples", this.raySamples);
    if (this.noiseFilter)
      this.mMaterial.EnableKeyword("NGSS_CONTACT_SHADOWS_USE_NOISE");
    else
      this.mMaterial.DisableKeyword("NGSS_CONTACT_SHADOWS_USE_NOISE");
  }
}
