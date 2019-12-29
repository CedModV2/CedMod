// Decompiled with JetBrains decompiler
// Type: GameplayOptionsMenu
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using UnityEngine;
using UnityEngine.UI;

public class GameplayOptionsMenu : MonoBehaviour
{
  public Slider classIntroFastFadeSlider;
  public Slider headBobSlider;
  public Slider toggleSprintSlider;
  public Slider artificialHealthSlider;
  public Slider modeSwitchToggle079;
  public Slider postProcessing079;
  public Slider healthBarShowsExact;
  public Slider richPresence;
  public Slider publicLobby;
  public Slider hideIP;
  private bool isAwake;

  public void Awake()
  {
    this.classIntroFastFadeSlider.value = PlayerPrefsSl.Get("ClassIntroFastFade", false) ? 1f : 0.0f;
    this.headBobSlider.value = PlayerPrefsSl.Get("HeadBob", true) ? 1f : 0.0f;
    this.toggleSprintSlider.value = PlayerPrefsSl.Get("ToggleSprint", false) ? 1f : 0.0f;
    this.artificialHealthSlider.value = (float) PlayerPrefsSl.Get("ArtificialHealthSliderType", 0);
    this.modeSwitchToggle079.value = PlayerPrefsSl.Get("ModeSwitchSetting079", false) ? 1f : 0.0f;
    this.postProcessing079.value = PlayerPrefsSl.Get("PostProcessing079", true) ? 1f : 0.0f;
    this.healthBarShowsExact.value = PlayerPrefsSl.Get("HealthBarShowsExact", false) ? 1f : 0.0f;
    this.richPresence.value = PlayerPrefsSl.Get("RichPresence", true) ? 1f : 0.0f;
    this.publicLobby.value = PlayerPrefsSl.Get("PublicLobby", true) ? 1f : 0.0f;
    this.hideIP.value = PlayerPrefsSl.Get("HideIP", false) ? 1f : 0.0f;
    this.isAwake = true;
  }

  public void SaveSettings()
  {
    if (!this.isAwake)
      return;
    PlayerPrefsSl.Set("ClassIntroFastFade", (double) (int) this.classIntroFastFadeSlider.value == 1.0);
    PlayerPrefsSl.Set("HeadBob", (double) (int) this.headBobSlider.value == 1.0);
    PlayerPrefsSl.Set("ToggleSprint", (double) (int) this.toggleSprintSlider.value == 1.0);
    PlayerPrefsSl.Set("ArtificialHealthSliderType", (int) this.artificialHealthSlider.value);
    PlayerPrefsSl.Set("ModeSwitchSetting079", (double) (int) this.modeSwitchToggle079.value == 1.0);
    PlayerPrefsSl.Set("PostProcessing079", (double) (int) this.postProcessing079.value == 1.0);
    PlayerPrefsSl.Set("HealthBarShowsExact", (double) (int) this.healthBarShowsExact.value == 1.0);
    PlayerPrefsSl.Set("RichPresence", (double) (int) this.richPresence.value == 1.0);
    PlayerPrefsSl.Set("PublicLobby", (double) (int) this.publicLobby.value == 1.0);
    PlayerPrefsSl.Set("HideIP", (double) (int) this.hideIP.value == 1.0);
    if (Console.Platform != DistributionPlatform.Steam)
      return;
    if ((double) (int) this.richPresence.value == 0.0)
    {
      SteamManager.ClearRichPresence();
    }
    else
    {
      SteamManager.ChangePreset(-2);
      SteamManager.ChangeLobbyStatus(0, 20);
    }
  }
}
