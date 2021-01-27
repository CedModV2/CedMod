using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CedMod.INIT;
using GameCore;
using Grenades;
using MEC;
using Mirror;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace CedMod.Handlers
{
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;

    /// <summary>
    /// Handles server-related events.
    /// </summary>
    public class Player
    {
        public void OnJoin(VerifiedEventArgs ev)
        {
            Task.Factory.StartNew(() => { BanSystem.HandleJoin(ev); });
            Timing.RunCoroutine(Name(ev));
            foreach (string b in ConfigFile.ServerConfig.GetStringList("cm_nicknamefilter"))
            {
                if (ev.Player.Nickname.ToUpper().Contains(b.ToUpper()))
                {
                    ev.Player.ReferenceHub.nicknameSync.DisplayName = "Filtered name";
                }
            }
            
            // if (RoundSummary.roundTime == 0 && ConfigFile.ServerConfig.GetBool("cm_customloadingscreen", true)) removed for now
            //     Timing.RunCoroutine(MiniGameHandler.Playerjoinhandle(ev));
        }
        
        public IEnumerator<float> Name(VerifiedEventArgs ev)
        {
            foreach (var pp in Exiled.API.Features.Player.List)
            {
                if (pp.UserId == ev.Player.UserId) yield return 0;
                if (CedModMain.config.KickSameName)
                {
                    if (pp.Nickname == ev.Player.Nickname)
                    {
                        if (ev.Player.ReferenceHub.serverRoles.RemoteAdmin && !pp.ReferenceHub.serverRoles.RemoteAdmin)
                        {
                            Server.sendDI(pp.Nickname + pp.UserId + " kicked for having a name of a staff member " + ev.Player.UserId + ev.Player.Nickname);
                            pp.Kick("You have been kicked by a plugin: \n Please change your name to something unique (A staff member joined with your name) \n This server is protected by CedMod");
                            yield return 0f;
                        }
                        else if (pp.UserId != ev.Player.UserId)
                        {
                            Server.sendDI(pp.Nickname + pp.UserId + " kicked for having a name of a server member " +
                                          ev.Player.UserId + ev.Player.Nickname);
                            ev.Player.Kick(
                                "You have been kicked by a plugin: \n Please change your name to something unique (there is already someone with your name) \n This server is protected by CedMod");
                        }
                    }
                }
            }

            yield return 0f;
        }

        public void OnLeave(DestroyingEventArgs ev)
        {
        }

        public void OnPickup(PickingUpItemEventArgs ev)
        {
            //MiniGameHandler.Pickup(ev);
        }
        
        public void OnDying(DyingEventArgs ev)
        {
            Initializer.Logger.Debug("CMEVENT", "Dyingevent triggered");
            FriendlyFireAutoban.HandleKill(ev);
            Timing.RunCoroutine(PlayerStatsDieEv(ev));
        }

        IEnumerator<float> PlayerStatsDieEv(DyingEventArgs ev)
        {
            /*if (!RoundSummary.RoundInProgress())
                yield return 0f;
            if (FriendlyFireAutoban.IsTeakill(ev))
                Task.Factory.StartNew(() =>
                {
                    API.APIRequest("playerstats/addstat.php",
                        $"?rounds=0&kills=0&deaths=0&teamkills=1&alias={API.GetAlias()}&id={ev.Killer.UserId}&dnt={Convert.ToInt32(ev.Target.ReferenceHub.serverRoles.DoNotTrack)}&ip={ev.Killer.IPAddress}&username={ev.Killer.Nickname}");
                });
            else
                Task.Factory.StartNew(() =>
                {
                    API.APIRequest("playerstats/addstat.php",
                        $"?rounds=0&kills=1&deaths=0&teamkills=0&alias={API.GetAlias()}&id={ev.Killer.UserId}&dnt={Convert.ToInt32(ev.Target.ReferenceHub.serverRoles.DoNotTrack)}&ip={ev.Killer.IPAddress}&username={ev.Killer.Nickname}");
                });
            Task.Factory.StartNew(() =>
            {
                API.APIRequest("playerstats/addstat.php",
                    $"?rounds=0&kills=0&deaths=1&teamkills=0&alias={API.GetAlias()}&id={ev.Target.UserId}&dnt={Convert.ToInt32(ev.Target.ReferenceHub.serverRoles.DoNotTrack)}&ip={ev.Target.IPAddress}&username={ev.Target.Nickname}");
            });*/
            yield return 0f;
        }
    }
}