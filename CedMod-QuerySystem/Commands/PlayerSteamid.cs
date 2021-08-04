using System;
using System.Collections.Generic;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using RemoteAdmin;
using UnityEngine;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class PlayerSteamidsCommand : ICommand
    {
        public string Command { get; } = "playerlistcoloredsteamid";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Gives the list of players for the cedmod panel";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            try
            {
                string text = "\n";
                
                foreach (ReferenceHub player in ReferenceHub.Hubs.Values)
                {
                    if (player.isLocalPlayer)
                        continue;

                    bool staff = false;
                    if (ServerStatic.PermissionsHandler._members.ContainsKey(player.characterClassManager.UserId) && ServerStatic.PermissionsHandler._groups.ContainsKey(ServerStatic.PermissionsHandler._members[player.characterClassManager.UserId]))
                    {
                        staff = ServerStatic.PermissionsHandler.IsRaPermitted(ServerStatic.PermissionsHandler._groups[ServerStatic.PermissionsHandler._members[player.characterClassManager.UserId]].Permissions);
                    }
                    text += $"{player.characterClassManager.UserId}:{player.serverRoles.DoNotTrack}:{staff}\n";
                }
                response = text;
                
            }
            catch (Exception ex2)
            {
                Log.Error(ex2);
                throw;
            }

            return true;
        }
    }
}