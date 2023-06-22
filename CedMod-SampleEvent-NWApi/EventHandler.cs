using PlayerStatsSystem;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;

namespace CedMod.SampleEvent
{
    public class EventHandler
    {

        [PluginEvent(ServerEventType.PlayerDeath)]
        public void OnPlayerDeath(PlayerDeathEvent ev)
        {
            ev.Player.SendBroadcast("You died!", 1, Broadcast.BroadcastFlags.Normal);
        }
    }
}