using System;
using CommandSystem;
using Exiled.API.Features;

namespace CedMod.Addons.QuerySystem.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class PlayerSteamidsCommand : ICommand
    {
        public string Command { get; } = "playerlistcoloredsteamid";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Gives the list of players for the cedmod panel";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "";
            foreach (Player player in Player.List)
            {
                response += $"{player.UserId}:{player.DoNotTrack}:{player.RemoteAdminAccess}\n";
            }

            return true;
        }
    }
}