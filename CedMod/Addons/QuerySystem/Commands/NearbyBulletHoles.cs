using System;
using System.Linq;
using CedMod.Addons.QuerySystem.Patches;
using CommandSystem;
using MapGeneration;
using PluginAPI.Core;
using RemoteAdmin;
using NWAPIPermissionSystem;

namespace CedMod.Addons.QuerySystem.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class NearbyBulletHolesCommand : ICommand
    {
        public string Command { get; } = "nearbybulletholes";

        public string[] Aliases { get; } = new string[]
        {
            "bulletholes",
            "nearbyholes"
        };

        public string Description { get; } =
            "Shows the creators of the nearby bullet holes and how much the bullet holes were";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (!sender.CheckPermission("cedmod.bulletholes"))
            {
                response = "No permission";
                return false;
            }

            response =
                "List of nearby bulletholes\nFormat: [Playername] ([PlayerId]) - [BulletHoleCreated] CON: [IsConnected]";
            var room = RoomIdUtils.RoomAtPosition(Player.Get((sender as PlayerCommandSender).ReferenceHub).Position);
            if (arguments.Count >= 1)
            {
                foreach (var creat in BulletHolePatch.HoleCreators.Where(hole =>
                             hole.Key.Nickname == arguments.At(0) || hole.Key.PlayerId.ToString() == arguments.At(0)))
                {
                    if (creat.Value.Rooms.ContainsKey(room))
                    {
                        response +=
                            $"\n{creat.Key.Nickname} ({creat.Key.PlayerId}) - {creat.Value.Rooms[room].Holes.Count} CON: {creat.Key.ReferenceHub != null}";
                    }
                }

                return true;
            }
            else
            {
                foreach (var creat in BulletHolePatch.HoleCreators)
                {
                    if (creat.Value.Rooms.ContainsKey(room))
                    {
                        response +=
                            $"\n{creat.Key.Nickname} ({creat.Key.PlayerId}) - {creat.Value.Rooms[room].Holes.Count} CON: {creat.Key.ReferenceHub != null}";
                    }
                }

                return true;
            }
        }
    }
}