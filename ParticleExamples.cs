// Decompiled with JetBrains decompiler
// Type: ParticleExamples
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

[Serializable]
public class ParticleExamples
{
  public string title;
  [TextArea]
  public string description;
  public bool isWeaponEffect;
  public GameObject particleSystemGO;
  public Vector3 particlePosition;
  public Vector3 particleRotation;
}
