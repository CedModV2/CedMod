using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AudioPooling;
using CedMod.Addons.Sentinal.Patches.Utilities;
using CentralAuth;
using CommandSystem.Commands.RemoteAdmin;
using CustomPlayerEffects;
using DrawableLine;
using HarmonyLib;
using Hints;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.MicroHID.Modules;
using InventorySystem.Items.Usables;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.FirstPersonControl.Thirdperson;
using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers;
using PlayerRoles.PlayableScps;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.Spectating;
using PlayerRoles.Subroutines;
using PlayerRoles.Visibility;
using PlayerRoles.Voice;
using RelativePositioning;
using UnityEngine;
using Utils.NonAllocLINQ;
using Logger = LabApi.Features.Console.Logger;
using MicroHIDItem = InventorySystem.Items.MicroHID.MicroHIDItem;
using RadioItem = InventorySystem.Items.Radio.RadioItem;
using Random = UnityEngine.Random;
using Scp1344Item = InventorySystem.Items.Usables.Scp1344.Scp1344Item;
using Scp1576Item = InventorySystem.Items.Usables.Scp1576.Scp1576Item;
using Scp1853Item = InventorySystem.Items.Usables.Scp1853Item;

namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.WriteAll))]
    public static class FpcServerPositionDistributorPatch
    {
        public static Dictionary<ReferenceHub, Dictionary<ReferenceHub, (float, bool)>> VisibilityCache = new Dictionary<ReferenceHub, Dictionary<ReferenceHub, (float, bool)>>();
        public static Dictionary<ReferenceHub, RelativePosition> SyncDatas = new Dictionary<ReferenceHub, RelativePosition>();
        public static FpcSyncData InvisibleSync = new FpcSyncData();
        
        public static event Func<ReferenceHub, ReferenceHub, RoleTypeId, RoleTypeId> RoleSync; 
        public static bool Prefix(ReferenceHub receiver, NetworkWriter writer)
        {
            SyncDatas.Clear();
            
            if (CedModMain.Singleton.Config.CedMod.DisableFakeSyncing)
                return true;
            
            ushort count = 0;

            bool hasVisCtrl;
            VisibilityController visCtrl;
            
            if (receiver.roleManager.CurrentRole is ICustomVisibilityRole icvr)
            {
                hasVisCtrl = true;
                visCtrl = icvr.VisibilityController;
            }
            else
            {
                hasVisCtrl = false;
                visCtrl = null;
            }
            
            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub.netId == receiver.netId)
                    continue;

                if (!(hub.roleManager.CurrentRole is IFpcRole fpc))
                    continue;

                bool invisible = hasVisCtrl && !visCtrl.ValidateVisibility(hub);
                bool spoofRole = true;
                if (!invisible && receiver.roleManager.CurrentRole.RoleTypeId == RoleTypeId.Scp106 && receiver.roleManager.CurrentRole is Scp106Role scp106Role && scp106Role.SubroutineModule.TryGetSubroutine(out Scp106StalkVisibilityController stalkVisibilityController))
                {
                    if (hub.roleManager.CurrentRole is FpcStandardRoleBase fpcStandardRoleBase && !stalkVisibilityController.GetVisibilityForPlayer(hub, fpcStandardRoleBase))
                    {
                        invisible = true;
                        spoofRole = false;
                    }
                }

                if (hub.playerEffectsController.TryGetEffect(out Scp1344 scp1344) && scp1344.IsEnabled)
                    spoofRole = false;

                bool doChecking = true;
                //if the player is a spectator we will perform checksfrom the POV of the spectator to prevent cloud ESP features from live-sharing data from spectator. 
                ReferenceHub toCheckPlayer = receiver;
                if (receiver.roleManager.CurrentRole is SpectatorRole spectatorRole)
                    toCheckPlayer = ReferenceHub.AllHubs.FirstOrDefault(s => s.netId == spectatorRole.SyncedSpectatedNetId);
                
                if (toCheckPlayer == null)
                    toCheckPlayer = receiver;

                if (invisible)
                    doChecking = false;

                if (receiver.GetTeam() == Team.SCPs || receiver.GetRoleId() == RoleTypeId.Overwatch)
                    doChecking = false;

                if (toCheckPlayer.playerEffectsController.TryGetEffect(out scp1344) && scp1344.IsEnabled)
                    doChecking = false;
                
                bool losCheckInvisible = false;
                
                if (doChecking)
                {
                    bool rayNeeded = false;
                    bool cacheUpdate = false;
                    bool wasInvis = false;
                    if (VisibilityCache.TryGetValue(toCheckPlayer, out Dictionary<ReferenceHub, (float, bool)> visibilityCache) && visibilityCache.TryGetValue(hub, out var visibility))
                    {
                        if (visibility.Item2)
                            wasInvis = true;
                        if (toCheckPlayer != receiver) //dont re-check for specators, its a waste. even if that means the spectator might miss 1 frame of pos sync
                        {
                            rayNeeded = false;
                            losCheckInvisible = visibility.Item2;
                        }
                        else if (Time.time > visibility.Item1)
                        {
                            rayNeeded = true;
                            cacheUpdate = true;
                        }
                        else
                        {
                            losCheckInvisible = visibility.Item2;
                        }
                    }
                    
                    if (Vector3.Distance(receiver.transform.position, hub.transform.position) <= 1) //raycasts become very heavy if too close
                    {
                        rayNeeded = false;
                        losCheckInvisible = false;
                    }
                    else if (hub.roleManager.CurrentRole is IFpcRole role && role.FpcModule.CharacterModelInstance is AnimatedCharacterModel animatedCharacterModel && animatedCharacterModel.FootstepPlayable && Vector3.Distance(receiver.transform.position, hub.transform.position) <= GetFootstepDistance(receiver, animatedCharacterModel))
                    {
                        rayNeeded = false;
                        losCheckInvisible = false;
                    }
                    else if (hub.inventory.CurInstance is Firearm firearm1 && firearm1.TryGetModule(out AudioModule module) && SentinalBehaviour.GunshotSource.TryGetValue(hub.netId, out var time) && time.Item1 >= Time.time && Vector3.Distance(hub.GetPosition(), receiver.GetPosition()) <= time.maxDistance)
                    {
                        if (wasInvis &&  module._clipToIndex.TryGetValue(time.Item2, out var index))
                        {
                            Timing.CallDelayed(0f, () =>
                            {
                                module.SendRpc(receiver, writer =>
                                {
                                    module.ServerSend(writer, index, module.RandomPitch, MixerChannel.Weapons, time.maxDistance, hub.GetPosition(), true);
                                });
                            });
                        }
                        rayNeeded = false;
                        losCheckInvisible = false;
                    }
                    else if (hub.inventory.CurInstance is MicroHIDItem hidItem && hidItem.CycleController.Phase != MicroHidPhase.Standby)
                    {
                        rayNeeded = false;
                        losCheckInvisible = false;
                    }
                    else
                    {
                        int roomRange = 2;
                        if (toCheckPlayer.inventory.CurInstance is Firearm firearm && firearm.TryGetModule(out IAdsModule adsModule) && adsModule.AdsAmount == 1)
                            roomRange = 3;

                        if (!losCheckInvisible && !RoomCache.InRange(toCheckPlayer, hub, roomRange))
                        {
                            rayNeeded = false;
                            losCheckInvisible = true;
                        }

                        if (VoicePacketPacket.Voice.TryGetValue(hub.netId, out var val) && val >= Time.time && Vector3.Distance(toCheckPlayer.GetPosition(), hub.GetPosition()) <= 22) //allow vc
                        {
                            rayNeeded = false;
                            losCheckInvisible = false;
                        }

                        if (rayNeeded)
                        {
                            losCheckInvisible = !PerformVisbilityRaycast(toCheckPlayer, hub) && !PerformVisbilityRaycast(hub, toCheckPlayer);
                        }
                    }
                    
                    if (!VisibilityCache.ContainsKey(toCheckPlayer))
                        VisibilityCache[toCheckPlayer] = new Dictionary<ReferenceHub, (float, bool)>();
                    
                    if (!VisibilityCache[toCheckPlayer].ContainsKey(hub) || cacheUpdate || losCheckInvisible != wasInvis)
                        VisibilityCache[toCheckPlayer][hub] = (Time.time + (losCheckInvisible ? 0.1f : 0.5f), losCheckInvisible);
                }
                
                if (spoofRole)
                    spoofRole = invisible;
                PlayerValidatedVisibilityEventArgs ev = new PlayerValidatedVisibilityEventArgs(receiver, hub, doChecking ? !losCheckInvisible : !invisible);
                LabApi.Events.Handlers.PlayerEvents.OnValidatedVisibility(ev);
                invisible = !ev.IsVisible;
                FpcSyncData data = GetNewSyncData(receiver, hub, fpc.FpcModule, invisible);
                
                if (!invisible)
                {
                    FpcServerPositionDistributor._bufferPlayerIDs[count] = hub.PlayerId;
                    FpcServerPositionDistributor._bufferSyncData[count] = data;
                    count++;

                    RoleTypeId toSend = hub.roleManager.CurrentRole.RoleTypeId;
                    if (hub.roleManager.CurrentRole is IObfuscatedRole ior)
                        toSend = ior.GetRoleForUser(receiver);
                    
                    if (hub.roleManager.CurrentRole is Scp079Role scp079Role)
                    {
                        if (Vector3.Distance(scp079Role.CameraPosition, receiver.transform.position) <= 30)
                            toSend = hub.roleManager.CurrentRole.RoleTypeId;
                    }

                    var send = RoleSync?.Invoke(hub, receiver, toSend);
                    if (send != null)
                        toSend = send.Value;

                    if (!hub.roleManager.PreviouslySentRole.TryGetValue(receiver.netId, out RoleTypeId prev) || prev != toSend)
                        SendRole(receiver, hub, toSend);
                }
                else
                { 
                    var toSend = hub.roleManager.CurrentRole.Team == Team.SCPs ? hub.roleManager.CurrentRole.RoleTypeId : RoleTypeId.Spectator;
                    if ((PermissionsHandler.IsPermitted(receiver.serverRoles.Permissions, PlayerPermissions.GameplayData) && !QuerySystem.QuerySystem.IsDev) || receiver.roleManager.CurrentRole.Team == Team.SCPs)
                        toSend = hub.roleManager.CurrentRole.RoleTypeId;
                    
                    if (hub.roleManager.CurrentRole is Scp079Role scp079Role)
                    {
                        if (Vector3.Distance(scp079Role.CameraPosition, receiver.transform.position) <= 30)
                            toSend = hub.roleManager.CurrentRole.RoleTypeId;
                    }

                    if (Intercom._singleton != null && Intercom.State == IntercomState.InUse)
                    {
                        if (Intercom._singleton._curSpeaker != null && (Intercom._singleton._curSpeaker == hub || Intercom._singleton._adminOverrides.Contains(hub)))
                            toSend = hub.roleManager.CurrentRole.RoleTypeId;
                    }
                    
                    if (hub.inventory.CurInstance != null && hub.inventory.CurInstance is Scp1576Item scp1576Item && scp1576Item.IsUsing)
                        toSend = hub.roleManager.CurrentRole.RoleTypeId;
                    
                    if (VoicePacketPacket.Radio.TryGetValue(hub.netId, out var val) && val >= Time.time)
                        toSend = hub.roleManager.CurrentRole.RoleTypeId;
                    
                    if (hub.roleManager.CurrentRole is IObfuscatedRole ior)
                        toSend = ior.GetRoleForUser(receiver);
                    
                    var send = RoleSync?.Invoke(hub, receiver, toSend);
                    if (send != null)
                        toSend = send.Value;
                    
                    if (!spoofRole)
                        toSend = hub.roleManager.CurrentRole.RoleTypeId;
                    
                    if (!hub.roleManager.PreviouslySentRole.TryGetValue(receiver.netId, out RoleTypeId prev) || prev != toSend)
                    {
                        FpcServerPositionDistributor._bufferPlayerIDs[count] = hub.PlayerId;
                        FpcServerPositionDistributor._bufferSyncData[count] = InvisibleSync; 
                        SendRole(receiver, hub, toSend);
                    }
                }
            }

            writer.WriteUShort(count);

            for (int i = 0; i < count; i++)
            {
                writer.WriteRecyclablePlayerId(new RecyclablePlayerId(FpcServerPositionDistributor._bufferPlayerIDs[i]));
                FpcServerPositionDistributor._bufferSyncData[i].Write(writer);
            }
            
            return false;
        }

        private static float GetFootstepDistance(ReferenceHub receiver, AnimatedCharacterModel animatedCharacterModel)
        {
            if (animatedCharacterModel.Role.Team == Team.SCPs)
                return animatedCharacterModel.FootstepLoudnessDistance;

            switch (animatedCharacterModel.FpcModule.SyncMovementState)
            {
                case PlayerMovementState.Sneaking:
                case PlayerMovementState.Crouching:
                    return 0;
                case PlayerMovementState.Walking:
                    return 8;
                case PlayerMovementState.Sprinting:
                    return Vector3.Angle(receiver.transform.forward, animatedCharacterModel.transform.position) >= 65 ? 10 : 16;
            }
            
            return animatedCharacterModel.FootstepLoudnessDistance;
        }

        public static FpcSyncData GetNewSyncData(ReferenceHub receiver, ReferenceHub target, FirstPersonMovementModule fpmm, bool isInvisible)
        {
            FpcSyncData prevSyncData = GetPrevSyncData(receiver, target);
            bool hasSync = SyncDatas.TryGetValue(target, out var sync);
            if (!hasSync)
                sync = new RelativePosition(target.transform.position);
            
            FpcSyncData newSyncData = isInvisible ? InvisibleSync : new FpcSyncData(prevSyncData, fpmm.SyncMovementState, fpmm.IsGrounded, sync, fpmm.MouseLook);
            FpcServerPositionDistributor.PreviouslySent[receiver.netId][target.netId] = newSyncData;
            if (!hasSync)
                SyncDatas[target] = sync;
            
            return newSyncData;
        }
        
        public static FpcSyncData GetPrevSyncData(ReferenceHub receiver, ReferenceHub target)
        {
            Dictionary<uint, FpcSyncData> dictionary;
            if (!FpcServerPositionDistributor.PreviouslySent.TryGetValue(receiver.netId, out dictionary))
            {
                FpcServerPositionDistributor.PreviouslySent.Add(receiver.netId, new Dictionary<uint, FpcSyncData>());
                return InvisibleSync;
            }
            FpcSyncData fpcSyncData;
            return !dictionary.TryGetValue(target.netId, out fpcSyncData) ? InvisibleSync : fpcSyncData;
        }
        
        public static void SendRole(ReferenceHub receiver, ReferenceHub hub, RoleTypeId toSend)
        { 
            if (receiver == hub)
                toSend = hub.roleManager.CurrentRole.RoleTypeId;
            
            NetworkConnection conn = receiver.connectionToClient;
            bool funny = false;
            if ((toSend == RoleTypeId.Spectator) && Player.Count >= 6 && Random.Range(0, 500) >= 250)
            {
                //Logger.Info($"Logging random OW to plr {receiver.PlayerId}");
                funny = true;
            }
            
            conn.Send(new RoleSyncInfo(hub, funny ? RoleTypeId.Overwatch : toSend, receiver));
            hub.roleManager.PreviouslySentRole[receiver.netId] = toSend;
                
            if (toSend == hub.roleManager.CurrentRole.RoleTypeId && hub.roleManager.CurrentRole is ISubroutinedRole subroutinedRole)
            {
                foreach (var routine in subroutinedRole.SubroutineModule.AllSubroutines)
                {
                    routine.ServerSendRpc(receiver);
                }
            }
        }
        
        public static Vector3[] CastPositions = new []
        {
            new Vector3(0, 0.885f), //head
            
            new Vector3(-0.3f, 0.6f, 0), //right top
            new Vector3(0.3f, 0.6f, 0),  //left top
            
            new Vector3(-0.3f, 0f, 0), //right mid
            new Vector3(0.3f, 0f, 0),  //left mid
            new Vector3(0f, 0f, 0),  //mid
            
            new Vector3(-0.3f, -0.90f, 0), //right bottom
            new Vector3(0.3f, -0.90f, 0),  //left bottom
        };
        
        private static List<Vector3> _castPositions = new List<Vector3>();
        
        private static bool PerformVisbilityRaycast(ReferenceHub receiver, ReferenceHub hub)
        {
            _castPositions.Clear();
            Vector3 camPos = receiver.PlayerCameraReference.position;
            float widthMultiplier = hub.transform.localScale.x;
            var pos = hub.GetPosition();

            foreach (var point in CastPositions)
            {
                var receiverRot = Quaternion.LookRotation((hub.GetPosition() - receiver.GetPosition()).normalized);
                var relative = pos + (receiverRot * new Vector3(point.x, point.y, point.z) * widthMultiplier);
                
                _castPositions.Add(relative);
            }
            
            if (hub.inventory.CurInstance is Firearm || hub.inventory.CurInstance is MicroHIDItem)
            {
                _castPositions.Add(pos + new Vector3(0, 0.235f, 0.255f));
                _castPositions.Add(pos + new Vector3(0, 0.235f, 0.644f));
                _castPositions.Add(pos + new Vector3(0, 0.235f, 1.205f));
            }
            
            foreach (var p in _castPositions)
            {
                var relative = p;
                if (receiver.authManager.InstanceMode == ClientInstanceMode.ReadyClient)
                    relative += hub.GetVelocity() * Mathf.Min(0.5f, LiteNetLib4MirrorServer.Peers[receiver.connectionToClient.connectionId].Ping * 2f / 1000f);
                
                if (!Physics.Linecast(camPos, relative, out var hit, VisionInformation.VisionLayerMask, QueryTriggerInteraction.Ignore))
                    return true;
                
                if (ItemPickupHandler.DoorColliders.TryGetValue(hit.collider, out var door) && door.IsMoving)
                    return true;
            }
            
            if (receiver.authManager.InstanceMode == ClientInstanceMode.ReadyClient && receiver.GetVelocity().magnitude > 0f)
            {
                var receiverPos = receiver.GetPosition() + receiver.GetVelocity() * Mathf.Min(0.75f, + 0.25f + (receiver.authManager.InstanceMode == ClientInstanceMode.ReadyClient ? LiteNetLib4MirrorServer.Peers[receiver.connectionToClient.connectionId].Ping * 2f / 1000f : 0));
                if (!Physics.Linecast(receiver.PlayerCameraReference.position, receiverPos, VisionInformation.VisionLayerMask, QueryTriggerInteraction.Ignore) && !Physics.Linecast(receiverPos, hub.PlayerCameraReference.position, VisionInformation.VisionLayerMask, QueryTriggerInteraction.Ignore))
                    return true;
            }

            return false;
        }
    }
}