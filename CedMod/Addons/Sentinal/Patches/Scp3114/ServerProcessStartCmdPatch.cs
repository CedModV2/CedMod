using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Scp127;
using PlayerRoles;

namespace CedMod.Addons.Sentinal.Patches.Scp3114
{
    [HarmonyPatch(typeof(AutomaticActionModule), nameof(AutomaticActionModule.ServerProcessCmd))]
    public class ServerProcessStartCmdPatch_Automatic
    {
        public static bool Prefix(AutomaticActionModule __instance) => __instance.Firearm.Owner.GetRoleId() != RoleTypeId.Scp3114;
    }
    
    [HarmonyPatch(typeof(DisruptorActionModule), nameof(DisruptorActionModule.ServerProcessCmd))]
    public class ServerProcessStartCmdPatch_Disruptor
    {
        public static bool Prefix(DisruptorActionModule __instance) => __instance.Firearm.Owner.GetRoleId() != RoleTypeId.Scp3114;
    }
    
    [HarmonyPatch(typeof(PumpActionModule), nameof(PumpActionModule.ServerProcessCmd))]
    public class ServerProcessStartCmdPatch_Pump
    {
        public static bool Prefix(PumpActionModule __instance) => __instance.Firearm.Owner.GetRoleId() != RoleTypeId.Scp3114;
    }
    
    [HarmonyPatch(typeof(DoubleActionModule), nameof(DoubleActionModule.ServerProcessCmd))]
    public class ServerProcessStartCmdPatch_Double
    {
        public static bool Prefix(DoubleActionModule __instance) => __instance.Firearm.Owner.GetRoleId() != RoleTypeId.Scp3114;
    }
    
    [HarmonyPatch(typeof(Scp127ActionModule), nameof(Scp127ActionModule.ServerProcessCmd))]
    public class ServerProcessStartCmdPatch_Scp127
    {
        public static bool Prefix(Scp127ActionModule __instance) => __instance.Firearm.Owner.GetRoleId() != RoleTypeId.Scp3114;
    }
}
