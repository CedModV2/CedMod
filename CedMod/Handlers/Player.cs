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
        public void OnLeave(LeftEventArgs ev)
        {
            if (BanSystem.BanSystem.Users.ContainsKey(ev.Player.ReferenceHub))
            {
                BanSystem.BanSystem.Users.Remove(ev.Player.ReferenceHub);
                ev.Player.ReferenceHub.serverRoles.SetText(null);
                ev.Player.ReferenceHub.serverRoles.SetColor(null);
                Initializer.Logger.Debug("BadgeHandler","Removing player from playerlist");
            }
        }
    }
}