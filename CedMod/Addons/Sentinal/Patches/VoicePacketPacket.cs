using System;
using System.Collections.Generic;
using HarmonyLib;
using Mirror;
using PluginAPI.Core;
using VoiceChat.Networking;

namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
    public static class VoicePacketPacket
    {
        public static Dictionary<int, int> PacketsSent = new Dictionary<int, int>();
        
        public static void Postfix(NetworkConnection conn, VoiceMessage msg)
        {
            try
            {
                if (!PacketsSent.ContainsKey(conn.connectionId))
                    PacketsSent.Add(conn.connectionId, 0);

                PacketsSent[conn.connectionId]++;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }
    }
}