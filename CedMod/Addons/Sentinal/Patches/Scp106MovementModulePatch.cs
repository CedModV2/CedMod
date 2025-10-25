using AdminToys;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp106;
using UnityEngine;

namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(Scp106MovementModule), nameof(Scp106MovementModule.GetSlowdownFromCollider))]
    public static class Scp106MovementModulePatch
    {
        public static bool Prefix(Collider col, out bool isPassable, ref float __result)
        {
            if (col.GetComponent<PrimitiveObjectToy>() != null)
            {
                __result = 0;
                isPassable = false;
                return false;
            }

            isPassable = false;
            return true;
        }
    }
}