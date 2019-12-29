// Decompiled with JetBrains decompiler
// Type: AnimationCurveUtils
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public static class AnimationCurveUtils
{
  public static AnimationCurve MakeLinearCurve(
    Keyframe[] keyframes,
    WrapMode preWrapMode = WrapMode.Once,
    WrapMode postWrapMode = WrapMode.Once)
  {
    return new AnimationCurve(AnimationCurveUtils.MakeLinearKeyframes(keyframes))
    {
      preWrapMode = preWrapMode,
      postWrapMode = postWrapMode
    };
  }

  public static Keyframe[] MakeLinearKeyframes(params Keyframe[] keyframes)
  {
    for (int index = 0; index < keyframes.Length; ++index)
      keyframes[index] = AnimationCurveUtils.MakeLinearKeyframe(keyframes[index]);
    return keyframes;
  }

  public static Keyframe MakeLinearKeyframe(float time, float value)
  {
    return new Keyframe(time, value, 0.0f, 0.0f, 0.0f, 0.0f);
  }

  public static Keyframe MakeLinearKeyframe(Keyframe keyframe)
  {
    return AnimationCurveUtils.MakeLinearKeyframe(keyframe.time, keyframe.value);
  }
}
