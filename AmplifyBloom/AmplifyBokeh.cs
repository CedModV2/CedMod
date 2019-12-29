// Decompiled with JetBrains decompiler
// Type: AmplifyBloom.AmplifyBokeh
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AmplifyBloom
{
  [Serializable]
  public sealed class AmplifyBokeh : IAmplifyItem, ISerializationCallbackReceiver
  {
    [SerializeField]
    private float m_bokehSampleRadius = 0.5f;
    [SerializeField]
    private Vector4 m_bokehCameraProperties = new Vector4(0.05f, 0.018f, 1.34f, 0.18f);
    [SerializeField]
    private ApertureShape m_apertureShape = ApertureShape.Hexagon;
    private const int PerPassSampleCount = 8;
    [SerializeField]
    private bool m_isActive;
    [SerializeField]
    private bool m_applyOnBloomSource;
    [SerializeField]
    private float m_offsetRotation;
    private List<AmplifyBokehData> m_bokehOffsets;

    public AmplifyBokeh()
    {
      this.m_bokehOffsets = new List<AmplifyBokehData>();
      this.CreateBokehOffsets(ApertureShape.Hexagon);
    }

    public void Destroy()
    {
      for (int index = 0; index < this.m_bokehOffsets.Count; ++index)
        this.m_bokehOffsets[index].Destroy();
    }

    private void CreateBokehOffsets(ApertureShape shape)
    {
      this.m_bokehOffsets.Clear();
      switch (shape)
      {
        case ApertureShape.Square:
          this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation)));
          this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation + 90f)));
          break;
        case ApertureShape.Hexagon:
          this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation)));
          this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation - 75f)));
          this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation + 75f)));
          break;
        case ApertureShape.Octagon:
          this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation)));
          this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation + 65f)));
          this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation + 90f)));
          this.m_bokehOffsets.Add(new AmplifyBokehData(this.CalculateBokehSamples(8, this.m_offsetRotation + 115f)));
          break;
      }
    }

    private Vector4[] CalculateBokehSamples(int sampleCount, float angle)
    {
      Vector4[] vector4Array = new Vector4[sampleCount];
      float f = (float) Math.PI / 180f * angle;
      float num = (float) Screen.width / (float) Screen.height;
      Vector4 b = new Vector4(this.m_bokehSampleRadius * Mathf.Cos(f), this.m_bokehSampleRadius * Mathf.Sin(f));
      b.x /= num;
      for (int index = 0; index < sampleCount; ++index)
      {
        float t = (float) index / ((float) sampleCount - 1f);
        vector4Array[index] = Vector4.Lerp(-b, b, t);
      }
      return vector4Array;
    }

    public void ApplyBokehFilter(RenderTexture source, Material material)
    {
      for (int index = 0; index < this.m_bokehOffsets.Count; ++index)
        this.m_bokehOffsets[index].BokehRenderTexture = AmplifyUtils.GetTempRenderTarget(source.width, source.height);
      material.SetVector(AmplifyUtils.BokehParamsId, this.m_bokehCameraProperties);
      for (int index1 = 0; index1 < this.m_bokehOffsets.Count; ++index1)
      {
        for (int index2 = 0; index2 < 8; ++index2)
          material.SetVector(AmplifyUtils.AnamorphicGlareWeightsStr[index2], this.m_bokehOffsets[index1].Offsets[index2]);
        Graphics.Blit((Texture) source, this.m_bokehOffsets[index1].BokehRenderTexture, material, 27);
      }
      for (int index = 0; index < this.m_bokehOffsets.Count - 1; ++index)
        material.SetTexture(AmplifyUtils.AnamorphicRTS[index], (Texture) this.m_bokehOffsets[index].BokehRenderTexture);
      source.DiscardContents();
      Graphics.Blit((Texture) this.m_bokehOffsets[this.m_bokehOffsets.Count - 1].BokehRenderTexture, source, material, 28 + (this.m_bokehOffsets.Count - 2));
      for (int index = 0; index < this.m_bokehOffsets.Count; ++index)
      {
        AmplifyUtils.ReleaseTempRenderTarget(this.m_bokehOffsets[index].BokehRenderTexture);
        this.m_bokehOffsets[index].BokehRenderTexture = (RenderTexture) null;
      }
    }

    public void OnAfterDeserialize()
    {
      this.CreateBokehOffsets(this.m_apertureShape);
    }

    public void OnBeforeSerialize()
    {
    }

    public ApertureShape ApertureShape
    {
      get
      {
        return this.m_apertureShape;
      }
      set
      {
        if (this.m_apertureShape == value)
          return;
        this.m_apertureShape = value;
        this.CreateBokehOffsets(value);
      }
    }

    public bool ApplyBokeh
    {
      get
      {
        return this.m_isActive;
      }
      set
      {
        this.m_isActive = value;
      }
    }

    public bool ApplyOnBloomSource
    {
      get
      {
        return this.m_applyOnBloomSource;
      }
      set
      {
        this.m_applyOnBloomSource = value;
      }
    }

    public float BokehSampleRadius
    {
      get
      {
        return this.m_bokehSampleRadius;
      }
      set
      {
        this.m_bokehSampleRadius = value;
      }
    }

    public float OffsetRotation
    {
      get
      {
        return this.m_offsetRotation;
      }
      set
      {
        this.m_offsetRotation = value;
      }
    }

    public Vector4 BokehCameraProperties
    {
      get
      {
        return this.m_bokehCameraProperties;
      }
      set
      {
        this.m_bokehCameraProperties = value;
      }
    }

    public float Aperture
    {
      get
      {
        return this.m_bokehCameraProperties.x;
      }
      set
      {
        this.m_bokehCameraProperties.x = value;
      }
    }

    public float FocalLength
    {
      get
      {
        return this.m_bokehCameraProperties.y;
      }
      set
      {
        this.m_bokehCameraProperties.y = value;
      }
    }

    public float FocalDistance
    {
      get
      {
        return this.m_bokehCameraProperties.z;
      }
      set
      {
        this.m_bokehCameraProperties.z = value;
      }
    }

    public float MaxCoCDiameter
    {
      get
      {
        return this.m_bokehCameraProperties.w;
      }
      set
      {
        this.m_bokehCameraProperties.w = value;
      }
    }
  }
}
