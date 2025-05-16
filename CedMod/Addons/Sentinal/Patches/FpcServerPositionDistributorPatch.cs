using System.Collections.Generic;
using CommandSystem.Commands.RemoteAdmin;
using HarmonyLib;
using InventorySystem.Items.Radio;
using InventorySystem.Items.Usables;
using LabApi.Events.Arguments.PlayerEvents;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.Subroutines;
using PlayerRoles.Visibility;
using PlayerRoles.Voice;
using UnityEngine;

namespace CedMod.Addons.Sentinal.Patches
{
    //patch responsible for hiding and showing users when in and out of range
    [HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.WriteAll))]
    public static class FpcServerPositionDistributorPatch
    {
        public static List<RoleTypeId> RandomRoles = new List<RoleTypeId>()
        {
            RoleTypeId.Filmmaker,
            RoleTypeId.Scientist,
            RoleTypeId.ClassD,
            RoleTypeId.ChaosMarauder,
            RoleTypeId.NtfCaptain,
            RoleTypeId.NtfSpecialist,
            RoleTypeId.Overwatch,
            RoleTypeId.Filmmaker,
            RoleTypeId.FacilityGuard,
            RoleTypeId.NtfSpecialist,
            RoleTypeId.Tutorial
        };
        
        public static bool Prefix(ReferenceHub receiver, NetworkWriter writer)
        {
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

            bool hasRadio = false;
            foreach (var it in receiver.inventory.UserInventory.Items)
            {
                if (it.Value is RadioItem radioItem && radioItem.IsUsable)
                    hasRadio = true;
            }

            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub.netId == receiver.netId)
                    continue;

                if (!(hub.roleManager.CurrentRole is IFpcRole fpc))
                    continue;

                bool invisible = hasVisCtrl && !visCtrl.ValidateVisibility(hub);
                FpcSyncData data = FpcServerPositionDistributor.GetNewSyncData(receiver, hub, fpc.FpcModule, invisible);
                PlayerValidatedVisibilityEventArgs ev = new PlayerValidatedVisibilityEventArgs(receiver, hub, !invisible);
                LabApi.Events.Handlers.PlayerEvents.OnValidatedVisibility(ev);
                invisible = !ev.IsVisible;

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

                    if (!hub.roleManager.PreviouslySentRole.TryGetValue(receiver.netId, out RoleTypeId prev) || prev != toSend)
                        SendRole(receiver, hub, toSend);
                }
                else
                { 
                    var toSend = hub.roleManager.CurrentRole.Team == Team.SCPs ? hub.roleManager.CurrentRole.RoleTypeId : RoleTypeId.Filmmaker;
                    if (PermissionsHandler.IsPermitted(receiver.serverRoles.Permissions, PlayerPermissions.GameplayData) || receiver.roleManager.CurrentRole.Team == Team.SCPs)
                        toSend = hub.roleManager.CurrentRole.RoleTypeId;
                    
                    if (hub.roleManager.CurrentRole is Scp079Role scp079Role)
                    {
                        if (Vector3.Distance(scp079Role.CameraPosition, receiver.transform.position) <= 30)
                            toSend = hub.roleManager.CurrentRole.RoleTypeId;
                    }

                    if (Intercom._singleton != null)
                    {
                        if (Intercom._singleton._curSpeaker != null && (Intercom._singleton._curSpeaker == hub || Intercom._singleton._adminOverrides.Contains(hub)))
                            toSend = hub.roleManager.CurrentRole.RoleTypeId;
                    }
                    
                    if (hub.inventory.CurInstance != null && hub.inventory.CurInstance is Scp1853Item scp1853Item && scp1853Item.IsUsing)
                        toSend = hub.roleManager.CurrentRole.RoleTypeId;
                    
                    if (hasRadio && VoicePacketPacket.Radio.Contains(hub.netId))
                        toSend = hub.roleManager.CurrentRole.RoleTypeId;
                    
                    if (hub.roleManager.CurrentRole is IObfuscatedRole ior)
                        toSend = ior.GetRoleForUser(receiver);
                    
                    if (!hub.roleManager.PreviouslySentRole.TryGetValue(receiver.netId, out RoleTypeId prev) || prev != toSend)
                    {
                        FpcServerPositionDistributor._bufferPlayerIDs[count] = hub.PlayerId;
                        FpcServerPositionDistributor._bufferSyncData[count] = new FpcSyncData();
                        count++;
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

        public static void SendRole(ReferenceHub receiver, ReferenceHub hub, RoleTypeId toSend)
        { 
            if (receiver == hub)
                toSend = hub.roleManager.CurrentRole.RoleTypeId;
            
            NetworkConnection conn = receiver.connectionToClient;
            conn.Send(new RoleSyncInfo(hub, toSend, receiver));
            hub.roleManager.PreviouslySentRole[receiver.netId] = toSend;
            
            if (toSend == hub.roleManager.CurrentRole.RoleTypeId && hub.roleManager.CurrentRole is ISubroutinedRole subroutinedRole)
            {
                foreach (var routine in subroutinedRole.SubroutineModule.AllSubroutines)
                {
                    routine.ServerSendRpc(receiver);
                }
            }
        }
    }
}