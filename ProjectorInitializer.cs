// Decompiled with JetBrains decompiler
// Type: ProjectorInitializer
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

public class ProjectorInitializer : MonoBehaviour
{
  public ProjectorInitializer.LightStruct[] lights;
  public AudioSource src;
  public AudioClip c_st;
  public AudioClip c_lp;
  public AudioClip c_sp;
  public Transform[] spools;
  private float time;
  public bool started;
  private bool prevStarted;
  private bool dir;

  private void InitLoop()
  {
    this.src.Stop();
    this.src.PlayOneShot(this.c_lp);
  }

  private void Update()
  {
  }

  [Serializable]
  public class LightStruct
  {
    public string label;
    public Color normalColor;
    public Light targetLight;
    public AnimationCurve curve;

    public void SetLight(float time)
    {
      this.targetLight.color = Color.Lerp(Color.black, this.normalColor, this.curve.Evaluate(time));
    }
  }
}
