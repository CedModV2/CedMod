// Decompiled with JetBrains decompiler
// Type: ButtonStages
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

public class ButtonStages : MonoBehaviour
{
  public ButtonStages.DoorType[] inspectorTypes;
  public static ButtonStages.DoorType[] types;

  private void Start()
  {
    ButtonStages.types = this.inspectorTypes;
  }

  [Serializable]
  public class Stage
  {
    public Sprite texture;
    public Material mat;
    [Multiline]
    public string info;
  }

  [Serializable]
  public class DoorType
  {
    public ButtonStages.Stage[] stages;
    public string description;
  }
}
