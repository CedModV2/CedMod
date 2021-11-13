using System;
using CedMod.EventManager;
using Exiled.API.Features;
using HarmonyLib;

namespace CedMod.SampleEvent
{
    public class SampleEvent : Plugin<EventManager.Config>, IEvent
    {
        public static Harmony Harmony;
        /// <inheritdoc/>
        public override string Author { get; } = "ced777ric#0001";

        public override string Name { get; } = "CedMod-SampleEvent";

        public override string Prefix { get; } = "cm_sampleevent";

        public override Version RequiredExiledVersion { get; } = new Version(3, 0, 0);
        public override Version Version { get; } = new Version(0, 0, 1);
        private EventHandler _handler;

        public override void OnDisabled()
        {
            StopEvent();
            base.OnDisabled();
        }

        public static string SecurityKey;

        public override void OnEnabled()
        {
            base.OnEnabled();
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
            _handler = new EventHandler(this);
            Log.Info("SampleEvent is preparing");
            Exiled.Events.Handlers.Player.Died += _handler.OnPlayerDeath;
            Log.Info("SampleEvent is prepared");
        }

        public void StopEvent()
        {
            if (_handler == null)
                return;
            Exiled.Events.Handlers.Player.Died -= _handler.OnPlayerDeath;
            _handler = null;
        }
    }
}