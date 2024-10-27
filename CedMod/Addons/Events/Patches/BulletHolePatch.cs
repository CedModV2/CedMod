using CedMod.Addons.Events.Interfaces;
using Decals;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UnityEngine;
using VoiceChat;

namespace CedMod.Addons.Events.Patches
{
    [HarmonyPatch(typeof(ImpactEffectsModule), nameof(ImpactEffectsModule.ServerSendImpactDecal))]
    public static class BulletHolePatch
    {
        public static bool Prefix(ImpactEffectsModule __instance, RaycastHit hit, Vector3 origin, DecalPoolType decalType)
        {
            if (CedModMain.Singleton.Config.CedMod.PreventBulletHolesWhenMuted && __instance.Firearm.Owner != null && VoiceChatMutes.GetFlags(__instance.Firearm.Owner).HasFlag(VcMuteFlags.LocalRegular))
                return false;
            if (EventManager.CurrentEvent != null && EventManager.CurrentEvent is IBulletHoleBehaviour dissallowBulletHoles && !dissallowBulletHoles.CanPlaceBulletHole(__instance, origin, hit))
                return false;
            return true;
        }
    }
}