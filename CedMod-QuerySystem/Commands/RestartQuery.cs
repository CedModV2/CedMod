using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CedMod.QuerySystem.WS;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using HarmonyLib;
using RemoteAdmin;
using UnityEngine;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class RestartQueryCommand : ICommand
    {
        public string Command { get; } = "restartqueryserver";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "restarts querysystem";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (!sender.CheckPermission("cedmod.restartquery"))
            {
                response = "No permission";
                return false;
            }

            Task.Factory.StartNew(() =>
            {
                QuerySystem.WS.WebSocketSystem.Stop();
                QuerySystem.WS.WebSocketSystem.Start();
            });
            response = "Query server restarted";
            return true;
        }
    }
}