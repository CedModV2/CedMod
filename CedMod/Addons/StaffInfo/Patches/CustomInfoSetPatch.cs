using HarmonyLib;

namespace CedMod.Addons.StaffInfo.Patches
{
    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.Network_customPlayerInfoString), MethodType.Setter)]
    public static class CustomInfoSetPatch
    {
        public static void Postfix(NicknameSync __instance, string value)
        {
            foreach (var staff in CedModPlayer.GetPlayers())
            {
                if (!staff.RemoteAdminAccess && StaffInfoHandler.StaffData.ContainsKey(staff.UserId) && StaffInfoHandler.StaffData[staff.UserId].ContainsKey(__instance._hub.authManager.UserId))
                    continue;
                
                var player = CedModPlayer.Get(__instance._hub);
                if (StaffInfoHandler.StaffData[staff.UserId][player.UserId].Item1 == "")
                    return;
                
                player.SendFakeCustomInfo(staff, value + "\n" + StaffInfoHandler.StaffData[staff.UserId][player.UserId].Item1);
            }
        }
    }
}