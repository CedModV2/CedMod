using System;
using System.Threading.Tasks;
using CedMod.INIT;
using HarmonyLib;
using UnityEngine;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), new Type[] {typeof(GameObject), typeof(int), typeof(string), typeof(string),  typeof(bool)})]
    public static class BanPatch
    {
        public static bool Prefix(GameObject user, int duration, string reason, string issuer, bool isGlobalBan)
        {
            try
            {
                Initializer.Logger.Info("BANSYSTEM", $"MainGame ban patch: banning user. {duration} {reason} {issuer}");
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        lock (BanSystem.banlock)
                        {
                            API.Ban(user, duration, issuer, reason, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Initializer.Logger.Error("BANSYSTEM", $"MainGame ban patch failed {ex.ToString()}");
                    }
                });
            }
            catch (Exception ex)
            {
                Initializer.Logger.Error("BANSYSTEM", $"MainGame ban patch failed {ex.ToString()}");
            }

            return false;
        }
    }
}