// Decompiled with JetBrains decompiler
// Type: ToggleableLight
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class ToggleableLight : MonoBehaviour
{
  public GameObject[] allLights;
  public bool isAlarm;

  public void SetLights(bool b)
  {
    foreach (GameObject allLight in this.allLights)
      allLight.SetActive(this.isAlarm ? b : !b);
  }
}
