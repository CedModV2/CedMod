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
    public class DisableEvent : ICommand, IUsageProvider
    {
        public string Command { get; } = "disable";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Disables the current event will restart the round immediately";

        public string[] Usage { get; } = new string[]
        {
            "%nextroundevent%",
        };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (arguments.Count < 1)
            {
                response = "To execute this command provide at least 1 arguments!\nUsage: " + this.DisplayCommandUsage();
                return false;
            }
            bool nextevent = Convert.ToBoolean(arguments.At(0));
            if (nextevent)
            {
                if (EventManager.Singleton.nextEvent == null)
                {
                    response = "There is no event pending for the next round";
                    return false;
                }
            }
            else
            {
                if (EventManager.Singleton.currentEvent == null)
                {
                    response = "There is no event in progress";
                    return false;
                }
            }
            if (sender.IsPanelUser())
            {
                if (!sender.CheckPermission(PlayerPermissions.FacilityManagement))
                {
                    response = "No permission";
                    return false;
                }
            }
            else
            {
                response = "";
                if (!sender.CheckPermission("cedmod.events.disable"))
                {
                    response = "No permission";
                    return false;
                }
                foreach (var ev in EventManager.Singleton.AvailableEvents)
                {
                    response += $"{ev.EventName} Author: {ev.EvenAuthor} - {ev.EventDescription}";
                }
            }
            
            if (nextevent)
            {
                Map.Broadcast(5, $"EventManager: {EventManager.Singleton.nextEvent.EventName} is no longer enabled for the next round");
                EventManager.Singleton.nextEvent = null;
            }
            else
            {
                Map.Broadcast(10, $"EventManager: {EventManager.Singleton.currentEvent.EventName} is being now disabled, round will restart in 3 seconds");
                Timing.CallDelayed(3, () =>
                {
                    Round.Restart(false, false);
                });
            }
            response = "Success";
            return true;
        }
    }
}