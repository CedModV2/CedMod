// Decompiled with JetBrains decompiler
// Type: FootstepThresholdArrayExtensions
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

public static class FootstepThresholdArrayExtensions
{
  public static bool ContainsValue(
    this FootstepThreshold[] footstepThresholds,
    float valueX,
    float valueY)
  {
    for (int index = 0; index < footstepThresholds.Length; ++index)
    {
      if (footstepThresholds[index].ContainsValue(valueX, valueY))
        return true;
    }
    return false;
  }

  public static bool ContainsXValue(this FootstepThreshold[] footstepThresholds, float valueX)
  {
    for (int index = 0; index < footstepThresholds.Length; ++index)
    {
      if (footstepThresholds[index].ContainsXValue(valueX))
        return true;
    }
    return false;
  }

  public static bool ContainsYValue(this FootstepThreshold[] footstepThresholds, float valueY)
  {
    for (int index = 0; index < footstepThresholds.Length; ++index)
    {
      if (footstepThresholds[index].ContainsYValue(valueY))
        return true;
    }
    return false;
  }
}
