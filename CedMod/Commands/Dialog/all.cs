using System;
using System.Linq;
using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;

namespace CedMod.Commands.Dialog
{
    public class DialogAllCommand : ICommand
    {
        public string Command { get; } = "all";

        public string[] Aliases { get; } = new string[]
        {
            
        };

        public string Description { get; } = "makes a popup appear for all players";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (!sender.HasPermissions("cedmod.dialog"))
            {
                response = "no permission";
                return false;
            }
            if (arguments.IsEmpty())
            {
                response = "Missing argument <message>";
                return false;
            }

            string msg = arguments.Skip(0).Aggregate((current, n) => current + " " + n);
            foreach (Player ply in Player.ReadyList) 
            {
                ply.SendConsoleMessage("[REPORTING] " + msg + " Press ESC to close", "green");
            }

            response = "Done";
            return true;
        }
    }
}