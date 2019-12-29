// Decompiled with JetBrains decompiler
// Type: AmplifyBloom.AmplifyGlare
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

namespace AmplifyBloom
{
  [Serializable]
  public sealed class AmplifyGlare : IAmplifyItem
  {
    [SerializeField]
    private bool m_applyGlare = true;
    [SerializeField]
    private Color _overallTint = Color.white;
    [SerializeField]
    private int m_glareMaxPassCount = 4;
    [SerializeField]
    private float m_perPassDisplacement = 4f;
    [SerializeField]
    private float m_intensity = 0.17f;
    [SerializeField]
    private float m_overallStreakScale = 1f;
    private bool m_isDirty = true;
    public const int MaxLineSamples = 8;
    public const int MaxTotalSamples = 16;
    public const int MaxStarLines = 4;
    public const int MaxPasses = 4;
    public const int MaxCustomGlare = 32;
    [SerializeField]
    private GlareDefData[] m_customGlareDef;
    [SerializeField]
    private int m_customGlareDefIdx;
    [SerializeField]
    private int m_customGlareDefAmount;
    [SerializeField]
    private Gradient m_cromaticAberrationGrad;
    private StarDefData[] m_starDefArr;
    private GlareDefData[] m_glareDefArr;
    private Matrix4x4[] m_weigthsMat;
    private Matrix4x4[] m_offsetsMat;
    private Color m_whiteReference;
    private float m_aTanFoV;
    private AmplifyGlareCache m_amplifyGlareCache;
    [SerializeField]
    private int m_currentWidth;
    [SerializeField]
    private int m_currentHeight;
    [SerializeField]
    private GlareLibType m_currentGlareType;
    [SerializeField]
    private int m_currentGlareIdx;
    private RenderTexture[] _rtBuffer;

    public AmplifyGlare()
    {
      this.m_currentGlareIdx = (int) this.m_currentGlareType;
      this.m_cromaticAberrationGrad = new Gradient();
      this._rtBuffer = new RenderTexture[16];
      this.m_weigthsMat = new Matrix4x4[4];
      this.m_offsetsMat = new Matrix4x4[4];
      this.m_amplifyGlareCache = new AmplifyGlareCache();
      this.m_whiteReference = new Color(0.63f, 0.63f, 0.63f, 0.0f);
      this.m_aTanFoV = Mathf.Atan(0.3926991f);
      this.m_starDefArr = new StarDefData[5]
      {
        new StarDefData(StarLibType.Cross, "Cross", 2, 4, 1f, 0.85f, 0.0f, 0.5f, -1f, 90f),
        new StarDefData(StarLibType.Cross_Filter, "CrossFilter", 2, 4, 1f, 0.95f, 0.0f, 0.5f, -1f, 90f),
        new StarDefData(StarLibType.Snow_Cross, "snowCross", 3, 4, 1f, 0.96f, 0.349f, 0.5f, -1f, -1f),
        new StarDefData(StarLibType.Vertical, "Vertical", 1, 4, 1f, 0.96f, 0.0f, 0.0f, -1f, -1f),
        new StarDefData(StarLibType.Sunny_Cross, "SunnyCross", 4, 4, 1f, 0.88f, 0.0f, 0.0f, 0.95f, 45f)
      };
      this.m_glareDefArr = new GlareDefData[9]
      {
        new GlareDefData(StarLibType.Cross, 0.0f, 0.5f),
        new GlareDefData(StarLibType.Cross_Filter, 0.44f, 0.5f),
        new GlareDefData(StarLibType.Cross_Filter, 1.22f, 1.5f),
        new GlareDefData(StarLibType.Snow_Cross, 0.17f, 0.5f),
        new GlareDefData(StarLibType.Snow_Cross, 0.7f, 1.5f),
        new GlareDefData(StarLibType.Sunny_Cross, 0.0f, 0.5f),
        new GlareDefData(StarLibType.Sunny_Cross, 0.79f, 1.5f),
        new GlareDefData(StarLibType.Vertical, 1.57f, 0.5f),
        new GlareDefData(StarLibType.Vertical, 0.0f, 0.5f)
      };
    }

