// Decompiled with JetBrains decompiler
// Type: ConsoleDebugMode
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ConsoleDebugMode
{
  public static readonly Color32 defaultColor = new Color32((byte) 85, (byte) 181, (byte) 125, byte.MaxValue);
  private static readonly Dictionary<string, ConsoleDebugMode.DebugChannel> debugChannels = new Dictionary<string, ConsoleDebugMode.DebugChannel>()
  {
    ["SCP079"] = new ConsoleDebugMode.DebugChannel((DebugLevel) Mathf.Clamp(PlayerPrefsSl.Get("DEBUG_SCP079", 2), 0, 4), "for SCP-079 client- and server-side logging", ConsoleDebugMode.defaultColor),
    ["MAPGEN"] = new ConsoleDebugMode.DebugChannel((DebugLevel) Mathf.Clamp(PlayerPrefsSl.Get("DEBUG_MAPGEN", 2), 0, 4), "for the Map Generator", ConsoleDebugMode.defaultColor),
    ["MISCEL"] = new ConsoleDebugMode.DebugChannel((DebugLevel) Mathf.Clamp(PlayerPrefsSl.Get("DEBUG_MISCEL", 2), 0, 4), "for miscellaneous small sub-systems", ConsoleDebugMode.defaultColor),
    ["SCP049"] = new ConsoleDebugMode.DebugChannel((DebugLevel) Mathf.Clamp(PlayerPrefsSl.Get("DEBUG_SCP049", 2), 0, 4), "for SCP-049 logging", ConsoleDebugMode.defaultColor),
    ["VC"] = new ConsoleDebugMode.DebugChannel((DebugLevel) Mathf.Clamp(PlayerPrefsSl.Get("DEBUG_VC", 2), 0, 4), "for Voice Chat logging", ConsoleDebugMode.defaultColor),
    ["PLIST"] = new ConsoleDebugMode.DebugChannel((DebugLevel) Mathf.Clamp(PlayerPrefsSl.Get("DEBUG_PLIST", 2), 0, 4), "for Player List", ConsoleDebugMode.defaultColor),
    ["MGCLTR"] = new ConsoleDebugMode.DebugChannel((DebugLevel) Mathf.Clamp(PlayerPrefsSl.Get("DEBUG_MGCLTR", 2), 0, 4), "for Map Generator Clutter System", new Color32((byte) 110, (byte) 160, (byte) 110, byte.MaxValue)),
    ["SDAUTH"] = new ConsoleDebugMode.DebugChannel((DebugLevel) Mathf.Clamp(PlayerPrefsSl.Get("DEBUG_SDAUTH", 2), 0, 4), "for Steam and Discord authenticator", new Color32((byte) 130, (byte) 130, (byte) 130, byte.MaxValue))
  };

  public static bool CheckImportance(string key, MessageImportance importance)
  {
    ConsoleDebugMode.DebugChannel debugChannel;
    return !ConsoleDebugMode.debugChannels.TryGetValue(key, out debugChannel) || debugChannel.Level >= (DebugLevel) importance;
  }

  public static bool CheckImportance(string key, MessageImportance importance, out Color32 color)
  {
    ConsoleDebugMode.DebugChannel debugChannel;
    if (ConsoleDebugMode.debugChannels.TryGetValue(key, out debugChannel))
    {
      color = debugChannel.Color;
      return debugChannel.Level >= (DebugLevel) importance;
    }
    color = ConsoleDebugMode.defaultColor;
    return true;
  }

  public static void GetList(out string[] keys, out string[] descriptions)
  {
    keys = ConsoleDebugMode.debugChannels.Keys.ToArray<string>();
    descriptions = new string[ConsoleDebugMode.debugChannels.Keys.Count];
    for (int index = 0; index < descriptions.Length; ++index)
      descriptions[index] = ConsoleDebugMode.debugChannels[keys[index]].Description;
  }

  public static bool ChangeImportance(string key, int newLevel)
  {
    if (!ConsoleDebugMode.debugChannels.ContainsKey(key))
      return false;
    ConsoleDebugMode.debugChannels[key] = new ConsoleDebugMode.DebugChannel((DebugLevel) newLevel, ConsoleDebugMode.debugChannels[key].Description, ConsoleDebugMode.debugChannels[key].Color);
    PlayerPrefsSl.Set("DEBUG_" + key, newLevel);
    return true;
  }

  public static string ConsoleGetLevel(string key)
  {
    key = key.ToUpper();
    ConsoleDebugMode.DebugChannel debugChannel;
    if (!ConsoleDebugMode.debugChannels.TryGetValue(key, out debugChannel))
      return "Module '" + key + "' could not be found.";
    return "The " + key + " is currently on the <i>" + debugChannel.Level.ToString().ToLower() + "</i> debug level.";
  }

  public struct DebugChannel
  {
    public DebugLevel Level;
    public string Description;
    public Color32 Color;

    public DebugChannel(DebugLevel lvl, string dsc, Color32 col)
    {
      this.Level = lvl;
      this.Description = dsc;
      this.Color = col;
    }
  }
}
