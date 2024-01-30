using HarmonyLib;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.Visibility;

namespace CedMod.Addons.Sentinal.Patches
{
    //patch responsible for hiding and showing users when in and out of range
    [HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.WriteAll))]
    public static class FpcServerPositionDistributorPatch
    {
        public static bool Prefix(ReferenceHub receiver, NetworkWriter writer)
        {
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

                if (!invisible)
                {
                    FpcServerPositionDistributor._bufferPlayerIDs[count] = hub.PlayerId;
                    FpcServerPositionDistributor._bufferSyncData[count] = data;
                    count++;

                    RoleTypeId toSend = hub.roleManager.CurrentRole.RoleTypeId;
                    if (hub.roleManager.CurrentRole is IObfuscatedRole ior)
                        toSend = ior.GetRoleForUser(receiver);

                    if (!hub.roleManager.PreviouslySentRole.TryGetValue(receiver.netId, out RoleTypeId prev) || prev != toSend)
                        SendRole(receiver, hub, toSend);
                }
                else
                {
                    var toSend = RoleTypeId.Filmmaker;
                    if (PermissionsHandler.IsPermitted(receiver.serverRoles.Permissions, PlayerPermissions.GameplayData))
                        toSend = hub.roleManager.CurrentRole.RoleTypeId;
                    
                    if (!hub.roleManager.PreviouslySentRole.TryGetValue(receiver.netId, out RoleTypeId prev) || prev != toSend)
                        SendRole(receiver, hub, toSend);
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
            NetworkConnection conn = receiver.connectionToClient;
            conn.Send(new RoleSyncInfo(hub, toSend, receiver));
            hub.roleManager.PreviouslySentRole[receiver.netId] = toSend;
        }
    }
}