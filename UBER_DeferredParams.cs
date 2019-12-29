// Decompiled with JetBrains decompiler
// Type: UBER_DeferredParams
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[AddComponentMenu("UBER/Deferred Params")]
[RequireComponent(typeof (Camera))]
[DisallowMultipleComponent]
[ExecuteInEditMode]
public class UBER_DeferredParams : MonoBehaviour
{
  [Header("Translucency setup 1")]
  [ColorUsage(false)]
  public Color TranslucencyColor1 = new Color(1f, 1f, 1f, 1f);
  [Tooltip("You can control strength per light using its color alpha (first enable in UBER config file)")]
  public float Strength1 = 4f;
  [Range(0.0f, 1f)]
  public float PointLightsDirectionality1 = 0.7f;
  [Range(0.0f, 0.5f)]
  public float Constant1 = 0.1f;
  [Range(0.0f, 0.3f)]
  public float Scattering1 = 0.05f;
  [Range(0.0f, 100f)]
  public float SpotExponent1 = 30f;
  [Range(0.0f, 20f)]
  public float SuppressShadows1 = 0.5f;
  [Space]
  [Header("Translucency setup 2")]
  [ColorUsage(false)]
  public Color TranslucencyColor2 = new Color(1f, 1f, 1f, 1f);
  [Tooltip("You can control strength per light using its color alpha (first enable in UBER config file)")]
  public float Strength2 = 4f;
  [Range(0.0f, 1f)]
  public float PointLightsDirectionality2 = 0.7f;
  [Range(0.0f, 0.5f)]
  public float Constant2 = 0.1f;
  [Range(0.0f, 0.3f)]
  public float Scattering2 = 0.05f;
  [Range(0.0f, 100f)]
  public float SpotExponent2 = 30f;
  [Range(0.0f, 20f)]
  public float SuppressShadows2 = 0.5f;
  [Space]
  [Header("Translucency setup 3")]
  [ColorUsage(false)]
  public Color TranslucencyColor3 = new Color(1f, 1f, 1f, 1f);
  [Tooltip("You can control strength per light using its color alpha (first enable in UBER config file)")]
  public float Strength3 = 4f;
  [Range(0.0f, 1f)]
  public float PointLightsDirectionality3 = 0.7f;
  [Range(0.0f, 0.5f)]
  public float Constant3 = 0.1f;
  [Range(0.0f, 0.3f)]
  public float Scattering3 = 0.05f;
  [Range(0.0f, 100f)]
  public float SpotExponent3 = 30f;
  [Range(0.0f, 20f)]
  public float SuppressShadows3 = 0.5f;
  [Space]
  [Header("Translucency setup 4")]
  [ColorUsage(false)]
  public Color TranslucencyColor4 = new Color(1f, 1f, 1f, 1f);
  [Tooltip("You can control strength per light using its color alpha (first enable in UBER config file)")]
  public float Strength4 = 4f;
  [Range(0.0f, 1f)]
  public float PointLightsDirectionality4 = 0.7f;
  [Range(0.0f, 0.5f)]
  public float Constant4 = 0.1f;
  [Range(0.0f, 0.3f)]
  public float Scattering4 = 0.05f;
  [Range(0.0f, 100f)]
  public float SpotExponent4 = 30f;
  [Range(0.0f, 20f)]
  public float SuppressShadows4 = 0.5f;
  private HashSet<Camera> sceneCamsWithBuffer = new HashSet<Camera>();
  [Range(0.0f, 1f)]
  public float NdotLReduction1;
  [Range(0.0f, 1f)]
  public float NdotLReduction2;
  [Range(0.0f, 1f)]
  public float NdotLReduction3;
  [Range(0.0f, 1f)]
  public float NdotLReduction4;
  private Camera mycam;
  private CommandBuffer combufPreLight;
  private CommandBuffer combufPostLight;
  private Material CopyPropsMat;
  private bool UBERPresenceChecked;
  private bool UBERPresent;
  [HideInInspector]
  public Texture2D TranslucencyPropsTex;

  private void Start()
  {
    this.SetupTranslucencyValues();
  }

