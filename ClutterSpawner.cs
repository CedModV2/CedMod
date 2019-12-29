// Decompiled with JetBrains decompiler
// Type: ClutterSpawner
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ClutterSpawner : MonoBehaviour
{
  [SerializeField]
  private List<ClutterStruct> clutters = new List<ClutterStruct>();
  private static bool noHolidays;

  public static bool IsHolidayActive(Holidays holiday)
  {
    DateTime utcNow = DateTime.UtcNow;
    if (ClutterSpawner.noHolidays && holiday == Holidays.NoHoliday)
      return true;
    switch (holiday)
    {
      case Holidays.Always:
        return true;
      case Holidays.Halloween:
        return (utcNow.Day >= 28 && utcNow.Month == 10 || utcNow.Day <= 3 && utcNow.Month == 11) && !ClutterSpawner.noHolidays;
      case Holidays.Christmas:
        return utcNow.Day >= 24 && utcNow.Month == 12 && !ClutterSpawner.noHolidays;
      case Holidays.AprilFools:
        return utcNow.Day == 1 && utcNow.Month == 4 && !ClutterSpawner.noHolidays;
      case Holidays.October:
        return utcNow.Month == 10 && !ClutterSpawner.noHolidays;
      case Holidays.December:
        return utcNow.Month == 12 && !ClutterSpawner.noHolidays;
      case Holidays.FirstHalfOfApril:
        return utcNow.Day <= 15 && utcNow.Month == 4 && !ClutterSpawner.noHolidays;
      case Holidays.OctoberOrDecember:
        return (utcNow.Month == 10 || utcNow.Month == 12) && !ClutterSpawner.noHolidays;
      default:
        return holiday == Holidays.NoHoliday;
    }
  }

  private void Start()
  {
    ClutterSpawner.noHolidays = ConfigFile.ServerConfig.GetBool("no_holidays", false);
  }

  public void GenerateClutter()
  {
    for (int index = this.clutters.Count - 1; index >= 0; --index)
    {
      ClutterStruct clutter = this.clutters[index];
      GameCore.Console.AddDebugLog("MGCLTR", "Checking spawn conditions for clutter struct \"" + clutter.descriptor + "\" on object \"" + this.gameObject.name + "\"", MessageImportance.LeastImportant, true);
      bool flag1 = true;
      if ((bool) (UnityEngine.Object) clutter.clutterComponent && !clutter.clutterComponent.spawned)
      {
        if ((double) clutter.chanceToSpawn <= 0.0)
          flag1 = false;
        else if ((double) UnityEngine.Random.Range(1, 101) > (double) clutter.chanceToSpawn)
          flag1 = false;
        else if (!clutter.invertTimespan)
        {
          bool flag2 = false;
          foreach (Holidays holiday in clutter.validTimespan)
          {
            if (ClutterSpawner.IsHolidayActive(holiday))
              flag2 = true;
          }
          flag1 = flag2;
        }
        else
        {
          foreach (Holidays holiday in clutter.validTimespan)
          {
            if (ClutterSpawner.IsHolidayActive(holiday))
              flag1 = false;
          }
        }
        if (flag1)
        {
          clutter.clutterComponent.SpawnClutter();
        }
        else
        {
          clutter.clutterComponent.gameObject.SetActive(false);
          UnityEngine.Object.Destroy((UnityEngine.Object) clutter.clutterComponent.holderObject);
        }
      }
    }
  }
}
