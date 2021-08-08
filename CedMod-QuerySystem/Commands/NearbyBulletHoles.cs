using System;
using System.Collections.Generic;
using System.Linq;
using CedMod.Patches;
using CedMod.QuerySystem.WS;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using HarmonyLib;
using RemoteAdmin;
using UnityEngine;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class NearbyBulletHolesCommand : ICommand
    {
        public string Command { get; } = "nearbybulletholes";

        public string[] Aliases { get; } = new string[]
        {
            "bulletholes",
            "neabyholes"
        };

        public string Description { get; } = "Shows the creators of the nearby bullet holes and how much the bullet holes were";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (!sender.CheckPermission("cedmod.bulletholes"))
            {
                response = "No permission";
                return false;
            }

            response = "";
            var room = Player.Get((sender as PlayerCommandSender).ReferenceHub).CurrentRoom;
            if (arguments.Count >= 1)
            {
                foreach (var creat in BulletHolePatch.HoleCreators.Where(hole => hole.Key.Nickname == arguments.At(0) || hole.Key.Id.ToString() == arguments.At(0)))
                {
                    if (creat.Value.Rooms.ContainsKey(room))
                    {
                        response = $"\n{creat.Key.Nickname} ({creat.Key.Id}) - {creat.Value.Rooms[room].Holes.Count} CON: {creat.Key.IsConnected}";
                    }
                }
            }
            else
            {
                foreach (var creat in BulletHolePatch.HoleCreators)
                {
                    if (creat.Value.Rooms.ContainsKey(room))
                    {
                        response = $"\n{creat.Key.Nickname} ({creat.Key.Id}) - {creat.Value.Rooms[room].Holes.Count} CON: {creat.Key.IsConnected}";
                    }
                }
            }

            if (response == "")
            {
                response = "Could not find any players";
            }
            
            return true;
        }
    }
}