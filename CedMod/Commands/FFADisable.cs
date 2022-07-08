﻿using System;
using CommandSystem;
using Exiled.Permissions.Extensions;

namespace CedMod.Commands
{
    /// <summary>
    /// <see cref="Description"/>.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class FfaDisableCommand : ICommand
    {
        public string Command { get; } = "disableffa";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Disable/Enable FFA for the rest of the round";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (!sender.CheckPermission("cedmod.ffadisable"))
            {
                response = "no permission";
                return false;
            }
            FriendlyFireAutoban.AdminDisabled = !FriendlyFireAutoban.AdminDisabled;
            response = FriendlyFireAutoban.AdminDisabled ? "FFA is now Disabled FFA wil reset at round end unless FFA is disabled" : "FFA is now Enabled FFA wil reset at round end unless FFA is disabled";
            return true;
        }
    }
}