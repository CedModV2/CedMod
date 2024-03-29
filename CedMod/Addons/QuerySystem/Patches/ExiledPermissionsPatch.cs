﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CedMod.Addons.QuerySystem.WS;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Exiled.Permissions.Features;
using GameCore;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using NorthwoodLib.Pools;
using RemoteAdmin;
using UnityEngine;

namespace CedMod.Addons.QuerySystem.Patches
{
#if EXILED
    [HarmonyPatch(typeof(Permissions), nameof(Permissions.CheckPermission), typeof(CommandSender), typeof(string))]
    public static class ExiledPermissionsPatch
    {
        public static bool Prefix(ref bool __result, CommandSender sender, string permission)
        {
            if (!sender.FullPermissions)
            {
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    PluginAPI.Core.Log.Info($"Exiled perms: {sender.GetType().FullName}");
                
                switch (sender)
                {
                    case ServerConsoleSender _:
                        break;
                    case PlayerCommandSender _:
                        Player player = Player.Get(sender.SenderId);
                        if (player == null)
                        {
                            __result = CheckPermission(sender.SenderId, permission);
                            return false;
                        }
                        __result = player == Server.Host || player.CheckPermission(permission);
                        return false;
                    case CmSender _:
                        __result = CheckPermission(sender.SenderId, permission);
                        return false;
                }
            }

            __result = true;
            return false;
        }

        public static bool CheckPermission(string userId, string permission)
        {
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                PluginAPI.Core.Log.Info(userId);
            if (string.IsNullOrEmpty(permission))
                return false;
            
            string plyGroupKey = Player.Get(userId) != null ? Player.Get(userId).GroupName : ServerStatic.GetPermissionsHandler()._members.FirstOrDefault(g => g.Key == userId).Value;
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                PluginAPI.Core.Log.Info(plyGroupKey);
            
            if (plyGroupKey is null || !Permissions.Groups.TryGetValue(plyGroupKey, out Group group))
            {
                group = Permissions.DefaultGroup;
            }

            if (CedModMain.Singleton.Config.QuerySystem.Debug)
            {
                foreach (var grp in Exiled.Permissions.Extensions.Permissions.Groups)
                {
                    PluginAPI.Core.Log.Info(grp.Key);
                }
            }

            if (group is null)
            {
                return false;
            }

            const char permSeparator = '.';
            const string allPerms = ".*";

            if (group.CombinedPermissions.Contains(allPerms) || group.CombinedPermissions.Contains("*"))
                return true;

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
                        strBuilder.Length -= allPerms.Length;
                        strBuilder.Append(permSeparator);
                    }

                    strBuilder.Append(seraratedPermissions[z]);
                    
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
                return result;
            }
            
            bool result2 = group.CombinedPermissions.Contains(permission, StringComparison.OrdinalIgnoreCase);
            return result2;
        }
    }
#endif
}