using System;
using HarmonyLib;
using UnityEngine;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), new Type[] {typeof(GameObject), typeof(int), typeof(string), typeof(string),  typeof(bool)})]
    public class BanPatch
    {
        public bool Prefix(GameObject user, int duration, string reason, string issuer, bool isGlobalBan)
        {
            API.Ban(user, duration, issuer,reason, true);
            return false;
        }
    }
}