    public void Init()
    {
      if (this.m_cromaticAberrationGrad.alphaKeys.Length != 0 || this.m_cromaticAberrationGrad.colorKeys.Length != 0)
        return;
      this.m_cromaticAberrationGrad.SetKeys(new GradientColorKey[5]
      {
        new GradientColorKey(Color.white, 0.0f),
        new GradientColorKey(Color.blue, 0.25f),
        new GradientColorKey(Color.green, 0.5f),
        new GradientColorKey(Color.yellow, 0.75f),
        new GradientColorKey(Color.red, 1f)
      }, new GradientAlphaKey[5]
      {
        new GradientAlphaKey(1f, 0.0f),
        new GradientAlphaKey(1f, 0.25f),
        new GradientAlphaKey(1f, 0.5f),
        new GradientAlphaKey(1f, 0.75f),
        new GradientAlphaKey(1f, 1f)
      });
    }

    public void Destroy()
    {
      for (int index = 0; index < this.m_starDefArr.Length; ++index)
        this.m_starDefArr[index].Destroy();
      this.m_glareDefArr = (GlareDefData[]) null;
      this.m_weigthsMat = (Matrix4x4[]) null;
      this.m_offsetsMat = (Matrix4x4[]) null;
      for (int index = 0; index < this._rtBuffer.Length; ++index)
      {
        if ((UnityEngine.Object) this._rtBuffer[index] != (UnityEngine.Object) null)
        {
          AmplifyUtils.ReleaseTempRenderTarget(this._rtBuffer[index]);
          this._rtBuffer[index] = (RenderTexture) null;
        }
      }
      this._rtBuffer = (RenderTexture[]) null;
      this.m_amplifyGlareCache.Destroy();
      this.m_amplifyGlareCache = (AmplifyGlareCache) null;
    }

    public void SetDirty()
    {
      this.m_isDirty = true;
    }

    public void OnRenderFromCache(
      RenderTexture source,
      RenderTexture dest,
      Material material,
      float glareIntensity,
      float cameraRotation)
    {
      for (int index = 0; index < this.m_amplifyGlareCache.TotalRT; ++index)
        this._rtBuffer[index] = AmplifyUtils.GetTempRenderTarget(source.width, source.height);
      int index1 = 0;
      for (int index2 = 0; index2 < this.m_amplifyGlareCache.StarDef.StarlinesCount; ++index2)
      {
        for (int index3 = 0; index3 < this.m_amplifyGlareCache.CurrentPassCount; ++index3)
        {
          this.UpdateMatrixesForPass(material, this.m_amplifyGlareCache.Starlines[index2].Passes[index3].Offsets, this.m_amplifyGlareCache.Starlines[index2].Passes[index3].Weights, glareIntensity, cameraRotation * this.m_amplifyGlareCache.StarDef.CameraRotInfluence);
          if (index3 == 0)
            Graphics.Blit((Texture) source, this._rtBuffer[index1], material, 2);
          else
            Graphics.Blit((Texture) this._rtBuffer[index1 - 1], this._rtBuffer[index1], material, 2);
          ++index1;
        }
      }
      for (int index2 = 0; index2 < this.m_amplifyGlareCache.StarDef.StarlinesCount; ++index2)
      {
        material.SetVector(AmplifyUtils.AnamorphicGlareWeightsStr[index2], this.m_amplifyGlareCache.AverageWeight);
        int index3 = (index2 + 1) * this.m_amplifyGlareCache.CurrentPassCount - 1;
        material.SetTexture(AmplifyUtils.AnamorphicRTS[index2], (Texture) this._rtBuffer[index3]);
      }
      int pass = 19 + this.m_amplifyGlareCache.StarDef.StarlinesCount - 1;
      dest.DiscardContents();
      Graphics.Blit((Texture) this._rtBuffer[0], dest, material, pass);
      for (int index2 = 0; index2 < this._rtBuffer.Length; ++index2)
      {
        AmplifyUtils.ReleaseTempRenderTarget(this._rtBuffer[index2]);
        this._rtBuffer[index2] = (RenderTexture) null;
      }
    }

