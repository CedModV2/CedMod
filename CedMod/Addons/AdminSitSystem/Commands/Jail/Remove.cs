using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CedMod.Handlers;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using InventorySystem;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.Radio;
using MEC;
using Mirror;
#if !EXILED
using NWAPIPermissionSystem;
#else
using Exiled.Permissions.Extensions;
#endif
using PlayerRoles;
using PluginAPI.Core;
using Vector3 = UnityEngine.Vector3;

namespace CedMod.Addons.AdminSitSystem.Commands.Jail
{
    public class Remove : ICommand
    {
        public string Command { get; } = "remove";

        public string[] Aliases { get; } = {
            "r"
        };

        public string Description { get; } = "Removes the specified player from your current jail.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (!sender.CheckPermission("cedmod.jail"))
            {
                response = "no permission";
                return false;
            }
            
            if (arguments.Count < 1)
            {
                response = "This command requires a PlayerId to be specified";
                return false;
            }
            
            var invoker = CedModPlayer.Get((sender as CommandSender).SenderId);
            if (!AdminSitHandler.Singleton.Sits.Any(s => s.Players.Any(s => s.UserId == invoker.UserId)))
            {
                response = "You are currently not part of any jail.";
                return false;
            }

            var plr = CedModPlayer.Get(arguments.At(0));
            if (plr == null)
            {
                response = $"Player '{arguments.At(0)}' could not be found.";
                return false;
            }
            var sit = AdminSitHandler.Singleton.Sits.FirstOrDefault(s => s.Players.Any(s => s.UserId == plr.UserId));
            if (sit == null)
            {
                response = "This player is not in a jail.";
                return false;
            }
            
            var sitPlr = sit.Players.First(s => s.UserId == plr.UserId);
            JailParentCommand.RemovePlr(plr, sitPlr, sit);

            if (!sit.Players.Any(s => s.PlayerType == AdminSitPlayerType.Staff || s.PlayerType == AdminSitPlayerType.Handler))
            {
                foreach (var sitPlr2 in sit.Players)
                {
                    var plr2 = CedModPlayer.Get(sitPlr2.UserId);
                    JailParentCommand.RemovePlr(plr2, sitPlr2, sit);
                }

                Timing.CallDelayed(0.5f, () =>
                {
                    JailParentCommand.RemoveJail(sit);
                });
            }

            response = "Player Removed";
            return false;
        }
    }
}