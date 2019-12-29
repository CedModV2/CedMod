// Decompiled with JetBrains decompiler
// Type: TutorialManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
  public static int curlog = -1;
  private List<TutorialManager.Log> logs = new List<TutorialManager.Log>();
  public static bool status;
  public static int levelID;
  private FirstPersonController fpc;
  public TutorialManager.TutorialScene[] tutorials;
  private AudioSource src;
  private float timeToNext;

  private void Awake()
  {
  }

  private void Start()
  {
  }

  private void LateUpdate()
  {
  }

  [Serializable]
  public class TutorialScene
  {
    public List<TutorialManager.Log> logs = new List<TutorialManager.Log>();
  }

  [Serializable]
  public class Log
  {
    [Multiline]
    public string content_en;
    public AudioClip clip_en;
    public float duration_en;
    public bool jumpforward;
    public bool stopPlayer;
    public string alias;
  }
}
