// Decompiled with JetBrains decompiler
// Type: WhiteList
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using System;
using System.Collections.Generic;
using System.IO;

public static class WhiteList
{
  private static readonly HashSet<string> Users = new HashSet<string>();

  static WhiteList()
  {
    WhiteList.Reload();
  }

  public static void Reload()
  {
    string path = ConfigSharing.Paths[2] + "UserIDWhitelist.txt";
    WhiteList.Users.Clear();
    if (!File.Exists(path))
    {
      FileManager.WriteStringToFile("#Put one UserID (eg. 76561198071934271@steam or 274613382353518592@discord) per line. Lines prefixed with \"#\" are ignored.", path);
    }
    else
    {
      using (StreamReader streamReader = new StreamReader(path))
      {
        string str;
        while ((str = streamReader.ReadLine()) != null)
        {
          if (!string.IsNullOrWhiteSpace(str) && !str.TrimStart((char[]) Array.Empty<char>()).StartsWith("#") && str.Contains("@"))
            WhiteList.Users.Add(str.Trim());
        }
      }
      ServerConsole.AddLog("Whitelist has been loaded.");
    }
  }

  public static bool IsOnWhitelist(string userId)
  {
    return WhiteList.Users.Contains(userId.Trim());
  }

  public static bool IsWhitelisted(string userId)
  {
    return WhiteList.Users.Contains(userId.Trim()) || !ConfigFile.ServerConfig.GetBool("enable_whitelist", false) || !CharacterClassManager.OnlineMode;
  }
}
