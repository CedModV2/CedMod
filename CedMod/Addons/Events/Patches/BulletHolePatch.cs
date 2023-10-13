using CedMod.Addons.Events.Interfaces;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UnityEngine;
using VoiceChat;

namespace CedMod.Addons.Events.Patches
{
    [HarmonyPatch(typeof(StandardHitregBase), nameof(StandardHitregBase.PlaceBulletholeDecal))]
    public static class BulletHolePatch
    {
        public static bool Prefix(StandardHitregBase __instance, Ray ray, RaycastHit hit)
        {
            if (CedModMain.Singleton.Config.CedMod.PreventBulletHolesWhenMuted && VoiceChatMutes.GetFlags(__instance.Hub).HasFlag(VcMuteFlags.LocalRegular))
                return false;
            if (EventManager.CurrentEvent != null && EventManager.CurrentEvent is IBulletHoleBehaviour dissallowBulletHoles && !dissallowBulletHoles.CanPlaceBulletHole(__instance, ray, hit))
                return false;
            return true;
        }
    }
}