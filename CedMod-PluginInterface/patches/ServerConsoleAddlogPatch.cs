using System;
using System.Collections.Generic;
using HarmonyLib;

namespace CedMod.PluginInterface.patches
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.AddLog))]
    public static class ServerConsoleAddlogPatch
    {
        static void Postfix(string q, ConsoleColor color = ConsoleColor.Gray )
        {
            if (CustomNetworkManager._queryserver == null)
                return;
            if (CustomNetworkManager._queryserver.Users == null)
                return;
            if (!CustomNetworkManager._queryserver._thr.IsAlive)
                return;
            try
            {
                foreach (QueryUser usr in CustomNetworkManager._queryserver.Users)
                {
                    if (PermissionsHandler.IsPermitted(usr.Permissions, PlayerPermissions.ServerConsoleCommands))
                        usr.Send(q);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); //silently log the exception
            }
        }
    }
}