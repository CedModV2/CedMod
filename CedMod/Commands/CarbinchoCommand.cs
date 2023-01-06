using System;
using System.Collections.Generic;
using CommandSystem;
using HarmonyLib;
#if !EXILED
using NWAPIPermissionSystem;
#else
using Exiled.Permissions.Extensions;
#endif
using Object = UnityEngine.Object;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class CarpinchoCommand : ICommand
    {
        public string Command { get; } = "carpincho";

        public string[] Aliases { get; } = new string[]
        {
        };

        public static Dictionary<Scp956Pinata, bool> Carpinchos = new Dictionary<Scp956Pinata, bool>();

        public string Description { get; } = "yes";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("cedmod.carpincho"))
            {
                response = "no permission";
                return false;
            }
            
            if (arguments.Count < 1)
            {
                response = "missing argument";
                return false;
            }

            bool state;
            if (!bool.TryParse(arguments.At(0), out state))
            {
                response = "Please specify the state";
                return false;
            }

            foreach (var cap in Object.FindObjectsOfType<Scp956Pinata>())
            {
                if (!Carpinchos.ContainsKey(cap))
                    Carpinchos.Add(cap, state);
                Carpinchos[cap] = state;
                cap.Network_carpincho = state ? (byte)69 : (byte) 1;
            }
            
            response = "Done";
            return true;
        }
    }

    [HarmonyPatch(typeof(Scp956Pinata), nameof(Scp956Pinata.Network_carpincho), MethodType.Setter)]
    public static class CarpinchoPatch
    {
        public static bool Prefix(Scp956Pinata __instance, ref byte value)
        {
            if (CarpinchoCommand.Carpinchos.ContainsKey(__instance) && CarpinchoCommand.Carpinchos[__instance])
                value = 69;
            
            return true;
        }
    }
}