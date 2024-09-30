using System;
using CedMod.Addons.Events;
using CedMod.Addons.Events.Interfaces;
using HarmonyLib;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using UnityEngine.XR;
using EventManager = PluginAPI.Events.EventManager;

namespace CedMod.SampleEvent
{
    public class SampleEvent : Plugin<Config>, IEvent
    {
        public static bool IsRunning = false;

        [PluginUnload]
        public void OnDisabled()
        {
            StopEvent();
        }
        
        public string EventName { get; } = "Example Event";
        public string EvenAuthor { get; } = "ced777ric";
        public string EventDescription { get; set; } = "A testing event, you can use this to make your own events";
        public string EventPrefix { get; } = "Sample";

        public IEventConfig EventConfig => Config;

        public void PrepareEvent()
        {
            Logger.Info("SampleEvent is preparing");
            IsRunning = true;
            Logger.Info("SampleEvent is prepared");
            EventManager.RegisterEvents<EventHandler>(this);
        }

        public void StopEvent()
        {
            IsRunning = false;
            EventManager.UnregisterEvents<EventHandler>(this);
        }

        public override void Enable()
        {
            throw new NotImplementedException();
        }

        public override void Disable()
        {
            throw new NotImplementedException();
        }

        public override string Name { get; }
        public override string Description { get; }
        public override string Author { get; }
        public override Version Version { get; }
        public override Version RequiredApiVersion { get; }
    }
}