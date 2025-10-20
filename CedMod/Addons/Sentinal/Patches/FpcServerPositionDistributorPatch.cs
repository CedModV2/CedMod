using System;
using System.Collections.Generic;
using System.Linq;
using AudioPooling;
using CedMod.Addons.Sentinal.Patches.Utilities;
using CentralAuth;
using CustomPlayerEffects;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.MicroHID.Modules;
using InventorySystem.Items.Usables.Scp1344;
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
using PlayerRoles.PlayableScps;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.PlayableScps.Scp173;
using PlayerRoles.Visibility;
using RelativePositioning;
using UnityEngine;
using VoiceChat;
using Logger = LabApi.Features.Console.Logger;
using MicroHIDItem = InventorySystem.Items.MicroHID.MicroHIDItem;
using Scp096Role = PlayerRoles.PlayableScps.Scp096.Scp096Role;
using Scp106Role = PlayerRoles.PlayableScps.Scp106.Scp106Role;
using Scp939Role = PlayerRoles.PlayableScps.Scp939.Scp939Role;

namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.WriteAll))]
    public static class FpcServerPositionDistributorPatch
    {
        public static Dictionary<ReferenceHub, Dictionary<ReferenceHub, float>> VisiblePlayers = new Dictionary<ReferenceHub, Dictionary<ReferenceHub, float>>();
        public static Dictionary<ReferenceHub, Dictionary<ReferenceHub, (float, bool, bool)>> VisibilityCache = new Dictionary<ReferenceHub, Dictionary<ReferenceHub, (float, bool, bool)>>();
        public static Dictionary<ReferenceHub, RelativePosition> SyncDatas = new Dictionary<ReferenceHub, RelativePosition>();
        public static FpcSyncData InvisibleSync = new FpcSyncData();
        public static Dictionary<uint, float> LastAudioSent = new Dictionary<uint, float>();
        
        public static event Func<ReferenceHub, ReferenceHub, bool, bool> EspCheck; 
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
            
            //if the player is a spectator we will perform checksfrom the POV of the spectator to prevent cloud ESP features from live-sharing data from spectator. 
            ReferenceHub toCheckPlayer = receiver;
            //if (receiver.roleManager.CurrentRole is SpectatorRole spectatorRole)
            //    toCheckPlayer = ReferenceHub.AllHubs.FirstOrDefault(s => s.netId == spectatorRole.SyncedSpectatedNetId);
            
            //if (toCheckPlayer == null)
            //    toCheckPlayer = receiver;
            
            if (toCheckPlayer.playerEffectsController.TryGetEffect(out Scp1344 scp1344) && scp1344.IsEnabled)
            {
                foreach (var xray in scp1344._xrayProviders)
                {
                    if (xray is Scp1344HumanXrayProvider humanXrayProvider)
                    {
                        foreach (var orb in humanXrayProvider._orbInstances)
                        {
                            FacilityZone ownZone = orb.Target.GetLastKnownZone();
                            float maxDistance = ownZone == FacilityZone.Surface ? Scp1344HumanXrayProvider.SurfaceVisionDistance : Scp1344HumanXrayProvider.VisionDistance;
                            if (Vector3.Distance(toCheckPlayer.GetPosition(), orb.Target.GetPosition()) > maxDistance + Scp1344HumanXrayProvider.VisionTranslucentDistance)
                                continue;
                            
                            double cycleRaw = NetworkTime.time * Scp1344HumanXrayProvider.OrbUpdateCycleFrequency;
                            long cycleIndex = (long) cycleRaw;
                            float cycleElapsed = (float) (cycleRaw - cycleIndex) / Scp1344HumanXrayProvider.OrbUpdateCycleFrequency;
                            
                            if (orb._prevCycleIndex != cycleIndex || cycleElapsed < Scp1344HumanXrayProvider.OrbTrackingDuration)
                            {
                                float alphaMultiplier = Mathf.InverseLerp(Scp1344HumanXrayProvider.OrbStartFade + Scp1344HumanXrayProvider.OrbFadeDuration, Scp1344HumanXrayProvider.OrbStartFade, cycleElapsed);
                                if (alphaMultiplier > 0)
                                {
                                    if (!VisiblePlayers.ContainsKey(toCheckPlayer))
                                        VisiblePlayers.TryAdd(toCheckPlayer, new Dictionary<ReferenceHub, float>());

                                    VisiblePlayers[toCheckPlayer][orb.Target] = Time.time + 0.1f;
                                }
                            }
                        }
                    }
                }
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

                bool nearPositioning = false;
                bool doChecking = true;
                //if (hub.playerEffectsController.TryGetEffect(out scp1344) && scp1344.IsEnabled)
                //    spoofRole = false;

                if (VisiblePlayers.ContainsKey(toCheckPlayer) && VisiblePlayers[toCheckPlayer].TryGetValue(hub, out var visiblePlayerOrb) && visiblePlayerOrb > Time.time && spoofRole)
                    spoofRole = false;
                
                if (invisible)
                    doChecking = false;

                if (receiver.GetTeam() == Team.SCPs || receiver.GetRoleId() == RoleTypeId.Overwatch || receiver.GetRoleId() == RoleTypeId.Spectator)
                    doChecking = false;

                if (hub.roleManager.CurrentRole is Scp096Role scp096 && (scp096.StateController.RageState != Scp096RageState.Docile || Vector3.Distance(toCheckPlayer.transform.position, hub.transform.position) <= 40))
                {
                    doChecking = false;
                }

                if (hub.roleManager.CurrentRole is Scp106Role scp106 && Vector3.Distance(toCheckPlayer.transform.position, hub.transform.position) <= 4)
                {
                    doChecking = false;
                }

                if (hub.roleManager.CurrentRole is Scp939Role scp939 && Vector3.Distance(toCheckPlayer.transform.position, hub.transform.position) <= 4)
                {
                    doChecking = false;
                }

                if (hub.roleManager.CurrentRole is Scp173Role scp173 && Vector3.Distance(toCheckPlayer.transform.position, hub.transform.position) <= 22)
                {
                    doChecking = false;
                }
                
                bool losCheckInvisible = false;
                
                if (doChecking)
                {
                    bool doNext = true;
                    bool rayNeeded = false;
                    bool cacheUpdate = false;
                    bool wasInvis = false;
                    if (VisibilityCache.TryGetValue(toCheckPlayer, out Dictionary<ReferenceHub, (float, bool, bool)> visibilityCache) && visibilityCache.TryGetValue(hub, out var visibility))
                    {
                        if (visibility.Item2)
                            wasInvis = true;
                        if (toCheckPlayer != receiver) //dont re-check for specators, its a waste. even if that means the spectator might miss 1 frame of pos sync
                        {
                            rayNeeded = false;
                            losCheckInvisible = visibility.Item2;
                            nearPositioning = visibility.Item3;
                        }
                        else if (Time.time > visibility.Item1)
                        {
                            rayNeeded = true;
                            cacheUpdate = true;
                        }
                        else
                        {
                            losCheckInvisible = visibility.Item2;
                            nearPositioning = visibility.Item3;
                        }
                    }
                    
                    if (doNext && Vector3.Distance(receiver.transform.position, hub.transform.position) <= 1) //raycasts become very heavy if too close
                    {
                        rayNeeded = false;
                        losCheckInvisible = false;
                        doNext = false;
                    }
                    
                    if (doNext && hub.roleManager.CurrentRole is IFpcRole role)
                    {
                        if (role.FpcModule.CharacterModelInstance is AnimatedCharacterModel animatedCharacterModel && animatedCharacterModel.FootstepPlayable && Vector3.Distance(receiver.transform.position, hub.transform.position) <= GetFootstepDistance(receiver, animatedCharacterModel))
                        {
                            rayNeeded = false;
                            losCheckInvisible = false;
                            doNext = false;
                        }
                    }
                    
                    if (doNext && hub.inventory.CurInstance is Firearm firearm1 && firearm1.TryGetModule(out AudioModule module) && SentinalBehaviour.GunshotSource.TryGetValue(hub.netId, out var time) && time.Item1 >= Time.time && Vector3.Distance(hub.GetPosition(), receiver.GetPosition()) <= time.maxDistance)
                    {
                        if (wasInvis &&  module._clipToIndex.TryGetValue(time.Item2, out var index))
                        {
                            Timing.CallDelayed(0f, () =>
                            {
                                if (LastAudioSent.TryGetValue(receiver.netId + hub.netId, out var cooldown) && cooldown >= Time.time)
                                    return;
                                    
                                LastAudioSent[receiver.netId + hub.netId] = Time.time + 0.2f;
                                module.SendRpc(receiver, writer =>
                                {
                                    module.ServerSend(writer, index, module.RandomPitch, MixerChannel.Weapons, time.maxDistance, hub.GetPosition(), true);
                                });
                            });
                        }
                        
                        rayNeeded = false;
                        losCheckInvisible = false;
                        doNext = false;
                    }
                    
                    if (doNext && hub.inventory.CurInstance is MicroHIDItem hidItem && hidItem.CycleController.Phase != MicroHidPhase.Standby)
                    {
                        rayNeeded = false;
                        losCheckInvisible = false;
                        doNext = false;
                    }
                    
                    if (doNext)
                    {
                        int roomRange = 2;
                        if (toCheckPlayer.inventory.CurInstance is Firearm firearm && firearm.TryGetModule(out IAdsModule adsModule) && adsModule.AdsAmount == 1)
                            roomRange = 3;

                        if (!losCheckInvisible && !RoomCache.InRange(toCheckPlayer, hub, roomRange))
                        {
                            rayNeeded = false;
                            losCheckInvisible = true;
                        }

                        if (VoicePacketPacket.Voice.TryGetValue(hub.netId, out var val) && val >= Time.time && 
                            receiver.roleManager.CurrentRole is FpcStandardRoleBase roleBase && hub.roleManager.CurrentRole is FpcStandardRoleBase hubRoleBase && 
                            roleBase.VoiceModule.ValidateReceive(hub, hubRoleBase.VoiceModule.CurrentChannel) != VoiceChatChannel.None && Vector3.Distance(toCheckPlayer.GetPosition(), hub.GetPosition()) <= 22)
                        {
                            rayNeeded = false;
                            losCheckInvisible = false;
                        }

                        if (rayNeeded)
                        {
                            losCheckInvisible = !PerformVisbilityRaycast(toCheckPlayer, hub, false) && !PerformVisbilityRaycast(hub, toCheckPlayer, true);
                        }
                    }
                    
                    if (!VisibilityCache.ContainsKey(toCheckPlayer))
                        VisibilityCache[toCheckPlayer] = new Dictionary<ReferenceHub, (float, bool, bool)>();
                    
                    if (!VisibilityCache[toCheckPlayer].ContainsKey(hub) || cacheUpdate || losCheckInvisible != wasInvis)
                        VisibilityCache[toCheckPlayer][hub] = (Time.time + (losCheckInvisible ? 0.1f : 0.5f), losCheckInvisible, nearPositioning);
                }
                
                if (losCheckInvisible && VisiblePlayers.TryGetValue(toCheckPlayer, out var visiblePlayers) && visiblePlayers.TryGetValue(hub, out var visiblePlayer) && visiblePlayer > Time.time)
                {
                    losCheckInvisible = false;
                }

                if (doChecking && EspCheck != null)
                    losCheckInvisible = EspCheck(receiver, hub, losCheckInvisible);
                    
                PlayerValidatedVisibilityEventArgs ev = new PlayerValidatedVisibilityEventArgs(receiver, hub, doChecking ? !losCheckInvisible : !invisible);
                LabApi.Events.Handlers.PlayerEvents.OnValidatedVisibility(ev);
                invisible = !ev.IsVisible;
                RoleTypeId toSend = FpcServerPositionDistributor.GetVisibleRole(receiver, hub);

                if (!spoofRole && hub.IsAlive())
                {
                    if (hub.roleManager.CurrentRole is IObfuscatedRole i)
                        toSend = i.GetRoleForUser(receiver);
                    else 
                        toSend = hub.roleManager.CurrentRole.RoleTypeId;
                }
                
                NetworkWriterPooled networkWriterPooled = NetworkWriterPool.Get();
                RoleTypeId? eventResult = FpcServerPositionDistributor._roleSyncEvent?.Invoke(hub, receiver, toSend, (NetworkWriter)networkWriterPooled);
                if (eventResult.HasValue)
                    toSend = eventResult.Value;
                
                if (!hub.roleManager.PreviouslySentRole.TryGetValue(receiver.netId, out RoleTypeId prev) || prev != toSend)
                {
                    FpcServerPositionDistributor.SendRole(receiver, hub, toSend, networkWriterPooled);
                }
                NetworkWriterPool.Return(networkWriterPooled);
                FpcSyncData data = GetNewSyncData(receiver, hub, fpc.FpcModule, invisible, nearPositioning);
                if (!invisible)
                {
                    FpcServerPositionDistributor._bufferPlayerIDs[count] = hub.PlayerId;
                    FpcServerPositionDistributor._bufferSyncData[count] = data;
                    count++;
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

            if (animatedCharacterModel.Role.Team == Team.ChaosInsurgency)
                return animatedCharacterModel.FootstepLoudnessDistance - 3;

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

        public static FpcSyncData GetNewSyncData(ReferenceHub receiver, ReferenceHub target, FirstPersonMovementModule fpmm, bool isInvisible, bool nearPositioning)
        {
            //nearPositioning = true;
            bool nearestValid = false;
            FpcSyncData prevSyncData = GetPrevSyncData(receiver, target);
            bool hasSync = SyncDatas.TryGetValue(target, out var sync);

            RoomIdentifier hubRoom = null;
            target.TryGetCurrentRoom(out hubRoom);
            if (hubRoom == null)
                target.TryGetLastKnownRoom(out hubRoom);
            
            //this doesnt do anything for now, logic needs rewriting to get nearpositioning to work without noticing
            if (nearPositioning && hubRoom != null && RoomCache.TryGetNearestWaypoint(hubRoom, receiver.GetPosition(), target.GetPosition(), out var nearestWaypoint))
            {
                Vector3 targetPosition = target.transform.position;
                nearestValid = true;
                hasSync = false;
                
                var velocity = target.GetVelocity();
                float speed = velocity.magnitude;
                
                //le rotate for footstep simulation
                float orbitRadius = Mathf.Clamp(speed * 0.2f, 0.1f, 0.75f);
                float spinSpeed = Mathf.Clamp(speed * 2f, 1f, 6f);
                float angle = Time.time * spinSpeed;
                
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * orbitRadius;
                
                targetPosition = new Vector3(nearestWaypoint.x + offset.x, nearestWaypoint.y * -2, nearestWaypoint.z + offset.z);

                sync = new RelativePosition(targetPosition);
            }

            if (!hasSync && !nearestValid)
                sync = new RelativePosition(target.transform.position);

            FpcSyncData newSyncData = InvisibleSync;
            if (!isInvisible && !nearPositioning)
                newSyncData = new FpcSyncData(prevSyncData, fpmm.SyncMovementState, fpmm.IsGrounded, sync, fpmm.MouseLook);
            
            if (nearPositioning)
                newSyncData = new FpcSyncData(prevSyncData, fpmm.SyncMovementState, fpmm.IsGrounded, sync, fpmm.MouseLook);
            

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
        
        private static bool PerformVisbilityRaycast(ReferenceHub receiver, ReferenceHub hub, bool reverse)
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
                
                if (ItemPickupHandler.DoorColliders.TryGetValue(hit.collider, out var door) && (door.IsMoving || door.NetworkTargetState))
                    return true;
            }
            
            if ((receiver.authManager.InstanceMode == ClientInstanceMode.ReadyClient || receiver.authManager.InstanceMode == ClientInstanceMode.Dummy) && receiver.GetVelocity().magnitude > 0f)
            {
                float chances = 0;
                int max = 3;
                float ping = (receiver.authManager.InstanceMode == ClientInstanceMode.ReadyClient ? LiteNetLib4MirrorServer.Peers[receiver.connectionToClient.connectionId].Ping * 2f / 1000f : 0);
                if (ping >= 70)
                    max = 5;
                
                while (chances <= max)
                {
                    chances++;
                    var chanceAmplfier = 0.25f * chances;
                    var amplifier = chanceAmplfier + Mathf.Min(0.75f, + 0.25f + (receiver.authManager.InstanceMode == ClientInstanceMode.ReadyClient ? LiteNetLib4MirrorServer.Peers[receiver.connectionToClient.connectionId].Ping * 2f / 1000f : 0));
                    var receiverPos = receiver.GetPosition() + receiver.GetVelocity() * amplifier;
                    RaycastHit hit2 = default;;
                    if (!Physics.Linecast(receiver.PlayerCameraReference.position, receiverPos, out var hit, VisionInformation.VisionLayerMask, QueryTriggerInteraction.Ignore) && !Physics.Linecast(receiverPos, hub.PlayerCameraReference.position, out hit2, VisionInformation.VisionLayerMask, QueryTriggerInteraction.Ignore))
                        return true;
                    
                    if (ItemPickupHandler.DoorColliders.TryGetValue(hit.collider, out var door) && (door.IsMoving || door.NetworkTargetState))
                        return true;
                    
                    if (hit2.collider != null && ItemPickupHandler.DoorColliders.TryGetValue(hit2.collider, out door) && (door.IsMoving || door.NetworkTargetState))
                        return true;
                }
            }

            RoomIdentifier hubRoom = null;
            hub.TryGetCurrentRoom(out hubRoom);
            if (hubRoom == null)
                hub.TryGetLastKnownRoom(out hubRoom);
            
            //fallback show when around doors
            if (reverse && hubRoom != null && DoorVariant.DoorsByRoom.TryGetValue(hubRoom, out var doorVariants))
            {
                foreach (var door in doorVariants)
                {
                    if (!door.NetworkTargetState && !door.IsMoving && (ItemPickupHandler.DoorMovetimes.TryGetValue(door, out var time) && time + 0.2f >= Time.time))
                        continue;

                    if (Vector3.Distance(door.transform.position, hub.GetPosition()) <= 1f)
                        return true;
                }
            }

            return false;
        }
    }
}