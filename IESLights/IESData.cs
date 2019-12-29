// Decompiled with JetBrains decompiler
// Type: IESLights.IESData
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

namespace IESLights
{
  public class IESData
  {
    public List<float> VerticalAngles { get; set; }

    public List<float> HorizontalAngles { get; set; }

    public List<List<float>> CandelaValues { get; set; }

    public List<List<float>> NormalizedValues { get; set; }

    public PhotometricType PhotometricType { get; set; }

    public VerticalType VerticalType { get; set; }

    public HorizontalType HorizontalType { get; set; }

    public int PadBeforeAmount { get; set; }

    public int PadAfterAmount { get; set; }

    public float HalfSpotlightFov { get; set; }
  }
}
