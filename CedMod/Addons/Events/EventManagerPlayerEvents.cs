using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using PluginAPI.Core;

namespace CedMod.Addons.Events
{
    public class EventManagerPlayerEvents
    {
        public void OnPlayerJoin(JoinedEventArgs ev)
        {
            Log.Debug($"Join {EventManager.currentEvent != null}", CedModMain.Singleton.Config.EventManager.Debug);
            if (EventManager.currentEvent != null)
            {
                Timing.CallDelayed(1, () =>
                {
                    ev.Player.Broadcast(10, $"EventManager: This server is currently running an event: {EventManager.currentEvent.EventName}\n{EventManager.currentEvent.EventDescription}");
                });
            }
        }
    }
}