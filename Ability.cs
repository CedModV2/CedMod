// Decompiled with JetBrains decompiler
// Type: Ability
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

[Serializable]
public class Ability
{
  public int id;
  private string _name;
  private string _description;
  public Sprite abilityIcon;
  public string abilityKeyBindingName;

  public string GetName()
  {
    return this._name;
  }

  public string GetDescription()
  {
    return this._description;
  }

  public KeyCode GetAbilityKey()
  {
    return !this.abilityKeyBindingName.Equals(string.Empty) ? NewInput.GetKey(this.abilityKeyBindingName) : KeyCode.None;
  }

  public void LoadNameAndDescriptionFromTranslation()
  {
    string[] strArray = TranslationReader.Get("Abilities", this.id, "NO_TRANSLATION").Split(':');
    this._name = strArray[0];
    this._description = strArray[1];
  }
}
