// Decompiled with JetBrains decompiler
// Type: Shutdown
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class Shutdown : MonoBehaviour
{
  private static bool _quitting;

  public static void SafeQuit()
  {
    if (Shutdown._quitting)
      return;
    Shutdown._quitting = true;
    SteamManager.StopClient();
    Application.Quit();
  }

  private void OnApplicationQuit()
  {
    if (Shutdown._quitting)
      return;
    Shutdown._quitting = true;
    SteamManager.StopClient();
  }
}
