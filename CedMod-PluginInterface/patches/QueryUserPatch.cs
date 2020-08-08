using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using HarmonyLib;
using Org.BouncyCastle.Asn1.X509.Qualified;

namespace CedMod.PluginInterface.patches
{
    [HarmonyPatch(typeof(QueryUser), MethodType.Constructor, new Type[] { typeof(QueryServer), typeof(TcpClient), typeof(string)})]
    public static class QueryUserPatch
    {
        static void Postfix(QueryUser __instance, QueryServer s, TcpClient c, string ip)
        {
            __instance.Send("CedMod Query protocol based of the SCP:SL query protocol");
            __instance.Send("Please authenticate first in order to send commands <i>authenticate userid username</i>");
        }
    }
}