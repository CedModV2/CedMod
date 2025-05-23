﻿using CentralAuth;
using LabApi.Features.Wrappers;

namespace CedMod
{
    public static class CedModPlayer
    {
        public static Player Get(string userid)
        {
            // Check id
            if (int.TryParse(userid, out int id))
                return Player.Get(id);

            // Check Userid
            if (userid.EndsWith("@steam") || userid.EndsWith("@discord") || userid.EndsWith("@northwood") || userid.EndsWith("@patreon") || (userid.StartsWith("ID_Offline") && !PlayerAuthenticationManager.OnlineMode))
            {
                foreach (var hub in ReferenceHub.AllHubs)
                {
                    if (hub.authManager.UserId == userid)
                        return Player.Get(hub);
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

                return Player.Get(hub);
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
        
        public static Player Get(int PlayerId)
        {
            foreach (var hub in ReferenceHub.AllHubs)
            {
                if (hub.PlayerId == PlayerId)
                    return Player.Get(hub);
            }

            return null;
        }
        
        public static Player Get(ReferenceHub refhub)
        {
            return Player.Get(refhub);
        }
    }
}