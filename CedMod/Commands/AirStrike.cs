using System;
using CommandSystem;
using EXILED.Extensions;
using MEC;
using UnityEngine;

namespace CedMod.Commands
{
    public class AirstrikeCommand : ICommand
    {
        public string Command { get; } = "airstrike";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Calls an airstrike";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (arguments.Count <= 2)
            {
                response = "Usage: AIRSTRIKE <delay> <duration>";
                return false;
            }
            Timing.RunCoroutine(Functions.Coroutines.AirSupportBomb(Convert.ToInt32(arguments.At(0)), Convert.ToInt32(arguments.At(1))), "airstrike");
            response = "Bombs away";
            return true;
        }
    }
}