  public void OnValidate()
  {
    this.SetupTranslucencyValues();
  }

  public void SetupTranslucencyValues()
  {
    if ((UnityEngine.Object) this.TranslucencyPropsTex == (UnityEngine.Object) null)
    {
      this.TranslucencyPropsTex = new Texture2D(4, 3, TextureFormat.RGBAFloat, false, true);
      this.TranslucencyPropsTex.anisoLevel = 0;
      this.TranslucencyPropsTex.filterMode = FilterMode.Point;
      this.TranslucencyPropsTex.wrapMode = TextureWrapMode.Clamp;
      this.TranslucencyPropsTex.hideFlags = HideFlags.HideAndDontSave;
    }
    Shader.SetGlobalTexture("_UBERTranslucencySetup", (Texture) this.TranslucencyPropsTex);
    byte[] numArray = new byte[192];
    this.EncodeRGBAFloatTo16Bytes(this.TranslucencyColor1.r, this.TranslucencyColor1.g, this.TranslucencyColor1.b, this.Strength1, numArray, 0, 0);
    this.EncodeRGBAFloatTo16Bytes(this.PointLightsDirectionality1, this.Constant1, this.Scattering1, this.SpotExponent1, numArray, 0, 1);
    this.EncodeRGBAFloatTo16Bytes(this.SuppressShadows1, this.NdotLReduction1, 1f, 1f, numArray, 0, 2);
    this.EncodeRGBAFloatTo16Bytes(this.TranslucencyColor2.r, this.TranslucencyColor2.g, this.TranslucencyColor2.b, this.Strength2, numArray, 1, 0);
    this.EncodeRGBAFloatTo16Bytes(this.PointLightsDirectionality2, this.Constant2, this.Scattering2, this.SpotExponent2, numArray, 1, 1);
    this.EncodeRGBAFloatTo16Bytes(this.SuppressShadows2, this.NdotLReduction2, 1f, 1f, numArray, 1, 2);
    this.EncodeRGBAFloatTo16Bytes(this.TranslucencyColor3.r, this.TranslucencyColor3.g, this.TranslucencyColor3.b, this.Strength3, numArray, 2, 0);
    this.EncodeRGBAFloatTo16Bytes(this.PointLightsDirectionality3, this.Constant3, this.Scattering3, this.SpotExponent3, numArray, 2, 1);
    this.EncodeRGBAFloatTo16Bytes(this.SuppressShadows3, this.NdotLReduction3, 1f, 1f, numArray, 2, 2);
    this.EncodeRGBAFloatTo16Bytes(this.TranslucencyColor4.r, this.TranslucencyColor4.g, this.TranslucencyColor4.b, this.Strength4, numArray, 3, 0);
    this.EncodeRGBAFloatTo16Bytes(this.PointLightsDirectionality4, this.Constant4, this.Scattering4, this.SpotExponent4, numArray, 3, 1);
    this.EncodeRGBAFloatTo16Bytes(this.SuppressShadows4, this.NdotLReduction4, 1f, 1f, numArray, 3, 2);
    this.TranslucencyPropsTex.LoadRawTextureData(numArray);
    this.TranslucencyPropsTex.Apply();
  }

