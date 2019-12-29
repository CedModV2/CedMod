// Decompiled with JetBrains decompiler
// Type: ClassPresetChooser
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

public class ClassPresetChooser : MonoBehaviour
{
  private List<ClassPresetChooser.PickerPreset> curPresets = new List<ClassPresetChooser.PickerPreset>();
  public GameObject bottomMenuItem;
  public Transform bottomMenuHolder;
  public ClassPresetChooser.PickerPreset[] presets;
  private string curKey;

  private void Update()
  {
  }

  [Serializable]
  public class PickerPreset
  {
    public string classID;
    public Texture icon;
    public int health;
    public float wSpeed;
    public float rSpeed;
    public float stamina;
    public Texture[] startingItems;
    public string en_additionalInfo;
    public string pl_additionalInfo;
  }
}
