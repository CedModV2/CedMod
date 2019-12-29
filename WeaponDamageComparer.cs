// Decompiled with JetBrains decompiler
// Type: WeaponDamageComparer
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using UnityEngine;

public class WeaponDamageComparer : MonoBehaviour
{
  public WeaponManager wm;
  public Offset[] weaponPositions;
  private GameObject[] weapons;
  private bool inProgress;

  private void Awake()
  {
    this.wm = GameObject.Find("Player").GetComponent<WeaponManager>();
    this.weapons = new GameObject[this.wm.weapons.Length];
    for (int index = 0; index < this.weapons.Length; ++index)
    {
      Item itemById = this.wm.GetComponent<Inventory>().GetItemByID(this.wm.weapons[index].inventoryID);
      this.weapons[index] = itemById.firstpersonModel;
      this.weapons[index].transform.parent = (Transform) null;
      this.weapons[index].SetActive(false);
    }
  }

  public void Recalculate()
  {
    if (this.inProgress)
      return;
    this.inProgress = true;
    this.StartCoroutine(this.Recalc());
  }

  private IEnumerator Recalc()
  {
    int amount = 0;
    int num1 = 0;
    foreach (WeaponManager.Weapon weapon in this.wm.weapons)
      num1 += weapon.mod_barrels.Length * weapon.mod_others.Length * weapon.mod_sights.Length;
    for (int i = 0; i < this.wm.weapons.Length; ++i)
    {
      this.weapons[i].SetActive(true);
      this.weapons[i].GetComponent<Animator>().enabled = false;
      float[,] dmg = new float[3, 2];
      float[,] dps = new float[3, 2];
      float[,] dpmir = new float[3, 2];
      WeaponManager.Weapon w = this.wm.weapons[i];
      for (int _s = 0; _s < this.wm.weapons[i].mod_sights.Length; ++_s)
      {
        for (int _b = 0; _b < this.wm.weapons[i].mod_barrels.Length; ++_b)
        {
          for (int _o = 0; _o < this.wm.weapons[i].mod_others.Length; ++_o)
          {
            ++amount;
            this.weapons[i].transform.position = this.weaponPositions[i].position;
            this.weapons[i].transform.rotation = Quaternion.Euler(this.weaponPositions[i].rotation);
            this.weapons[i].transform.localScale = this.weaponPositions[i].scale;
            foreach (ModPrefab componentsInChild in this.weapons[i].GetComponentsInChildren<ModPrefab>(true))
            {
              if (componentsInChild.modType == ModPrefab.ModType.Sight)
                componentsInChild.gameObject.SetActive(componentsInChild.modId == _s);
              if (componentsInChild.modType == ModPrefab.ModType.Barrel)
                componentsInChild.gameObject.SetActive(componentsInChild.modId == _b);
              if (componentsInChild.modType == ModPrefab.ModType.Other)
                componentsInChild.gameObject.SetActive(componentsInChild.modId == _o);
            }
            WeaponManager.Weapon.WeaponMod.WeaponModEffects allEffects = w.GetAllEffects(new int[3]
            {
              _s,
              _b,
              _o
            });
            float num2 = w.damageOverDistance.Evaluate(0.0f) * this.wm.overallDamagerFactor * allEffects.damageMultiplier;
            float num3 = num2 * w.shotsPerSecond * allEffects.firerateMultiplier;
            float num4 = 0.0f;
            float num5 = 0.0f;
            int maxAmmo = w.maxAmmo;
            while ((double) num5 < 60.0)
            {
              num4 += num2;
              num5 += (float) (1.0 / ((double) allEffects.firerateMultiplier * (double) w.shotsPerSecond));
              --maxAmmo;
              if (maxAmmo <= 0)
              {
                maxAmmo = w.maxAmmo;
                num5 += w.reloadingTime;
              }
            }
            if (_s == 0 && _b == 0 && _o == 0)
            {
              for (int index = 0; index < 3; ++index)
              {
                dmg[index, 0] = num2;
                dps[index, 0] = num3;
                dpmir[index, 0] = num4;
              }
            }
            else
            {
              if ((double) num2 < (double) dmg[1, 0])
              {
                dmg[1, 0] = num2;
                dmg[1, 1] = (float) _b;
              }
              if ((double) num3 < (double) dps[1, 0])
              {
                dps[1, 0] = num3;
                dps[1, 1] = (float) _b;
              }
              if ((double) num4 < (double) dpmir[1, 0])
              {
                dpmir[1, 0] = num4;
                dpmir[1, 1] = (float) _b;
              }
              if ((double) num2 > (double) dmg[2, 0])
              {
                dmg[2, 0] = num2;
                dmg[2, 1] = (float) _b;
              }
              if ((double) num3 > (double) dps[2, 0])
              {
                dps[2, 0] = num3;
                dps[2, 1] = (float) _b;
              }
              if ((double) num4 > (double) dpmir[2, 0])
              {
                dpmir[2, 0] = num4;
                dpmir[2, 1] = (float) _b;
              }
            }
            for (int n = 0; n < 3; ++n)
              yield return (object) new WaitForFixedUpdate();
          }
        }
      }
      string label = this.wm.GetComponent<Inventory>().GetItemByID(w.inventoryID).label;
      this.weapons[i].SetActive(false);
      yield return (object) new WaitForFixedUpdate();
      dmg = (float[,]) null;
      dps = (float[,]) null;
      dpmir = (float[,]) null;
      w = (WeaponManager.Weapon) null;
    }
    this.inProgress = false;
  }
}
