// Decompiled with JetBrains decompiler
// Type: Hitmarker
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class Hitmarker : MonoBehaviour
{
  private float t = 10f;
  public static Hitmarker singleton;
  public AnimationCurve size;
  public AnimationCurve opacity;
  private float multiplier;

  private void Awake()
  {
    Hitmarker.singleton = this;
  }

  public static void Hit(float size = 1f)
  {
    Hitmarker.singleton.Trigger(size);
  }

  private void Trigger(float size = 1f)
  {
    this.t = 0.0f;
    this.multiplier = size;
  }

  private void Update()
  {
  }
}
