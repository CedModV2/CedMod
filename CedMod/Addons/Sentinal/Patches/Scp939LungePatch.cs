using System.Collections.Generic;
using CedMod.Addons.QuerySystem.WS;
using HarmonyLib;
using Mirror;
using PlayerRoles.PlayableScps.Scp939;

namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(Scp939LungeAbility), nameof(Scp939LungeAbility.ServerProcessCmd))]
    public class Scp939LungePatch
    {
        public static bool Prefix(NetworkReader reader, Scp939LungeAbility __instance)
        {
            if (__instance.State == Scp939LungeState.Triggered)
                return true;
            
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    {"SentinalType", "SCP939LungeExploit"},
                    {"UserId", __instance.Owner.authManager.UserId},
                    {"State", __instance.State.ToString()},
                    {"Pos", __instance.Owner.authManager.ToString()}
                }
            });
            return false;
        }
    }
}