﻿using System;
using System.Collections.Generic;
using CommandSystem;
using Exiled.API.Features;
using UnityEngine;

namespace CedMod.Commands
{
    public class RedirectCommand : ICommand
    {
        public string Command { get; } = "redirect";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "redirects ALL to the specified port";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (arguments.IsEmpty())
            {
                response = "you must specify a port";
                return false;
            }

            foreach (Player p in Player.List)
            {
                p.ReferenceHub.playerStats.RpcRoundrestartRedirect(1f, Convert.ToUInt16(arguments.At(0)));
            }
            response = "redirecting";
            return true;
        }
    }
}