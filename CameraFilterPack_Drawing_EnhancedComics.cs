// Decompiled with JetBrains decompiler
// Type: CameraFilterPack_Drawing_EnhancedComics
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Camera Filter Pack/Drawing/EnhancedComics")]
public class CameraFilterPack_Drawing_EnhancedComics : MonoBehaviour
{
  private float TimeX = 1f;
  [Range(0.0f, 1f)]
  public float DotSize = 0.15f;
  [Range(0.0f, 1f)]
  public float _ColorR = 0.9f;
  [Range(0.0f, 1f)]
  public float _ColorG = 0.4f;
  [Range(0.0f, 1f)]
  public float _ColorB = 0.4f;
  [Range(0.0f, 1f)]
  public float _Blood = 0.5f;
  [Range(0.0f, 1f)]
  public float _SmoothStart = 0.02f;
  [Range(0.0f, 1f)]
  public float _SmoothEnd = 0.1f;
  public Color ColorRGB = new Color(1f, 0.0f, 0.0f);
  public Shader SCShader;
  private Material SCMaterial;

  private Material material
  {
    get
    {
      if ((Object) this.SCMaterial == (Object) null)
      {
        this.SCMaterial = new Material(this.SCShader);
        this.SCMaterial.hideFlags = HideFlags.HideAndDontSave;
      }
      return this.SCMaterial;
    }
  }

  private void Start()
  {
    this.SCShader = Shader.Find("CameraFilterPack/Drawing_EnhancedComics");
    if (SystemInfo.supportsImageEffects)
      return;
    this.enabled = false;
  }

  private void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
  {
    if ((Object) this.SCShader != (Object) null)
    {
      this.TimeX += Time.deltaTime;
      if ((double) this.TimeX > 100.0)
        this.TimeX = 0.0f;
      this.material.SetFloat("_TimeX", this.TimeX);
      this.material.SetFloat("_DotSize", this.DotSize);
      this.material.SetFloat("_ColorR", this._ColorR);
      this.material.SetFloat("_ColorG", this._ColorG);
      this.material.SetFloat("_ColorB", this._ColorB);
      this.material.SetFloat("_Blood", this._Blood);
      this.material.SetColor("_ColorRGB", this.ColorRGB);
      this.material.SetFloat("_SmoothStart", this._SmoothStart);
      this.material.SetFloat("_SmoothEnd", this._SmoothEnd);
      Graphics.Blit((Texture) sourceTexture, destTexture, this.material);
    }
    else
      Graphics.Blit((Texture) sourceTexture, destTexture);
  }

  private void Update()
  {
  }

  private void OnDisable()
  {
    if (!(bool) (Object) this.SCMaterial)
      return;
    Object.DestroyImmediate((Object) this.SCMaterial);
  }
}
