using System;
using System.Linq;
using CedMod.Addons.Events.Interfaces;
using CedMod.Addons.QuerySystem;
using CommandSystem;
using Exiled.API.Features;
using LabApi.Features.Permissions;
using MEC;

namespace CedMod.Addons.Events.Commands
{
    public class BumpEvent : ICommand, IUsageProvider
    {
        public string Command { get; } = "bump";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "moves an event to the front of the queue";

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

            if (sender.IsPanelUser() ? !sender.CheckPermission(PlayerPermissions.FacilityManagement) : !sender.HasPermissions("cedmod.events.bump"))
            {
                response = "No permission";
                return false;
            }
            
            EventManager.EventQueue.RemoveAll(ev => ev.EventName == @event.EventName);
            EventManager.EventQueue.Insert(0, @event);
            
            if (force)
            {
                
                Broadcast.Singleton.RpcAddElement($"EventManager: {@event.EventName} is being enabled.\nRound will restart in 3 seconds", 5, Broadcast.BroadcastFlags.Normal);
                Timing.CallDelayed(3, () =>
                {
                    Round.Restart(false, false);
                });
            }
            else
            {
                Broadcast.Singleton.RpcAddElement($"EventManager: {@event.EventName} has been moved to queue position {EventManager.EventQueue.IndexOf(@event)}", 10, Broadcast.BroadcastFlags.Normal);
            }
            ThreadDispatcher.SendHeartbeatMessage(true);
            response = "Success";
            return true;
        }
    }
}