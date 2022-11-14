using System;
using System.Threading.Tasks;
using HarmonyLib;
using PluginAPI.Core;
using UnityEngine;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), new Type[] {typeof(ReferenceHub), typeof(ReferenceHub), typeof(string), typeof(long)})]
    public static class BanPatch
    {
        public static bool Prefix(ReferenceHub target, ReferenceHub issuer, string reason, long duration)
        {
            try
            {
                Log.Info($"MainGame ban patch: banning user. {duration} {reason} {issuer}");
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        lock (BanSystem.Banlock)
                        {
                            API.Ban(Player.Get<CedModPlayer>(target), duration, issuer.LoggedNameFromRefHub(), reason);
                        }
                    }
                    catch (Exception ex)
                    {
                       Log.Error($"MainGame ban patch failed {ex}");
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error($"MainGame ban patch failed {ex}");
            }

            return false;
        }
    }
}