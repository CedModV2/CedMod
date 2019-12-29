// Decompiled with JetBrains decompiler
// Type: CPPS_NV
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

public class CPPS_NV : CustomPostProcessingSight
{
  public Slider distanceSlider;
  public Text infoText;
  public GameObject light;
  public AnimationCurve sliderValueOverDistance;

  private void Awake()
  {
    Object.Destroy((Object) this);
  }

  private void Start()
  {
  }

  private void Update()
  {
  }
}
