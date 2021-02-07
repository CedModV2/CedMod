using System;
using CommandSystem;
using Exiled.Permissions.Extensions;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
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
            if (!sender.CheckPermission("cedmod.ffadisable"))
            {
                response = "no permission";
                return false;
            }
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