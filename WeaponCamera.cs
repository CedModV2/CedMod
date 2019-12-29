// Decompiled with JetBrains decompiler
// Type: WeaponCamera
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using AmplifyBloom;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityStandardAssets.ImageEffects;

public class WeaponCamera : MonoBehaviour
{
  private VignetteAndChromaticAberration vaca;
  private VignetteAndChromaticAberration myvaca;
  private PostProcessingBehaviour ppbeh;
  private AmplifyBloomEffect bloom;
  public AnimationCurve intensityOverScreen;
  public float _intens;
  public float _glare;

  private void Start()
  {
    this.bloom = this.GetComponent<AmplifyBloomEffect>();
    this.ppbeh = this.GetComponent<PostProcessingBehaviour>();
    this.myvaca = this.GetComponent<VignetteAndChromaticAberration>();
    this.vaca = this.GetComponentInParent<VignetteAndChromaticAberration>();
    this.bloom.enabled = PlayerPrefsSl.Get("gfxsets_cc", 1) == 1 && !ServerStatic.IsDedicated;
  }

  private void Update()
  {
    this.myvaca = this.vaca;
    float num = this.intensityOverScreen.Evaluate((float) Screen.height);
    this.bloom.OverallIntensity = this._intens * num;
    this.bloom.LensGlareInstance.Intensity = this._glare * num;
  }
}
