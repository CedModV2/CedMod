using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerStatsSystem;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace CedMod.Addons.QuerySystem.Patches.DummySpecific
{
    [HarmonyPatch(typeof(PlayerPositionManager), nameof(PlayerPositionManager.TransmitData))]
    internal static class TransmitPositionData
    {
        private static List<GameObject> GetPlayers => PlayerManager.players.Where(gameObject => !gameObject.IsDummy()).ToList();

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex(instruction => instruction.opcode == OpCodes.Ldsfld);

            newInstructions.RemoveAt(index);
            newInstructions.Insert(index, new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(TransmitPositionData), nameof(GetPlayers))));

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}