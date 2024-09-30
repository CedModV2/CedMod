using System;
using System.Threading;
using HarmonyLib;
using LabApi.Features.Console;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(BanHandler), nameof(BanHandler.IssueBan), new Type[] {typeof(BanDetails), typeof(BanHandler.BanType), typeof(bool)})]
    public static class IssueBanPatch
    {
        public static bool Prefix(BanDetails ban, BanHandler.BanType banType, bool forced)
        {
            try
            {
                Logger.Info($"MainGame ban IssueBanpatch: banning user. {ban.Id} {banType} {ban.OriginalName}");
                new Thread(() =>
                {
                    try
                    {
                        lock (BanSystem.Banlock)
                        {
                            API.BanId(ban.Id,  (long)(new DateTime(ban.Expires) - new DateTime(ban.IssuanceTime)).TotalSeconds, ban.Issuer, ban.Reason, false).Wait();
                        }
                    }
                    catch (Exception ex)
                    {
                       Logger.Error($"MainGame IssueBan patch failed {ex}");
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                Logger.Error($"MainGame IssueBan patch failed {ex}");
            }

            return false;
        }
    }
}