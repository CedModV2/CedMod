using System;
using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;

namespace CedMod.QuerySystem.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class PlayersCommand : ICommand
    {
        public string Command { get; } = "playerlistcolored";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Gives the list of players for the cedmod panel";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "";
            PlayerCommandSender playerCommandSender = sender as PlayerCommandSender;
            foreach (Player player in Player.List)
            {
                string playerText = "";
                string prefixText = player.IsNorthwoodStaff ? "[@] " : player.RemoteAdminAccess ? "[RA] " : string.Empty;
                playerText = $"{prefixText} ({player.Id}) {player.Nickname.Replace("\n", "")} {(player.IsOverwatchEnabled ? "<OVRM>" : "")}";

                response += $"<color={player.RoleColor.ToHex()}>" + playerText + "</color>\n";
            }
            return true;
        }
    }
}