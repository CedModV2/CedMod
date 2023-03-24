using System;
using System.Collections.Generic;
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
                Task.Factory.StartNew(() => { WebSocketSystem.ApplyRa(); });
                return false;
            }

            return true;
        }
    }
    
    [HarmonyPatch(typeof(RefreshRaConfigsPatch), nameof(ServerConfigSynchronizer.RefreshRAConfigs))]
    public static class RefreshRaConfigsPatch
    {
        public static CoroutineHandle CoroutineHandle;
        public static bool Prefix()
        {
            if (CoroutineHandle != null && CoroutineHandle.IsRunning)
                return false;

            CoroutineHandle = Timing.RunCoroutine(QuerySystem.QueryServerEvents.SyncStart(false));

            return false;
        }
    }
}