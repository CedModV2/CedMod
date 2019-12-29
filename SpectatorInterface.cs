// Decompiled with JetBrains decompiler
// Type: SpectatorInterface
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class SpectatorInterface : MonoBehaviour
{
  public GameObject rootPanel;
  public static SpectatorInterface singleton;

  private void Awake()
  {
    SpectatorInterface.singleton = this;
  }
}
