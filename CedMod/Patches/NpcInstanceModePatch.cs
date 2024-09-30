using CedMod.Addons.Audio;
using CentralAuth;
using HarmonyLib;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(PlayerAuthenticationManager), "InstanceMode", MethodType.Setter)]
    public static class NpcInstanceModePatch
    {
        public static bool Prefix(PlayerAuthenticationManager __instance, ClientInstanceMode value)
        {
            if (AudioCommand.FakeConnectionsIds.ContainsValue(__instance._hub))
            {
                if (value != ClientInstanceMode.Unverified && value != ClientInstanceMode.Host && value != ClientInstanceMode.DedicatedServer)
                    return false;
            }

            return true;
        }
    }
}