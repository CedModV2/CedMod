// Decompiled with JetBrains decompiler
// Type: SeasonalMaterialStruct
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

[Serializable]
public struct SeasonalMaterialStruct
{
  public string editorDescriptor;
  public Material[] initialMaterial;
  public Material replaceMaterial;
  public Holidays condition;
}
