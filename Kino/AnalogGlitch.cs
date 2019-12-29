// Decompiled with JetBrains decompiler
// Type: Kino.AnalogGlitch
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

namespace Kino
{
  [ExecuteInEditMode]
  [RequireComponent(typeof (Camera))]
  [AddComponentMenu("Kino Image Effects/Analog Glitch")]
  public class AnalogGlitch : MonoBehaviour
  {
    [SerializeField]
    [Range(0.0f, 1f)]
    private float _scanLineJitter;
    [SerializeField]
    [Range(0.0f, 1f)]
    private float _verticalJump;
    [SerializeField]
    [Range(0.0f, 1f)]
    private float _horizontalShake;
    [SerializeField]
    [Range(0.0f, 1f)]
    private float _colorDrift;
    [SerializeField]
    private Shader _shader;
    private Material _material;
    private float _verticalJumpTime;

    public float scanLineJitter
    {
      get
      {
        return this._scanLineJitter;
      }
      set
      {
        this._scanLineJitter = value;
      }
    }

    public float verticalJump
    {
      get
      {
        return this._verticalJump;
      }
      set
      {
        this._verticalJump = value;
      }
    }

    public float horizontalShake
    {
      get
      {
        return this._horizontalShake;
      }
      set
      {
        this._horizontalShake = value;
      }
    }

    public float colorDrift
    {
      get
      {
        return this._colorDrift;
      }
      set
      {
        this._colorDrift = value;
      }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
      if ((Object) this._material == (Object) null)
      {
        this._material = new Material(this._shader);
        this._material.hideFlags = HideFlags.DontSave;
      }
      this._verticalJumpTime += (float) ((double) Time.deltaTime * (double) this._verticalJump * 11.3000001907349);
      this._material.SetVector("_ScanLineJitter", (Vector4) new Vector2((float) (1.0 / 500.0 + (double) Mathf.Pow(this._scanLineJitter, 3f) * 0.0500000007450581), Mathf.Clamp01((float) (1.0 - (double) this._scanLineJitter * 1.20000004768372))));
      this._material.SetVector("_VerticalJump", (Vector4) new Vector2(this._verticalJump, this._verticalJumpTime));
      this._material.SetFloat("_HorizontalShake", this._horizontalShake * 0.2f);
      this._material.SetVector("_ColorDrift", (Vector4) new Vector2(this._colorDrift * 0.04f, Time.time * 606.11f));
      Graphics.Blit((Texture) source, destination, this._material);
    }
  }
}
