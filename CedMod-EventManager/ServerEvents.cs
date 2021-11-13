using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.EventManager.Patches;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using Newtonsoft.Json;

namespace CedMod.EventManager
{
    public class ServerEvents
    {
        public void EndingRound(EndingRoundEventArgs ev)
        {
            if (EventManager.Singleton.currentEvent != null && EventManager.Singleton.currentEvent.OverrideWinConditions)
            {
                bool canEnd = EventManager.Singleton.currentEvent.CanRoundEnd();
                ev.IsAllowed = canEnd;
                ev.IsRoundEnded = canEnd;
            }
        }

        public void EndRound(RoundEndedEventArgs ev)
        {
            if (EventManager.Singleton.currentEvent != null)
            {
                Log.Info($"Enabled {EventManager.Singleton.currentEvent.EventName} has been disabled due to round end");
                EventManager.Singleton.currentEvent.StopEvent();
                EventManager.Singleton.currentEvent = null;
            }
        }

        public void WaitingForPlayers()
        {
            if (EventManager.Singleton.nextEvent != null)
            {
                EventManager.Singleton.currentEvent = EventManager.Singleton.nextEvent;
                EventManager.Singleton.nextEvent = null;
            }
            if (EventManager.Singleton.currentEvent != null)
            {
                EventManager.Singleton.currentEvent.PrepareEvent();
                Log.Info($"Enabled {EventManager.Singleton.currentEvent.EventName} for this round");
            }
        }

        public void RestartingRound()
        {
            if (EventManager.Singleton.currentEvent != null)
            {
                Log.Info($"Enabled {EventManager.Singleton.currentEvent.EventName} has been disabled due to round restart");
                EventManager.Singleton.currentEvent.StopEvent();
                EventManager.Singleton.currentEvent = null;
            }
        }
    }
}