// Decompiled with JetBrains decompiler
// Type: GameSettings
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
  public AudioMixer MasterAudioMixer;
  public static GameSettings singleton;
  public GameSettings.GameSettingsAudioSlider[] AudioSliders;

  private void Awake()
  {
    GameSettings.singleton = this;
    foreach (GameSettings.GameSettingsAudioSlider audioSlider in this.AudioSliders)
      audioSlider.LoadState();
  }

  private void OnSliderValueChange(Slider activator)
  {
    foreach (GameSettings.GameSettingsAudioSlider audioSlider in this.AudioSliders)
    {
      if ((UnityEngine.Object) audioSlider.SliderReference == (UnityEngine.Object) activator)
      {
        float f = activator.value;
        if ((UnityEngine.Object) audioSlider.OptionalText != (UnityEngine.Object) null)
          audioSlider.OptionalText.text = Mathf.RoundToInt(f * 100f).ToString() + " %";
        float num = (double) f == 0.0 ? -144f : 20f * Mathf.Log10(f);
        this.MasterAudioMixer.SetFloat(audioSlider.Key, num);
        audioSlider.SaveState();
      }
    }
  }

  [Serializable]
  public struct GameSettingsAudioSlider
  {
    public string Key;
    public Slider SliderReference;
    public Text OptionalText;
    [Range(0.0f, 1f)]
    public float DefaultValue;

    public void SaveState()
    {
      PlayerPrefsSl.Set(this.Key, this.SliderReference.value);
    }

    public void LoadState()
    {
      Slider refer = this.SliderReference;
      refer.value = PlayerPrefsSl.Get(this.Key, this.DefaultValue);
      this.SliderReference.onValueChanged.AddListener((UnityAction<float>) (_param1 => GameSettings.singleton.OnSliderValueChange(refer)));
      GameSettings.singleton.OnSliderValueChange(refer);
    }
  }
}
