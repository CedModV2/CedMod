using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.WS;
using HarmonyLib;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;
using Mirror;
using NWAPIPermissionSystem;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using RelativePositioning;
using UnityEngine;

namespace CedMod.Addons.QuerySystem.Patches
{
    [HarmonyPatch(typeof(HitscanHitregModuleBase), nameof(HitscanHitregModuleBase.ServerPerformHitscan))]
    public static class DoubleActionShootPatch
    {
        public static void Postfix(HitscanHitregModuleBase __instance, Ray targetRay, float targetDamage)
        {
            var player = __instance.Owner;
            if (player.roleManager.CurrentRole is IFpcRole role)
            {
                var horLook = role.FpcModule.MouseLook.CurrentHorizontal;
                var verLook = role.FpcModule.MouseLook.CurrentVertical;
                var pos = player.transform.position;
                var shotDirection = targetRay.direction.normalized;
                Quaternion lookRotation = Quaternion.Euler(-verLook, horLook, 0);
                Vector3 lookDirection = lookRotation * Vector3.forward;
                
                float angle = Vector3.Angle(lookDirection, shotDirection);
                
                try
                {
                    var plr = Player.Get(player);
                    WebSocketSystem.Enqueue(new QueryCommand()
                    {
                        Recipient = "ALL",
                        Data = new Dictionary<string, string>()
                        {
                            {"ItemType", plr.CurrentItem.ItemTypeId.ToString()},
                            {"UserId", plr.UserId},
                            {"UserName", plr.Nickname},
                            {"RayAngle", angle.ToString()},
                            {"RayPos", targetRay.direction.ToString()},
                            {"PlrPos", plr.Position.ToString()},
                            {"Type", "OnPlayerShoot"},
                            {
                                "Message", string.Format(
                                    "{0} - {1} (<color={2}>{3}</color>) has shot a {4}.", new object[]
                                    {
                                        plr.Nickname,
                                        plr.UserId,
                                        Misc.ToHex(player.roleManager.CurrentRole.RoleColor),
                                        plr.Role,
                                        plr.CurrentItem.ItemTypeId
                                    })
                            }
                        }
                    });
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
        }
    }
}