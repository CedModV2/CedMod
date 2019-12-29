// Decompiled with JetBrains decompiler
// Type: ReservedSlot
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using System;
using System.Collections.Generic;
using System.IO;

public static class ReservedSlot
{
  internal static readonly HashSet<string> Users = new HashSet<string>();

  static ReservedSlot()
  {
    ReservedSlot.Reload();
  }

  public static void Reload()
  {
    string path = ConfigSharing.Paths[3] + "UserIDReservedSlots.txt";
    ReservedSlot.Users.Clear();
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
            ReservedSlot.Users.Add(str.Trim());
        }
      }
      ServerConsole.AddLog("Reserved slots list has been loaded.");
    }
  }

  public static bool HasReservedSlot(string userId)
  {
    return ReservedSlot.Users.Contains(userId.Trim()) || !CharacterClassManager.OnlineMode;
  }
}
