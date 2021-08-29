using System;
using CommandSystem;
using Exiled.Permissions.Extensions;
using UnityEngine;

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

            if (!float.TryParse(arguments.At(0), out float R))
            {
                response = "R must be a float.";
                return false;
            }
            
            if (!float.TryParse(arguments.At(1), out float G))
            {
                response = "G must be a float.";
                return false;
            }
            
            if (!float.TryParse(arguments.At(2), out float B))
            {
                response = "B must be a float.";
                return false;
            }
            
            foreach (FlickerableLightController flc in GameObject.FindObjectsOfType<FlickerableLightController>())
            {
                flc.WarheadLightColor = new Color(R, G, B);
                flc.WarheadLightOverride = true;
            }

            response = "Light intensity set to " + new Color(R, G, B);
            return true;
        }
    }
}