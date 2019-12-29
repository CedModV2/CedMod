// Decompiled with JetBrains decompiler
// Type: PlayerListElement
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Dissonance;
using Dissonance.Integrations.MirrorIgnorance;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListElement : MonoBehaviour
{
  public GameObject instance;
  public TextMeshProUGUI TextNick;
  public TextMeshProUGUI TextBadge;
  public RawImage ImgVerified;
  public Image ImgBackground;
  public Toggle ToggleMute;

  public void Mute(bool b)
  {
    CharacterClassManager component = this.instance.GetComponent<CharacterClassManager>();
    if (component.isLocalPlayer)
      return;
    if (!string.IsNullOrEmpty(component.UserId) && !component.Muted)
    {
      if (b)
        MuteHandler.IssuePersistantMute(component.UserId);
      else
        MuteHandler.RevokePersistantMute(component.UserId);
    }
    Object.FindObjectOfType<DissonanceComms>().FindPlayer(this.instance.GetComponent<MirrorIgnorancePlayer>().PlayerId).IsLocallyMuted = b;
  }

  public void OpenSteamAccount()
  {
  }

  public void Report()
  {
  }
}
