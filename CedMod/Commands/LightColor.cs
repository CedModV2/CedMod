using System;
using CommandSystem;
#if !EXILED
using NWAPIPermissionSystem;
#else
using Exiled.Permissions.Extensions;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class LightColor : ICommand
    {
        public string Command { get; } = "lightcolor";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Change the color of all lights.";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (!sender.CheckPermission("cedmod.colors"))
            {
                response = "no permission";
                return false;
            }
            if (arguments.IsEmpty())
            {
                response = "You must specify colors (255, 255, 255).";
                return false;
            }

            if (!float.TryParse(arguments.At(0), out float r))
            {
                response = "R must be a float.";
                return false;
            }
            
            if (!float.TryParse(arguments.At(1), out float g))
            {
                response = "G must be a float.";
                return false;
            }
            
            if (!float.TryParse(arguments.At(2), out float b))
            {
                response = "B must be a float.";
                return false;
            }
            
            Color lightColors = new Color(r, g, b);
            foreach (FlickerableLightController flc in FlickerableLightController.Instances)
            {
                flc.WarheadLightColor = lightColors;
                flc.WarheadLightOverride = true;
            }

            response = "Light intensity set to " + lightColors;
            return true;
        }
    }
}