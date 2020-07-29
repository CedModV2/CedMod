using System;
using System.Collections.Generic;
using CedMod.Handlers;
using CommandSystem;
using RemoteAdmin;
using UnityEngine;
using Player = Exiled.API.Features.Player;

namespace CedMod.Commands
{
    public class TotalBansCommand : ICommand
    {
        public string Command { get; } = "totalbans";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Gives the ammount of prior bans on record";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            CommandSender sndr = (sender as CommandSender);
            if (Player.Get(sndr.SenderId).ReferenceHub.serverRoles.RemoteAdmin)
            {
                if (arguments.IsEmpty())
                {
                    response = "Usage: totalbans <playerid>";
                    return false;
                }
                Player ply = Player.Get(arguments.At(0));
                Dictionary<string, string> resp = (Dictionary<string, string>) API.APIRequest("banning/userdetails.php", $"?id={ply.UserId}&alias={API.GetAlias()}&total=1");
                response = resp["reason"];
                return true;
            }

            if (!Player.Get(sndr.SenderId).ReferenceHub.serverRoles.RemoteAdmin)
            {
                Dictionary<string, string> resp = (Dictionary<string, string>)API.APIRequest("banning/userdetails.php", $"?id={sndr.SenderId}&alias={API.GetAlias()}&total=1");
                response = resp["reason"];
                return true;
            }

            response = "Unable to select commandtype";
            return false;
        }
    }
}