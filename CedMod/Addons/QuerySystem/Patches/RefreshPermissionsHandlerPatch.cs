using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.WS;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using MEC;
using UnityEngine;

namespace CedMod.Addons.QuerySystem.Patches
{
    [HarmonyPatch(typeof(PermissionsHandler), nameof(PermissionsHandler.RefreshPermissions))]
    public static class RefreshPermissionsHandlerPatch
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
    
    [HarmonyPatch(typeof(ServerConfigSynchronizer), nameof(ServerConfigSynchronizer.RefreshRAConfigs))]
    public static class RefreshRaConfigsPatch
    {
        public static CoroutineHandle CoroutineHandle;
        public static bool Prefix()
        {
            if (CoroutineHandle.IsRunning)
                return false;

            CoroutineHandle = Timing.RunCoroutine(QuerySystem.QueryServerEvents.SyncStart(false));

            return true;
        }
    }
}