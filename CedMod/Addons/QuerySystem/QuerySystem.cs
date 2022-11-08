using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.WS;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;
using MEC;
using Object = UnityEngine.Object;

namespace CedMod.Addons.QuerySystem
{
    public class QuerySystem
    {
        public static QueryMapEvents QueryMapEvents;
        public static QueryServerEvents QueryServerEvents;
        public static QueryPlayerEvents QueryPlayerEvents;
        public static List<string> ReservedSlotUserids = new List<string>();
        public static string PanelUrl = MainPanelUrl;
        private static string _querySystemKey;

        public static string QuerySystemKey
        {
            get
            {
                if (string.IsNullOrEmpty(_querySystemKey))
                {
                    if (!Directory.Exists(Path.Combine(Paths.Configs, "CedMod")))
                    {
                        Directory.CreateDirectory(Path.Combine(Paths.Configs, "CedMod"));
                    }
                    if (File.Exists(Path.Combine(Paths.Configs, "CedMod", "QuerySystemSecretKey.txt")))
                    {
                        File.WriteAllText(Path.Combine(Paths.Configs, "CedMod", $"QuerySystemSecretKey-{Server.Port}.txt"), File.ReadAllText(Path.Combine(Paths.Configs, "CedMod", "QuerySystemSecretKey.txt")));
                        File.Delete(Path.Combine(Paths.Configs, "CedMod", "QuerySystemSecretKey.txt"));
                    }
                    Log.Info("Read QueryKey from persistant storage");
                    _querySystemKey = File.ReadAllText(Path.Combine(Paths.Configs, "CedMod", $"QuerySystemSecretKey-{Server.Port}.txt"));
                }
                return _querySystemKey;
            }
            set
            {
                if (!Directory.Exists(Path.Combine(Paths.Configs, "CedMod")))
                {
                    Directory.CreateDirectory(Path.Combine(Paths.Configs, "CedMod"));
                }
                if (File.Exists(Path.Combine(Paths.Configs, "CedMod", "QuerySystemSecretKey.txt")))
                {
                    File.WriteAllText(Path.Combine(Paths.Configs, "CedMod", $"QuerySystemSecretKey-{Server.Port}.txt"), File.ReadAllText(Path.Combine(Paths.Configs, "CedMod", "QuerySystemSecretKey.txt")));
                    File.Delete(Path.Combine(Paths.Configs, "CedMod", "QuerySystemSecretKey.txt"));
                }
                Log.Info("Saved QueryKey to persistant storage");
                _querySystemKey = value;
                File.WriteAllText(Path.Combine(Paths.Configs, "CedMod", $"QuerySystemSecretKey-{Server.Port}.txt"), _querySystemKey);
            }
        }
        
        public static string CurrentMaster = MainPanelUrl;
        public const string MainPanelUrl = "cedmodcommunitymanagementpanelv2.cedmod.nl";
        public const string DevPanelUrl = "communitymanagementpanel.dev.cedmod.nl";
    }
}