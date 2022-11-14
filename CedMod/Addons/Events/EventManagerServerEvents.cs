using System.Linq;
using CedMod.Addons.QuerySystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;

namespace CedMod.Addons.Events
{
    public class EventManagerServerEvents
    {
        [PluginEvent(ServerEventType.RoundEnd)]
        public void EndRound()
        {
            if (EventManager.currentEvent != null)
            {
                Log.Info($"Enabled {EventManager.currentEvent.EventName} has been disabled due to round end");
                EventManager.currentEvent.StopEvent();
                EventManager.currentEvent = null;
            }
        }

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        public void WaitingForPlayers()
        {
            if (EventManager.nextEvent.Count >= 1)
            {
                var next = EventManager.nextEvent.FirstOrDefault();
                EventManager.currentEvent = next;
                EventManager.nextEvent.Remove(next);
            }
            if (EventManager.currentEvent != null)
            {
                EventManager.currentEvent.PrepareEvent();
                Log.Info($"Enabled {EventManager.currentEvent.EventName} for this round");
            }
            ThreadDispatcher.SendHeartbeatMessage(true);
        }

        [PluginEvent(ServerEventType.RoundRestart)]
        public void RestartingRound()
        {
            if (EventManager.currentEvent != null)
            {
                Log.Info($"Enabled {EventManager.currentEvent.EventName} has been disabled due to round restart");
                EventManager.currentEvent.StopEvent();
                EventManager.currentEvent = null;
            }
        }
    }
}