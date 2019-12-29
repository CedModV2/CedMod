// Decompiled with JetBrains decompiler
// Type: ReproProjectSettings
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

public class ReproProjectSettings : ScriptableObject
{
  public string ProjectName;
  public string ProjectPath;
  public bool OpenProject;
  public int TextureScale;
  public ReproProjectSettings.InputItem[] InputFiles;
  public ReproProjectSettings.InputItem[] ProjectFiles;

  [Serializable]
  public struct InputItem
  {
    public ReproProjectAssetType AssetType;
    public string AssetPath;
  }
}