    public void UpdateMatrixesForPass(
      Material material,
      Vector4[] offsets,
      Vector4[] weights,
      float glareIntensity,
      float rotation)
    {
      float num1 = Mathf.Cos(rotation);
      float num2 = Mathf.Sin(rotation);
      for (int index1 = 0; index1 < 16; ++index1)
      {
        int index2 = index1 >> 2;
        int index3 = index1 & 3;
        this.m_offsetsMat[index2][index3, 0] = (float) ((double) offsets[index1].x * (double) num1 - (double) offsets[index1].y * (double) num2);
        this.m_offsetsMat[index2][index3, 1] = (float) ((double) offsets[index1].x * (double) num2 + (double) offsets[index1].y * (double) num1);
        this.m_weigthsMat[index2][index3, 0] = glareIntensity * weights[index1].x;
        this.m_weigthsMat[index2][index3, 1] = glareIntensity * weights[index1].y;
        this.m_weigthsMat[index2][index3, 2] = glareIntensity * weights[index1].z;
      }
      for (int index = 0; index < 4; ++index)
      {
        material.SetMatrix(AmplifyUtils.AnamorphicGlareOffsetsMatStr[index], this.m_offsetsMat[index]);
        material.SetMatrix(AmplifyUtils.AnamorphicGlareWeightsMatStr[index], this.m_weigthsMat[index]);
      }
    }

