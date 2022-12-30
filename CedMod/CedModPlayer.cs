using PluginAPI.Core;
using PluginAPI.Core.Interfaces;
using UnityEngine;

namespace CedMod
{
    public class CedModPlayer: Player
    {
        public CedModPlayer(IGameComponent component) : base(component)
        {
        }

        public static CedModPlayer Get(string userid)
        {
            foreach (var hub in ReferenceHub.AllHubs)
            {
                if (hub.characterClassManager.UserId == userid)
                    return Get<CedModPlayer>(hub);
            }

            return null;
        }
        
        public static CedModPlayer Get(int PlayerId)
        {
            foreach (var hub in ReferenceHub.AllHubs)
            {
                if (hub.PlayerId == PlayerId)
                    return Get<CedModPlayer>(hub);
            }

            return null;
        }
        
        public static CedModPlayer Get(ReferenceHub refhub)
        {
            return Get<CedModPlayer>(refhub);
        }
    }
}