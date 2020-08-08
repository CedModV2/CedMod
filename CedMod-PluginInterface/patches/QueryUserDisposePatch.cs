using System.Net.Sockets;
using HarmonyLib;

namespace CedMod.PluginInterface.patches
{
    [HarmonyPatch(typeof(QueryUser), nameof(QueryUser.Dispose))]
    public static class QueryUserDisposePatch
    {
        static bool Prefix(QueryUser __instance)
        {
            NetworkStream s = __instance._s;
            if (s != null)
            {
                s.Dispose();
            }
            UserPrint print = new UserPrint(__instance, CedModPluginInterface.autheduers[__instance]);
            if (global::ServerConsole.ConsoleOutputs.Contains(print))
            {
                global::ServerConsole.ConsoleOutputs.Remove(print);
            }

            return false;
        }
    }
}