// Decompiled with JetBrains decompiler
// Type: FullscreenToggle
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class FullscreenToggle : MonoBehaviour
{
  public GameObject checkmark;
  public bool isOn;

  private void OnEnable()
  {
    this.isOn = PlayerPrefsSl.Get("SavedFullscreen", true);
    this.checkmark.SetActive(this.isOn);
  }

  public void Click()
  {
    this.isOn = !this.isOn;
    this.checkmark.SetActive(this.isOn);
    PlayerPrefsSl.Set("SavedFullscreen", this.isOn);
    ResolutionManager.ChangeFullscreen(this.isOn);
  }
}
