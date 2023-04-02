using System;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using PluginAPI.Core;
using UnityEngine;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(BanHandler), nameof(BanHandler.IssueBan), new Type[] {typeof(BanDetails), typeof(BanHandler.BanType), typeof(bool)})]
    public static class IssueBanPatch
    {
        public static bool Prefix(BanDetails ban, BanHandler.BanType banType, bool forced)
        {
            try
            {
                Log.Info($"MainGame ban IssueBanpatch: banning user. {ban.Id} {banType} {ban.OriginalName}");
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
                       Log.Error($"MainGame IssueBan patch failed {ex}");
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                Log.Error($"MainGame IssueBan patch failed {ex}");
            }

            return false;
        }
    }
}