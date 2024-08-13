using System;
using System.Collections.Generic;
using CedMod.Addons.QuerySystem;
using HarmonyLib;
using Mirror;
using PluginAPI.Core;
using VoiceChat.Networking;

namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
    public static class VoicePacketPacket
    {
        public static Dictionary<uint, List<int>> PacketsSent = new Dictionary<uint, List<int>>();
        public static Dictionary<uint, int> Tracker = new Dictionary<uint, int>();

        public static bool Prefix(NetworkConnection conn, VoiceMessage msg)
        {
            try
            {
                var plr = CedModPlayer.Get(msg.Speaker);
                if (!PacketsSent.ContainsKey(conn.identity.netId))
                {
                   PacketsSent.Add(conn.identity.netId, new List<int>());
                }

                if (!Tracker.ContainsKey(conn.identity.netId))
                {
                    if (BanSystem.Authenticating.Contains(msg.Speaker)) //hint is removed by authenticator
                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => plr.ReceiveHint("Muted: Awaiting CedMod Authentication", 2));
                    
                    Tracker.Add(conn.identity.netId, 0);
                }

                PacketsSent[conn.identity.netId].Add(msg.Data.Length);
                
                if (BanSystem.Authenticating.Contains(msg.Speaker))
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