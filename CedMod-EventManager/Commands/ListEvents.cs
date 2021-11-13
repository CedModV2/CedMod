using System;
using System.Collections.Generic;
using CedMod.QuerySystem;
using CedMod.QuerySystem.WS;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Newtonsoft.Json;

namespace CedMod.EventManager.Commands
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
            if (sender.IsPanelUser())
            {
                if (!sender.CheckPermission(PlayerPermissions.FacilityManagement))
                {
                    response = "No permission";
                    return false;
                }

                List<Dictionary<string, string>> events = new List<Dictionary<string, string>>();
                foreach (var ev in EventManager.Singleton.AvailableEvents)
                {
                    events.Add(new Dictionary<string, string>()
                    {
                        {"Name", ev.EventName},
                        {"Author", ev.EvenAuthor},
                        {"Description", ev.EventDescription},
                        {"Prefix", ev.EventDescription}
                    });
                }

                response = JsonConvert.SerializeObject(events);
            }
            else
            {
                response = "";
                if (!sender.CheckPermission("cedmod.events.list"))
                {
                    response = "No permission";
                    return false;
                }
                foreach (var ev in EventManager.Singleton.AvailableEvents)
                {
                    response += $"{ev.EventName} (Prefix: {ev.EventPrefix}) Author: {ev.EvenAuthor} - {ev.EventDescription}";
                }
            }
            
            return true;
        }
    }
}