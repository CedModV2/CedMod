// Decompiled with JetBrains decompiler
// Type: GameCore.ConfigFile
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Security;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameCore
{
  public static class ConfigFile
  {
    public static YamlConfig ServerConfig;
    public static YamlConfig SharingConfig;
    public static YamlConfig HosterPolicy;
    public static Dictionary<string, int[]> smBalancedPicker;
    private static bool _loaded;

    static ConfigFile()
    {
      if (!Directory.Exists(FileManager.GetAppFolder(true, false, "")))
        Directory.CreateDirectory(FileManager.GetAppFolder(true, false, ""));
      if (File.Exists(FileManager.GetAppFolder(true, false, "") + "config.txt") && !File.Exists(FileManager.GetAppFolder(true, false, "") + "LEGACY CONFIG BACKUP - NOT WORKING.txt"))
        File.Move(FileManager.GetAppFolder(true, false, "") + "config.txt", FileManager.GetAppFolder(true, false, "") + "LEGACY CONFIG BACKUP - NOT WORKING.txt");
      ConfigFile.ReloadGameConfigs(true);
    }

    public static void ReloadGameConfigs(bool firstTime = false)
    {
      if (firstTime && ConfigFile._loaded)
        return;
      ConfigFile._loaded = true;
      ServerConsole.AddLog("Loading gameplay config...");
      string configPath1 = ConfigFile.GetConfigPath("config_gameplay");
      if (ConfigFile.ServerConfig == null)
        ConfigFile.ServerConfig = new YamlConfig(configPath1);
      else
        ConfigFile.ServerConfig.LoadConfigFile(configPath1);
      ServerConsole.RefreshEmailSetStatus();
      ServerConsole.AddLog("Processing rate limits...");
      RateLimitCreator.Load();
      ServerConsole.AddLog("Loading sharing config...");
      string configPath2 = ConfigFile.GetConfigPath("config_sharing");
      if (ConfigFile.SharingConfig == null)
        ConfigFile.SharingConfig = new YamlConfig(configPath2);
      else
        ConfigFile.SharingConfig.LoadConfigFile(configPath2);
      ServerConsole.AddLog("Processing shares...");
      ConfigSharing.Reload();
      BanHandler.Init();
      WhiteList.Reload();
      MuteHandler.Reload();
      ReservedSlot.Reload();
      ServerConsole.FriendlyFire = ConfigFile.ServerConfig.GetBool("friendly_fire", false);
      ServerConsole.WhiteListEnabled = ConfigFile.ServerConfig.GetBool("enable_whitelist", false) || ConfigFile.ServerConfig.GetBool("custom_whitelist", false);
      ServerConsole.AccessRestriction = ConfigFile.ServerConfig.GetBool("server_access_restriction", false);
      ServerConsole.RateLimitKick = ConfigFile.ServerConfig.GetBool("ratelimit_kick", true);
      ServerConsole.EnforceSameIp = ConfigFile.ServerConfig.GetBool("enforce_same_ip", true);
      ServerConsole.EnforceSameAsn = ConfigFile.ServerConfig.GetBool("enforce_same_asn", true);
      ServerConsole.SkipEnforcementForLocalAddresses = ConfigFile.ServerConfig.GetBool("no_enforcement_for_local_ip_addresses", true);
      ServerConsole.ReloadServerName();
    }

    public static string GetConfigPath(string name)
    {
      string appFolder = FileManager.GetAppFolder(true, true, "");
      if (!Directory.Exists(appFolder))
        Directory.CreateDirectory(appFolder);
      if (File.Exists(appFolder + name + ".txt"))
        return appFolder + name + ".txt";
      try
      {
        File.Copy("ConfigTemplates/" + name + ".template.txt", appFolder + name + ".txt");
      }
      catch (Exception ex)
      {
        ServerConsole.AddLog("Error during copying config file: " + ex.Message);
        return (string) null;
      }
      return appFolder + name + ".txt";
    }
  }
}
