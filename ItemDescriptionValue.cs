// Decompiled with JetBrains decompiler
// Type: ItemDescriptionValue
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

public class ItemDescriptionValue : MonoBehaviour
{
  public ItemDescriptionValue.IdvWeapon idvWeapon;
  public ItemDescriptionValue.IdvKeycard idvKeycard;
  public ItemDescriptionValue.IdvMisc idvMisc;

  [Serializable]
  public class IdvWeapon
  {
    public float maxSliderDistance;
    public float sliderThickness;
  }

  [Serializable]
  public class IdvKeycard
  {
    public string[] accessNames;
  }

  [Serializable]
  public class IdvMisc
  {
  }
}
