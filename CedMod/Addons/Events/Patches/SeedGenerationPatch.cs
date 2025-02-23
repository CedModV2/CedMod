using HarmonyLib;
using MapGeneration;

namespace CedMod.Addons.Events.Patches
{
    [HarmonyPatch(typeof(SeedSynchronizer), nameof(SeedSynchronizer.Awake))]
    public static class SeedGenerationPatch
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
        
        public static bool Prefix(SeedSynchronizer __instance)
        {
            if (NextSeed == -1)
                return true;

            SeedSynchronizer.Seed= NextSeed;
            return false;
        }
    }
}