    public void OnRenderImage(
      Material material,
      RenderTexture source,
      RenderTexture dest,
      float cameraRot)
    {
      Graphics.Blit((Texture) Texture2D.blackTexture, dest);
      if (this.m_isDirty || this.m_currentWidth != source.width || this.m_currentHeight != source.height)
      {
        this.m_isDirty = false;
        this.m_currentWidth = source.width;
        this.m_currentHeight = source.height;
        bool flag = false;
        GlareDefData glareDefData;
        if (this.m_currentGlareType == GlareLibType.Custom)
        {
          if (this.m_customGlareDef != null && this.m_customGlareDef.Length != 0)
          {
            glareDefData = this.m_customGlareDef[this.m_customGlareDefIdx];
            flag = true;
          }
          else
            glareDefData = this.m_glareDefArr[0];
        }
        else
          glareDefData = this.m_glareDefArr[this.m_currentGlareIdx];
        this.m_amplifyGlareCache.GlareDef = glareDefData;
        float width = (float) source.width;
        float height = (float) source.height;
        StarDefData starDefData = flag ? glareDefData.CustomStarData : this.m_starDefArr[(int) glareDefData.StarType];
        this.m_amplifyGlareCache.StarDef = starDefData;
        int num1 = this.m_glareMaxPassCount < starDefData.PassCount ? this.m_glareMaxPassCount : starDefData.PassCount;
        this.m_amplifyGlareCache.CurrentPassCount = num1;
        float num2 = glareDefData.StarInclination + starDefData.Inclination;
        for (int index1 = 0; index1 < this.m_glareMaxPassCount; ++index1)
        {
          float t = (float) (index1 + 1) / (float) this.m_glareMaxPassCount;
          for (int index2 = 0; index2 < 8; ++index2)
          {
            Color b = this._overallTint * Color.Lerp(this.m_cromaticAberrationGrad.Evaluate((float) index2 / 7f), this.m_whiteReference, t);
            this.m_amplifyGlareCache.CromaticAberrationMat[index1, index2] = (Vector4) Color.Lerp(this.m_whiteReference, b, glareDefData.ChromaticAberration);
          }
        }
        this.m_amplifyGlareCache.TotalRT = starDefData.StarlinesCount * num1;
        for (int index = 0; index < this.m_amplifyGlareCache.TotalRT; ++index)
          this._rtBuffer[index] = AmplifyUtils.GetTempRenderTarget(source.width, source.height);
        int index3 = 0;
        for (int index1 = 0; index1 < starDefData.StarlinesCount; ++index1)
        {
          StarLineData starLineData = starDefData.StarLinesArr[index1];
          double num3 = (double) num2 + (double) starLineData.Inclination;
          float num4 = Mathf.Sin((float) num3);
          float num5 = Mathf.Cos((float) num3);
          Vector2 vector2 = new Vector2();
          vector2.x = (float) ((double) num5 / (double) width * ((double) starLineData.SampleLength * (double) this.m_overallStreakScale));
          vector2.y = (float) ((double) num4 / (double) height * ((double) starLineData.SampleLength * (double) this.m_overallStreakScale));
          float num6 = (float) (((double) this.m_aTanFoV + 0.100000001490116) * 280.0 / ((double) width + (double) height) * 1.20000004768372);
          for (int index2 = 0; index2 < num1; ++index2)
          {
            for (int index4 = 0; index4 < 8; ++index4)
            {
              float num7 = Mathf.Pow(starLineData.Attenuation, num6 * (float) index4);
              this.m_amplifyGlareCache.Starlines[index1].Passes[index2].Weights[index4] = this.m_amplifyGlareCache.CromaticAberrationMat[num1 - 1 - index2, index4] * num7 * ((float) index2 + 1f) * 0.5f;
              this.m_amplifyGlareCache.Starlines[index1].Passes[index2].Offsets[index4].x = vector2.x * (float) index4;
              this.m_amplifyGlareCache.Starlines[index1].Passes[index2].Offsets[index4].y = vector2.y * (float) index4;
              if ((double) Mathf.Abs(this.m_amplifyGlareCache.Starlines[index1].Passes[index2].Offsets[index4].x) >= 0.899999976158142 || (double) Mathf.Abs(this.m_amplifyGlareCache.Starlines[index1].Passes[index2].Offsets[index4].y) >= 0.899999976158142)
              {
                this.m_amplifyGlareCache.Starlines[index1].Passes[index2].Offsets[index4].x = 0.0f;
                this.m_amplifyGlareCache.Starlines[index1].Passes[index2].Offsets[index4].y = 0.0f;
                this.m_amplifyGlareCache.Starlines[index1].Passes[index2].Weights[index4] *= 0.0f;
              }
            }
            for (int index4 = 8; index4 < 16; ++index4)
            {
              this.m_amplifyGlareCache.Starlines[index1].Passes[index2].Offsets[index4] = -this.m_amplifyGlareCache.Starlines[index1].Passes[index2].Offsets[index4 - 8];
              this.m_amplifyGlareCache.Starlines[index1].Passes[index2].Weights[index4] = this.m_amplifyGlareCache.Starlines[index1].Passes[index2].Weights[index4 - 8];
            }
            this.UpdateMatrixesForPass(material, this.m_amplifyGlareCache.Starlines[index1].Passes[index2].Offsets, this.m_amplifyGlareCache.Starlines[index1].Passes[index2].Weights, this.m_intensity, starDefData.CameraRotInfluence * cameraRot);
            if (index2 == 0)
              Graphics.Blit((Texture) source, this._rtBuffer[index3], material, 2);
            else
              Graphics.Blit((Texture) this._rtBuffer[index3 - 1], this._rtBuffer[index3], material, 2);
            ++index3;
            vector2 *= this.m_perPassDisplacement;
            num6 *= this.m_perPassDisplacement;
          }
        }
        this.m_amplifyGlareCache.AverageWeight = Vector4.one / (float) starDefData.StarlinesCount;
        for (int index1 = 0; index1 < starDefData.StarlinesCount; ++index1)
        {
          material.SetVector(AmplifyUtils.AnamorphicGlareWeightsStr[index1], this.m_amplifyGlareCache.AverageWeight);
          int index2 = (index1 + 1) * num1 - 1;
          material.SetTexture(AmplifyUtils.AnamorphicRTS[index1], (Texture) this._rtBuffer[index2]);
        }
        int pass = 19 + starDefData.StarlinesCount - 1;
        dest.DiscardContents();
        Graphics.Blit((Texture) this._rtBuffer[0], dest, material, pass);
        for (int index1 = 0; index1 < this._rtBuffer.Length; ++index1)
        {
          AmplifyUtils.ReleaseTempRenderTarget(this._rtBuffer[index1]);
          this._rtBuffer[index1] = (RenderTexture) null;
        }
      }
      else
        this.OnRenderFromCache(source, dest, material, this.m_intensity, cameraRot);
    }

