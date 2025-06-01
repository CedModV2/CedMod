using System;
using System.Collections.Generic;
using System.Diagnostics;
using CedMod.Addons.QuerySystem.WS;
using CustomPlayerEffects;
using HarmonyLib;
using InventorySystem.Searching;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.PlayableScps.Scp049.Zombies;
using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.Visibility;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;
using Scp049Role = PlayerRoles.PlayableScps.Scp049.Scp049Role;
using Scp106Role = PlayerRoles.PlayableScps.Scp106.Scp106Role;

namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(FpcSyncData), nameof(FpcSyncData.TryApply))] //todo implement when there is a need for it
    public static class FpcSyncDataPatch
    {
        public static Dictionary<uint, Dictionary<ulong, List<(string position, int ping)>>> MovementViolations = new Dictionary<uint, Dictionary<ulong, List<(string position, int ping)>>>();
        public static Dictionary<ReferenceHub, FpcSyncData> SyncDatas = new Dictionary<ReferenceHub, FpcSyncData>();
        public static Dictionary<uint, float> PingTolerance = new Dictionary<uint, float>();
        
        public static bool Prefix(FpcSyncData __instance, ReferenceHub hub, ref FirstPersonMovementModule module, ref bool bit)
        {
            try
            {
                bool disallowMovement = false;
                bool tempBypass = false;
                if (SyncDatas.TryGetValue(hub, out FpcSyncData data) && __instance == data)
                    return true;
                
                if (hub.roleManager.CurrentRole is Scp106Role scp106Role && scp106Role.SubroutineModule.TryGetSubroutine(out Scp106StalkAbility stalkAbility) && stalkAbility._sinkhole.IsDuringAnimation && SentinalBehaviour.Stalking106.TryGetValue(hub.netId, out var stalk) && stalk + stalkAbility.SubmergeTime > Time.time) //no movement during animation
                    disallowMovement = true;
                
                if (hub.roleManager.CurrentRole is Scp049Role scp049Role && scp049Role.SubroutineModule.TryGetSubroutine(out Scp049ResurrectAbility resurrectAbility) && resurrectAbility.IsInProgress) //no movement during reviving
                    disallowMovement = true;
                
                if (hub.roleManager.CurrentRole is ZombieRole zombieRole && zombieRole.SubroutineModule.TryGetSubroutine(out ZombieConsumeAbility consumeAbility) && consumeAbility.IsInProgress) //no movement during consuming
                    disallowMovement = true;
                
                //todo: when the sl movement system doesnt think people are behind walls
                //if (hub.searchCoordinator.SessionPipe.Status != SearchSessionPipe.Activity.Idle) //no movement during pickup
                //    disallowMovement = true;

                if (disallowMovement && PingTolerance.TryGetValue(hub.netId, out float tolerance) && tolerance >= Time.time)
                {
                    tempBypass = true;
                    Logger.Info($"Granted passage for hub {tolerance} >= {Time.time}");
                }
                else if (disallowMovement && !PingTolerance.ContainsKey(hub.netId))
                {
                    Logger.Info($"Granting bypass for hub tolt {Mathf.Min(0.5f, (LiteNetLib4MirrorServer.Peers[hub.connectionToClient.connectionId].Ping * 1.2f) / 1000)} curt {Time.time}");
                    PingTolerance[hub.netId] = Time.time + Mathf.Min(0.5f, (LiteNetLib4MirrorServer.Peers[hub.connectionToClient.connectionId].Ping * 2f) / 1000);
                }
                
                if (disallowMovement && !tempBypass)
                {
                    if (Vector3.Distance(hub.GetPosition(), __instance._position.Position) >= 1)
                        hub.TryOverridePosition(hub.GetPosition());
                    
                    if (!MovementViolations.ContainsKey(hub.netId))
                    {
                        MovementViolations[hub.netId] = new Dictionary<ulong, List<(string position, int ping)>>();
                    }

                    if (!MovementViolations[hub.netId].ContainsKey(SentinalBehaviour.UFrames))
                        MovementViolations[hub.netId][SentinalBehaviour.UFrames] = new List<(string position, int ping)>();
                
                    MovementViolations[hub.netId][SentinalBehaviour.UFrames].Add((__instance._position.Position.ToString(), LiteNetLib4MirrorServer.Peers[hub.connectionToClient.connectionId].Ping * 2));
                    return false;
                }

                if (!disallowMovement && PingTolerance.ContainsKey(hub.netId))
                    PingTolerance.Remove(hub.netId);
                
                SyncDatas[hub] = __instance;
                if (!disallowMovement && SentinalBehaviour.Stalking106.ContainsKey(hub.netId))
                    SentinalBehaviour.Stalking106.Remove(hub.netId);
                if (WebSocketSystem.HelloMessage == null || !WebSocketSystem.HelloMessage.SentinalPositions)
                    return true;
                
                SentinalBehaviour.DirtyPositions.Enqueue((hub, hub.transform.eulerAngles, hub.transform.position, SentinalBehaviour.UFrames));
            }
            catch (Exception e)
            {
                LabApi.Features.Console.Logger.Error(e.ToString());
            }

            return true;
        }
    }
}