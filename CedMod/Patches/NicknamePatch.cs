using CentralAuth;
using HarmonyLib;
using PluginAPI.Core;
using PluginAPI.Events;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.SetNick))]
    public static class NicknamePatch
    {
        //offline mode patch
        public static void Postfix(NicknameSync __instance, string nick)
        {
            if (PlayerAuthenticationManager.OnlineMode)
                return;
            
            if (!Player.PlayersUserIds.ContainsKey(__instance._hub.authManager.UserId))
                Player.PlayersUserIds.Add(__instance._hub.authManager.UserId, __instance._hub);

            EventManager.ExecuteEvent(new PlayerJoinedEvent(__instance._hub));
        }
    }
}