using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using CameraShaking;
using CedMod.Addons.QuerySystem.WS;
using CentralAuth;
#if EXILED
using Exiled.Events.EventArgs.Player;
#endif
using HarmonyLib;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;
using LabApi.Features.Wrappers;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using Newtonsoft.Json;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps;
using RelativePositioning;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

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
                    var shotDirection = targetRay.direction.normalized;
                    float angle = Vector3.Angle(player.PlayerCameraReference.forward, shotDirection);

                    int targetId = -1;
                    List<HitboxType> hitboxTypes = new List<HitboxType>();
                    Vector3 raycastHit = Vector3.zero;
                    foreach (var destructable in toAppend.DamagedDestructibles)
                    {
                        if (destructable.Destructible is not HitboxIdentity hitboxIdentity)
                            continue;
                        
                        if (toAppend.Destructibles.Any(s => s.Destructible == destructable.Destructible))
                            raycastHit = toAppend.Destructibles.FirstOrDefault(s => s.Destructible == destructable.Destructible).Hit.point;
                        targetId = hitboxIdentity.TargetHub.PlayerId;
                        hitboxTypes.Add(hitboxIdentity.HitboxType);
                    }

                    RecoilSettings intendedRecoil = new RecoilSettings();
                    if (__instance.Owner.inventory.CurInstance is Firearm firearm && firearm.TryGetModule(out RecoilPatternModule recoil))
                    {
                        if (recoil._counter == null)
                            return;
                        
                        int shots = recoil._counter.SubsequentShots;
                        float adsAmount = firearm.TryGetModule(out IAdsModule ads) ? ads.AdsAmount : 0;
                        
                        float hipScale = firearm.AttachmentsValue(AttachmentParam.OverallRecoilMultiplier);
                        float adsScale = recoil.HipToAds(hipScale);
                        intendedRecoil = recoil.Evaluate(shots, Mathf.Lerp(hipScale, adsScale, adsAmount));
                    }
                    
                    var plr = Player.Get(player);
                    WebSocketSystem.Enqueue(new QueryCommand()
                    {
                        Recipient = "ALL",
                        Data = new Dictionary<string, string>()
                        {
                            {"ItemType", plr.CurrentItem.Type.ToString()},
                            {"TargetPlayer", targetId.ToString()},
                            {"UserId", plr.UserId},
                            {"UserName", plr.Nickname},
                            {"RayAngle", angle.ToString()},
                            {"RayPos", targetRay.direction.ToString()},
                            {"RayHit", raycastHit.ToString()},
                            {"PlrPos", plr.Position.ToString()},
                            {"plrRot", plr.Rotation.ToString()},
                            {"hitBoxes", JsonConvert.SerializeObject(hitboxTypes)},
                            {"recoil", JsonConvert.SerializeObject(intendedRecoil)},
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