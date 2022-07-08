﻿using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using UnityEngine;

namespace CedMod.Commands
{
    /// <summary>
    /// <see cref="Description"/>.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class LightsoutCommand : ICommand
    {
        public string Command { get; } = "lightsout";

        public string[] Aliases { get; } = new string[]
        {
            "lamps"
        };

        public string Description { get; } = "Turns off the lamps";
        internal static bool isEnabled;
        internal static CoroutineHandle CoroutineHandle;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("cedmod.lightsout"))
            {
                response = "no permission";
                return false;
            }
            if (isEnabled == false)
            {
                if (arguments.Count != 5)
                {
                    response =
                        "Usage: <b>lightsout <surface> <entrance> <heavy> <light> <duration></b> first 3 values are booleans and the last one is a number";
                    return false;
                }

                isEnabled = true;
                CoroutineHandle = Timing.RunCoroutine(LightsOut(Convert.ToBoolean(arguments.At(0)), Convert.ToBoolean(arguments.At(1)), Convert.ToBoolean(arguments.At(2)), Convert.ToBoolean(arguments.At(3)), Convert.ToInt32(arguments.At(4))), "LightsOut");
                response = "Lights have been turned off";
                return true;
            }
            else
            {
                if (isEnabled)
                {
                    isEnabled = false;
                    Timing.KillCoroutines(CoroutineHandle);
                    foreach (FlickerableLightController r in FlickerableLightController.Instances)
                    {
                        r.ServerFlickerLights(0);
                    }
                    response = "Lights have been turned on";
                    return true;
                }
            }

            response = "Something went wrong";
            return true;
        }

        public static IEnumerator<float> LightsOut(bool surface, bool entrance, bool heavy, bool light, float dur)
        {
            while (isEnabled)
            {
                if (surface)
                    Map.TurnOffAllLights(dur, ZoneType.Surface);
                if (entrance)
                    Map.TurnOffAllLights(dur, ZoneType.Entrance);
                if (heavy)
                    Map.TurnOffAllLights(dur, ZoneType.HeavyContainment);
                if (light)
                    Map.TurnOffAllLights(dur, ZoneType.LightContainment);
                yield return Timing.WaitForSeconds(dur);
            }
        }
    }
}