// Decompiled with JetBrains decompiler
// Type: DecontaminationGas
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

public class DecontaminationGas : MonoBehaviour
{
  public static List<DecontaminationGas> gases = new List<DecontaminationGas>();

  public static void TurnOn()
  {
    if (ServerStatic.IsDedicated)
      return;
    foreach (DecontaminationGas gase in DecontaminationGas.gases)
    {
      if ((Object) gase != (Object) null)
        gase.gameObject.SetActive(true);
    }
  }

  private void Start()
  {
    DecontaminationGas.gases.Add(this);
    this.gameObject.SetActive(false);
  }
}
