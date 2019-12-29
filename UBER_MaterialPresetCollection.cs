// Decompiled with JetBrains decompiler
// Type: UBER_MaterialPresetCollection
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class UBER_MaterialPresetCollection : ScriptableObject
{
  [SerializeField]
  [HideInInspector]
  public string currentPresetName;
  [SerializeField]
  [HideInInspector]
  public UBER_PresetParamSection whatToRestore;
  [SerializeField]
  [HideInInspector]
  public UBER_MaterialPreset[] matPresets;
  [SerializeField]
  [HideInInspector]
  public string[] names;
}
