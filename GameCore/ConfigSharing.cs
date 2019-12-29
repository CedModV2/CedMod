// Decompiled with JetBrains decompiler
// Type: GameCore.ConfigSharing
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.IO;

namespace GameCore
{
  public static class ConfigSharing
  {
    public static readonly string[] Shares = new string[6];
    public static readonly string[] Paths = new string[6];
    private const int Bans = 0;
    private const int Mutes = 1;
    private const int Whitelist = 2;
    private const int ReservedSlots = 3;
    private const int Groups = 4;
    private const int GroupsMembers = 5;

    static ConfigSharing()
    {
      ConfigSharing.Reload();
    }

    internal static void Reload()
    {
      ConfigSharing.Shares[0] = ConfigFile.SharingConfig.GetString("bans", "");
      ConfigSharing.Shares[1] = ConfigFile.SharingConfig.GetString("mutes", "");
      ConfigSharing.Shares[2] = ConfigFile.SharingConfig.GetString("whitelist", "");
      ConfigSharing.Shares[3] = ConfigFile.SharingConfig.GetString("reserved_slots", "");
      ConfigSharing.Shares[4] = ConfigFile.SharingConfig.GetString("groups", "");
      ConfigSharing.Shares[5] = ConfigFile.SharingConfig.GetString("groups_members", "");
      for (ushort index = 0; (int) index < ConfigSharing.Shares.Length; ++index)
      {
        if (ConfigSharing.Shares[(int) index] == "disable")
        {
          ConfigSharing.Paths[(int) index] = index == (ushort) 4 || index == (ushort) 5 ? (string) null : FileManager.GetAppFolder(true, true, "");
        }
        else
        {
          ConfigSharing.Paths[(int) index] = FileManager.GetAppFolder(true, true, ConfigSharing.Shares[(int) index]);
          string appFolder = FileManager.GetAppFolder(true, true, ConfigSharing.Shares[(int) index]);
          if (!Directory.Exists(appFolder))
            Directory.CreateDirectory(appFolder);
          switch (index)
          {
            case 4:
              if (!File.Exists(ConfigSharing.Paths[(int) index] + "shared_groups.txt"))
              {
                File.Copy("ConfigTemplates/shared_groups.template.txt", ConfigSharing.Paths[(int) index] + "shared_groups.txt");
                continue;
              }
              continue;
            case 5:
              if (!File.Exists(ConfigSharing.Paths[(int) index] + "shared_groups_members.txt"))
              {
                File.Copy("ConfigTemplates/shared_groups_members.template.txt", ConfigSharing.Paths[(int) index] + "shared_groups_members.txt");
                continue;
              }
              continue;
            default:
              continue;
          }
        }
      }
      ServerConsole.AddLog("Config sharing loaded.");
    }
  }
}
