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
            
            var loc = sit.Location;
            var sitPlr = sit.Players.First(s => s.UserId == plr.UserId);

            plr.SetRole(sitPlr.Role, RoleChangeReason.RemoteAdmin);
            Timing.CallDelayed(0.1f, () => {
            {
                if (!AlphaWarheadController.Detonated)
                    plr.Position = sitPlr.Position;
                else 
                    plr.Position = new Vector3(132.714f, 995.456f, -38.340f);
                plr.Health = sitPlr.Health;
                plr.ClearInventory();
                foreach (var item in sitPlr.Items)
                {
                    var a = plr.ReferenceHub.inventory.ServerAddItem(item.Value.ItemTypeId);
                    
                    if (a is Firearm newFirearm && item.Value is Firearm oldFirearm)
                    {
                        newFirearm.Status = new FirearmStatus(oldFirearm.Status.Ammo, oldFirearm.Status.Flags, oldFirearm.Status.Attachments);
                    }

                    if (a is MicroHIDItem microHidItem && item.Value is MicroHIDItem oldMicroHidItem)
                    {
                        microHidItem.RemainingEnergy = oldMicroHidItem.RemainingEnergy;
                    }
                    
                    if (a is RadioItem radioItem && item.Value is RadioItem oldRadioItem)
                    {
                        radioItem._battery = oldRadioItem._battery;
                    }
                }

                foreach (var ammo in sitPlr.Ammo)
                {
                    plr.SetAmmo(ammo.Key, ammo.Value);
                }

                sit.Players.Remove(sitPlr);

                if (!sit.Players.Any(s => s.PlayerType == AdminSitPlayerType.Staff || s.PlayerType == AdminSitPlayerType.Handler))
                {
                    foreach (var sitPlr2 in sit.Players)
                    {
                        var plr2 = CedModPlayer.Get(sitPlr2.UserId);
                        plr2.SetRole(sitPlr2.Role, RoleChangeReason.RemoteAdmin);
                        Timing.CallDelayed(0.1f, () =>
                        {
                            if (!AlphaWarheadController.Detonated)
                                plr2.Position = sitPlr2.Position;
                            else 
                                plr2.Position = new Vector3(132.714f, 995.456f, -38.340f);
                            plr2.Health = sitPlr2.Health;
                            plr2.ClearInventory();
                            foreach (var item in sitPlr2.Items)
                            {
                                var a = plr2.ReferenceHub.inventory.ServerAddItem(item.Value.ItemTypeId);
                                if (a is Firearm newFirearm && item.Value is Firearm oldFirearm)
                                {
                                    newFirearm.Status = new FirearmStatus(oldFirearm.Status.Ammo, oldFirearm.Status.Flags, oldFirearm.Status.Attachments);
                                }

                                if (a is MicroHIDItem microHidItem && item.Value is MicroHIDItem oldMicroHidItem)
                                {
                                    microHidItem.RemainingEnergy = oldMicroHidItem.RemainingEnergy;
                                }
                    
                                if (a is RadioItem radioItem && item.Value is RadioItem oldRadioItem)
                                {
                                    radioItem._battery = oldRadioItem._battery;
                                }
                            }

                            foreach (var ammo in sitPlr2.Ammo)
                            {
                                plr.SetAmmo(ammo.Key, ammo.Value);
                            }

                            sit.Players.Remove(sitPlr2);
                        });
                    }

                    Timing.CallDelayed(0.5f, () =>
                    {
                        foreach (var obj in sit.SpawnedObjects)
                        {
                            NetworkServer.Destroy(obj.gameObject);
                        }
                        
                        sit.SpawnedObjects.Clear();
                        AdminSitHandler.Singleton.Sits.Remove(sit);
                        loc.InUse = false;
                    });
                }
            } });

            response = "Player Removed";
            return false;
        }
    }
}