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
        public static Dictionary<uint, int> Tracker = new Dictionary<uint, int>();

        public static bool Prefix(NetworkConnection conn, VoiceMessage msg)
        {
            try
            {
                var plr = CedModPlayer.Get(msg.Speaker);
                if (!PacketsSent.ContainsKey(conn.identity.netId))
                {
                   PacketsSent.Add(conn.identity.netId, 0);
                }

                if (!Tracker.ContainsKey(conn.identity.netId))
                {
                    if (!plr.CedModAuthenticated) //hint is removed by authenticator
                        plr.ReceiveHint("Muted: Awaiting CedMod Authentication", 2);
                    
                    Tracker.Add(conn.identity.netId, 0);
                }

                PacketsSent[conn.identity.netId]++;
                
                if (!plr.CedModAuthenticated)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            return true;
        }
    }
}