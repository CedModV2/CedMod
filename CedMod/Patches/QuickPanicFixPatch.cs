using HarmonyLib;
using PlayerStatsSystem;
using PluginAPI.Core;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.Health), MethodType.Getter)]
    public static class QuickPanicFixPatch
    {
        public static bool Prefix(Player __instance, ref float __result)
        {
            __result = __instance.GetStatModule<HealthStat>().CurValue;
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.Health), MethodType.Setter)]
    public static class QuickPanicFixPatch2
    {
        public static bool Prefix(Player __instance, float value)
        {
            __instance.GetStatModule<HealthStat>().CurValue = value;
            return false;
        }
    }
}