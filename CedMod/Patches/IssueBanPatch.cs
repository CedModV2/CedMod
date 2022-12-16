using System;
using System.Threading.Tasks;
using HarmonyLib;
using PluginAPI.Core;
using UnityEngine;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(BanHandler), nameof(BanHandler.IssueBan), new Type[] {typeof(BanDetails), typeof(BanHandler.BanType)})]
    public static class IssueBanPatch
    {
        public static bool Prefix(BanDetails ban, BanHandler.BanType banType)
        {
            try
            {
                Log.Info($"MainGame ban IssueBanpatch: banning user. {ban.Id} {banType} {ban.OriginalName}");
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        lock (BanSystem.Banlock)
                        {
                            API.BanId(ban.Id,  (long)(new DateTime(ban.Expires) - new DateTime(ban.IssuanceTime)).TotalSeconds, ban.Issuer, ban.Reason, false);
                        }
                    }
                    catch (Exception ex)
                    {
                       Log.Error($"MainGame IssueBan patch failed {ex}");
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error($"MainGame IssueBan patch failed {ex}");
            }

            return false;
        }
    }
}