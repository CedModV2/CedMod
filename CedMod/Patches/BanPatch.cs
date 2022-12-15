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
        public static bool Prefix(BanDetails details, BanHandler.BanType type)
        {
            try
            {
                Log.Info($"MainGame ban IssueBanpatch: banning user. {details.Id} {type} {details.OriginalName}");
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        lock (BanSystem.Banlock)
                        {
                            API.BanId(details.Id,  (long)(new DateTime(details.Expires) - new DateTime(details.IssuanceTime)).TotalSeconds, details.Issuer, details.Reason, false);
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