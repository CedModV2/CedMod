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
    public class Delete : ICommand
    {
        public string Command { get; } = "delete";

        public string[] Aliases { get; } = {
            "d"
        };

        public string Description { get; } = "Removes all players currently in a jail and deletes the jail.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("cedmod.jail"))
            {
                response = "no permission";
                return false;
            }
            
            var invoker = CedModPlayer.Get((sender as CommandSender).SenderId);
            if (!AdminSitHandler.Singleton.Sits.Any(s => s.Players.Any(s => s.UserId == invoker.UserId)))
            {
                response = "You are currently not part of any jail.";
                return false;
            }
            
            var sit = AdminSitHandler.Singleton.Sits.FirstOrDefault(s => s.Players.Any(s => s.UserId == invoker.UserId));

            foreach (var plr in sit.Players)
            {
                var cmPlr = CedModPlayer.Get(plr.UserId);
                if (cmPlr == null)
                    continue;
                
                JailParentCommand.RemovePlr(cmPlr, plr, sit);
            }
            
            Timing.CallDelayed(0.5f, () =>
            {
                JailParentCommand.RemoveJail(sit);
            });

            response = "Jail Removed";
            return false;
        }
    }
}