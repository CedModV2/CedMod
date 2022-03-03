using System;
using System.Collections.Generic;
using System.Linq;
using CedMod.Addons.QuerySystem;
using CedMod.ApiModals;
using CommandSystem;
using Exiled.Permissions.Extensions;
using Newtonsoft.Json;

namespace CedMod.Addons.Events.Commands
{
    public class ListEvents : ICommand
    {
        public string Command { get; } = "list";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Gets a list of events";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            response = "";
            if (!sender.CheckPermission("cedmod.events.list"))
            {
                response = "No permission";
                return false;
            }
            foreach (var ev in EventManager.AvailableEvents)
            {
                response += $"{ev.EventName} (Prefix: {ev.EventPrefix}) Author: {ev.EvenAuthor} - {ev.EventDescription}";
            }
            
            return true;
        }
    }
}