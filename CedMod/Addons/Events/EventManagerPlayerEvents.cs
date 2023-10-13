using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;

namespace CedMod.Addons.Events
{
    public class EventManagerPlayerEvents
    {
        
        [PluginEvent(ServerEventType.PlayerJoined)]
        public void OnPlayerJoin(PlayerJoinedEvent ev)
        {
            if (CedModMain.Singleton.Config.EventManager.Debug)
                Log.Debug($"Join {EventManager.CurrentEvent != null}");
            if (EventManager.CurrentEvent != null)
            {
                ev.Player.GameObject.AddComponent<HintManager>();
            }
        }
    }
}