  private void EncodeRGBAFloatTo16Bytes(
    float r,
    float g,
    float b,
    float a,
    byte[] rawTexdata,
    int idx_u,
    int idx_v)
  {
    int num1 = idx_v * 4 * 16 + idx_u * 16;
    UBER_RGBA_ByteArray uberRgbaByteArray = new UBER_RGBA_ByteArray();
    uberRgbaByteArray.R = r;
    uberRgbaByteArray.G = g;
    uberRgbaByteArray.B = b;
    uberRgbaByteArray.A = a;
    byte[] numArray1 = rawTexdata;
    int index1 = num1;
    int num2 = index1 + 1;
    int byte0 = (int) uberRgbaByteArray.Byte0;
    numArray1[index1] = (byte) byte0;
    byte[] numArray2 = rawTexdata;
    int index2 = num2;
    int num3 = index2 + 1;
    int byte1 = (int) uberRgbaByteArray.Byte1;
    numArray2[index2] = (byte) byte1;
    byte[] numArray3 = rawTexdata;
    int index3 = num3;
    int num4 = index3 + 1;
    int byte2 = (int) uberRgbaByteArray.Byte2;
    numArray3[index3] = (byte) byte2;
    byte[] numArray4 = rawTexdata;
    int index4 = num4;
    int num5 = index4 + 1;
    int byte3 = (int) uberRgbaByteArray.Byte3;
    numArray4[index4] = (byte) byte3;
    byte[] numArray5 = rawTexdata;
    int index5 = num5;
    int num6 = index5 + 1;
    int byte4 = (int) uberRgbaByteArray.Byte4;
    numArray5[index5] = (byte) byte4;
    byte[] numArray6 = rawTexdata;
    int index6 = num6;
    int num7 = index6 + 1;
    int byte5 = (int) uberRgbaByteArray.Byte5;
    numArray6[index6] = (byte) byte5;
    byte[] numArray7 = rawTexdata;
    int index7 = num7;
    int num8 = index7 + 1;
    int byte6 = (int) uberRgbaByteArray.Byte6;
    numArray7[index7] = (byte) byte6;
    byte[] numArray8 = rawTexdata;
    int index8 = num8;
    int num9 = index8 + 1;
    int byte7 = (int) uberRgbaByteArray.Byte7;
    numArray8[index8] = (byte) byte7;
    byte[] numArray9 = rawTexdata;
    int index9 = num9;
    int num10 = index9 + 1;
    int byte8 = (int) uberRgbaByteArray.Byte8;
    numArray9[index9] = (byte) byte8;
    byte[] numArray10 = rawTexdata;
    int index10 = num10;
    int num11 = index10 + 1;
    int byte9 = (int) uberRgbaByteArray.Byte9;
    numArray10[index10] = (byte) byte9;
    byte[] numArray11 = rawTexdata;
    int index11 = num11;
    int num12 = index11 + 1;
    int byte10 = (int) uberRgbaByteArray.Byte10;
    numArray11[index11] = (byte) byte10;
    byte[] numArray12 = rawTexdata;
    int index12 = num12;
    int num13 = index12 + 1;
    int byte11 = (int) uberRgbaByteArray.Byte11;
    numArray12[index12] = (byte) byte11;
    byte[] numArray13 = rawTexdata;
    int index13 = num13;
    int num14 = index13 + 1;
    int byte12 = (int) uberRgbaByteArray.Byte12;
    numArray13[index13] = (byte) byte12;
    byte[] numArray14 = rawTexdata;
    int index14 = num14;
    int num15 = index14 + 1;
    int byte13 = (int) uberRgbaByteArray.Byte13;
    numArray14[index14] = (byte) byte13;
    byte[] numArray15 = rawTexdata;
    int index15 = num15;
    int num16 = index15 + 1;
    int byte14 = (int) uberRgbaByteArray.Byte14;
    numArray15[index15] = (byte) byte14;
    byte[] numArray16 = rawTexdata;
    int index16 = num16;
    int num17 = index16 + 1;
    int byte15 = (int) uberRgbaByteArray.Byte15;
    numArray16[index16] = (byte) byte15;
  }

  public void OnEnable()
  {
    this.SetupTranslucencyValues();
    if (this.NotifyDecals())
      return;
    if ((UnityEngine.Object) this.mycam == (UnityEngine.Object) null)
    {
      this.mycam = this.GetComponent<Camera>();
      if ((UnityEngine.Object) this.mycam == (UnityEngine.Object) null)
        return;
    }
    this.Initialize();
    Camera.onPreRender += new Camera.CameraCallback(this.SetupCam);
  }

  public void OnDisable()
  {
    this.NotifyDecals();
    this.Cleanup();
  }

  public void OnDestroy()
  {
    this.NotifyDecals();
    this.Cleanup();
  }

