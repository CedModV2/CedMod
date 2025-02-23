using System;
using System.Collections.Generic;
using CedMod.Addons.QuerySystem;
using HarmonyLib;
using LabApi.Features.Console;
using Mirror;
using UnityEngine;
using VoiceChat;
using VoiceChat.Codec;
using VoiceChat.Networking;
using Logger = LabApi.Features.Console.Logger;

namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
    public static class VoicePacketPacket
    {
        public static Dictionary<uint, List<(int, float, float)>> PacketsSent = new Dictionary<uint, List<(int, float, float)>>();
        public static Dictionary<uint, int> Tracker = new Dictionary<uint, int>();
        public static Dictionary<uint, OpusDecoder> OpusDecoders = new Dictionary<uint, OpusDecoder>();
        public static Dictionary<uint, float[]> Floats = new Dictionary<uint, float[]>();
        public static HashSet<uint> Radio = new HashSet<uint>();

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
                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => plr.SendHint("Muted: Awaiting CedMod Authentication", 2));
                    
                    Tracker.Add(conn.identity.netId, 0);
                }
                
                if (!OpusDecoders.ContainsKey(conn.identity.netId))
                    OpusDecoders.Add(conn.identity.netId, new OpusDecoder());
                
                if (!Floats.ContainsKey(conn.identity.netId)) 
                    Floats[conn.identity.netId] = new float[480];
                
                var len = OpusDecoders[conn.identity.netId].Decode(msg.Data, msg.DataLength,  Floats[conn.identity.netId]);
                float highest = 0;
                float lowest = 0;
                foreach (var f in  Floats[conn.identity.netId])
                {
                    if (f <= 0 && f <= lowest)
                        lowest = f;

                    if (f >= 0 && f >= highest)
                        highest = f;
                }

                PacketsSent[conn.identity.netId].Add((len, lowest, highest));
                
                if (BanSystem.Authenticating.Contains(msg.Speaker) || lowest <= -6 || highest >= 6 || len != 480)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            if (msg.Channel == VoiceChatChannel.Radio || msg.Channel == VoiceChatChannel.Intercom)
                Radio.Add(conn.identity.netId);
            return true;
        }
    }
}