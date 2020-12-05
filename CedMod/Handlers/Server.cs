using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CedMod.INIT;
using CommandSystem;
using Exiled.Events;
using GameCore;
using MEC;
using UnityEngine;
using Exiled.API.Features;
using Mirror;
using Console = System.Console;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace CedMod.Handlers
{
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;

    /// <summary>
    /// Handles server-related events.
    /// </summary>
    public class Server
    {
        /// <inheritdoc cref="Events.Handlers.Server.OnWaitingForPlayers"/>
        public void OnWaitingForPlayers()
        {
            // if (ConfigFile.ServerConfig.GetBool("cm_customloadingscreen", true))
            //     Timing.RunCoroutine(MiniGameHandler.WaitingForPlayers());
        }

        public void onRoundStart()
        {
            if (ConfigFile.ServerConfig.GetBool("cm_customloadingscreen", true))
                MiniGameHandler.RoundStart();
        }
        Dictionary<ReferenceHub, ReferenceHub> reported = new Dictionary<ReferenceHub, ReferenceHub>();
        public void OnReport(LocalReportingEventArgs ev)
        {
            if (CedModMain.config.ReportBlacklist.Contains(ev.Issuer.UserId))
            {
                ev.IsAllowed = false;
                ev.Issuer.GameObject.GetComponent<GameConsoleTransmission>().SendToClient(ev.Issuer.Connection,
                    $"[REPORTING] You are banned from ingame reports", "green");
                return;
            }
            if (ev.Issuer.UserId == ev.Target.UserId)
            {
                ev.IsAllowed = false;
                ev.Issuer.GameObject.GetComponent<GameConsoleTransmission>().SendToClient(ev.Issuer.Connection,
                    $"[REPORTING] You can't report yourself", "green");
                return;
            }
            if (reported.ContainsKey(ev.Target.ReferenceHub))
            {
                ev.IsAllowed = false;
                ev.Issuer.GameObject.GetComponent<GameConsoleTransmission>().SendToClient(ev.Issuer.Connection,
                    $"[REPORTING] {ev.Target.Nickname} ({ev.Target.UserId}) has already been reported by {Exiled.API.Features.Player.Get(reported[ev.Target.ReferenceHub]).Nickname}",
                    "green");
                return;
            }
            if (ev.Target.RemoteAdminAccess && !CedModMain.config.StaffReportAllowed)
            {
                ev.Issuer.GameObject.GetComponent<GameConsoleTransmission>().SendToClient(ev.Issuer.Connection,
                    $"[REPORTING] " + CedModMain.config.StaffReportMessage,
                    "green");
                return;
            }
            if (ev.Reason.IsEmpty())
            {
                ev.Issuer.GameObject.GetComponent<GameConsoleTransmission>().SendToClient(ev.Issuer.Connection,
                    $"[REPORTING] You have to enter a reason",
                    "green");
                return;
            }
            
            reported.Add(ev.Target.ReferenceHub, ev.Issuer.ReferenceHub);
            Timing.RunCoroutine(removefromlist(ev.Target.ReferenceHub));
            
            sendDI();
        }

        public IEnumerator<float> removefromlist(ReferenceHub target)
        {
            yield return Timing.WaitForSeconds(60f);
            reported.Remove(target);
        }
        public static void sendDI(string msg = "Default")
        {
            if (msg == "Default") msg = CedModMain.config.ReportContent;
            try
            {
                DiscordIntegration_Plugin.ProcessSTT.SendData(msg,
                    CedModMain.config.ReportChannel);
            }
            catch (Exception e)
            {
                Initializer.Logger.Debug("DIReport", $"DI is not installed{e.Message}");
            }
        }

        /// <inheritdoc cref="Events.Handlers.Server.OnEndingRound(EndingRoundEventArgs)"/>
        public void OnRoundRestart()
        {
            FriendlyFireAutoban.Teamkillers.Clear();
            FriendlyFireAutoban.Victims.Clear();
            Timing.KillCoroutines("LightsOut");
        }

        public void OnSendingRemoteAdmin(SendingRemoteAdminCommandEventArgs ev)
        {
            BanSystem.HandleRACommand(ev);
        }

        IEnumerator<float> PlayerStatsRound(RoundEndedEventArgs ev)
        {
            // foreach (Exiled.API.Features.Player ply in Exiled.API.Features.Player.List)
            // {
            //     Task.Factory.StartNew(() =>
            //     {
            //         API.APIRequest("playerstats/addstat.php",
            //             $"?rounds=1&kills=0&deaths=0&teamkills=0&alias={API.GetAlias()}&id={ply.UserId}&dnt={Convert.ToInt32(ply.ReferenceHub.serverRoles.DoNotTrack)}&ip={ply.IPAddress}&username={ply.Nickname}");
            //     });
            // }

            yield return 0;
        }
    }
}