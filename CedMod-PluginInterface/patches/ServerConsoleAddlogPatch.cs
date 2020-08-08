using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using HarmonyLib;

namespace CedMod.PluginInterface.patches
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.AddLog))]
    public static class ServerConsoleAddlogPatch
    {
        static void Postfix(string q, ConsoleColor color = ConsoleColor.Gray )
        {
            try
            {

                // foreach (QueryUser usr in CustomNetworkManager._queryserver.Users)
                // {
                //     if (QueryUserRecievePatch.consoleusers.Contains(usr))
                //         usr.Send(q);
                // }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}