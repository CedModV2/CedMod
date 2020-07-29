using System;
using System.Collections.Generic;
using CommandSystem;
using MEC;
using UnityEngine;

namespace CedMod.Commands
{
    public class LightsoutCommand : ICommand
    {
        public string Command { get; } = "lightsout";

        public string[] Aliases { get; } = new string[]
        {
            "lamps"
        };

        public string Description { get; } = "Turns off the lämps";
        bool IsEnabled = false;
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (arguments.Count < 1)
            {
                response = "Usage: <b>lightsout true|false</b> the bool stands for heavy only";
                return true;
            }
            if (IsEnabled == false)
            {
                IsEnabled = true;
                Timing.RunCoroutine(LightsOut(Convert.ToBoolean(arguments.At(0))), "LightsOut");
                response = "Lights have been turned off";
                return true;
            }
            else
            {
                if (IsEnabled)
                {
                    IsEnabled = false;
                    Timing.KillCoroutines("LightsOut");
                    response = "Lights have been turned on";
                    return true;
                }
            }
            response = "Something went wrong";
            return true;
        }
        public static IEnumerator<float> LightsOut(bool heavyOnly)
        {
            Generator079.mainGenerator.RpcCustomOverchargeForOurBeautifulModCreators(9.5f, heavyOnly);
            yield return Timing.WaitForSeconds(10f);
            Timing.RunCoroutine(LightsOut(Convert.ToBoolean(heavyOnly)), "LightsOut");
        }
    }
}