  private bool NotifyDecals()
  {
    System.Type type = System.Type.GetType("UBERDecalSystem.DecalManager");
    if (!(type != (System.Type) null) || (!(UnityEngine.Object.FindObjectOfType(type) != (UnityEngine.Object) null) || !(UnityEngine.Object.FindObjectOfType(type) is MonoBehaviour) ? 0 : ((UnityEngine.Object.FindObjectOfType(type) as MonoBehaviour).enabled ? 1 : 0)) == 0)
      return false;
    (UnityEngine.Object.FindObjectOfType(type) as MonoBehaviour).Invoke("OnDisable", 0.0f);
    (UnityEngine.Object.FindObjectOfType(type) as MonoBehaviour).Invoke("OnEnable", 0.0f);
    return true;
  }

  private void Cleanup()
  {
    if ((bool) (UnityEngine.Object) this.TranslucencyPropsTex)
    {
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object) this.TranslucencyPropsTex);
      this.TranslucencyPropsTex = (Texture2D) null;
    }
    if (this.combufPreLight != null)
    {
      if ((bool) (UnityEngine.Object) this.mycam)
      {
        this.mycam.RemoveCommandBuffer(CameraEvent.BeforeReflections, this.combufPreLight);
        this.mycam.RemoveCommandBuffer(CameraEvent.AfterLighting, this.combufPostLight);
      }
      foreach (Camera camera in this.sceneCamsWithBuffer)
      {
        if ((bool) (UnityEngine.Object) camera)
        {
          camera.RemoveCommandBuffer(CameraEvent.BeforeReflections, this.combufPreLight);
          camera.RemoveCommandBuffer(CameraEvent.AfterLighting, this.combufPostLight);
        }
      }
    }
    this.sceneCamsWithBuffer.Clear();
    Camera.onPreRender -= new Camera.CameraCallback(this.SetupCam);
  }

  private void SetupCam(Camera cam)
  {
    bool isSceneCam = false;
    if (!((UnityEngine.Object) cam == (UnityEngine.Object) this.mycam | isSceneCam))
      return;
    this.RefreshComBufs(cam, isSceneCam);
  }

  public void RefreshComBufs(Camera cam, bool isSceneCam)
  {
    if (!(bool) (UnityEngine.Object) cam || this.combufPreLight == null || this.combufPostLight == null)
      return;
    CommandBuffer[] commandBuffers = cam.GetCommandBuffers(CameraEvent.BeforeReflections);
    bool flag = false;
    foreach (CommandBuffer commandBuffer in commandBuffers)
    {
      if (commandBuffer.name == this.combufPreLight.name)
      {
        flag = true;
        break;
      }
    }
    if (flag)
      return;
    cam.AddCommandBuffer(CameraEvent.BeforeReflections, this.combufPreLight);
    cam.AddCommandBuffer(CameraEvent.AfterLighting, this.combufPostLight);
    if (!isSceneCam)
      return;
    this.sceneCamsWithBuffer.Add(cam);
  }

  public void Initialize()
  {
    if (this.combufPreLight != null)
      return;
    int id = Shader.PropertyToID("_UBERPropsBuffer");
    if ((UnityEngine.Object) this.CopyPropsMat == (UnityEngine.Object) null)
    {
      if ((UnityEngine.Object) this.CopyPropsMat != (UnityEngine.Object) null)
        UnityEngine.Object.DestroyImmediate((UnityEngine.Object) this.CopyPropsMat);
      this.CopyPropsMat = new Material(Shader.Find("Hidden/UBER_CopyPropsTexture"));
      this.CopyPropsMat.hideFlags = HideFlags.DontSave;
    }
    this.combufPreLight = new CommandBuffer();
    this.combufPreLight.name = "UBERPropsPrelight";
    this.combufPreLight.GetTemporaryRT(id, -1, -1, 0, FilterMode.Point, RenderTextureFormat.RHalf);
    this.combufPreLight.Blit((RenderTargetIdentifier) BuiltinRenderTextureType.CameraTarget, (RenderTargetIdentifier) id, this.CopyPropsMat);
    this.combufPostLight = new CommandBuffer();
    this.combufPostLight.name = "UBERPropsPostlight";
    this.combufPostLight.ReleaseTemporaryRT(id);
  }
}
