using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerStatsSystem;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace CedMod.Addons.QuerySystem.Patches.DummySpecific
{
    [HarmonyPatch(typeof(SyncedStatBase), nameof(SyncedStatBase.Update))]
    internal static class StatBaseUpdate
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex(instruction => instruction.opcode == OpCodes.Br);
            Label continueLabel = (Label)newInstructions[index].operand;

            const int offset = 1;
            index = newInstructions.FindIndex(instruction => instruction.opcode == OpCodes.Stloc_1) + offset;

            newInstructions.InsertRange(index, new[]
            {
                new CodeInstruction(OpCodes.Ldloca_S, 1),
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(KeyValuePair<GameObject, ReferenceHub>), nameof(KeyValuePair<GameObject, ReferenceHub>.Key))),
                new CodeInstruction(OpCodes.Call, Method(typeof(HandleQueryplayer), nameof(HandleQueryplayer.IsDummy), new[] { typeof(GameObject) })),
                new CodeInstruction(OpCodes.Brtrue_S, continueLabel),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}