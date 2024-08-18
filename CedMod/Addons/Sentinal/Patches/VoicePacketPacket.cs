using System;
using System.Collections.Generic;
using System.Linq;
using CedMod.Addons.QuerySystem;
using HarmonyLib;
using Mirror;
using PluginAPI.Core;
using VoiceChat.Codec;
using VoiceChat.Networking;

namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
    public static class VoicePacketPacket
    {
        public static Dictionary<uint, List<(int, float, float)>> PacketsSent = new Dictionary<uint, List<(int, float, float)>>();
        public static Dictionary<uint, int> Tracker = new Dictionary<uint, int>();
        public static OpusDecoder OpusDecoder = new OpusDecoder();
        public static float[] Floats = new float[24000];

        public static bool Prefix(NetworkConnection conn, VoiceMessage msg)
        {
            try
            {
                if (msg.Speaker == null || conn.identity.netId != msg.Speaker.netId)
                    return false;
                
                var plr = CedModPlayer.Get(msg.Speaker);
                if (!PacketsSent.ContainsKey(conn.identity.netId))
                {
                   PacketsSent.Add(conn.identity.netId, new List<(int, float, float)>());
                }

                if (!Tracker.ContainsKey(conn.identity.netId))
                {
                    if (BanSystem.Authenticating.Contains(msg.Speaker)) //hint is removed by authenticator
                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => plr.ReceiveHint("Muted: Awaiting CedMod Authentication", 2));
                    
                    Tracker.Add(conn.identity.netId, 0);
                }
                
                Floats = new float[24000];
                var len = OpusDecoder.Decode(msg.Data, msg.DataLength, Floats);
                float highest = 0;
                float lowest = 0;
                foreach (var f in Floats)
                {
                    if (f <= 0 && f <= lowest)
                        lowest = f;

                    if (f >= 0 && f >= highest)
                        highest = f;
                }

                PacketsSent[conn.identity.netId].Add((len, lowest, highest));
                
                if (BanSystem.Authenticating.Contains(msg.Speaker) || lowest <= -2 || highest >= 2 || len != 480)
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