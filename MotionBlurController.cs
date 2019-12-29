// Decompiled with JetBrains decompiler
// Type: MotionBlurController
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.PostProcessing;

public class MotionBlurController : MonoBehaviour
{
  private int f;
  private float t;
  private bool b;
  private PostProcessingProfile[] profiles;

  private void Start()
  {
    this.profiles = Resources.FindObjectsOfTypeAll<PostProcessingProfile>();
  }

  private void Update()
  {
    this.t += Time.deltaTime;
    ++this.f;
    if ((double) this.t <= 1.0)
      return;
    --this.t;
    if (this.b && this.f < 30 || !this.b && this.f > 50)
      this.Change();
    this.f = 0;
  }

  private void Change()
  {
    this.b = !this.b;
    if (!PlayerPrefsSl.Get("gfxsets_mb", true) || ServerStatic.IsDedicated)
      return;
    foreach (PostProcessingProfile profile in this.profiles)
      profile.motionBlur.enabled = false;
  }
}
