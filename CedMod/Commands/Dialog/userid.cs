﻿using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;

namespace CedMod.Commands.Dialog
{
    /// <summary>
    /// <see cref="Description"/>.
    /// </summary>
    public class DialogUseridCommand : ICommand
    {
        public string Command { get; } = "userid";

        public string[] Aliases { get; } = {
            "playerid"
        };

        public string Description { get; } = "makes a popup appear to a player with a specific userid";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (!sender.CheckPermission("cedmod.dialog"))
            {
                response = "no permission";
                return false;
            }
            if (arguments.Count >= 2)
            {
                Player player;
                string msg = arguments.Skip(1).Aggregate((current, n) => current + " " + n);

                player = Player.Get(arguments.At(0));
                if (player == null)
                {
                    response = "Could not find player with the specified userid";
                    return false;
                }
                player.SendConsoleMessage("[REPORTING] " + msg + " Press ESC to close", "green");
                response = "Done";
                return true;
            }

            response = "missing arguments: usage userid message";
            return false;
        }
    }
}