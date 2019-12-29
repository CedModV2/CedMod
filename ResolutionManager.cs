// Decompiled with JetBrains decompiler
// Type: ResolutionManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ResolutionManager : MonoBehaviour
{
  public static List<ResolutionManager.ResolutionPreset> Presets = new List<ResolutionManager.ResolutionPreset>();
  public static int Preset;
  public static bool Fullscreen;
  private static bool _initialized;

  private static bool FindResolution(Resolution res)
  {
    foreach (ResolutionManager.ResolutionPreset preset in ResolutionManager.Presets)
    {
      if (preset.Height == res.height && preset.Width == res.width)
        return true;
    }
    return false;
  }

  private void Start()
  {
    if (!ResolutionManager._initialized)
      ResolutionManager.InitialisePresets();
    ResolutionManager.Preset = Mathf.Clamp(PlayerPrefsSl.Get("SavedResolutionSet", ResolutionManager.Presets.Count - 1), 0, ResolutionManager.Presets.Count - 1);
    ResolutionManager.Fullscreen = PlayerPrefsSl.Get("SavedFullscreen", true);
    if (!ServerStatic.IsDedicated)
      Application.targetFrameRate = PlayerPrefsSl.Get("MaxFramerate", -1);
    ResolutionManager.RefreshScreen();
    SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(ResolutionManager.OnSceneWasLoaded);
  }

  private static void OnSceneWasLoaded(Scene scene, LoadSceneMode mode)
  {
    ResolutionManager.RefreshScreen();
  }

  private static void InitialisePresets()
  {
    ResolutionManager.Presets.Clear();
    foreach (Resolution resolution in Screen.resolutions)
    {
      if (!ResolutionManager.FindResolution(resolution))
        ResolutionManager.Presets.Add(new ResolutionManager.ResolutionPreset(resolution));
    }
    ResolutionManager._initialized = true;
  }

  public static void RefreshScreen()
  {
    if (!ResolutionManager._initialized)
      ResolutionManager.InitialisePresets();
    if (ResolutionManager.Presets.Count == 0)
      return;
    ResolutionManager.Presets[0].SetResolution();
  }

  public static void ChangeResolution(int id)
  {
    ResolutionManager.Preset = Mathf.Clamp(ResolutionManager.Preset + id, 0, ResolutionManager.Presets.Count - 1);
    PlayerPrefsSl.Set("SavedResolutionSet", ResolutionManager.Preset);
    ResolutionManager.RefreshScreen();
  }

  public static void ChangeFullscreen(bool isTrue)
  {
    ResolutionManager.Fullscreen = isTrue;
    PlayerPrefsSl.Set("SavedFullscreen", isTrue);
    ResolutionManager.RefreshScreen();
  }

  public static string CurrentResolutionString()
  {
    return ResolutionManager.Presets[Mathf.Clamp(ResolutionManager.Preset, 0, ResolutionManager.Presets.Count - 1)].Width.ToString() + " × " + (object) ResolutionManager.Presets[Mathf.Clamp(ResolutionManager.Preset, 0, ResolutionManager.Presets.Count - 1)].Height;
  }

  [Serializable]
  public class ResolutionPreset
  {
    public int Height;
    public int Width;

    public ResolutionPreset(Resolution template)
    {
      this.Width = template.width;
      this.Height = template.height;
    }

    public void SetResolution()
    {
      Screen.SetResolution(this.Width, this.Height, ResolutionManager.Fullscreen);
    }
  }
}
