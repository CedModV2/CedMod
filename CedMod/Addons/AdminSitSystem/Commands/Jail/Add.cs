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
    public class Add : ICommand
    {
        public string Command { get; } = "add";

        public string[] Aliases { get; } = {
            "a"
        };

        public string Description { get; } = "Adds the specified player to your current jail.";

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

            var loc = AdminSitHandler.Singleton.Sits.FirstOrDefault(s => s.Players.Any(s => s.UserId == invoker.UserId)).Location;
            
            var plr = CedModPlayer.Get(arguments.At(0));
            if (plr is null)
            {
                response = $"Player '{arguments.At(0)}' could not be found";
                return false;
            }
            if (AdminSitHandler.Singleton.Sits.Any(s => s.Players.Any(s => s.UserId == plr.UserId)))
            {
                response = "The specified player is already part of a jail.";
                return false;
            }
            
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
                        Player = plr,
                        PlayerType = plr.RemoteAdminAccess ? AdminSitPlayerType.Staff : AdminSitPlayerType.User,
                        UserId = plr.UserId,
                        Ammo = new Dictionary<ItemType, ushort>(plr.ReferenceHub.inventory.UserInventory.ReserveAmmo),
                        Health = plr.Health,
                        Items = new Dictionary<ushort, ItemBase>(plr.ReferenceHub.inventory.UserInventory.Items),
                        Position = plr.Position,
                        Role = plr.Role
                    }
                }
            });
            
            plr.SetRole(RoleTypeId.Tutorial, RoleChangeReason.RemoteAdmin);
            Timing.CallDelayed(0.1f, () => {
            {
                plr.Position = loc.SpawnPosition;
            } });

            response = "Player Added, use jail remove {playerId} to remove the player.";
            return false;
        }
    }
}