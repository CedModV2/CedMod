using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;

namespace CedMod.Addons.Events
{
    public class EventManagerPlayerEvents
    {
        
        [PluginEvent(ServerEventType.PlayerJoined)]
        public void OnPlayerJoin(CedModPlayer player)
        {
            if (CedModMain.Singleton.Config.EventManager.Debug)
                Log.Debug($"Join {EventManager.currentEvent != null}");
            if (EventManager.currentEvent != null)
            {
                Timing.CallDelayed(1, () =>
                {
                    Broadcast.Singleton.TargetAddElement(player.Connection, $"EventManager: This server is currently running an event: {EventManager.currentEvent.EventName}\n{EventManager.currentEvent.EventDescription}", 10, Broadcast.BroadcastFlags.Normal);
                });
            }
        }
    }
}