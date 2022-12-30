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
                player.GameObject.AddComponent<HintManager>();
            }
        }
    }
}