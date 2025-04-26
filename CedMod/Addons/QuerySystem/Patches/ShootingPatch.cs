using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.WS;
using HarmonyLib;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;
using LabApi.Features.Wrappers;
using Mirror;
using PlayerRoles.FirstPersonControl;
using RelativePositioning;
using UnityEngine;

namespace CedMod.Addons.QuerySystem.Patches
{
    [HarmonyPatch(typeof(HitscanHitregModuleBase), nameof(HitscanHitregModuleBase.ServerAppendPrescan))]
    public static class DoubleActionShootPatch
    {
        public static void Postfix(HitscanHitregModuleBase __instance, Ray targetRay, HitscanResult toAppend)
        {
            try
            {
                var player = __instance.Owner;
                if (player != null && player.roleManager != null && player.roleManager.CurrentRole is IFpcRole role)
                {
                    var horLook = role.FpcModule.MouseLook.CurrentHorizontal;
                    var verLook = role.FpcModule.MouseLook.CurrentVertical;
                    var pos = player.transform.position;
                    var shotDirection = targetRay.direction.normalized;
                    Quaternion lookRotation = Quaternion.Euler(-verLook, horLook, 0);
                    Vector3 lookDirection = lookRotation * Vector3.forward;
                
                    float angle = Vector3.Angle(lookDirection, shotDirection);
                
                    var plr = Player.Get(player);
                    WebSocketSystem.Enqueue(new QueryCommand()
                    {
                        Recipient = "ALL",
                        Data = new Dictionary<string, string>()
                        {
                            {"ItemType", plr.CurrentItem.Type.ToString()},
                            {"UserId", plr.UserId},
                            {"UserName", plr.Nickname},
                            {"RayAngle", angle.ToString()},
                            {"RayPos", targetRay.direction.ToString()},
                            {"PlrPos", plr.Position.ToString()},
                            {"plrRot", plr.Rotation.ToString()},
                            {"Type", "OnPlayerShoot"},
                            {
                                "Message", string.Format(
                                    "{0} - {1} (<color={2}>{3}</color>) has shot a {4}.", new object[]
                                    {
                                        plr.Nickname,
                                        plr.UserId,
                                        Misc.ToHex(player.roleManager.CurrentRole.RoleColor),
                                        plr.Role,
                                        plr.CurrentItem.Type
                                    })
                            }
                        }
                    });
                }
            }
            catch (Exception e)
            {
                LabApi.Features.Console.Logger.Error(e.ToString());
            }
        }
    }
}