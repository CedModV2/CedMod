using System;
using System.IO;
using System.Linq;
using CedMod.Addons.Events.Interfaces;
using CedMod.Addons.QuerySystem;
using LabApi.Events.Arguments.PlayerEvents;
#if EXILED
using Exiled.API.Features;
#endif
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using VoiceChat;

namespace CedMod.Addons.Events
{
    public class EventManagerServerEvents: CustomEventsHandler
    {
        private static int NextSeed = -1;
        
        public override void OnServerRoundEnding(RoundEndingEventArgs ev)
        {
            if (EventManager.CurrentEvent != null && EventManager.CurrentEvent is IEndConditionBehaviour endConditionBehaviour)
            { 
                ev.IsAllowed = endConditionBehaviour.CanRoundEnd(false);
            }
        }

        public override void OnPlayerPlacingBulletHole(PlayerPlacingBulletHoleEventArgs ev)
        {
            if (CedModMain.Singleton.Config.CedMod.PreventBulletHolesWhenMuted &&
                VoiceChatMutes.GetFlags(ev.Player.ReferenceHub).HasFlag(VcMuteFlags.LocalRegular))
                ev.IsAllowed = false;

            if (EventManager.CurrentEvent != null &&
                EventManager.CurrentEvent is IBulletHoleBehaviour dissallowBulletHoles &&
                !dissallowBulletHoles.CanPlaceBulletHole(ev))
                ev.IsAllowed = false;
                
            base.OnPlayerPlacingBulletHole(ev);
        }

        public override void OnServerMapGenerating(MapGeneratingEventArgs ev)
        {
            if (NextSeed <= -1)
                return;

            ev.Seed = NextSeed;
            base.OnServerMapGenerating(ev);
        }

        public override void OnServerRoundEnded(RoundEndedEventArgs ev)
        {
            if (EventManager.CurrentEvent != null)
            {
                Logger.Info($"Enabled {EventManager.CurrentEvent.EventName} has been disabled due to round end");
                EventManager.CurrentEvent.StopEvent();
                EventManager.CurrentEvent = null;
            }
        }
        
        public override void OnServerWaitingForPlayers()
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
                Logger.Info($"Enabled {EventManager.CurrentEvent.EventName} for this round");
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

        public override void OnServerRoundRestarted()
        {
            if (EventManager.CurrentEvent != null)
            {
                Logger.Info($"Enabled {EventManager.CurrentEvent.EventName} has been disabled due to round restart");
                EventManager.CurrentEvent.StopEvent();
                EventManager.CurrentEvent = null;
            }
            HintManager.HintProcessed.Clear();
            
            NextSeed = -1;

            if (EventManager.EventQueue.Count >= 1)
            {
                var next = EventManager.EventQueue.FirstOrDefault();
                if (next is IMapgenBehaviour mapgenBehaviour)
                {
                    NextSeed = mapgenBehaviour.Seed;
                }
            }
        }
    }
}