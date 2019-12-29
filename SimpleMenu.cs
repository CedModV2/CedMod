// Decompiled with JetBrains decompiler
// Type: SimpleMenu
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleMenu : MonoBehaviour
{
  private static string targetSceneName;
  private static bool server;
  private static SimpleMenu singleton;
  public bool isPreloader;

  private void Awake()
  {
    if (this.isPreloader)
      return;
    SimpleMenu.singleton = this;
    foreach (string commandLineArg in Environment.GetCommandLineArgs())
    {
      if (!(commandLineArg == "-fastmenu"))
      {
        if (commandLineArg == "-nographics")
          SimpleMenu.server = true;
      }
      else
        PlayerPrefsSl.Set("fastmenu", true);
    }
    SimpleMenu.Refresh();
  }

  private void Start()
  {
    if (!this.isPreloader)
      return;
    SceneManager.LoadScene("Loader");
  }

  public void ChangeMode()
  {
    PlayerPrefsSl.Set("fastmenu", !PlayerPrefsSl.Get("fastmenu", false));
    SimpleMenu.Refresh();
    SimpleMenu.LoadCorrectScene();
  }

  private static void Refresh()
  {
    SimpleMenu.targetSceneName = SimpleMenu.server || PlayerPrefsSl.Get("fastmenu", false) ? "FastMenu" : "MainMenuRemastered";
    UnityEngine.Object.FindObjectOfType<CustomNetworkManager>().offlineScene = SimpleMenu.targetSceneName;
  }

  public static void LoadCorrectScene()
  {
    SceneManager.LoadScene(SimpleMenu.targetSceneName);
  }
}
