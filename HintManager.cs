// Decompiled with JetBrains decompiler
// Type: HintManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (AudioSource))]
public class HintManager : MonoBehaviour
{
  public List<HintManager.Hint> hintQueue = new List<HintManager.Hint>();
  public static HintManager singleton;
  public HintManager.Hint[] hints;

  private void Awake()
  {
  }

  private void Start()
  {
  }

  [Serializable]
  public class Hint
  {
    [Multiline]
    public string content_en;
    public string keyName;
    public float duration;
  }
}
