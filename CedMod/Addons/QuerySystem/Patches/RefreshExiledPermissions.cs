using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.WS;
using Exiled.Permissions;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using NWAPIPermissionSystem;
using UnityEngine;

namespace CedMod.Addons.QuerySystem.Patches
{
#if !EXILED
    [HarmonyPatch(typeof(NWAPIPermissionSystem.PermissionHandler), nameof(PermissionHandler.ReloadPermissions))]
    public static class RefreshPermissions
    {
        public static bool Prefix()
        {
            if (!WebSocketSystem.UseRa)
            {
                Task.Factory.StartNew(() => { WebSocketSystem.ApplyRa(); });
                return false;
            }

            return true;
        }
    }
#else
    [HarmonyPatch(typeof(Exiled.Permissions.Extensions.Permissions), nameof(Exiled.Permissions.Extensions.Permissions.Reload))]
    public static class RefreshExiledPermissions
    {
        public static bool Prefix()
        {
            if (!WebSocketSystem.UseRa)
            {
                new Thread(() => { WebSocketSystem.ApplyRa(); }).Start();
                return false;
            }

            return true;
        }
    }
#endif
}