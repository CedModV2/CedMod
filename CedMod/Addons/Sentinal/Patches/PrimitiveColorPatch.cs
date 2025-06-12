using AdminToys;
using CedMod.Addons.Audio;
using CentralAuth;
using CommandSystem.Commands.RemoteAdmin;
using HarmonyLib;
using Interactables;
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
            if (!CedModMain.Singleton.Config.CedMod.PrimitiveTransparancyDetection)
                return true;
            
            Logger.Info("patch col");
            if (!__instance.NetworkPrimitiveFlags.HasFlag(PrimitiveFlags.Visible))
            {
                Logger.Info($"Setting to glass {Glass}");
                __instance.gameObject.layer = Glass;
                return true;
            }
            
            if (value.a < 1 && __instance.gameObject.layer != Glass)
            {
                Logger.Info($"Setting to glass {Glass}");
                __instance.gameObject.layer = Glass;
            }
            else if (value.a >= 1 && __instance.gameObject.layer != Default)
            {
                Logger.Info($"Setting to default {Default}");
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
            if (CedModMain.Singleton == null || CedModMain.Singleton.Config == null || !CedModMain.Singleton.Config.CedMod.PrimitiveTransparancyDetection)
                return true;
            
            Logger.Info("patch type");
            if (!value.HasFlag(PrimitiveFlags.Visible))
            {
                Logger.Info($"Setting to glass {Glass}");
                __instance.gameObject.layer = Glass;
                return true;
            }
            
            if (__instance.NetworkMaterialColor.a < 1 && __instance.gameObject.layer != Glass)
            {
                Logger.Info($"Setting to glass {Glass}");
                __instance.gameObject.layer = Glass;
            }
            else if (__instance.NetworkMaterialColor.a >= 1 && __instance.gameObject.layer != Default)
            {
                Logger.Info($"Setting to default {Default}");
                __instance.gameObject.layer = Default;
            }
            return true;
        }
    }
}