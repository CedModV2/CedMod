using System;
using System.Threading.Tasks;
using Exiled.API.Features;
using HarmonyLib;
using UnityEngine;

namespace CedMod.Patches
{
    /// <summary>
    /// Patches <see cref="BanPlayer"/>.
    /// </summary>
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), new Type[] {typeof(GameObject), typeof(long), typeof(string), typeof(string),  typeof(bool)})]
    public static class BanPatch
    {
        public static bool Prefix(GameObject user, long duration, string reason, string issuer, bool isGlobalBan)
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
                            API.Ban(Player.Get(user), duration, issuer, reason);
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