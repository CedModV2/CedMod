using HarmonyLib;
using Respawning;

namespace CedMod.LightsPlugin
{
    [HarmonyPatch(typeof(RespawnEffectsController), nameof(RespawnEffectsController.PlayCassieAnnouncement))]
    public class CassiePatch
    {
        public static bool Prefix(string words, bool makeHold, bool makeNoise)
        {
            if (ServerEventHandler.BlackoutOn && CedModLightsPlugin.config.CassieMalfunction)
            {
                words = $"pitch_0.1 .g1 pitch_0.75 JAM_4_7 {words} pitch_0.1 .g1 .g3 pitch_1";
                foreach (RespawnEffectsController respawnEffectsController in RespawnEffectsController.AllControllers)
                {
                    if (respawnEffectsController != null)
                    {
                        respawnEffectsController.ServerPassCassie(words, makeHold, makeNoise);
                        break;
                    }
                }
                return false;
            }

            return true;
        }
    }
}