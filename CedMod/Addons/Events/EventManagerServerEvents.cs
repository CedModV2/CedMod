using System;
using System.IO;
using System.Linq;
using CedMod.Addons.QuerySystem;
using Exiled.API.Features;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using Log = PluginAPI.Core.Log;

namespace CedMod.Addons.Events
{
    public class EventManagerServerEvents
    {
        [PluginEvent(ServerEventType.RoundEnd)]
        public void EndRound(RoundSummary.LeadingTeam team)
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
            
#if EXILED
            if (AppDomain.CurrentDomain.GetAssemblies().Any(s => s.GetName().Name == "ScriptedEvents") && Directory.Exists(Path.Combine(Paths.Configs, "ScriptedEvents")) && EventManager.nextEvent.Count == 0)
            {
                EventManager.AvailableEvents.RemoveAll(s => s.GetType() == typeof(ScriptedEventIntergration));
                foreach (var file in Directory.GetFiles(Path.Combine(Paths.Configs, "ScriptedEvents")))
                {
                    string data = File.ReadAllText(file);
                    if (!data.Contains("!-- DISABLE") && data.Contains("!-- ADMINEVENT"))
                    {
                        EventManager.AvailableEvents.Add(new ScriptedEventIntergration()
                        {
                            BulletHolesAllowed = true,
                            config = new ScriptedEventConfig(){ IsEnabled = true},
                            eventAuthor = "ScriptedEvents",
                            EventDescription = "ScriptedEvent",
                            eventName = Path.GetFileNameWithoutExtension(file),
                            eventPrefix = Path.GetFileNameWithoutExtension(file),
                            FilePath = file
                        });
                    }
                }
            }
#endif
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
            HintManager.HintProcessed.Clear();
        }
    }
}