    public GlareLibType CurrentGlare
    {
      get
      {
        return this.m_currentGlareType;
      }
      set
      {
        if (this.m_currentGlareType == value)
          return;
        this.m_currentGlareType = value;
        this.m_currentGlareIdx = (int) value;
        this.m_isDirty = true;
      }
    }

    public int GlareMaxPassCount
    {
      get
      {
        return this.m_glareMaxPassCount;
      }
      set
      {
        this.m_glareMaxPassCount = value;
        this.m_isDirty = true;
      }
    }

    public float PerPassDisplacement
    {
      get
      {
        return this.m_perPassDisplacement;
      }
      set
      {
        this.m_perPassDisplacement = value;
        this.m_isDirty = true;
      }
    }

    public float Intensity
    {
      get
      {
        return this.m_intensity;
      }
      set
      {
        this.m_intensity = (double) value < 0.0 ? 0.0f : value;
        this.m_isDirty = true;
      }
    }

    public Color OverallTint
    {
      get
      {
        return this._overallTint;
      }
      set
      {
        this._overallTint = value;
        this.m_isDirty = true;
      }
    }

    public bool ApplyLensGlare
    {
      get
      {
        return this.m_applyGlare;
      }
      set
      {
        this.m_applyGlare = value;
      }
    }

    public Gradient CromaticColorGradient
    {
      get
      {
        return this.m_cromaticAberrationGrad;
      }
      set
      {
        this.m_cromaticAberrationGrad = value;
        this.m_isDirty = true;
      }
    }

    public float OverallStreakScale
    {
      get
      {
        return this.m_overallStreakScale;
      }
      set
      {
        this.m_overallStreakScale = value;
        this.m_isDirty = true;
      }
    }

    public GlareDefData[] CustomGlareDef
    {
      get
      {
        return this.m_customGlareDef;
      }
      set
      {
        this.m_customGlareDef = value;
      }
    }

    public int CustomGlareDefIdx
    {
      get
      {
        return this.m_customGlareDefIdx;
      }
      set
      {
        this.m_customGlareDefIdx = value;
      }
    }

    public int CustomGlareDefAmount
    {
      get
      {
        return this.m_customGlareDefAmount;
      }
      set
      {
        if (value == this.m_customGlareDefAmount)
          return;
        if (value == 0)
        {
          this.m_customGlareDef = (GlareDefData[]) null;
          this.m_customGlareDefIdx = 0;
          this.m_customGlareDefAmount = 0;
        }
        else
        {
          GlareDefData[] glareDefDataArray = new GlareDefData[value];
          for (int index = 0; index < value; ++index)
            glareDefDataArray[index] = index >= this.m_customGlareDefAmount ? new GlareDefData() : this.m_customGlareDef[index];
          this.m_customGlareDefIdx = Mathf.Clamp(this.m_customGlareDefIdx, 0, value - 1);
          this.m_customGlareDef = glareDefDataArray;
          this.m_customGlareDefAmount = value;
        }
      }
    }
  }
}
