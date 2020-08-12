﻿using System;
using System.Collections.Generic;
using CedMod.Handlers;
using CommandSystem;
using GameCore;
using RemoteAdmin;
using UnityEngine;
using Player = Exiled.API.Features.Player;

namespace CedMod.Commands
{
    public class PriorBansCommand : ICommand
    {
        public string Command { get; } = "priorbans";

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
                string resp = API.APIRequest("banning/userdetails.php",
                    $"?id={ply.UserId}&alias={API.GetAlias()}&priors=1", true).ToString();
                response = resp;
                return true;
            }
            if (!Player.Get(sndr.SenderId).ReferenceHub.serverRoles.RemoteAdmin)
            {
                string resp = API.APIRequest("banning/userdetails.php", $"?id={sndr.SenderId}&alias={API.GetAlias()}&priors=1", true).ToString();
                response = resp;
                return true;
            }

            response = "Unable to select commandtype";
            return false;
        }
    }
}