﻿using System;
using CommandSystem;
namespace CedMod.Commands
{
    public class FFADisableCommand : ICommand
    {
        public string Command { get; } = "disableffa";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Disable/Enable FFA for the rest of the round";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            FriendlyFireAutoban.AdminDisabled = !FriendlyFireAutoban.AdminDisabled;
            if (FriendlyFireAutoban.AdminDisabled)
            {
                response = "FFA is now Disabled FFA wil reset at round end unless FFA is disabled";
                return true;
            }
            else
            {
                response = "FFA is now Enabled FFA wil reset at round end unless FFA is disabled";
                return true;
            }
        }
    }
}