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
                Log.Debug($"Join {EventManager.CurrentEvent != null}");
            if (EventManager.CurrentEvent != null)
            {
                player.GameObject.AddComponent<HintManager>();
            }
        }
    }
}