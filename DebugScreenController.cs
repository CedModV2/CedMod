// Decompiled with JetBrains decompiler
// Type: DebugScreenController
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Globalization;
using System.IO;
using UnityEngine;

public class DebugScreenController : MonoBehaviour
{
  public static int Asserts;
  public static int Errors;
  public static int Exceptions;
  private static bool _logged;

  private void Awake()
  {
    Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(Application.dataPath)));
    if (Environment.GetCommandLineArgs().Contains<string>("-nographics"))
      return;
    Application.Quit();
  }

  private void Start()
  {
    UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) this.gameObject);
    Application.logMessageReceivedThreaded += new Application.LogCallback(DebugScreenController.LogMessage);
    DebugScreenController.Log();
  }

  private static void Log()
  {
    if (DebugScreenController._logged)
      return;
    DebugLog.LogBuild("GPU: " + SystemInfo.graphicsDeviceName + "\r\nVRAM: " + (object) SystemInfo.graphicsMemorySize + "MB\r\nShaderLevel: " + SystemInfo.graphicsShaderLevel.ToString().Insert(1, ".") + "\r\nVendor: " + SystemInfo.graphicsDeviceVendor + "\r\nAPI: " + (object) SystemInfo.graphicsDeviceType + "\r\nInfo: " + SystemInfo.graphicsDeviceVersion + "\r\nResolution: " + (object) Screen.width + "x" + (object) Screen.height + "\r\nFPS Limit: " + (object) Application.targetFrameRate + "\r\nFullscreen: " + ResolutionManager.Fullscreen.ToString() + "\r\nCPU: " + SystemInfo.processorType + "\r\nThreads: " + (object) SystemInfo.processorCount + "\r\nFrequency: " + (object) SystemInfo.processorFrequency + "MHz\r\nRAM: " + (object) SystemInfo.systemMemorySize + "MB\r\nAudio Supported: " + SystemInfo.supportsAudio.ToString() + "\r\nOS: " + SystemInfo.operatingSystem?.Replace("  ", " ") + "\r\nOS Version: " + OperatingSystem.GetSystemVersionString() + "\r\nUnity: " + Application.unityVersion + "\r\nVersion: " + CustomNetworkManager.CompatibleVersions[0] + "\r\nBuild: " + Application.buildGUID + "\r\nSystem Language: " + CultureInfo.CurrentCulture.EnglishName + "(" + CultureInfo.CurrentCulture.Name + ")\r\nGame Language:" + PlayerPrefsSl.Get("translation_path", "English (default)"));
    if (SystemInfo.operatingSystemFamily != OperatingSystemFamily.Windows || OperatingSystem.GetSystemVersion().Major >= 10 || File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.System) + Path.DirectorySeparatorChar.ToString() + "API-MS-WIN-CRT-MATH-L1-1-0.dll"))
      return;
    Debug.LogError((object) "Important system file that is needed for voicechat is missing, please install this windows update in order to get your voicechat working https://support.microsoft.com/en-us/help/2999226/update-for-universal-c-runtime-in-windows");
  }

  private static void LogMessage(string condition, string stackTrace, LogType type)
  {
    switch (type)
    {
      case LogType.Error:
        ++DebugScreenController.Errors;
        break;
      case LogType.Assert:
        ++DebugScreenController.Asserts;
        break;
      case LogType.Exception:
        ++DebugScreenController.Exceptions;
        break;
    }
  }

  private void Update()
  {
  }
}
