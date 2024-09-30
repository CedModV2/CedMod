using Mirror;
using PlayerRoles;
using PlayerRoles.Visibility;

namespace CedMod.Addons.Sentinal.Patches
{
    //patch responsible for hiding and showing users without leaking info on initialjoin, any missed syncs are caught by the position distributor
    //[HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.Update))]
    public static class PlayerRoleManagerPatch
    {
        public static bool Prefix(PlayerRoleManager __instance)
        {
            if (!NetworkServer.active || !__instance._sendNextFrame)
                return false;

            __instance._sendNextFrame = false;

            foreach (ReferenceHub receiver in ReferenceHub.AllHubs)
            {
                if (receiver.isLocalPlayer)
                    continue;

                RoleTypeId toSend = __instance.CurrentRole.RoleTypeId;

                if (__instance.CurrentRole is IObfuscatedRole ior)
                    toSend = ior.GetRoleForUser(receiver);
                
                
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
                
                bool invisible = hasVisCtrl && !visCtrl.ValidateVisibility(__instance._hub) && !PermissionsHandler.IsPermitted(receiver.serverRoles.Permissions, PlayerPermissions.GameplayData) && receiver.roleManager.CurrentRole.Team != Team.SCPs;
                if (invisible)
                    toSend = RoleTypeId.Filmmaker;
                
                if (__instance.PreviouslySentRole.TryGetValue(receiver.netId, out RoleTypeId prev) && prev == toSend)
                    continue;

                NetworkConnection conn = receiver.connectionToClient;
                conn.Send(new RoleSyncInfo(__instance.Hub, toSend, receiver));
                __instance.PreviouslySentRole[receiver.netId] = toSend;
            }

            return false;
        }
    }
}