using System;
using CedMod.INIT;
using HarmonyLib;

namespace CedMod.PluginInterface.patches
{
    [HarmonyPatch(typeof(QueryServer), nameof(QueryServer.CheckClients))]
    public static class QueryCheckClientPatch
    {
        static bool Prefix(QueryServer __instance)
        {
            while (!__instance._serverStop)
            {
                for (int i = __instance.Users.Count - 1; i >= 0; i--)
                {
                    if (!__instance.Users[i].IsConnected())
                    {
                        global::ServerConsole.AddLog("Query user connected from " + __instance.Users[i].Ip + " timed out.",
                            ConsoleColor.Gray);
                        try
                        {
                            __instance.Users[i].CloseConn(false);
                            __instance.Users.RemoveAt(i);
                        }
                        catch (Exception ex)
                        {
                            Initializer.Logger.Error("CMQuery", ex.ToString());
                        }
                    }
                }
            }
            return false;
        }
    }
}