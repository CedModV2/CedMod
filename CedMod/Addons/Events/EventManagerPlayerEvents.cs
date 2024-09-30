using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;

namespace CedMod.Addons.Events
{
    public class EventManagerPlayerEvents: CustomEventsHandler
    {
        public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
        {
            if (CedModMain.Singleton.Config.EventManager.Debug)
                Logger.Debug($"Join {EventManager.CurrentEvent != null}");
            if (EventManager.CurrentEvent != null)
            {
                ev.Player.GameObject.AddComponent<HintManager>();
            }
        }
    }
}