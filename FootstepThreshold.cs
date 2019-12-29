// Decompiled with JetBrains decompiler
// Type: FootstepThreshold
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

public struct FootstepThreshold
{
  public static readonly FootstepThreshold[][] Thresholds = new FootstepThreshold[5][]
  {
    new FootstepThreshold[1],
    new FootstepThreshold[1]
    {
      new FootstepThreshold(float.NegativeInfinity, 1f, float.NegativeInfinity, float.PositiveInfinity)
    },
    new FootstepThreshold[1]
    {
      new FootstepThreshold(1f, float.PositiveInfinity, float.NegativeInfinity, float.PositiveInfinity)
    },
    new FootstepThreshold[1]
    {
      new FootstepThreshold(float.NegativeInfinity, float.PositiveInfinity, float.NegativeInfinity, 1f)
    },
    new FootstepThreshold[1]
    {
      new FootstepThreshold(float.NegativeInfinity, float.PositiveInfinity, 1f, float.PositiveInfinity)
    }
  };
  public readonly float minX;
  public readonly float maxX;
  public readonly float minY;
  public readonly float maxY;

  public FootstepThreshold(float minX = float.NegativeInfinity, float maxX = float.PositiveInfinity, float minY = float.NegativeInfinity, float maxY = float.PositiveInfinity)
  {
    this.minX = minX;
    this.maxX = maxX;
    this.minY = minY;
    this.maxY = maxY;
  }

  public bool ContainsValue(float valueX, float valueY)
  {
    return this.ContainsXValue(valueX) && this.ContainsYValue(valueY);
  }

  public bool ContainsXValue(float valueX)
  {
    return FootstepThreshold.CheckBounds(valueX, this.minX, this.maxX);
  }

  public bool ContainsYValue(float valueY)
  {
    return FootstepThreshold.CheckBounds(valueY, this.minY, this.maxY);
  }

  private static bool CheckBounds(float value, float min, float max)
  {
    return (double) value >= (double) min && (double) value <= (double) max;
  }
}
