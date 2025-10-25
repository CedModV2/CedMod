using System;
using System.Collections.Generic;
using HarmonyLib;
using LabApi.Features.Console;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp939;
using RelativePositioning;
using Utils.Networking;

namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(Scp939LungeAbility), nameof(Scp939LungeAbility.ServerProcessCmd))]
    public class Scp939LungePatch
    {
        public static Dictionary<uint, Dictionary<ulong, List<(string position, string state, int ping)>>> LungeTime = new Dictionary<uint, Dictionary<ulong, List<(string position, string state, int ping)>>>();
        
        public static bool Prefix(NetworkReader reader, Scp939LungeAbility __instance)
        {
            try
            {
                if (!LungeTime.ContainsKey(__instance.Owner.netId))
                {
                    LungeTime[__instance.Owner.netId] = new Dictionary<ulong, List<(string position, string state, int ping)>>();
                }

                if (!LungeTime[__instance.Owner.netId].ContainsKey(SentinalBehaviour.UFrames))
                    LungeTime[__instance.Owner.netId][SentinalBehaviour.UFrames] = new List<(string position, string state, int ping)>();
                
                LungeTime[__instance.Owner.netId][SentinalBehaviour.UFrames].Add((__instance.Owner.GetPosition().ToString(), __instance.State.ToString(), LiteNetLib4MirrorServer.Peers[__instance.Owner.connectionToClient.connectionId].Ping * 2));
                if (__instance.State == Scp939LungeState.Triggered)
                {
                    return true;
                }
                
                reader.ReadRelativePosition();
                reader.ReadReferenceHub();
                reader.ReadRelativePosition();
                return false;
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to invoke 939 exploit patch: {e.ToString()}");
            }

            return true;
        }
    }
}