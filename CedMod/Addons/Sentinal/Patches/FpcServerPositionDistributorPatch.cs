using System.Collections.Generic;
using System.Linq;
using CommandSystem.Commands.RemoteAdmin;
using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using MapGeneration;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.Subroutines;
using PlayerRoles.Visibility;
using PlayerRoles.Voice;
using RelativePositioning;
using UnityEngine;
using Utils.NonAllocLINQ;
using Logger = LabApi.Features.Console.Logger;
using RadioItem = InventorySystem.Items.Radio.RadioItem;
using Scp1576Item = InventorySystem.Items.Usables.Scp1576.Scp1576Item;
using Scp1853Item = InventorySystem.Items.Usables.Scp1853Item;

namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.WriteAll))]
    public static class FpcServerPositionDistributorPatch
    {
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
            
            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub.netId == receiver.netId)
                    continue;

                if (!(hub.roleManager.CurrentRole is IFpcRole fpc))
                    continue;

                bool invisible = hasVisCtrl && !visCtrl.ValidateVisibility(hub);
                FpcSyncData data = FpcServerPositionDistributor.GetNewSyncData(receiver, hub, fpc.FpcModule, invisible);
                
                if (!invisible && receiver.roleManager.CurrentRole.RoleTypeId == RoleTypeId.Scp106 && receiver.roleManager.CurrentRole is Scp106Role scp106Role && scp106Role.SubroutineModule.TryGetSubroutine(out Scp106StalkVisibilityController stalkVisibilityController))
                {
                    if (hub.roleManager.CurrentRole is FpcStandardRoleBase fpcStandardRoleBase && !stalkVisibilityController.GetVisibilityForPlayer(hub, fpcStandardRoleBase))
                        invisible = false;
                }
                
                
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
                    var toSend = hub.roleManager.CurrentRole.Team == Team.SCPs ? hub.roleManager.CurrentRole.RoleTypeId : RoleTypeId.Spectator;
                    if (PermissionsHandler.IsPermitted(receiver.serverRoles.Permissions, PlayerPermissions.GameplayData) || receiver.roleManager.CurrentRole.Team == Team.SCPs)
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
                    
                    if (VoicePacketPacket.Radio.Contains(hub.netId))
                        toSend = hub.roleManager.CurrentRole.RoleTypeId;
                    
                    if (hub.roleManager.CurrentRole is IObfuscatedRole ior)
                        toSend = ior.GetRoleForUser(receiver);
                    
                    if (!hub.roleManager.PreviouslySentRole.TryGetValue(receiver.netId, out RoleTypeId prev) || prev != toSend)
                    {
                        FpcServerPositionDistributor._bufferPlayerIDs[count] = hub.PlayerId;
                        FpcServerPositionDistributor._bufferSyncData[count] = new FpcSyncData(); 
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
    }
}