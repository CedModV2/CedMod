// Decompiled with JetBrains decompiler
// Type: ServerStatic
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ServerStatic : MonoBehaviour
{
  public static bool IsDedicated;
  public static bool ProcessIdPassed;
  public bool Simulate;
  internal static bool DisableConfigValidation;
  internal static bool KeepSession;
  internal static bool StopNextRound;
  internal static bool ServerPortSet;
  internal static ushort ServerPort;
  internal static YamlConfig RolesConfig;
  internal static YamlConfig SharedGroupsConfig;
  internal static YamlConfig SharedGroupsMembersConfig;
  internal static string RolesConfigPath;
  internal static PermissionsHandler PermissionsHandler;

  private void Awake()
  {
    string[] commandLineArgs = Environment.GetCommandLineArgs();
    ServerStatic.DisableConfigValidation = commandLineArgs.Contains<string>("-disableconfigvalidation");
    for (int index = 0; index < commandLineArgs.Length - 1; ++index)
    {
      string str = commandLineArgs[index];
      if (!(str == "-appdatapath"))
      {
        if (str == "-configpath")
          FileManager.SetConfigFolder(commandLineArgs[index + 1]);
      }
      else
        FileManager.SetAppFolder(commandLineArgs[index + 1]);
    }
    foreach (string str1 in commandLineArgs)
    {
      if (!(str1 == "-nographics"))
      {
        if (str1 == "-keepsession")
          ServerStatic.KeepSession = true;
        else if (str1.StartsWith("-key"))
          ServerConsole.Session = str1.Remove(0, 4);
        else if (str1.StartsWith("-id"))
        {
          ServerStatic.ProcessIdPassed = true;
          string str2 = str1.Remove(0, 3);
          foreach (Process process in Process.GetProcesses())
          {
            if (!(process.Id.ToString() != str2))
            {
              ServerConsole.ConsoleId = process;
              ServerConsole.ConsoleId.Exited += new EventHandler(ServerStatic.OnConsoleExited);
              break;
            }
          }
          if (ServerConsole.ConsoleId == null)
            ServerStatic.OnConsoleExited((object) null, (EventArgs) null);
        }
        else if (str1.StartsWith("-port"))
        {
          if (!ushort.TryParse(str1.Remove(0, 5), out ServerStatic.ServerPort))
          {
            ServerConsole.AddLog("\"-port\" argument value is not valid unsigned short integer (0 - 65535). Aborting startup.");
            Application.Quit();
            return;
          }
          ServerStatic.ServerPortSet = true;
        }
      }
      else
        this.Simulate = true;
    }
    if (this.Simulate)
    {
      ServerStatic.IsDedicated = true;
      AudioListener.volume = 0.0f;
      AudioListener.pause = true;
      QualitySettings.pixelLightCount = 0;
      GUI.enabled = false;
      ServerConsole.AddLog("SCP Secret Laboratory process started. Creating match... LOGTYPE02");
    }
    if (ServerStatic.IsDedicated && !ServerStatic.ServerPortSet)
    {
      ServerConsole.AddLog("\"-port\" argument is required for dedicated server. Aborting startup.");
      ServerConsole.AddLog("Make sure you are using latest version of LocalAdmin (works on linux).");
      Application.Quit();
    }
    else
      SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnSceneWasLoaded);
  }

  private static void OnConsoleExited(object sender, EventArgs e)
  {
    ServerConsole.DisposeStatic();
    ServerStatic.IsDedicated = false;
    UnityEngine.Debug.Log((object) nameof (OnConsoleExited));
    Application.Quit();
  }

  private void OnSceneWasLoaded(Scene scene, LoadSceneMode mode)
  {
    if (!ServerStatic.IsDedicated || scene.buildIndex != 3 && scene.buildIndex != 4)
      return;
    this.GetComponent<CustomNetworkManager>().CreateMatch();
  }

  public static PermissionsHandler GetPermissionsHandler()
  {
    return ServerStatic.PermissionsHandler;
  }
}
