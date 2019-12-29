// Decompiled with JetBrains decompiler
// Type: AmplifyBloom.StarDefData
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

namespace AmplifyBloom
{
  [Serializable]
  public class StarDefData
  {
    [SerializeField]
    private string m_starName = string.Empty;
    [SerializeField]
    private int m_starlinesCount = 2;
    [SerializeField]
    private int m_passCount = 4;
    [SerializeField]
    private float m_sampleLength = 1f;
    [SerializeField]
    private float m_attenuation = 0.85f;
    [SerializeField]
    private float m_customIncrement = 90f;
    [SerializeField]
    private StarLibType m_starType;
    [SerializeField]
    private float m_inclination;
    [SerializeField]
    private float m_rotation;
    [SerializeField]
    private StarLineData[] m_starLinesArr;
    [SerializeField]
    private float m_longAttenuation;

    public StarDefData()
    {
    }

    public void Destroy()
    {
      this.m_starLinesArr = (StarLineData[]) null;
    }

    public StarDefData(
      StarLibType starType,
      string starName,
      int starLinesCount,
      int passCount,
      float sampleLength,
      float attenuation,
      float inclination,
      float rotation,
      float longAttenuation = 0.0f,
      float customIncrement = -1f)
    {
      this.m_starType = starType;
      this.m_starName = starName;
      this.m_passCount = passCount;
      this.m_sampleLength = sampleLength;
      this.m_attenuation = attenuation;
      this.m_starlinesCount = starLinesCount;
      this.m_inclination = inclination;
      this.m_rotation = rotation;
      this.m_customIncrement = customIncrement;
      this.m_longAttenuation = longAttenuation;
      this.CalculateStarData();
    }

    public void CalculateStarData()
    {
      if (this.m_starlinesCount == 0)
        return;
      this.m_starLinesArr = new StarLineData[this.m_starlinesCount];
      float num = ((double) this.m_customIncrement > 0.0 ? this.m_customIncrement : 180f / (float) this.m_starlinesCount) * ((float) Math.PI / 180f);
      for (int index = 0; index < this.m_starlinesCount; ++index)
      {
        this.m_starLinesArr[index] = new StarLineData();
        this.m_starLinesArr[index].PassCount = this.m_passCount;
        this.m_starLinesArr[index].SampleLength = this.m_sampleLength;
        this.m_starLinesArr[index].Attenuation = (double) this.m_longAttenuation <= 0.0 ? this.m_attenuation : (index % 2 == 0 ? this.m_longAttenuation : this.m_attenuation);
        this.m_starLinesArr[index].Inclination = num * (float) index;
      }
    }

    public StarLibType StarType
    {
      get
      {
        return this.m_starType;
      }
      set
      {
        this.m_starType = value;
      }
    }

    public string StarName
    {
      get
      {
        return this.m_starName;
      }
      set
      {
        this.m_starName = value;
      }
    }

    public int StarlinesCount
    {
      get
      {
        return this.m_starlinesCount;
      }
      set
      {
        this.m_starlinesCount = value;
        this.CalculateStarData();
      }
    }

    public int PassCount
    {
      get
      {
        return this.m_passCount;
      }
      set
      {
        this.m_passCount = value;
        this.CalculateStarData();
      }
    }

    public float SampleLength
    {
      get
      {
        return this.m_sampleLength;
      }
      set
      {
        this.m_sampleLength = value;
        this.CalculateStarData();
      }
    }

    public float Attenuation
    {
      get
      {
        return this.m_attenuation;
      }
      set
      {
        this.m_attenuation = value;
        this.CalculateStarData();
      }
    }

    public float Inclination
    {
      get
      {
        return this.m_inclination;
      }
      set
      {
        this.m_inclination = value;
        this.CalculateStarData();
      }
    }

    public float CameraRotInfluence
    {
      get
      {
        return this.m_rotation;
      }
      set
      {
        this.m_rotation = value;
      }
    }

    public StarLineData[] StarLinesArr
    {
      get
      {
        return this.m_starLinesArr;
      }
    }

    public float CustomIncrement
    {
      get
      {
        return this.m_customIncrement;
      }
      set
      {
        this.m_customIncrement = value;
        this.CalculateStarData();
      }
    }

    public float LongAttenuation
    {
      get
      {
        return this.m_longAttenuation;
      }
      set
      {
        this.m_longAttenuation = value;
        this.CalculateStarData();
      }
    }
  }
}
