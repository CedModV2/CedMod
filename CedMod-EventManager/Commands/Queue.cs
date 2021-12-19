using System;
using System.Collections.Generic;
using System.Linq;
using CedMod.QuerySystem;
using CedMod.QuerySystem.WS;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Newtonsoft.Json;

namespace CedMod.EventManager.Commands
{
    public class Queue : ICommand
    {
        public string Command { get; } = "queue";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Gets the current event queue";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (sender.IsPanelUser())
            {
                if (!sender.CheckPermission(PlayerPermissions.FacilityManagement))
                {
                    response = "No permission";
                    return false;
                }

                response = "";
            }
            else
            {
                response = "";
                if (!sender.CheckPermission("cedmod.events.list"))
                {
                    response = "No permission";
                    return false;
                }
            }

            response += $"Current event: [0] {(EventManager.Singleton.currentEvent == null ? "None" : $"{EventManager.Singleton.currentEvent.EventName} - ({EventManager.Singleton.currentEvent.EventPrefix})")}\n\n\nQueue:\n";
            foreach (var evnt in EventManager.Singleton.nextEvent)
            {
                response += $"[{EventManager.Singleton.nextEvent.IndexOf(evnt)}] {evnt.EventName} - ({evnt.EventPrefix})\n";
            }

            return true;
        }
    }
}