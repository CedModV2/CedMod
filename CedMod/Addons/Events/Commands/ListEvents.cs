using System;
using System.Collections.Generic;
using System.Linq;
using CedMod.Addons.QuerySystem;
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
            if (sender.IsPanelUser())
            {
                if (!sender.CheckPermission(PlayerPermissions.FacilityManagement))
                {
                    response = "No permission";
                    return false;
                }

                List<EventModel> events = new List<EventModel>();
                foreach (var ev in EventManager.AvailableEvents)
                {
                    events.Add(new EventModel()
                    {
                        Active = EventManager.currentEvent != null && EventManager.currentEvent.EventPrefix == ev.EventPrefix,
                        Author = ev.EvenAuthor,
                        Description = ev.EventDescription,
                        Name = ev.EventName,
                        Prefix = ev.EventPrefix,
                        QueuePos = EventManager.nextEvent.Any(ev1 => ev1.EventName == ev.EventName) ? EventManager.nextEvent.FindIndex(ev1 => ev1.EventName == ev.EventName) + 1 : -1
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
                foreach (var ev in EventManager.AvailableEvents)
                {
                    response += $"{ev.EventName} (Prefix: {ev.EventPrefix}) Author: {ev.EvenAuthor} - {ev.EventDescription}";
                }
            }
            
            return true;
        }
    }
    
    public class EventModel
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Prefix { get; set; }
        public bool Active { get; set; }
        public int QueuePos { get; set; }
    }
}