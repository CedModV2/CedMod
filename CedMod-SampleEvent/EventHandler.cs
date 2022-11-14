using PlayerStatsSystem;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;

namespace CedMod.SampleEvent
{
    public class EventHandler
    {
        public readonly SampleEvent Plugin;
        public EventHandler(SampleEvent plugin)
        {
            Plugin = plugin;
        }

        [PluginEvent(ServerEventType.PlayerDeath)]
        public void OnPlayerDeath(CedModPlayer player, CedModPlayer target, DamageHandlerBase damageHandler)
        {
            target.SendBroadcast("You died!", 1, Broadcast.BroadcastFlags.Normal);
        }
    }
}