using CedMod.INIT;
using GameCore;
using MEC;
using UnityEngine;

namespace CedMod.Handlers
{
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;

    /// <summary>
    /// Handles server-related events.
    /// </summary>
    public class Player
    {
        public void OnJoin(JoinedEventArgs ev)
        {
            Timing.RunCoroutine(BanSystem.HandleJoin(ev));
            foreach (string b in ConfigFile.ServerConfig.GetStringList("cm_nicknamefilter"))
            {
                if (ev.Player.Nickname.ToUpper().Contains(b.ToUpper()))
                {
                    ev.Player.ReferenceHub.nicknameSync.DisplayName = "Filtered name";
                }
            }
            Initializer.Logger.Debug("Joined", "Join event fired");
        }
        public void OnLeave(LeftEventArgs ev)
        {
            Initializer.Logger.Debug("Left", "Left event fired");
        }

        public void OnDying(DyingEventArgs ev)
        {
            FriendlyFireAutoban.HandleKill(ev);
        }
    }
}