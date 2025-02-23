using CedMod.Addons.QuerySystem;
using HarmonyLib;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
    public class ReloadServerNamePatch
    {
        public static void Postfix()
        {
            if (Verification.ServerId != 0 && !ServerConsole.ServerName.Contains($"<color=#00000000><size=1>CedModVerification{Verification.ServerId}</size></color>"))
            {
                ServerConsole.ServerName += $"<color=#00000000><size=1>CedModVerification{Verification.ServerId}</size></color>";
            }
        }
    }
}