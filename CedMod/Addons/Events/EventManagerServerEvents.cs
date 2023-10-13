using System;
using System.IO;
using System.Linq;
using CedMod.Addons.Events.Interfaces;
using CedMod.Addons.Events.Patches;
using CedMod.Addons.QuerySystem;
using Exiled.API.Features;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using Log = PluginAPI.Core.Log;

namespace CedMod.Addons.Events
{
    public class EventManagerServerEvents
    {
        [PluginEvent(ServerEventType.RoundEndConditionsCheck)]
        public RoundEndConditionsCheckCancellationData OnRoundEndConditionsCheck(bool baseGameConditionsSatisfied)
        {
            if (EventManager.CurrentEvent != null && EventManager.CurrentEvent is IEndConditionBehaviour endConditionBehaviour)
            { 
                return RoundEndConditionsCheckCancellationData.Override(endConditionBehaviour.CanRoundEnd(baseGameConditionsSatisfied));
            }
            return RoundEndConditionsCheckCancellationData.LeaveUnchanged();
        }
        
        [PluginEvent(ServerEventType.RoundEnd)]
        public void EndRound(RoundEndEvent ev)
        {
            if (EventManager.CurrentEvent != null)
            {
                Log.Info($"Enabled {EventManager.CurrentEvent.EventName} has been disabled due to round end");
                EventManager.CurrentEvent.StopEvent();
                EventManager.CurrentEvent = null;
            }
        }

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        public void WaitingForPlayers(WaitingForPlayersEvent ev)
        {
            if (EventManager.EventQueue.Count >= 1)
            {
                var next = EventManager.EventQueue.FirstOrDefault();
                EventManager.CurrentEvent = next;
                EventManager.EventQueue.Remove(next);
            }
            if (EventManager.CurrentEvent != null)
            {
                EventManager.CurrentEvent.PrepareEvent();
                Log.Info($"Enabled {EventManager.CurrentEvent.EventName} for this round");
            }
            ThreadDispatcher.SendHeartbeatMessage(true);
            
#if EXILED
            if (AppDomain.CurrentDomain.GetAssemblies().Any(s => s.GetName().Name == "ScriptedEvents") && Directory.Exists(Path.Combine(Paths.Configs, "ScriptedEvents")) && EventManager.EventQueue.Count == 0)
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
        public void RestartingRound(RoundRestartEvent ev)
        {
            if (EventManager.CurrentEvent != null)
            {
                Log.Info($"Enabled {EventManager.CurrentEvent.EventName} has been disabled due to round restart");
                EventManager.CurrentEvent.StopEvent();
                EventManager.CurrentEvent = null;
            }
            HintManager.HintProcessed.Clear();
            
            SeedGenerationPatch.NextSeed = -1;

            if (EventManager.EventQueue.Count >= 1)
            {
                var next = EventManager.EventQueue.FirstOrDefault();
                if (next is IMapgenBehaviour mapgenBehaviour)
                {
                    SeedGenerationPatch.NextSeed = mapgenBehaviour.Seed;
                }
            }
        }
    }
}