using PlayerStatsSystem;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;

namespace CedMod.SampleEvent
{
    public class EventHandler
    {

        [PluginEvent(ServerEventType.PlayerDeath)]
        public void OnPlayerDeath(CedModPlayer target, CedModPlayer player, DamageHandlerBase damageHandler)
        {
            target.SendBroadcast("You died!", 1, Broadcast.BroadcastFlags.Normal);
        }
    }
}