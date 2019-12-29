// Decompiled with JetBrains decompiler
// Type: MenuMusicManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class MenuMusicManager : MonoBehaviour
{
  public float lerpSpeed = 1f;
  private float curState;
  private bool creditsChanged;
  [Space(15f)]
  public AudioSource mainSource;
  public AudioSource creditsSource;
  [Space(8f)]
  public GameObject creditsHolder;

  private void Update()
  {
    this.curState = Mathf.Lerp(this.curState, this.creditsHolder.activeSelf ? 1f : 0.0f, this.lerpSpeed * Time.deltaTime);
    this.mainSource.mute = (double) this.curState > 0.5;
    this.creditsSource.volume = this.curState;
    if (this.creditsChanged == this.creditsHolder.activeSelf)
      return;
    this.creditsChanged = this.creditsHolder.activeSelf;
    if (!this.creditsChanged)
      return;
    this.creditsSource.Play();
  }
}
