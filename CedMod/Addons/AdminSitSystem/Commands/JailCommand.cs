using System;
using System.Collections.Generic;
using System.Linq;
using CedMod.Addons.AdminSitSystem.Commands.Jail;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using CommandSystem.Commands.RemoteAdmin.Doors;
using CustomPlayerEffects;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.Radio;
using MEC;
using Mirror;
using PlayerRoles;
using PluginAPI.Core;
using UnityEngine;

namespace CedMod.Addons.AdminSitSystem.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class JailParentCommand : ParentCommand
    {
        public override string Command => "jail";

        public override string[] Aliases { get; } = new string[]
        {
            "adminsit",
            "cmjail"
        };

        public override string Description => "Manages the jail system.";

        public JailParentCommand() => LoadGeneratedCommands();

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new Create());
            RegisterCommand(new Remove());
            RegisterCommand(new Add());
            RegisterCommand(new Join());
            RegisterCommand(new Delete());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            response = "jail -\ncreate [player1 player2 player3] - creates and assigns a jail location to you for use, and adds any players you put in\njail (a)add {playerid} - adds a player to the jail\njail (r)remove {playerid} - removes a player from the jail\njail (j)join {playerid} - adds you to the jail of the specified player\njail (d)delete - deletes the jail your currently in.";
            return false;
        }

        public static void AddPlr(Player plr, AdminSit sit)
        {
            Vector3 playerPos = plr.Position;
            foreach (var lift in ElevatorManager.SpawnedChambers)
            {
                if (lift.Value.WorldspaceBounds.Contains(plr.Position))
                {
                    if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                        Log.Info($"Player in lift {lift.Key}");
                    var door = DoorVariant.AllDoors.Where(s => s is not ElevatorDoor).OrderBy(s => Vector3.Distance(s.transform.position, lift.Value.transform.position)).FirstOrDefault();
                    if (door != null)
                    {
                        var pos = DoorTPCommand.EnsurePositionSafety(door.transform);
                        playerPos = pos;
                    }
                }
            }

            var sitPlr = new AdminSitPlayer()
            {
                Player = plr,
                PlayerType = plr.RemoteAdminAccess ? AdminSitPlayerType.Staff : AdminSitPlayerType.User,
                UserId = plr.UserId,
                Ammo = new Dictionary<ItemType, ushort>(plr.ReferenceHub.inventory.UserInventory.ReserveAmmo),
                Health = plr.Health,
                Items = new Dictionary<ushort, ItemBase>(plr.ReferenceHub.inventory.UserInventory.Items),
                Position = playerPos,
                Role = plr.Role,
            };

            foreach (var effect in plr.ReferenceHub.playerEffectsController.AllEffects)
            {
                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    Log.Debug($"Saving effect {effect.ToString()} {effect.IsEnabled} {effect.Intensity} {effect.Duration} {effect.TimeLeft}");
                sitPlr.Effects.Add(effect.ToString(), new Tuple<bool, byte, float>(effect.IsEnabled, effect.Intensity, effect.Duration != 0.0f ? effect.TimeLeft : 0.0f));
            }
            
            sit.Players.Add(sitPlr);
            plr.ReferenceHub.inventory.UserInventory.ReserveAmmo.Clear();
            plr.SetRole(RoleTypeId.Tutorial, RoleChangeReason.RemoteAdmin);
            Timing.CallDelayed(0.1f, () => {
            {
                plr.Position = sit.Location.SpawnPosition;
            } });
        }

        public static void RemoveJail(AdminSit sit)
        {
            foreach (var obj in sit.SpawnedObjects)
            {
                NetworkServer.Destroy(obj.gameObject);
            }

            sit.SpawnedObjects.Clear();
            AdminSitHandler.Singleton.Sits.Remove(sit);
            sit.Location.InUse = false;
        }

        public static void RemovePlr(Player plr, AdminSitPlayer sitPlr, AdminSit sit)
        {
            plr.SetRole(sitPlr.Role, RoleChangeReason.RemoteAdmin);
            Timing.CallDelayed(0.1f, () =>
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
                        newFirearm.Status = new FirearmStatus(oldFirearm.Status.Ammo, oldFirearm.Status.Flags,
                            oldFirearm.Status.Attachments);
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

                Timing.CallDelayed(0.1f, () =>
                {
                    foreach (var ammo in sitPlr.Ammo)
                    {
                        plr.SetAmmo(ammo.Key, ammo.Value);
                    }
                });

                foreach (var effect in sitPlr.Effects)
                {
                    if (!effect.Value.Item1)
                        continue;
                    
                    Log.Info($"Enabling effect {effect.Key} {effect.Value.Item1} {effect.Value.Item2} {effect.Value.Item3}");

                    if (plr.EffectsManager.TryGetEffect(effect.Key, out var effectData))
                    {
                        Log.Info($"Found effect");
                        effectData.ServerSetState(effect.Value.Item2, effect.Value.Item3);
                    }
                }

                sit.Players.Remove(sitPlr);
                AdminSitHandler.Singleton.LeftPlayers.Remove(sitPlr.UserId);
            });
        }
    }
}