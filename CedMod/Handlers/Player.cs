﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Exiled.Events.EventArgs;
using MEC;

namespace CedMod.Handlers
{
    /// <summary>
    /// Used to handle player events.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Called when a player joins.
        /// </summary>
        /// <param name="ev">The <see cref="VerifiedEventArgs"/>.</param>
        public void OnJoin(VerifiedEventArgs ev)
        {
            Task.Factory.StartNew(() => { BanSystem.HandleJoin(ev); });
            Timing.RunCoroutine(Name(ev));
        }
        
        private IEnumerator<float> Name(VerifiedEventArgs ev)
        {
            foreach (var pp in Exiled.API.Features.Player.List)
            {
                if (pp.UserId == ev.Player.UserId) 
                    yield break;
                if (CedModMain.Singleton.Config.CedMod.KickSameName)
                {
                    if (pp.Nickname == ev.Player.Nickname)
                    {
                        if (ev.Player.ReferenceHub.serverRoles.RemoteAdmin && !pp.ReferenceHub.serverRoles.RemoteAdmin)
                        {
                            pp.Kick("You have been kicked by a plugin: \n Please change your name to something unique (A staff member joined with your name)");
                            yield break;
                        }
                        else if (pp.UserId != ev.Player.UserId)
                        {
                            ev.Player.Kick("You have been kicked by a plugin: \n Please change your name to something unique (there is already someone with your name)");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Called when a player dies.
        /// </summary>
        /// <param name="ev">The <see cref="DyingEventArgs"/>.</param>
        public void OnDying(DyingEventArgs ev)
        {
            if (ev.Target == null || ev.Killer == null)
                return;
            FriendlyFireAutoban.HandleKill(ev);
        }
    }
}