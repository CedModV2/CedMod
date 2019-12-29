// Decompiled with JetBrains decompiler
// Type: ModPrefab
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class ModPrefab : MonoBehaviour
{
  public string label;
  public int weaponId;
  public ModPrefab.ModType modType;
  public int modId;
  public bool firstperson;
  public GameObject gameObject;

  private void Start()
  {
    WeaponManager componentInParent = this.GetComponentInParent<WeaponManager>();
    if ((Object) componentInParent == (Object) null)
      return;
    componentInParent.forceSyncModsNextFrame = true;
    switch (this.modType)
    {
      case ModPrefab.ModType.Sight:
        if (this.firstperson)
        {
          componentInParent.weapons[this.weaponId].mod_sights[this.modId].prefab_firstperson = this.gameObject;
          break;
        }
        componentInParent.weapons[this.weaponId].mod_sights[this.modId].prefab_thirdperson = this.gameObject;
        break;
      case ModPrefab.ModType.Barrel:
        if (this.firstperson)
        {
          componentInParent.weapons[this.weaponId].mod_barrels[this.modId].prefab_firstperson = this.gameObject;
          break;
        }
        componentInParent.weapons[this.weaponId].mod_barrels[this.modId].prefab_thirdperson = this.gameObject;
        break;
      case ModPrefab.ModType.Other:
        if (this.firstperson)
        {
          componentInParent.weapons[this.weaponId].mod_others[this.modId].prefab_firstperson = this.gameObject;
          break;
        }
        componentInParent.weapons[this.weaponId].mod_others[this.modId].prefab_thirdperson = this.gameObject;
        break;
    }
  }

  public enum ModType
  {
    Sight,
    Barrel,
    Other,
  }
}
