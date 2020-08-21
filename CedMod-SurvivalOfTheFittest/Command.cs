

namespace CedMod.SurvivalOfTheFittest
{
using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using UnityEngine;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class LightsoutCommand : ICommand
    {
        public string Command { get; } = "survival";

        public string[] Aliases { get; } = new string[]
        {
            "survival"
        };

        public string Description { get; } = "Enables or disables the 173 survival gamemodee";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (EventHandlers.GameModeRunning)
            {
                response = "The gamemode can not be stopped mid round";
                return false;
            }

            if (!EventHandlers.RunOnStart)
            {
                EventHandlers.RunOnStart = true;
                response = "The gamemode will now start on roundstart";
                return true;
            }
            if (EventHandlers.RunOnStart)
            {
                EventHandlers.RunOnStart = true;
                response = "The gamemode will now not start on roundstart";
                return true;
            }

            response = "Something went wrong";
            return false;
        }
    }
}