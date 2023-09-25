using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using InventorySystem.Items;
using MEC;
#if !EXILED
using NWAPIPermissionSystem;
#else
using Exiled.Permissions.Extensions;
#endif
using PlayerRoles;

namespace CedMod.Addons.AdminSitSystem.Commands.Jail
{
    public class Join : ICommand
    {
        public string Command { get; } = "join";

        public string[] Aliases { get; } = {
            "j"
        };

        public string Description { get; } = "Adds yourself to the jail of the specified player";

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
            var plr = CedModPlayer.Get(int.Parse(arguments.At(0)));
            if (plr == null)
            {
                response = $"Player '{arguments.At(0)}' could not be found.";
                return false;
            }
            if (!AdminSitHandler.Singleton.Sits.Any(s => s.Players.Any(s => s.UserId == plr.UserId)))
            {
                response = "The specified player is not part of any jail.";
                return false;
            }

            var sit = AdminSitHandler.Singleton.Sits.FirstOrDefault(s => s.Players.Any(s => s.UserId == plr.UserId));

            JailParentCommand.AddPlr(invoker, sit);

            response = "Added, use jail remove {playerId} to remove yourself";
            return false;
        }
    }
}