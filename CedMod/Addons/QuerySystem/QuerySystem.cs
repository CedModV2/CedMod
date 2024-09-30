using System.Collections.Generic;
using System.IO;
using LabApi.Features.Console;

namespace CedMod.Addons.QuerySystem
{
    public class QuerySystem
    {
        public static List<string> ReservedSlotUserids = new List<string>();
        private static string _querySystemKey;

        public static string QuerySystemKey
        {
            get
            {
                if (string.IsNullOrEmpty(_querySystemKey))
                {
                    if (!Directory.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod")))
                    {
                        Directory.CreateDirectory(Path.Combine(CedModMain.PluginConfigFolder, "CedMod"));
                    }
                    if (File.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "QuerySystemSecretKey.txt")))
                    {
                        File.WriteAllText(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", $"QuerySystemSecretKey-{ServerConsole.PortToReport}.txt"), File.ReadAllText(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "QuerySystemSecretKey.txt")));
                        File.Delete(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "QuerySystemSecretKey.txt"));
                    }

                    if (!File.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", $"QuerySystemSecretKey-{ServerConsole.PortToReport}.txt")))
                    {
                        return "";
                    }
                    _querySystemKey = File.ReadAllText(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", $"QuerySystemSecretKey-{ServerConsole.PortToReport}.txt"));
                    
                    if (_querySystemKey == "")
                    {
                        return "";
                    }
                    
                    Logger.Info("Read QueryKey from persistant storage");
                }
                return _querySystemKey;
            }
            set
            {
                if (!Directory.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod")))
                {
                    Directory.CreateDirectory(Path.Combine(CedModMain.PluginConfigFolder, "CedMod"));
                }
                if (File.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "QuerySystemSecretKey.txt")))
                {
                    File.WriteAllText(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", $"QuerySystemSecretKey-{ServerConsole.PortToReport}.txt"), File.ReadAllText(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "QuerySystemSecretKey.txt")));
                    File.Delete(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "QuerySystemSecretKey.txt"));
                }
                Logger.Info("Saved QueryKey to persistant storage");
                _querySystemKey = value;
                File.WriteAllText(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", $"QuerySystemSecretKey-{ServerConsole.PortToReport}.txt"), _querySystemKey);
            }
        }
        
        public static string CurrentPanel = "";
        public static string CurrentMaster = MainPanelUrl;
        public static string CurrentMasterQuery = "";
        public const string MainPanelUrl = "panelapi.cedmod.nl";
        public static string DevPanelUrl = "gameapi.dev.cedmod.nl";
        public static bool UseSSL = true;
        public static bool IsDev { get; set; }
        public static List<string> Whitelist { get; set; } = new List<string>();
        public static bool UseWhitelist = false;
    }
}