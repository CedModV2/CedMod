// Decompiled with JetBrains decompiler
// Type: LCZ_LabelManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LCZ_LabelManager : MonoBehaviour
{
  private List<LCZ_Label> labels = new List<LCZ_Label>();
  private List<GameObject> rooms = new List<GameObject>();
  public LCZ_LabelManager.LCZ_Label_Preset[] chars;
  public Material[] numbers;

  private void Start()
  {
    this.labels = ((IEnumerable<LCZ_Label>) UnityEngine.Object.FindObjectsOfType<LCZ_Label>()).ToList<LCZ_Label>();
    for (int index = 0; index < this.transform.childCount; ++index)
    {
      if (this.transform.GetChild(index).name.StartsWith("Root_") || this.transform.GetChild(index).name.StartsWith("LCZ_"))
        this.rooms.Add(this.transform.GetChild(index).gameObject);
    }
  }

  public void RefreshLabels()
  {
    foreach (LCZ_Label label in this.labels)
    {
      bool flag = true;
      Vector3 vector3 = label.transform.position + label.transform.forward * 10f;
      Debug.DrawLine(vector3, vector3 + Vector3.up * 30f, Color.red, 20f);
      foreach (GameObject room in this.rooms)
      {
        if ((double) Vector3.Distance(room.transform.position, vector3) < 10.0)
        {
          int num = 0;
          foreach (LCZ_LabelManager.LCZ_Label_Preset lczLabelPreset in this.chars)
          {
            if (room.name.Contains(lczLabelPreset.nameToContain))
            {
              flag = false;
              int index = 0;
              if (room.name.Contains("("))
              {
                try
                {
                  string str = room.name.Remove(0, room.name.IndexOf('(') + 1);
                  index = int.Parse(str.Remove(str.IndexOf(')')));
                }
                catch
                {
                }
              }
              label.Refresh(lczLabelPreset.mat, this.numbers[index], num.ToString());
            }
          }
          ++num;
        }
      }
      if (flag)
        label.Refresh(this.chars[0].mat, this.numbers[0], "none");
    }
  }

  [Serializable]
  public class LCZ_Label_Preset
  {
    public string nameToContain;
    public Material mat;
  }
}
