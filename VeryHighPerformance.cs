// Decompiled with JetBrains decompiler
// Type: VeryHighPerformance
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class VeryHighPerformance : MonoBehaviour
{
  private void Start()
  {
    if (PlayerPrefsSl.Get("gfxsets_hp", 0) == 0 && !ServerStatic.IsDedicated)
    {
      LocalCurrentRoomEffects.isVhigh = false;
    }
    else
    {
      foreach (Component component in Object.FindObjectsOfType<Light>())
        Object.Destroy((Object) component.transform.gameObject);
      LocalCurrentRoomEffects.isVhigh = true;
    }
  }
}
