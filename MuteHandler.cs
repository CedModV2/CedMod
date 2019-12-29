// Decompiled with JetBrains decompiler
// Type: MuteHandler
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class MuteHandler
{
  private static readonly HashSet<string> Mutes = new HashSet<string>();
  private static string _path;

  static MuteHandler()
  {
    MuteHandler.Reload();
  }

  public static void Reload()
  {
    MuteHandler._path = ConfigSharing.Paths[1] + "mutes.txt";
    Console.AddLog("Loading saved mutes...", Color.gray, false);
    try
    {
      using (StreamReader streamReader = new StreamReader(MuteHandler._path))
      {
        string str;
        while ((str = streamReader.ReadLine()) != null)
        {
          if (!string.IsNullOrWhiteSpace(str))
            MuteHandler.Mutes.Add(str.Trim());
        }
      }
    }
    catch
    {
      ServerConsole.AddLog("Can't load the mute file!");
    }
  }

  public static bool QueryPersistantMute(string userId)
  {
    return MuteHandler.Mutes.Contains(userId.Trim());
  }

  public static void IssuePersistantMute(string userId)
  {
    if (string.IsNullOrWhiteSpace(userId) || !MuteHandler.Mutes.Add(userId.Trim()))
      return;
    File.AppendAllText(MuteHandler._path, "\r\n" + userId);
    Console.AddLog("Mute for player " + userId + " saved.", Color.gray, false);
  }

  public static void RevokePersistantMute(string userId)
  {
    if (!MuteHandler.Mutes.Remove(userId.Trim()))
      return;
    FileManager.WriteToFile((IEnumerable<string>) MuteHandler.Mutes, MuteHandler._path, false);
    Console.AddLog("Mute for player " + userId + " removed.", Color.gray, false);
  }
}
