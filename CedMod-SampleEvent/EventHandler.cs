using Exiled.API.Enums;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs;

namespace CedMod.SampleEvent
{
    public class EventHandler
    {
        public readonly SampleEvent Plugin;
        public EventHandler(SampleEvent plugin)
        {
            Plugin = plugin;
        }

        public void OnPlayerDeath(DiedEventArgs ev)
        {
            ev.Target.Broadcast(1, "You died!", Broadcast.BroadcastFlags.Normal);
        }
    }
}