using System;
using System.Collections.Generic;
using System.Linq;
using CedMod.QuerySystem;
using CedMod.QuerySystem.WS;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using Newtonsoft.Json;

namespace CedMod.EventManager.Commands
{
    public class EnableEvent : ICommand, IUsageProvider
    {
        public string Command { get; } = "enable";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Enables the specified event, if Force is true the server will restart the round immediately";

        public string[] Usage { get; } = new string[]
        {
            "%eventprefix%",
            "%force%"
        };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (arguments.Count < 2)
            {
                response = "To execute this command provide at least 2 arguments!\nUsage: " + this.DisplayCommandUsage();
                return false;
            }
            IEvent @event = EventManager.Singleton.AvailableEvents.FirstOrDefault(ev => ev.EventPrefix == arguments.At(0));
            bool force = Convert.ToBoolean(arguments.At(1));
            if (@event == null)
            {
                response = "Event does not exist";
                return false;
            }
            
            if (sender.IsPanelUser() ? !sender.CheckPermission(PlayerPermissions.FacilityManagement) : !sender.CheckPermission("cedmod.events.enable"))
            {
                response = "No permission";
                return false;
            }

            if (force)
            {
                EventManager.Singleton.nextEvent.RemoveAll(ev => ev.EventName == @event.EventName);
                EventManager.Singleton.nextEvent.Insert(0, @event);
                Map.Broadcast(5, $"EventManager: {@event.EventName} is being enabled.\nRound will restart in 3 seconds");
                Timing.CallDelayed(3, () =>
                {
                    Round.Restart(false, false);
                });
            }
            else
            {
                if (EventManager.Singleton.nextEvent.Any(ev => ev.EventName == @event.EventName))
                {
                    response = "This event is already added to the queue";
                    return false;
                }
                EventManager.Singleton.nextEvent.Add(@event);
                Map.Broadcast(10, $"EventManager: {@event.EventName} has been added to the event queue: position {EventManager.Singleton.nextEvent.IndexOf(@event)}");
            }
            response = "Success";
            return true;
        }
    }
}