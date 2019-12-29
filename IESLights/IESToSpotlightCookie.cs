// Decompiled with JetBrains decompiler
// Type: IESLights.IESToSpotlightCookie
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Linq;
using UnityEngine;

namespace IESLights
{
  [ExecuteInEditMode]
  public class IESToSpotlightCookie : MonoBehaviour
  {
    private Material _spotlightMaterial;
    private Material _fadeSpotlightEdgesMaterial;
    private Material _verticalFlipMaterial;

    private void OnDestroy()
    {
      if ((UnityEngine.Object) this._spotlightMaterial != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) this._spotlightMaterial);
      if ((UnityEngine.Object) this._fadeSpotlightEdgesMaterial != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) this._fadeSpotlightEdgesMaterial);
      if (!((UnityEngine.Object) this._verticalFlipMaterial != (UnityEngine.Object) null))
        return;
      UnityEngine.Object.Destroy((UnityEngine.Object) this._verticalFlipMaterial);
    }

    public void CreateSpotlightCookie(
      Texture2D iesTexture,
      IESData iesData,
      int resolution,
      bool applyVignette,
      bool flipVertically,
      out Texture2D cookie)
    {
      if (iesData.PhotometricType != PhotometricType.TypeA)
      {
        if ((UnityEngine.Object) this._spotlightMaterial == (UnityEngine.Object) null)
          this._spotlightMaterial = new Material(Shader.Find("Hidden/IES/IESToSpotlightCookie"));
        this.CalculateAndSetSpotHeight(iesData);
        this.SetShaderKeywords(iesData, applyVignette);
        cookie = this.CreateTexture(iesTexture, resolution, flipVertically);
      }
      else
      {
        if ((UnityEngine.Object) this._fadeSpotlightEdgesMaterial == (UnityEngine.Object) null)
          this._fadeSpotlightEdgesMaterial = new Material(Shader.Find("Hidden/IES/FadeSpotlightCookieEdges"));
        float verticalCenter = applyVignette ? this.CalculateCookieVerticalCenter(iesData) : 0.0f;
        Vector2 vector2 = applyVignette ? this.CalculateCookieFadeEllipse(iesData) : Vector2.zero;
        cookie = this.BlitToTargetSize(iesTexture, resolution, vector2.x, vector2.y, verticalCenter, applyVignette, flipVertically);
      }
    }

    private float CalculateCookieVerticalCenter(IESData iesData)
    {
      return (float) (1.0 - (double) iesData.PadBeforeAmount / (double) iesData.NormalizedValues[0].Count) - (float) ((double) (iesData.NormalizedValues[0].Count - iesData.PadBeforeAmount - iesData.PadAfterAmount) / (double) iesData.NormalizedValues.Count / 2.0);
    }

    private Vector2 CalculateCookieFadeEllipse(IESData iesData)
    {
      if (iesData.HorizontalAngles.Count > iesData.VerticalAngles.Count)
        return new Vector2(0.5f, (float) (0.5 * ((double) (iesData.NormalizedValues[0].Count - iesData.PadBeforeAmount - iesData.PadAfterAmount) / (double) iesData.NormalizedValues[0].Count)));
      return iesData.HorizontalAngles.Count < iesData.VerticalAngles.Count ? new Vector2((float) (0.5 * ((double) iesData.HorizontalAngles.Max() - (double) iesData.HorizontalAngles.Min()) / ((double) iesData.VerticalAngles.Max() - (double) iesData.VerticalAngles.Min())), 0.5f) : new Vector2(0.5f, 0.5f);
    }

    private Texture2D CreateTexture(
      Texture2D iesTexture,
      int resolution,
      bool flipVertically)
    {
      RenderTexture temporary1 = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      temporary1.filterMode = FilterMode.Trilinear;
      temporary1.DiscardContents();
      RenderTexture.active = temporary1;
      Graphics.Blit((Texture) iesTexture, this._spotlightMaterial);
      if (flipVertically)
      {
        RenderTexture temporary2 = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        Graphics.Blit((Texture) temporary1, temporary2);
        this.FlipVertically((Texture) temporary2, temporary1);
        RenderTexture.ReleaseTemporary(temporary2);
      }
      Texture2D texture2D = new Texture2D(resolution, resolution, TextureFormat.RGBAFloat, false, true);
      texture2D.filterMode = FilterMode.Trilinear;
      texture2D.wrapMode = TextureWrapMode.Clamp;
      texture2D.ReadPixels(new Rect(0.0f, 0.0f, (float) resolution, (float) resolution), 0, 0);
      texture2D.Apply();
      RenderTexture.active = (RenderTexture) null;
      RenderTexture.ReleaseTemporary(temporary1);
      return texture2D;
    }

    private Texture2D BlitToTargetSize(
      Texture2D iesTexture,
      int resolution,
      float horizontalFadeDistance,
      float verticalFadeDistance,
      float verticalCenter,
      bool applyVignette,
      bool flipVertically)
    {
      if (applyVignette)
      {
        this._fadeSpotlightEdgesMaterial.SetFloat("_HorizontalFadeDistance", horizontalFadeDistance);
        this._fadeSpotlightEdgesMaterial.SetFloat("_VerticalFadeDistance", verticalFadeDistance);
        this._fadeSpotlightEdgesMaterial.SetFloat("_VerticalCenter", verticalCenter);
      }
      RenderTexture temporary = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      temporary.filterMode = FilterMode.Trilinear;
      temporary.DiscardContents();
      if (applyVignette)
      {
        RenderTexture.active = temporary;
        Graphics.Blit((Texture) iesTexture, this._fadeSpotlightEdgesMaterial);
      }
      else if (flipVertically)
        this.FlipVertically((Texture) iesTexture, temporary);
      else
        Graphics.Blit((Texture) iesTexture, temporary);
      Texture2D texture2D = new Texture2D(resolution, resolution, TextureFormat.RGBAFloat, false, true);
      texture2D.filterMode = FilterMode.Trilinear;
      texture2D.wrapMode = TextureWrapMode.Clamp;
      texture2D.ReadPixels(new Rect(0.0f, 0.0f, (float) resolution, (float) resolution), 0, 0);
      texture2D.Apply();
      RenderTexture.active = (RenderTexture) null;
      RenderTexture.ReleaseTemporary(temporary);
      return texture2D;
    }

    private void FlipVertically(Texture iesTexture, RenderTexture renderTarget)
    {
      if ((UnityEngine.Object) this._verticalFlipMaterial == (UnityEngine.Object) null)
        this._verticalFlipMaterial = new Material(Shader.Find("Hidden/IES/VerticalFlip"));
      Graphics.Blit(iesTexture, renderTarget, this._verticalFlipMaterial);
    }

    private void CalculateAndSetSpotHeight(IESData iesData)
    {
      this._spotlightMaterial.SetFloat("_SpotHeight", 0.5f / Mathf.Tan(iesData.HalfSpotlightFov * ((float) Math.PI / 180f)));
    }

    private void SetShaderKeywords(IESData iesData, bool applyVignette)
    {
      if (applyVignette)
        this._spotlightMaterial.EnableKeyword("VIGNETTE");
      else
        this._spotlightMaterial.DisableKeyword("VIGNETTE");
      if (iesData.VerticalType == VerticalType.Top)
        this._spotlightMaterial.EnableKeyword("TOP_VERTICAL");
      else
        this._spotlightMaterial.DisableKeyword("TOP_VERTICAL");
      if (iesData.HorizontalType == HorizontalType.None)
      {
        this._spotlightMaterial.DisableKeyword("QUAD_HORIZONTAL");
        this._spotlightMaterial.DisableKeyword("HALF_HORIZONTAL");
        this._spotlightMaterial.DisableKeyword("FULL_HORIZONTAL");
      }
      else if (iesData.HorizontalType == HorizontalType.Quadrant)
      {
        this._spotlightMaterial.EnableKeyword("QUAD_HORIZONTAL");
        this._spotlightMaterial.DisableKeyword("HALF_HORIZONTAL");
        this._spotlightMaterial.DisableKeyword("FULL_HORIZONTAL");
      }
      else if (iesData.HorizontalType == HorizontalType.Half)
      {
        this._spotlightMaterial.DisableKeyword("QUAD_HORIZONTAL");
        this._spotlightMaterial.EnableKeyword("HALF_HORIZONTAL");
        this._spotlightMaterial.DisableKeyword("FULL_HORIZONTAL");
      }
      else
      {
        if (iesData.HorizontalType != HorizontalType.Full)
          return;
        this._spotlightMaterial.DisableKeyword("QUAD_HORIZONTAL");
        this._spotlightMaterial.DisableKeyword("HALF_HORIZONTAL");
        this._spotlightMaterial.EnableKeyword("FULL_HORIZONTAL");
      }
    }
  }
}
