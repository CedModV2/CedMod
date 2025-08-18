using System.Collections.Generic;
using System.Reflection.Emit;
using AdminToys;
using HarmonyLib;
using NorthwoodLib.Pools;
using UnityEngine;

namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(PrimitiveObjectToy), nameof(PrimitiveObjectToy.NetworkPrimitiveFlags), MethodType.Setter)]
    public static class PrimitiveFlagsPatchTranspiler 
    {
        public static int Glass = LayerMask.NameToLayer("Glass");
        public static int Default = LayerMask.NameToLayer("Default");
        
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions,
            ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(codeInstructions);

            newInstructions.InsertRange(0, new[]
            {
                // PrimitiveColorPatchTranspiler.SetColor(this, value);
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                CodeInstruction.Call(typeof(PrimitiveColorPatchTranspiler), nameof(SetPrimitiveFlags), new []{ typeof(PrimitiveObjectToy), typeof(PrimitiveFlags) }), 
            });
            
            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        public static void SetPrimitiveFlags(PrimitiveObjectToy instance, PrimitiveFlags value)
        {
            if (CedModMain.Singleton == null || CedModMain.Singleton.Config == null || !CedModMain.Singleton.Config.CedMod.PrimitiveTransparancyDetection || CedModMain.Singleton.Config.CedMod.DisableFakeSyncing)
                return;
            
            if (!value.HasFlag(PrimitiveFlags.Visible))
            {
                instance.gameObject.layer = Glass;
                return;
            }
            
            if (instance.NetworkMaterialColor.a < 1 && instance.gameObject.layer != Glass)
            {
                instance.gameObject.layer = Glass;
            }
            else if (instance.NetworkMaterialColor.a >= 1 && instance.gameObject.layer != Default)
            {
                instance.gameObject.layer = Default;
            }
        }
    }
}