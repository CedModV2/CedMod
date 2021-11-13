using System.Collections.Generic;
using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UnityEngine;

namespace CedMod.EventManager.Patches
{
    [HarmonyPatch(typeof(StandardHitregBase), nameof(StandardHitregBase.PlaceBullethole))]
    public static class BulletHolePatch
    {
        public static bool Prefix(StandardHitregBase __instance, Ray ray, RaycastHit hit)
        {
            if (EventManager.Singleton.currentEvent != null && !EventManager.Singleton.currentEvent.BulletHolesAllowed)
                return false;
            return true;
        }
    }
}