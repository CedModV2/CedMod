using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Loader;
using Exiled.Permissions.Extensions;
using MEC;
using Newtonsoft.Json;
using RoundRestarting;
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
            var pluginConfigs = ConfigManager.LoadSorted(ConfigManager.Read());
            bool queryInstalled = pluginConfigs.ContainsKey("cm_WAPI");
            string key = arguments.At(0);
            Config cedModCnf = pluginConfigs[CedModMain.Singleton.Prefix] as Config;
            HttpClient client = new HttpClient();
            var panelResponse = client.GetAsync($"https://cedmodcommunitymanagementpanelv2.cedmod.nl/Api/AutomaticSetup?key={key}").Result;
            string result = panelResponse.Content.ReadAsStringAsync().Result;
            if (!panelResponse.IsSuccessStatusCode)
            {
                response = result;
                return false;
            }
            
            Dictionary<string, string> PanelResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
            cedModCnf.CedMod.CedModApiKey = key;
            cedModCnf.QuerySystem.SecurityKey = PanelResponse["QueryKey"];
            cedModCnf.QuerySystem.Identifier = PanelResponse["QueryIdentifier"];
            ConfigManager.Save(pluginConfigs);

            response = $"CedMod has been setup, using the identifier: {cedModCnf.QuerySystem.Identifier}\nThe server will now be restarted,";
            ServerStatic.StopNextRound = ServerStatic.NextRoundAction.Restart;
            RoundRestart.ChangeLevel(true);
            return true;
        }

        public static IEnumerator<float> LightsOut(bool surface, bool entrance, bool heavy, bool light, float dur)
        {
            while (isEnabled)
            {
                if (surface)
                    Map.TurnOffAllLights(dur, ZoneType.Surface);
                if (entrance)
                    Map.TurnOffAllLights(dur, ZoneType.Entrance);
                if (heavy)
                    Map.TurnOffAllLights(dur, ZoneType.HeavyContainment);
                if (light)
                    Map.TurnOffAllLights(dur, ZoneType.LightContainment);
                yield return Timing.WaitForSeconds(dur);
            }
        }
    }
}