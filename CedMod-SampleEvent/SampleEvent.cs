using System;
using CedMod.Addons.Events;
using HarmonyLib;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;

namespace CedMod.SampleEvent
{
    public class SampleEvent : IEvent
    {
        public static Harmony Harmony;

        public static bool IsRunning = false;

        [PluginUnload]
        public void OnDisabled()
        {
            StopEvent();
        }
        
        [PluginEntryPoint("CedMod-ExampleGameMode", "0.0.1", "Example gamemode for the cedmod gamemode manager", "ced777ric#0001")]
        public void OnEnabled()
        {
            
        }

        public string EventName { get; } = "Example Event";
        public string EvenAuthor { get; } = "ced777ric";
        public string EventDescription { get; set; } = "A testing event, you can use this to make your own events";
        public string EventPrefix { get; } = "Sample";
        public bool OverrideWinConditions { get; }
        public bool BulletHolesAllowed { get; set; } = false;
        public bool CanRoundEnd()
        {
            return true;
        }

        public void PrepareEvent()
        {
            Log.Info("SampleEvent is preparing");
            IsRunning = true;
            Log.Info("SampleEvent is prepared");
        }

        public void StopEvent()
        {
            IsRunning = false;
        }
    }
}