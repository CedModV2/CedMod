// Decompiled with JetBrains decompiler
// Type: ResolutionButton
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

public class ResolutionButton : MonoBehaviour
{
  private int cachedResolutionPreset;
  private int resolutionPreset;
  private Text txt;

  public void Click(int id)
  {
    ResolutionManager.ChangeResolution(id);
  }

  protected void Start()
  {
    this.txt = this.GetComponent<Text>();
    if ((Object) this.txt != (Object) null)
      this.txt.text = ResolutionManager.CurrentResolutionString();
    this.cachedResolutionPreset = ResolutionManager.Preset;
  }

  protected void Update()
  {
    this.resolutionPreset = ResolutionManager.Preset;
    if (this.cachedResolutionPreset != this.resolutionPreset && (Object) this.txt != (Object) null)
      this.txt.text = ResolutionManager.CurrentResolutionString();
    this.cachedResolutionPreset = this.resolutionPreset;
  }
}
