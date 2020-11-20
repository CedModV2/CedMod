using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using UnityEngine;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class LightIntensity : ICommand
    {
        public string Command { get; } = "lightintensity";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Change the intensity of all lights.";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (arguments.IsEmpty())
            {
                response = "You must specify a intensity.";
                return false;
            }

            if (!float.TryParse(arguments.At(0), out float intensity))
            {
                response = "Intensity must be a float.";
                return false;
            }
                
            foreach (Room r in Map.Rooms.ToList())
            {
                GameObject gameObject = GameObject.Find(r.Transform.parent.name + "/" + r.Transform.name);
                    foreach (FlickerableLightController flc in gameObject
                        .GetComponentsInChildren<FlickerableLightController>())
                    {
                        flc.ServerSetLightIntensity(intensity);
                    }
            }

            response = "Light intensity set to " + intensity;
            return true;
        }
    }
}