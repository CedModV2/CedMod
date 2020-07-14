using System;
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
                Timing.RunCoroutine(Functions.LightsOut(Convert.ToBoolean(arguments.At(0))), "LightsOut");
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
    }
}