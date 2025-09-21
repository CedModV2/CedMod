using HarmonyLib;

namespace CedMod.Addons.QuerySystem.Patches
{
    [HarmonyPatch(typeof(WhiteList), nameof(WhiteList.IsOnWhitelist))]
    public static class WhitelistPatch
    {
        public static bool Prefix(string userId, bool __result)
        {
            if (QuerySystem.UseWhitelist && QuerySystem.Whitelist.Contains(userId))
            {
                __result = true;
                return false;
            }
            
            return true;
        }
    }
}