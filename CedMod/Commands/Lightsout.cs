using System;
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
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class LightsoutCommand : ICommand
    {
        public string Command { get; } = "lightsout";

        public string[] Aliases { get; } = new string[]
        {
            "lamps"
        };

        public string Description { get; } = "Turns off the lämps";
        private bool _isEnabled;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (!sender.CheckPermission("cedmod.lightsout"))
            {
                response = "no permission";
                return false;
            }
            if (_isEnabled == false)
            {
                if (arguments.Count != 5)
                {
                    response =
                        "Usage: <b>lightsout <surface> <entrance> <heavy> <light> <duration></b> first 3 values are booleans and the last one is a number";
                    return false;
                }

                _isEnabled = true;
                Timing.RunCoroutine(
                    LightsOut(Convert.ToBoolean(arguments.At(0)), Convert.ToBoolean(arguments.At(1)),
                        Convert.ToBoolean(arguments.At(2)), Convert.ToBoolean(arguments.At(3)),
                        Convert.ToInt32(arguments.At(4))), "LightsOut");
                response = "Lights have been turned off";
                return true;
            }
            else
            {
                if (_isEnabled)
                {
                    _isEnabled = false;
                    List<Room> rooms = Map.Rooms.ToList();
                    foreach (Room r in rooms)
                    {
                        GameObject gameObject = GameObject.Find(r.Transform.parent.name + "/" + r.Transform.name);
                        foreach (FlickerableLightController flc in gameObject
                            .GetComponentsInChildren<FlickerableLightController>())
                        {
                            flc.ServerFlickerLights(0);
                        }
                    }

                    Timing.KillCoroutines("LightsOut");
                    response = "Lights have been turned on";
                    return true;
                }
            }

            response = "Something went wrong";
            return true;
        }

        public static IEnumerator<float> LightsOut(bool surface, bool entrance, bool heavy, bool light, float dur)
        {
            List<Room> rooms = Map.Rooms.ToList();
            foreach (Room r in rooms)
            {
                if (surface && r.Zone == ZoneType.Surface)
                {
                    GameObject gameObject = GameObject.Find(r.Transform.parent.name + "/" + r.Transform.name);
                    foreach (FlickerableLightController flc in gameObject
                        .GetComponentsInChildren<FlickerableLightController>())
                    {
                        flc.ServerFlickerLights(dur);
                    }
                }

                if (entrance && r.Zone == ZoneType.Entrance)
                {
                    GameObject gameObject = GameObject.Find(r.Transform.parent.name + "/" + r.Transform.name);
                    foreach (FlickerableLightController flc in gameObject
                        .GetComponentsInChildren<FlickerableLightController>())
                    {
                        flc.ServerFlickerLights(dur);
                    }
                }

                if (heavy && r.Zone == ZoneType.HeavyContainment)
                {
                    GameObject gameObject = GameObject.Find(r.Transform.parent.name + "/" + r.Transform.name);
                    foreach (FlickerableLightController flc in gameObject
                        .GetComponentsInChildren<FlickerableLightController>())
                    {
                        flc.ServerFlickerLights(dur);
                    }
                }

                if (light && r.Zone == ZoneType.LightContainment)
                {
                    GameObject gameObject = GameObject.Find(r.Transform.parent.name + "/" + r.Transform.name);
                    foreach (FlickerableLightController flc in gameObject
                        .GetComponentsInChildren<FlickerableLightController>())
                    {
                        flc.ServerFlickerLights(dur);
                    }
                }
            }

            yield return Timing.WaitForSeconds(10f);
            Timing.RunCoroutine(LightsOut(surface, entrance, heavy, light, dur + 0.6f), "LightsOut");
        }
    }
}