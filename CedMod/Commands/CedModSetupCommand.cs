﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using CedMod.Addons.QuerySystem;
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
    /// <summary>
    /// <see cref="Description"/>.
    /// </summary>
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
            
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage panelResponse = null;
                if (arguments.Count >= 2)
                {
                    panelResponse = client.GetAsync($"https://{QuerySystem.CurrentMaster}/Api/v3/AutomaticSetup?key={key}&token={arguments.At(1)}").Result;
                }
                else
                {
                    panelResponse = client.GetAsync($"https://{QuerySystem.CurrentMaster}/Api/v3/AutomaticSetup?key={key}").Result;
                }
                string result = panelResponse.Content.ReadAsStringAsync().Result;
                if (!panelResponse.IsSuccessStatusCode)
                {
                    response = result;
                    return false;
                }
            
                Dictionary<string, string> PanelResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                cedModCnf.CedMod.CedModApiKey = key;
                QuerySystem.QuerySystemKey = PanelResponse["QueryKey"];
                ConfigManager.Save(pluginConfigs);

                response = $"CedMod has been setup, using the identifier: {PanelResponse["QueryIdentifier"]}\nThe server will now be restarted,";
                ServerStatic.StopNextRound = ServerStatic.NextRoundAction.Restart;
                RoundRestart.ChangeLevel(true);
            }
            return true;
        }
    }
}