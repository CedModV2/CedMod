using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using UnityEngine;

namespace CedMod.Commands.Dialog
{
    public class DialogPlayeridCommand : ICommand
    {
        public string Command { get; } = "playerid";

        public string[] Aliases { get; } = new string[]
        {
            
        };

        public string Description { get; } = "makes a popup appear to a player with a specific playerid";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (arguments.Count >= 2)
            {
                Player Ply = null;
                string msg = arguments.Skip(1).Aggregate((current, n) => current + " " + n);
                foreach (Player ply in Player.List)
                {
                    if (ply.ReferenceHub.queryProcessor.PlayerId.ToString() == arguments.At(0))
                        Ply = ply;
                }

                if (Ply == null)
                {
                    response = "Could not find player with the specified playerid";
                    return false;
                }
                Ply.SendConsoleMessage("[REPORTING] " + msg, "green");
                response = "Done";
                return true;
            }

            response = "missing arguments: usage userid message";
            return false;
        }
    }
}