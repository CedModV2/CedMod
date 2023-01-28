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
            // Check id
            if (int.TryParse(userid, out int id))
                return Get<CedModPlayer>(id);

            // Check Userid
            if (userid.EndsWith("@steam") || userid.EndsWith("@discord") || userid.EndsWith("@northwood") ||
                userid.EndsWith("@patreon"))
            {
            foreach (var hub in ReferenceHub.AllHubs)
            {
                if (hub.characterClassManager.UserId == userid)
                    return Get<CedModPlayer>(hub);
            }

            }
            else // Check username
            {
                var hub = processHubUsername(userid); // try getting a hub with no spaces
                                                      
                // if no hub is found, try again but replace any '_' with spaces
                if (hub == null) // This Allows mods to select a user with a space in their name. 
                    hub = processHubUsername(userid.Replace("_", " "));

                if (hub == null) // if nobody is found after this, return null
                    return null;

                return Get<CedModPlayer>(hub);
            }

            return null;
        }

        private static ReferenceHub processHubUsername(string userid)
        {
            int lastnameDifference = 31;
            string firstString = userid.ToLower();

            foreach (ReferenceHub player in ReferenceHub.AllHubs)
            {
                if (player.nicknameSync.Network_myNickSync == null)
                    continue;

                if (!player.nicknameSync.Network_myNickSync.ToLower().Contains(userid.ToLower()))
                    continue;

                string secondString = player.nicknameSync.Network_myNickSync.ToLower();

                int nameDifference = secondString.Length - firstString.Length;
                if (nameDifference < lastnameDifference)
                {
                    lastnameDifference = nameDifference;
                    return player;
                }
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