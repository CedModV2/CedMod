using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs;

namespace CedMod.Addons.Events
{
    public class EventManagerServerEvents
    {
        public void EndingRound(EndingRoundEventArgs ev)
        {
            if (EventManager.currentEvent != null && EventManager.currentEvent.OverrideWinConditions)
            {
                bool canEnd = EventManager.currentEvent.CanRoundEnd();
                ev.IsAllowed = canEnd;
                ev.IsRoundEnded = canEnd;
            }
        }

        public void EndRound(RoundEndedEventArgs ev)
        {
            if (EventManager.currentEvent != null)
            {
                Log.Info($"Enabled {EventManager.currentEvent.EventName} has been disabled due to round end");
                EventManager.currentEvent.StopEvent();
                EventManager.currentEvent = null;
            }
        }

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
        }

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