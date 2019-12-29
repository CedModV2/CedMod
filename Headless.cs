// Decompiled with JetBrains decompiler
// Type: Headless
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using Windows;

public static class Headless
{
  public static readonly string version = "1.6.4";
  private static bool isHeadless = false;
  private static bool checkedHeadless = false;
  private static bool initializedHeadless = false;
  private static bool buildingHeadless = false;
  private static bool debuggingHeadless = false;
  private static string currentProfile = "";
  private static HeadlessRuntime headlessRuntime;

  public static string GetProfileName()
  {
    if (!Headless.IsHeadless())
      return (string) null;
    Headless.InitializeHeadless();
    return Headless.currentProfile;
  }

  public static bool IsHeadless()
  {
    if (Headless.checkedHeadless)
      return Headless.isHeadless;
    if (File.Exists(Application.dataPath + "/~HeadlessDebug.txt"))
    {
      Headless.debuggingHeadless = true;
      Headless.isHeadless = true;
    }
    else if (Array.IndexOf<string>(Environment.GetCommandLineArgs(), "-batchmode") >= 0)
      Headless.isHeadless = true;
    else if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
      Headless.isHeadless = true;
    Headless.checkedHeadless = true;
    return Headless.isHeadless;
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  private static void OnBeforeSceneLoadRuntimeMethod()
  {
    if (!Headless.IsHeadless())
      return;
    Headless.InitializeHeadless();
    HeadlessCallbacks.InvokeCallbacks("HeadlessBeforeSceneLoad");
  }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static void OnAfterSceneLoadRuntimeMethod()
  {
    if (!Headless.IsHeadless())
      return;
    if (Headless.headlessRuntime.valueCamera)
    {
      GameObject gameObject = GameObject.Find("HeadlessBehaviour");
      if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
        gameObject = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("HeadlessBehaviour"));
      HeadlessBehaviour headlessBehaviour = gameObject.GetComponent<HeadlessBehaviour>();
      if ((UnityEngine.Object) headlessBehaviour == (UnityEngine.Object) null)
        headlessBehaviour = gameObject.AddComponent<HeadlessBehaviour>();
      Camera.onPreCull += new Camera.CameraCallback(headlessBehaviour.GetComponent<HeadlessBehaviour>().NullifyCamera);
    }
    HeadlessCallbacks.InvokeCallbacks("HeadlessAfterSceneLoad");
  }

  private static void InitializeHeadless()
  {
    if (Headless.initializedHeadless)
      return;
    Headless.headlessRuntime = Resources.Load("HeadlessRuntime") as HeadlessRuntime;
    if ((UnityEngine.Object) Headless.headlessRuntime != (UnityEngine.Object) null)
    {
      Headless.currentProfile = Headless.headlessRuntime.profileName;
      if (Headless.headlessRuntime.valueConsole && !Application.isEditor)
      {
        HeadlessConsole headlessConsole = new HeadlessConsole();
        headlessConsole.Initialize();
        headlessConsole.SetTitle(Application.productName);
        Application.logMessageReceived += new Application.LogCallback(Headless.HandleLog);
      }
      if (Headless.headlessRuntime.valueLimitFramerate)
      {
        Application.targetFrameRate = Headless.headlessRuntime.valueFramerate;
        QualitySettings.vSyncCount = 0;
        Debug.Log((object) ("Application target framerate set to " + (object) Headless.headlessRuntime.valueFramerate));
      }
    }
    Headless.initializedHeadless = true;
    HeadlessCallbacks.InvokeCallbacks("HeadlessBeforeFirstSceneLoad");
  }

  private static void HandleLog(string logString, string stackTrace, LogType type)
  {
    Console.WriteLine(logString);
    if (stackTrace.Length <= 1)
      return;
    Console.WriteLine("in: " + stackTrace);
  }

  public static bool IsBuildingHeadless()
  {
    return Headless.buildingHeadless;
  }

  public static bool IsDebuggingHeadless()
  {
    return Headless.debuggingHeadless;
  }

  public static void SetBuildingHeadless(bool value, string profileName)
  {
    Headless.buildingHeadless = value;
    Headless.currentProfile = profileName;
  }
}
