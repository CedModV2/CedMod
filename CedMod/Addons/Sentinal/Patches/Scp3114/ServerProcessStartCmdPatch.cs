using System.Collections.Generic;
using CedMod.Addons.QuerySystem.WS;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Scp127;
using InventorySystem.Items.Jailbird;
using InventorySystem.Items.MicroHID.Modules;
using InventorySystem.Items.Scp1509;
using Mirror.LiteNetLib4Mirror;
using Newtonsoft.Json;
using PlayerRoles;

namespace CedMod.Addons.Sentinal.Patches.Scp3114
{
    [HarmonyPatch(typeof(AutomaticActionModule), nameof(AutomaticActionModule.ServerProcessCmd))]
    public class ServerProcessStartCmdPatch_Automatic
    {
        public static bool Prefix(AutomaticActionModule __instance)
        {
            if (__instance.Firearm.Owner.GetRoleId() != RoleTypeId.Scp3114 || __instance.Firearm.Owner == null || __instance.Firearm.Owner.authManager.UserId == null)
                return true;

            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    { "SentinalType", "SCP3114UsingGuns" }, 
                    { "UserId", __instance.Firearm.Owner.authManager.UserId },
                    { "Firearm", __instance.Firearm.ItemTypeId.ToString()},
                }
            });
            return false;
        }
    }
    
    [HarmonyPatch(typeof(DisruptorActionModule), nameof(DisruptorActionModule.ServerProcessCmd))]
    public class ServerProcessStartCmdPatch_Disruptor
    {
        public static bool Prefix(DisruptorActionModule __instance)
        {
            if (__instance.Firearm.Owner.GetRoleId() != RoleTypeId.Scp3114 || __instance.Firearm.Owner == null || __instance.Firearm.Owner.authManager.UserId == null)
                return true;

            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    { "SentinalType", "SCP3114UsingGuns" }, 
                    { "UserId", __instance.Firearm.Owner.authManager.UserId },
                    { "Firearm", __instance.Firearm.ItemTypeId.ToString()},
                }
            });
            return false;
        }
    }
    
    [HarmonyPatch(typeof(PumpActionModule), nameof(PumpActionModule.ServerProcessCmd))]
    public class ServerProcessStartCmdPatch_Pump
    {
        public static bool Prefix(PumpActionModule __instance)
        {
            if (__instance.Firearm.Owner.GetRoleId() != RoleTypeId.Scp3114 || __instance.Firearm.Owner == null || __instance.Firearm.Owner.authManager.UserId == null)
                return true;

            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    { "SentinalType", "SCP3114UsingGuns" }, 
                    { "UserId", __instance.Firearm.Owner.authManager.UserId },
                    { "Firearm", __instance.Firearm.ItemTypeId.ToString()},
                }
            });
            return false;
        }
    }
    
    [HarmonyPatch(typeof(DoubleActionModule), nameof(DoubleActionModule.ServerProcessCmd))]
    public class ServerProcessStartCmdPatch_Double
    {
        public static bool Prefix(DoubleActionModule __instance)
        {
            if (__instance.Firearm.Owner.GetRoleId() != RoleTypeId.Scp3114 || __instance.Firearm.Owner == null || __instance.Firearm.Owner.authManager.UserId == null)
                return true;

            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    { "SentinalType", "SCP3114UsingGuns" }, 
                    { "UserId", __instance.Firearm.Owner.authManager.UserId },
                    { "Firearm", __instance.Firearm.ItemTypeId.ToString()},
                }
            });
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Scp127ActionModule), nameof(Scp127ActionModule.ServerProcessCmd))]
    public class ServerProcessStartCmdPatch_Scp127
    {
        public static bool Prefix(Scp127ActionModule __instance)
        {
            if (__instance.Firearm.Owner.GetRoleId() != RoleTypeId.Scp3114 || __instance.Firearm.Owner == null || __instance.Firearm.Owner.authManager.UserId == null)
                return true;

            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    { "SentinalType", "SCP3114UsingGuns" }, 
                    { "UserId", __instance.Firearm.Owner.authManager.UserId },
                    { "Firearm", __instance.Firearm.ItemTypeId.ToString()},
                }
            });
            return false;
        }
    }
    
    [HarmonyPatch(typeof(BacktrackerModule), nameof(BacktrackerModule.ServerProcessCmd))]
    public class ServerProcessStartCmdPatch_Micro
    {
        public static bool Prefix(Scp127ActionModule __instance)
        {
            if (__instance.Firearm.Owner.GetRoleId() != RoleTypeId.Scp3114 || __instance.Firearm.Owner == null || __instance.Firearm.Owner.authManager.UserId == null)
                return true;

            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    { "SentinalType", "SCP3114UsingGuns" }, 
                    { "UserId", __instance.Firearm.Owner.authManager.UserId },
                    { "Firearm", __instance.Firearm.ItemTypeId.ToString()},
                }
            });
            return false;
        }
    }
    
    [HarmonyPatch(typeof(JailbirdItem), nameof(JailbirdItem.ServerProcessCmd))]
    public class ServerProcessStartCmdPatch_Jailbird
    {
        public static bool Prefix(Scp127ActionModule __instance)
        {
            if (__instance.Firearm.Owner.GetRoleId() != RoleTypeId.Scp3114 || __instance.Firearm.Owner == null || __instance.Firearm.Owner.authManager.UserId == null)
                return true;

            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    { "SentinalType", "SCP3114UsingGuns" }, 
                    { "UserId", __instance.Firearm.Owner.authManager.UserId },
                    { "Firearm", __instance.Firearm.ItemTypeId.ToString()},
                }
            });
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Scp1509Item), nameof(Scp1509Item.ServerProcessCmd))]
    public class ServerProcessStartCmdPatch_Scp1509Item
    {
        public static bool Prefix(Scp127ActionModule __instance)
        {
            if (__instance.Firearm.Owner.GetRoleId() != RoleTypeId.Scp3114 || __instance.Firearm.Owner == null || __instance.Firearm.Owner.authManager.UserId == null)
                return true;

            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    { "SentinalType", "SCP3114UsingGuns" }, 
                    { "UserId", __instance.Firearm.Owner.authManager.UserId },
                    { "Firearm", __instance.Firearm.ItemTypeId.ToString()},
                }
            });
            return false;
        }
    }
    
}
