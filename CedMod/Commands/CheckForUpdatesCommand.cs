using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using CedMod.Components;
using CommandSystem;
using MEC;
using Newtonsoft.Json;
using PluginAPI.Core;
using RoundRestarting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CedMod.Commands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class CheckForUpdatesCommand : ICommand
    {
        public string Command { get; } = "checkforcedmodupdate";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Checks if there are any updates available for your CedMod Version";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            AutoUpdater updater = Object.FindObjectOfType<AutoUpdater>();
            if (!CedModMain.Singleton.Config.CedMod.AutoUpdate)
            {
                Task.Factory.StartNew(() =>
                {
                    var data = updater.CheckForUpdates(true);
                    if (data != null)
                        Log.Info($"As AutoUpdate is disabled in your CedMod plugin configuration this update will not be install automatically, please run the installcedmodupdate command to install this update");
                });
                response = "";
                return true;
            }
            updater.TimePassedCheck = 300;
            response = "Update check requested to the UpdateService";
            return true;
        }
    }
}