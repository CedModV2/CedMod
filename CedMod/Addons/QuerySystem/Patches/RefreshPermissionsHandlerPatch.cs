using System.Threading;
using CedMod.Addons.QuerySystem.WS;
using HarmonyLib;
using MEC;

namespace CedMod.Addons.QuerySystem.Patches
{
    [HarmonyPatch(typeof(PermissionsHandler), nameof(PermissionsHandler.RefreshPermissions))]
    public static class RefreshPermissionsHandlerPatch
    {
        public static bool Prefix()
        {
            if (!WebSocketSystem.UseRa)
            {
                new Thread(() => { WebSocketSystem.ApplyRa(true); }).Start();
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

            CoroutineHandle = Timing.RunCoroutine(CedModMain.Singleton.QueryServerEvents.SyncStart(false));

            return true;
        }
    }
}