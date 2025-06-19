using AdminToys;
using CedMod.Addons.Audio;
using CentralAuth;
using CommandSystem.Commands.RemoteAdmin;
using CustomPlayerEffects;
using HarmonyLib;
using Interactables;
using PlayerRoles.PlayableScps.Scp106;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(PrimitiveObjectToy), "NetworkMaterialColor", MethodType.Setter)]
    public static class PrimitiveColorPatch
    {
        public static int Glass = LayerMask.NameToLayer("Glass");
        public static int Default = LayerMask.NameToLayer("Default");
        
        public static bool Prefix(PrimitiveObjectToy __instance, Color value)
        {
            if (CedModMain.Singleton == null || CedModMain.Singleton.Config == null || !CedModMain.Singleton.Config.CedMod.PrimitiveTransparancyDetection || CedModMain.Singleton.Config.CedMod.DisableFakeSyncing)
                return true;
            
            if (!__instance.NetworkPrimitiveFlags.HasFlag(PrimitiveFlags.Visible))
            {
                __instance.gameObject.layer = Glass;
                return true;
            }
            
            if (value.a < 1 && __instance.gameObject.layer != Glass)
            {
                __instance.gameObject.layer = Glass;
            }
            else if (value.a >= 1 && __instance.gameObject.layer != Default)
            {
                __instance.gameObject.layer = Default;
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(PrimitiveObjectToy), "NetworkPrimitiveFlags", MethodType.Setter)]
    public static class PrimitiveFlagsPatch
    {
        public static int Glass = LayerMask.NameToLayer("Glass");
        public static int Default = LayerMask.NameToLayer("Default");
        
        public static bool Prefix(PrimitiveObjectToy __instance, PrimitiveFlags value)
        {
            if (CedModMain.Singleton == null || CedModMain.Singleton.Config == null || !CedModMain.Singleton.Config.CedMod.PrimitiveTransparancyDetection || CedModMain.Singleton.Config.CedMod.DisableFakeSyncing)
                return true;
            
            if (!value.HasFlag(PrimitiveFlags.Visible))
            {
                __instance.gameObject.layer = Glass;
                return true;
            }
            
            if (__instance.NetworkMaterialColor.a < 1 && __instance.gameObject.layer != Glass)
            {
                __instance.gameObject.layer = Glass;
            }
            else if (__instance.NetworkMaterialColor.a >= 1 && __instance.gameObject.layer != Default)
            {
                __instance.gameObject.layer = Default;
            }
            return true;
        }
    }
    
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