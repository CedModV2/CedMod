using System.Collections.Generic;
using CedMod.Addons.QuerySystem.WS;
using HarmonyLib;
using Mirror;
using PlayerRoles.PlayableScps.Scp3114;

namespace CedMod.Addons.Sentinal.Patches.Scp3114
{
    [HarmonyPatch(typeof(Scp3114Disguise), nameof(Scp3114Disguise.ServerValidateBegin))]
    public class Scp3114AbilityCooldownPatch_Disguise
    {
        public static bool Prefix(Scp3114Disguise __instance)
        {
            if (__instance.Cooldown.IsReady)
                return true;

            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    { "SentinalType", "SCP3114CooldownSkip" }, 
                    { "UserId", __instance.Owner.authManager.UserId },
                }
            });
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.ProcessAttackRequest))]
    public class Scp3114AbilityCooldownPatch_Attack
    {
        public static bool Prefix(Scp3114Strangle __instance, NetworkReader reader, ref Scp3114Strangle.StrangleTarget? __result)
        {
            if (__instance.ClientCooldown.IsReady)
                return true;

            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    { "SentinalType", "SCP3114CooldownSkip" }, 
                    { "UserId", __instance.Owner.authManager.UserId },
                }
            });
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.ServerWriteRpc))]
    public class Scp3114AbilityCooldownPatch_ServerWriteRPC
    {
        public static bool Prefix(Scp3114Strangle __instance, NetworkWriter writer)
        {
            switch (__instance._rpcType)
            {
                case Scp3114Strangle.RpcType.TargetKilled:
                    __instance.ClientCooldown.Trigger((double) __instance._onKillCooldown);
                    break;
                case Scp3114Strangle.RpcType.AttackInterrupted:
                    __instance.ClientCooldown.Trigger((double) __instance._onInterruptedCooldown);
                    break;
                default:
                    __instance.ClientCooldown.Trigger((double) __instance._onReleaseCooldown);
                    break;
            }
            return true;
        }
    }
}
