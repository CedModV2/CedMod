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
    [HarmonyPatch(typeof(FpcBacktracker))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[] {
        typeof(ReferenceHub), typeof(Vector3), typeof(Quaternion),
        typeof(float), typeof(float), typeof(bool), typeof(bool)
    })]
    public static class FpcBacktrackerConstructorPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            var encapsulate = typeof(Bounds).GetMethod("Encapsulate", new[] { typeof(Vector3) });
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].Calls(encapsulate))
                {
                    code.InsertRange(i, new[]
                    {
                        // ref Bounds
                        new CodeInstruction(OpCodes.Ldloca_S, 2),  // bounds (local #2)

                        // _prevPos
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(FpcBacktracker), "_prevPos")),
                        
                        // forecast
                        new CodeInstruction(OpCodes.Ldarg_S, 5),

                        // claimedPos
                        new CodeInstruction(OpCodes.Ldarg_2),

                        // hub
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FpcBacktrackerConstructorPatch), nameof(TryEncapsulateIfVisible))),
                    });

                    break;
                }
            }

            return code;
        }
        
        public static void TryEncapsulateIfVisible(ref Bounds bounds, Vector3 prevPos, float forecast, Vector3 claimedPos, ReferenceHub hub)
        {
            try
            {
                if (hub.roleManager.CurrentRole is not IFpcRole fpc)
                    return;

                var velocity = fpc.FpcModule.Motor.Velocity;
                Vector3 castStart = hub.PlayerCameraReference.position;
                //we exclude firearms because we already check this somewhere else.
                if (hub.inventory.CurInstance is Firearm || !Physics.Linecast(castStart, claimedPos, VisionInformation.VisionLayerMask))
                {
                    bounds.Encapsulate(prevPos + velocity * forecast);
                }
                else
                {
                    //allow it for now to scope out false positives
                    bounds.Encapsulate(prevPos + velocity * forecast);

                    var plr = Player.Get(hub);
                    if (plr.Room != null)
                    {
                        WebSocketSystem.Enqueue(new QueryCommand()
                        {
                            Recipient = "PANEL",
                            Data = new Dictionary<string, string>()
                            {
                                { "SentinalType", "BarrelLOS" },
                                { "UserId", hub.authManager.UserId },
                                { "LocalCamPos", plr.Room != null ? plr.Room.Transform.InverseTransformPoint(plr.ReferenceHub.PlayerCameraReference.position).ToString() : "" },
                                { "ClaimedPos", plr.Room != null ? plr.Room.Transform.InverseTransformPoint(claimedPos).ToString() : "" },
                                { "TargetPos", Vector3.zero.ToString() },
                                { "Room", plr.Room != null ? plr.Room.Base.name : "" },
                                { "RoomRotation", plr.Room != null ? plr.Room.Rotation.ToString() : "" },
                                { "Ping", (LiteNetLib4MirrorServer.Peers[plr.ReferenceHub.connectionToClient.connectionId].Ping * 2).ToString() },
                            }
                        });
                    }
                    
                    //me when i lie.
                    //plr.SendHitMarker(1);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in cedmod backtrack patch: {e.ToString()}");
            }
        }
    }

    
    
    [HarmonyPatch(typeof(ShotBacktrackData), nameof(ShotBacktrackData.ProcessShot))]
    public static class BacktrackPatch
    {
        public static bool Prefix(ShotBacktrackData __instance, Firearm firearm, Action<ReferenceHub> processingMethod)
        {
            if (!WaypointBase.TryGetWaypoint(__instance.RelativeOwnerPosition.WaypointId, out WaypointBase waypoint) || firearm.Owner == null)
                return true;
            
            try
            {
                Vector3 targetPos = __instance.HasPrimaryTarget ? __instance.PrimaryTargetRelativePosition.Position : Vector3.zero;
                Quaternion claimedRot = waypoint.GetWorldspaceRotation(__instance.RelativeOwnerRotation);
                Vector3 claimedPos = __instance.RelativeOwnerPosition.Position;
                if (firearm.Owner != null && Physics.Linecast(firearm.Owner.GetPosition(), claimedPos, VisionInformation.VisionLayerMask, QueryTriggerInteraction.Ignore))
                {
                    var plr = Player.Get(firearm.Owner);
                    if (plr.Room != null)
                    {
                        WebSocketSystem.Enqueue(new QueryCommand()
                        {
                            Recipient = "PANEL",
                            Data = new Dictionary<string, string>()
                            {
                                { "SentinalType", "BarrelLOS" },
                                { "UserId", firearm.Owner.authManager.UserId },
                                { "LocalCamPos", plr.Room != null ? plr.Room.Transform.InverseTransformPoint(plr.ReferenceHub.PlayerCameraReference.position).ToString() : "" },
                                { "ClaimedPos", plr.Room != null ? plr.Room.Transform.InverseTransformPoint(claimedPos).ToString() : "" },
                                { "TargetPos", plr.Room != null ? plr.Room.Transform.InverseTransformPoint(targetPos).ToString() : "" },
                                { "Room", plr.Room != null ? plr.Room.Base.name : "" },
                                { "RoomRotation", plr.Room != null ? plr.Room.Rotation.ToString() : "" },
                                { "Ping", (LiteNetLib4MirrorServer.Peers[plr.ReferenceHub.connectionToClient.connectionId].Ping * 2).ToString() },
                            }
                        });
                    }

                    if (QuerySystem.IsDev)
                    {
                        Logger.Info("Rejected shot due to not having LOS to barrel.");
                    }

                    //me when i lie.
                    //plr.SendHitMarker(1);
                    //return false;
                }

                using (new FpcBacktracker(firearm.Owner, claimedPos, claimedRot))
                {
                    if (__instance.HasPrimaryTarget)
                    {
                        var ping = __instance.PrimaryTargetHub.authManager.InstanceMode == ClientInstanceMode.ReadyClient ? (LiteNetLib4MirrorServer.Peers[firearm.Owner.connectionToClient.connectionId].Ping * 2 / (float)1000) * 2 + 0.15f : 0.4f;
                        if (Server.Tps / Server.MaxTps <= 0.6)
                            ping += (float)Server.Tps / (float)Server.MaxTps * 1f;
                        else
                        {
                            ping += Math.Abs(1f - (float)Server.Tps / (float)Server.MaxTps);
                        }
                        
                        using (new FpcBacktracker(__instance.PrimaryTargetHub, targetPos, Math.Min(0.4f, ping)))
                        {
#if !EXILED
                            processingMethod(__instance.PrimaryTargetHub);
#else //why the fuck did exiled put their event here
                            ShootingEventArgs args = new(firearm, ref __instance);
                            Exiled.Events.Handlers.Player.Shooting.InvokeSafely(args);
                            if (args.IsAllowed)
                                processingMethod(__instance.PrimaryTargetHub);
#endif
                        }
                    }
                    else
                    {
#if !EXILED
                        processingMethod(null);
#else //why the fuck did exiled put their event here
                        ShootingEventArgs args = new(firearm, ref __instance);
                        Exiled.Events.Handlers.Player.Shooting.InvokeSafely(args);
                        if (args.IsAllowed)
                            processingMethod(null);
#endif
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in cedmod backtrack patch: {e.ToString()}");
                return true;
            }
            return false;
        }
    }
    
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