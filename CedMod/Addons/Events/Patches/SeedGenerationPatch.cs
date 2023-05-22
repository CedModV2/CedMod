using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using GameCore;
using HarmonyLib;
using MapGeneration;
using UnityEngine;

namespace CedMod.Addons.Events.Patches
{
    [HarmonyPatch(typeof(SeedSynchronizer), nameof(SeedSynchronizer.Start))]
    public class SeedGenerationPatch
    {
        private static int _nextSeed = -1;

        public static int NextSeed
        {
            private get => _nextSeed;
            set
            {
                if (value < 0)
                    _nextSeed = -1;
                else
                    _nextSeed = value;
            }
        }

        public static int GetSeed()
        {
            if (NextSeed == -1)
            {
                var seed = ConfigFile.ServerConfig.GetInt("map_seed", -1);
                return seed < 1 ? Random.Range(1, int.MaxValue) : seed;
            }
            
            return NextSeed;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var newInstructions = instructions.ToList();
            var offset = newInstructions.FindIndex(x => x.opcode == OpCodes.Bge_S) + 1;

            newInstructions.RemoveRange(offset, 3);
            newInstructions.Insert(offset,
                new CodeInstruction(OpCodes.Call, 
                    AccessTools.Method(typeof(SeedGenerationPatch), nameof(GetSeed))));

            foreach (var code in newInstructions)
                yield return code;
        }
    }
}