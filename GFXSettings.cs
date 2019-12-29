// Decompiled with JetBrains decompiler
// Type: GFXSettings
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.PostProcessing;

public class GFXSettings : MonoBehaviour
{
  private void Start()
  {
    this.LoadSavedSettings();
  }

  public void RefreshGUI()
  {
  }

  public void RefreshValues()
  {
  }

  public void SaveSettings()
  {
  }

  public void LoadSavedSettings()
  {
    QualitySettings.SetQualityLevel(4);
    GFXSettings.RefreshPPB();
    this.RefreshGUI();
  }

  private static void RefreshPPB()
  {
    foreach (PostProcessingBehaviour processingBehaviour in Resources.FindObjectsOfTypeAll<PostProcessingBehaviour>())
    {
      if ((UnityEngine.Object) processingBehaviour.profile != (UnityEngine.Object) null && !processingBehaviour.profile.fog.enabled)
      {
        processingBehaviour.profile.ambientOcclusion.enabled = false;
        processingBehaviour.profile.antialiasing.enabled = false;
        processingBehaviour.profile.bloom.enabled = false;
        processingBehaviour.profile.chromaticAberration.enabled = false;
        processingBehaviour.profile.colorGrading.enabled = false;
        processingBehaviour.profile.depthOfField.enabled = false;
        processingBehaviour.profile.dithering.enabled = false;
        processingBehaviour.profile.eyeAdaptation.enabled = false;
        processingBehaviour.profile.fog.enabled = false;
        processingBehaviour.profile.grain.enabled = false;
        processingBehaviour.profile.ambientOcclusion.enabled = false;
        processingBehaviour.profile.screenSpaceReflection.enabled = false;
        processingBehaviour.profile.motionBlur.enabled = false;
        processingBehaviour.profile.userLut.enabled = false;
        processingBehaviour.profile.vignette.enabled = false;
      }
    }
  }

  [Serializable]
  public class SliderValue
  {
    public string overrideText = string.Empty;
    public string en;

    public string Return()
    {
      return !string.IsNullOrEmpty(this.overrideText) ? this.overrideText : this.en;
    }
  }
}
