// Decompiled with JetBrains decompiler
// Type: Kino.DigitalGlitch
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

namespace Kino
{
  [ExecuteInEditMode]
  [RequireComponent(typeof (Camera))]
  [AddComponentMenu("Kino Image Effects/Digital Glitch")]
  public class DigitalGlitch : MonoBehaviour
  {
    [SerializeField]
    [Range(0.0f, 1f)]
    private float _intensity;
    [SerializeField]
    private Shader _shader;
    private Material _material;
    private Texture2D _noiseTexture;
    private RenderTexture _trashFrame1;
    private RenderTexture _trashFrame2;

    public float intensity
    {
      get
      {
        return this._intensity;
      }
      set
      {
        this._intensity = value;
      }
    }

    private static Color RandomColor()
    {
      return new Color(Random.value, Random.value, Random.value, Random.value);
    }

    private void SetUpResources()
    {
      if ((Object) this._material != (Object) null)
        return;
      this._material = new Material(this._shader);
      this._material.hideFlags = HideFlags.DontSave;
      this._noiseTexture = new Texture2D(64, 32, TextureFormat.ARGB32, false);
      this._noiseTexture.hideFlags = HideFlags.DontSave;
      this._noiseTexture.wrapMode = TextureWrapMode.Clamp;
      this._noiseTexture.filterMode = FilterMode.Point;
      this._trashFrame1 = new RenderTexture(Screen.width, Screen.height, 0);
      this._trashFrame2 = new RenderTexture(Screen.width, Screen.height, 0);
      this._trashFrame1.hideFlags = HideFlags.DontSave;
      this._trashFrame2.hideFlags = HideFlags.DontSave;
      this.UpdateNoiseTexture();
    }

    private void UpdateNoiseTexture()
    {
      Color color = DigitalGlitch.RandomColor();
      for (int y = 0; y < this._noiseTexture.height; ++y)
      {
        for (int x = 0; x < this._noiseTexture.width; ++x)
        {
          if ((double) Random.value > 0.889999985694885)
            color = DigitalGlitch.RandomColor();
          this._noiseTexture.SetPixel(x, y, color);
        }
      }
      this._noiseTexture.Apply();
    }

    private void Update()
    {
      if ((double) Random.value <= (double) Mathf.Lerp(0.9f, 0.5f, this._intensity))
        return;
      this.SetUpResources();
      this.UpdateNoiseTexture();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
      this.SetUpResources();
      int frameCount = Time.frameCount;
      if (frameCount % 13 == 0)
        Graphics.Blit((Texture) source, this._trashFrame1);
      if (frameCount % 73 == 0)
        Graphics.Blit((Texture) source, this._trashFrame2);
      this._material.SetFloat("_Intensity", this._intensity);
      this._material.SetTexture("_NoiseTex", (Texture) this._noiseTexture);
      this._material.SetTexture("_TrashTex", (double) Random.value > 0.5 ? (Texture) this._trashFrame1 : (Texture) this._trashFrame2);
      Graphics.Blit((Texture) source, destination, this._material);
    }
  }
}
