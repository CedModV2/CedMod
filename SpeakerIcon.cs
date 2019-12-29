// Decompiled with JetBrains decompiler
// Type: SpeakerIcon
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Dissonance.Audio.Playback;
using Mirror;
using UnityEngine;

public class SpeakerIcon : MonoBehaviour
{
  private Transform cam;
  private CharacterClassManager ccm;
  private NetworkIdentity nid;
  private Radio r;
  private VoicePlayback vp;
  public Texture[] sprites;
  public static bool iAmHuman;
  public SpeakerIcon.SpeakerIconState state;

  private void Start()
  {
    this.ccm = this.GetComponentInParent<CharacterClassManager>();
    this.nid = this.GetComponentInParent<NetworkIdentity>();
    this.r = this.GetComponentInParent<Radio>();
  }

  private void Update()
  {
    if (this.nid.isLocalPlayer || !((Object) this.vp == (Object) null) || !((Object) this.r.playerSource != (Object) null))
      return;
    this.vp = this.r.playerSource.GetComponent<VoicePlayback>();
  }

  private void LateUpdate()
  {
    Role role = this.ccm.Classes.SafeGet(this.ccm.CurClass);
    if (this.nid.isLocalPlayer)
      SpeakerIcon.iAmHuman = (uint) role.team > 0U;
    if ((Object) this.cam == (Object) null)
    {
      this.cam = GameObject.Find("SpectatorCamera").transform;
    }
    else
    {
      if (role == null)
        return;
      this.transform.localPosition = Vector3.up * 1.42f + Vector3.up * role.iconHeightOffset;
      this.transform.LookAt(this.cam);
    }
  }

  public enum SpeakerIconState
  {
    Off,
    Local,
    Radio,
  }
}
