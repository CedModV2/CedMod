using CedMod.Addons.QuerySystem;
using HarmonyLib;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
    public class ReloadServerNamePatch
    {
        public static bool IncludeString = false;
        public static void Postfix()
        {
            string data = $"<color=#00000000><size=1>CedModVerification{Verification.ServerId}</size></color>";
            if (Verification.ServerId != 0 && !ServerConsole.ServerName.Contains(data))
            {
                ServerConsole.ServerName += data;
            }

            if (!IncludeString)
            {
                ServerConsole.ServerName = ServerConsole.ServerName.Replace(data, "");
            }
        }
    }
    
    [HarmonyPatch(typeof(PlayerList), nameof(PlayerList.RefreshTitle))]
    public class RefreshTitleSafePatch
    {
        public static bool Prefix()
        {
            string data = $"<color=#00000000><size=1>CedModVerification{Verification.ServerId}</size></color>";
            
            if (string.IsNullOrEmpty(PlayerList.Title.Value))
            {
                PlayerList.ServerName = ServerConsole.Singleton.RefreshServerNameSafe().Replace(data, "");
            }
            else
            {
                string result;
                if (!ServerConsole.Singleton.NameFormatter.TryProcessExpression(PlayerList.Title.Value, "player list title", out result))
                    ServerConsole.AddLog(result);
                else
                    PlayerList.ServerName = result;
            }

            return false;
        }
    }
    
    [HarmonyPatch(typeof(PlayerList), nameof(PlayerList.RefreshTitle))]
    public class ReloadTitlePatch
    {
        public static bool Prefix()
        {
            string data = $"<color=#00000000><size=1>CedModVerification{Verification.ServerId}</size></color>";
            
            PlayerList.ServerName = (string.IsNullOrEmpty(PlayerList.Title.Value) ? ServerConsole.Singleton.RefreshServerName() : ServerConsole.Singleton.NameFormatter.ProcessExpression(PlayerList.Title.Value)).Replace(data, "");
            
            return false;
        }
    }
}