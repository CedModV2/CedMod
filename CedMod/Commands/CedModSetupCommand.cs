using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using CedMod.Addons.QuerySystem;
using CommandSystem;
using Exiled.Loader;
using MEC;
using Newtonsoft.Json;
using RoundRestarting;
using Serialization;
using UnityEngine;

namespace CedMod.Commands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class CedModsetupCommand : ICommand
    {
        public string Command { get; } = "setupcedmod";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Automatically configure cedmod using the token given by the Admin Panel or CommunityManagementPanel";
        internal static bool isEnabled;
        internal static CoroutineHandle CoroutineHandle;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            string key = arguments.At(0);

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage panelResponse = null;
                if (arguments.Count >= 2)
                {
                    panelResponse = client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.CurrentMaster}/Api/v3/AutomaticSetup?key={key}&token={arguments.At(1)}").Result;
                }
                else
                {
                    panelResponse = client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.CurrentMaster}/Api/v3/AutomaticSetup?key={key}").Result;
                }
                string result = panelResponse.Content.ReadAsStringAsync().Result;
                if (!panelResponse.IsSuccessStatusCode)
                {
                    response = result;
                    return false;
                }
            
                Dictionary<string, string> PanelResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                CedModMain.Singleton.Config.CedMod.CedModApiKey = key;
                QuerySystem.QuerySystemKey = PanelResponse["QueryKey"];
#if !EXILED
                File.WriteAllText(Path.Combine(CedModMain.PluginConfigFolder, "config.yml"), YamlParser.Serializer.Serialize(CedModMain.Singleton.Config));
#else
                var pluginConfigs = ConfigManager.LoadSorted(ConfigManager.Read());
                Config cedModCnf = pluginConfigs[CedModMain.Singleton.Prefix] as Config;
                cedModCnf.CedMod.CedModApiKey = key;
                ConfigManager.Save(pluginConfigs);
#endif
                response = $"CedMod has been setup, using the identifier: {PanelResponse["QueryIdentifier"]}\nThe server will now be restarted,";
                ServerStatic.StopNextRound = ServerStatic.NextRoundAction.Restart;
                RoundRestart.ChangeLevel(true);
            }
            return true;
        }
    }
}