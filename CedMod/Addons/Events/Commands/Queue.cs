using System;
using CedMod.Addons.QuerySystem;
using CommandSystem;
using LabApi.Features.Permissions;

namespace CedMod.Addons.Events.Commands
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
            if (sender.IsPanelUser() ? !sender.CheckPermission(PlayerPermissions.FacilityManagement) : !sender.HasPermissions("cedmod.events.queue"))
            {
                response = "No permission";
                return false;
            }

            response = "";
            response += $"Current event: [0] {(EventManager.CurrentEvent == null ? "None" : $"{EventManager.CurrentEvent.EventName} - ({EventManager.CurrentEvent.EventPrefix})")}\n\n\nQueue:\n";
            foreach (var evnt in EventManager.EventQueue)
            {
                response += $"[{EventManager.EventQueue.IndexOf(evnt) + 1}] {evnt.EventName} - ({evnt.EventPrefix})\n";
            }
            ThreadDispatcher.SendHeartbeatMessage(true);
            return true;
        }
    }
}