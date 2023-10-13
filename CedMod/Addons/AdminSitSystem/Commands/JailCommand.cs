using System;
using System.Collections.Generic;
using CedMod.Addons.AdminSitSystem.Commands.Jail;
using CommandSystem;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.Radio;
using MEC;
using Mirror;
using PlayerRoles;
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
            response =
                "jail -\ncreate [player1 player2 player3] - creates and assigns a jail location to you for use, and adds any players you put in\njail (a)add {playerid} - adds a player to the jail\njail (r)remove {playerid} - removes a player from the jail\njail (j)join {playerid} - adds you to the jail of the specified player\njail (d)delete - deletes the jail your currently in.";
            return false;
        }

        public static void AddPlr(CedModPlayer plr, AdminSit sit)
        {
            sit.Players.Add( new AdminSitPlayer()
            {
                Player = plr,
                PlayerType = plr.RemoteAdminAccess ? AdminSitPlayerType.Staff : AdminSitPlayerType.User,
                UserId = plr.UserId,
                Ammo = new Dictionary<ItemType, ushort>(plr.ReferenceHub.inventory.UserInventory.ReserveAmmo),
                Health = plr.Health,
                Items = new Dictionary<ushort, ItemBase>(plr.ReferenceHub.inventory.UserInventory.Items),
                Position = plr.Position,
                Role = plr.Role
            });
            
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

        public static void RemovePlr(CedModPlayer plr, AdminSitPlayer sitPlr, AdminSit sit)
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

                foreach (var ammo in sitPlr.Ammo)
                {
                    plr.SetAmmo(ammo.Key, ammo.Value);
                }

                sit.Players.Remove(sitPlr);
                AdminSitHandler.Singleton.LeftPlayers.Remove(sitPlr.UserId);
            });
        }
    }
}