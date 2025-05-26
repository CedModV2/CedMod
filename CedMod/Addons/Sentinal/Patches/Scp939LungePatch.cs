using System.Collections.Generic;
using System.Diagnostics;
using CedMod.Addons.QuerySystem.WS;
using HarmonyLib;
using Mirror;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp939;

namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(Scp939LungeAbility), nameof(Scp939LungeAbility.ServerProcessCmd))]
    public class Scp939LungePatch
    {
        public static Dictionary<uint, Stopwatch> LungeTime = new Dictionary<uint, Stopwatch>();
        
        public static bool Prefix(NetworkReader reader, Scp939LungeAbility __instance)
        {
            if (__instance.State == Scp939LungeState.Triggered)
                return true;
            
            if (!LungeTime.ContainsKey(__instance.Owner.netId) || LungeTime[__instance.Owner.netId].Elapsed.TotalSeconds >= 2)
                LungeTime[__instance.Owner.netId] = Stopwatch.StartNew();
            else
                return false;
            
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    {"SentinalType", "SCP939LungeExploit"},
                    {"UserId", __instance.Owner.authManager.UserId},
                    {"State", __instance.State.ToString()},
                    {"Pos", __instance.Owner.GetPosition().ToString()}
                }
            });
            return false;
        }
    }
}