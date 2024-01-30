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
        public static Dictionary<uint, int> PacketsSent = new Dictionary<uint, int>();
        
        public static void Postfix(NetworkConnection conn, VoiceMessage msg)
        {
            try
            {
                if (!PacketsSent.ContainsKey(conn.identity.netId))
                    PacketsSent.Add(conn.identity.netId, 0);

                PacketsSent[conn.identity.netId]++;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }
    }
}