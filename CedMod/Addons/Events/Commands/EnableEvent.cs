using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using CedMod.Addons.QuerySystem;
using CommandSystem;
using MEC;
#if !EXILED
using NWAPIPermissionSystem;
#else
using Exiled.Permissions.Extensions;
#endif
using PluginAPI.Core;

namespace CedMod.Addons.Events.Commands
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
            IEvent @event = EventManager.AvailableEvents.FirstOrDefault(ev => ev.EventPrefix == arguments.At(0));
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
                EventManager.nextEvent.RemoveAll(ev => ev.EventName == @event.EventName);
                EventManager.nextEvent.Insert(0, @event);
                Broadcast.Singleton.RpcAddElement($"EventManager: {@event.EventName} is being enabled.\nRound will restart in 3 seconds", 5, Broadcast.BroadcastFlags.Normal);
                Timing.CallDelayed(3, () =>
                {
                    Round.Restart(false, false);
                });
            }
            else
            {
                if (EventManager.nextEvent.Any(ev => ev.EventName == @event.EventName))
                {
                    response = "This event is already added to the queue";
                    return false;
                }
                EventManager.nextEvent.Add(@event);
                Broadcast.Singleton.RpcAddElement($"EventManager: {@event.EventName} has been added to the event queue: position {EventManager.nextEvent.IndexOf(@event)}", 10, Broadcast.BroadcastFlags.Normal);
            }
            ThreadDispatcher.SendHeartbeatMessage(true);
            response = "Success";
            return true;
        }
    }
}