using CedMod.Addons.QuerySystem;
using HarmonyLib;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
    public class ReloadServerNamePatch
    {
        public static void Postfix()
        {
            if (Verification.ServerId != 0 && !ServerConsole._serverName.Contains($"<color=#00000000><size=1>CedModVerification{Verification.ServerId}</size></color>"))
            {
                ServerConsole._serverName += $"<color=#00000000><size=1>CedModVerification{Verification.ServerId}</size></color>";
            }
        }
    }
}