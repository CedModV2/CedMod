﻿using System;
using System.Linq;
using System.Text;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Exiled.Permissions.Features;
using HarmonyLib;
using LabApi.Features.Console;
using NorthwoodLib.Pools;

namespace CedMod.Addons.QuerySystem.Patches
{
#if EXILED
    [HarmonyPatch(typeof(Permissions), nameof(Permissions.CheckPermission), typeof(Player), typeof(string))]
    public static class ExiledPermissionsPatchPlayer
    {
        public static bool Prefix(ref bool __result, Player player, string permission)
        {
            if (string.IsNullOrEmpty(permission))
            {
                __result = false;
                return false;
            }

            if (Server.Host == player)
            {
                __result = true;
                return false;
            }

            if (player is null || player.GameObject is null || Permissions.Groups is null || Permissions.Groups.Count == 0)
            {
                __result = false;
                return false;
            }
            
            Logger.Debug($"UserID: {player.UserId} | PlayerId: {player.Id}", Exiled.Permissions.Permissions.Instance.Config.ShouldDebugBeShown);
            Logger.Debug($"Permission string: {permission}", Exiled.Permissions.Permissions.Instance.Config.ShouldDebugBeShown);

            string plyGroupKey = player.Group is not null ? (string.IsNullOrEmpty(player.GroupName) ? ServerStatic.PermissionsHandler.Groups.FirstOrDefault(g => g.Value.EqualsTo(player.Group)).Key : ServerStatic.PermissionsHandler.Groups.FirstOrDefault(g => g.Key == player.GroupName).Key) : null;
            Logger.Debug($"GroupKey: {plyGroupKey ?? "(null)"}", Exiled.Permissions.Permissions.Instance.Config.ShouldDebugBeShown);

            if (plyGroupKey is null || !Permissions.Groups.TryGetValue(plyGroupKey, out Group group))
            {
                Logger.Debug("The source group is null, the default group is used", Exiled.Permissions.Permissions.Instance.Config.ShouldDebugBeShown);
                group = Permissions.DefaultGroup;
            }

            if (group is null)
            {
                Logger.Debug("There's no default group, returning false...", Exiled.Permissions.Permissions.Instance.Config.ShouldDebugBeShown);
                
                __result = false;
                return false;
            }

            const char permSeparator = '.';
            const string allPerms = ".*";

            if (group.CombinedPermissions.Contains(allPerms) || group.CombinedPermissions.Contains("*"))
            {
                __result = true;
                return false;
            }

            if (permission.Contains(permSeparator))
            {
                StringBuilder strBuilder = StringBuilderPool.Shared.Rent();
                string[] seraratedPermissions = permission.Split(permSeparator);

                bool Check(string source) => group.CombinedPermissions.Contains(source, StringComparison.OrdinalIgnoreCase);

                bool result = false;
                for (int z = 0; z < seraratedPermissions.Length; z++)
                {
                    if (z != 0)
                    {
                        // We need to clear the last ALL_PERMS line
                        // or it'll be like 'permission.*.subpermission'.
                        strBuilder.Length -= allPerms.Length;

                        // Separate permission groups by using its separator.
                        strBuilder.Append(permSeparator);
                    }

                    strBuilder.Append(seraratedPermissions[z]);

                    // If it's the last index,
                    // then we don't need to check for all permissions of the subpermission.
                    if (z == seraratedPermissions.Length - 1)
                    {
                        result = Check(strBuilder.ToString());
                        break;
                    }

                    strBuilder.Append(allPerms);
                    if (Check(strBuilder.ToString()))
                    {
                        result = true;
                        break;
                    }
                }

                StringBuilderPool.Shared.Return(strBuilder);

                Logger.Debug($"Result in the block: {result}", Exiled.Permissions.Permissions.Instance.Config.ShouldDebugBeShown);
                __result = result;
                return false;
            }

            // It'll work when there is no dot in the permission.
            bool result2 = group.CombinedPermissions.Contains(permission, StringComparison.OrdinalIgnoreCase);
            Logger.Debug($"Result outside the block: {result2}", Exiled.Permissions.Permissions.Instance.Config.ShouldDebugBeShown);
            __result = result2;
            return false;
        }
    }
#endif
}