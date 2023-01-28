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

            var loc = AdminSitHandler.Singleton.Sits.FirstOrDefault(s => s.Players.Any(s => s.UserId == plr.UserId)).Location;

            AdminSitHandler.Singleton.Sits.Add(new AdminSit()
            {
                AssociatedReportId = 0,
                InitialDuration = 0,
                InitialReason = "",
                Location = loc,
                Players = new List<AdminSitPlayer>()
                {
                    new AdminSitPlayer()
                    {
                        Player = invoker,
                        PlayerType = invoker.RemoteAdminAccess ? AdminSitPlayerType.Staff : AdminSitPlayerType.User,
                        UserId = invoker.UserId,
                        Ammo = new Dictionary<ItemType, ushort>(invoker.ReferenceHub.inventory.UserInventory.ReserveAmmo),
                        Health = invoker.Health,
                        Items = new Dictionary<ushort, ItemBase>(invoker.ReferenceHub.inventory.UserInventory.Items),
                        Position = invoker.Position,
                        Role = invoker.Role
                    }
                }
            });
            
            invoker.SetRole(RoleTypeId.Tutorial, RoleChangeReason.RemoteAdmin);
            Timing.CallDelayed(0.1f, () => {
            {
                invoker.Position = loc.SpawnPosition;
            } });

            response = "Added, use jail remove {playerId} to remove yourself";
            return false;
        }
    }
}