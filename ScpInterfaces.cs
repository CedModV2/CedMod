// Decompiled with JetBrains decompiler
// Type: ScpInterfaces
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class ScpInterfaces : MonoBehaviour
{
  public GameObject Scp106_eq;
  public GameObject Scp049_eq;
  public static int remTargs;

  private void Start()
  {
  }

  private GameObject FindLocalPlayer()
  {
    return PlayerManager.localPlayer;
  }
}
