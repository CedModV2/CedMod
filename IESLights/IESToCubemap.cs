// Decompiled with JetBrains decompiler
// Type: IESLights.IESToCubemap
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

namespace IESLights
{
  [ExecuteInEditMode]
  public class IESToCubemap : MonoBehaviour
  {
    private Material _iesMaterial;
    private Material _horizontalMirrorMaterial;

    private void OnDestroy()
    {
      if (!((Object) this._horizontalMirrorMaterial != (Object) null))
        return;
      Object.DestroyImmediate((Object) this._horizontalMirrorMaterial);
    }

    public void CreateCubemap(
      Texture2D iesTexture,
      IESData iesData,
      int resolution,
      out Cubemap cubemap)
    {
      this.PrepMaterial(iesTexture, iesData);
      this.CreateCubemap(resolution, out cubemap);
    }

    public Color[] CreateRawCubemap(Texture2D iesTexture, IESData iesData, int resolution)
    {
      this.PrepMaterial(iesTexture, iesData);
      RenderTexture[] renderTextureArray = new RenderTexture[6];
      for (int index = 0; index < 6; ++index)
      {
        renderTextureArray[index] = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        renderTextureArray[index].filterMode = FilterMode.Trilinear;
      }
      Camera[] componentsInChildren = this.transform.GetChild(0).GetComponentsInChildren<Camera>();
      for (int index = 0; index < 6; ++index)
      {
        componentsInChildren[index].targetTexture = renderTextureArray[index];
        componentsInChildren[index].Render();
        componentsInChildren[index].targetTexture = (RenderTexture) null;
      }
      RenderTexture temporary = RenderTexture.GetTemporary(resolution * 6, resolution, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      temporary.filterMode = FilterMode.Trilinear;
      if ((Object) this._horizontalMirrorMaterial == (Object) null)
        this._horizontalMirrorMaterial = new Material(Shader.Find("Hidden/IES/HorizontalFlip"));
      RenderTexture.active = temporary;
      for (int index = 0; index < 6; ++index)
      {
        GL.PushMatrix();
        GL.LoadPixelMatrix(0.0f, (float) (resolution * 6), 0.0f, (float) resolution);
        Graphics.DrawTexture(new Rect((float) (index * resolution), 0.0f, (float) resolution, (float) resolution), (Texture) renderTextureArray[index], this._horizontalMirrorMaterial);
        GL.PopMatrix();
      }
      Texture2D texture2D1 = new Texture2D(resolution * 6, resolution, TextureFormat.RGBAFloat, false, true);
      texture2D1.filterMode = FilterMode.Trilinear;
      Texture2D texture2D2 = texture2D1;
      texture2D2.ReadPixels(new Rect(0.0f, 0.0f, (float) texture2D2.width, (float) texture2D2.height), 0, 0);
      Color[] pixels = texture2D2.GetPixels();
      RenderTexture.active = (RenderTexture) null;
      foreach (RenderTexture temp in renderTextureArray)
        RenderTexture.ReleaseTemporary(temp);
      RenderTexture.ReleaseTemporary(temporary);
      Object.DestroyImmediate((Object) texture2D2);
      return pixels;
    }

    private void PrepMaterial(Texture2D iesTexture, IESData iesData)
    {
      if ((Object) this._iesMaterial == (Object) null)
        this._iesMaterial = this.GetComponent<Renderer>().sharedMaterial;
      this._iesMaterial.mainTexture = (Texture) iesTexture;
      this.SetShaderKeywords(iesData, this._iesMaterial);
    }

    private void SetShaderKeywords(IESData iesData, Material iesMaterial)
    {
      if (iesData.VerticalType == VerticalType.Bottom)
      {
        iesMaterial.EnableKeyword("BOTTOM_VERTICAL");
        iesMaterial.DisableKeyword("TOP_VERTICAL");
        iesMaterial.DisableKeyword("FULL_VERTICAL");
      }
      else if (iesData.VerticalType == VerticalType.Top)
      {
        iesMaterial.EnableKeyword("TOP_VERTICAL");
        iesMaterial.DisableKeyword("BOTTOM_VERTICAL");
        iesMaterial.DisableKeyword("FULL_VERTICAL");
      }
      else
      {
        iesMaterial.DisableKeyword("TOP_VERTICAL");
        iesMaterial.DisableKeyword("BOTTOM_VERTICAL");
        iesMaterial.EnableKeyword("FULL_VERTICAL");
      }
      if (iesData.HorizontalType == HorizontalType.None)
      {
        iesMaterial.DisableKeyword("QUAD_HORIZONTAL");
        iesMaterial.DisableKeyword("HALF_HORIZONTAL");
        iesMaterial.DisableKeyword("FULL_HORIZONTAL");
      }
      else if (iesData.HorizontalType == HorizontalType.Quadrant)
      {
        iesMaterial.EnableKeyword("QUAD_HORIZONTAL");
        iesMaterial.DisableKeyword("HALF_HORIZONTAL");
        iesMaterial.DisableKeyword("FULL_HORIZONTAL");
      }
      else if (iesData.HorizontalType == HorizontalType.Half)
      {
        iesMaterial.DisableKeyword("QUAD_HORIZONTAL");
        iesMaterial.EnableKeyword("HALF_HORIZONTAL");
        iesMaterial.DisableKeyword("FULL_HORIZONTAL");
      }
      else
      {
        if (iesData.HorizontalType != HorizontalType.Full)
          return;
        iesMaterial.DisableKeyword("QUAD_HORIZONTAL");
        iesMaterial.DisableKeyword("HALF_HORIZONTAL");
        iesMaterial.EnableKeyword("FULL_HORIZONTAL");
      }
    }

    private void CreateCubemap(int resolution, out Cubemap cubemap)
    {
      ref Cubemap local = ref cubemap;
      Cubemap cubemap1 = new Cubemap(resolution, TextureFormat.ARGB32, false);
      cubemap1.filterMode = FilterMode.Trilinear;
      local = cubemap1;
      this.GetComponent<Camera>().RenderToCubemap(cubemap);
    }
  }
}
