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
        public class WorkStation
        {
            public WorkStation(Vector3 pos, Vector3 rot, Vector3 size)
            {
                Pos = pos;
                Rot = rot;
                Size = size;
            }
            public Vector3 Pos;
            public Vector3 Rot;
            public Vector3 Size;
        }
        public static List<WorkStation> WorkBenchposses()
        {
            return new List<WorkStation>
            {
                new WorkStation(new Vector3(174.7735f, 986.9232f, 126.773f), new Vector3(0,0,0), new Vector3(18,18,1)),
                new WorkStation(new Vector3(174.7735f, 986.9232f, 126.773f), new Vector3(0,0,180), new Vector3(18,18,1)),
                new WorkStation(new Vector3(191.3532f, 986.9232f, 112.6005f), new Vector3(0,90,0), new Vector3(18,18,1)),
                new WorkStation(new Vector3(191.3532f, 986.9232f, 112.6005f), new Vector3(0,90,180), new Vector3(18,18,1)),
                new WorkStation(new Vector3(163.9842f, 986.9232f, 112.6005f), new Vector3(0,90,0), new Vector3(18,18,1)),
                new WorkStation(new Vector3(163.9842f, 986.9232f, 112.6005f), new Vector3(0,90,180), new Vector3(18,18,1)),
                new WorkStation(new Vector3(174.7735f, 986.9232f, 98.24794f), new Vector3(0,0,0), new Vector3(18,18,1)),
                new WorkStation(new Vector3(174.7735f, 986.9232f, 98.24794f), new Vector3(0,0,180), new Vector3(18,18,1)),
                new WorkStation(new Vector3(178.7831f, 999.4953f, 113.6765f), new Vector3(0,0,0), new Vector3(10,1,20))
            };
        }
        public void OnWaitingForPlayers()
        {
            if (ConfigFile.ServerConfig.GetBool("cm_customloadingscreen", true))
            {
                GameObject.Find("StartRound").transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                foreach (WorkStation wk in WorkBenchposses())
                {
                    GameObject bench =
                        Object.Instantiate(
                            NetworkManager.singleton.spawnPrefabs.Find(p => p.gameObject.name == "Work Station"));
                    Offset offset = new Offset();
                    offset.position = wk.Pos;
                    offset.rotation = wk.Rot;
                    offset.scale = Vector3.one;
                    bench.gameObject.transform.localScale = wk.Size;
                    Initializer.Logger.Debug("PreRoudMiniGame",
                        $"Spawning workstations at {wk.Pos.ToString()} with a rotation of {wk.Rot} and a size of {wk.Size}");
                    NetworkServer.Spawn(bench);
                    bench.GetComponent<global::WorkStation>().Networkposition = offset;
                    bench.AddComponent<WorkStationUpgrader>();
                }
            }
        }

        public void onRoundStart()
        {
        }
        Dictionary<ReferenceHub, ReferenceHub> reported = new Dictionary<ReferenceHub, ReferenceHub>();
        public void OnReport(LocalReportingEventArgs ev)
        {
            if (reported.ContainsKey(ev.Target.ReferenceHub))
            {
                ev.IsAllowed = false;
                ev.Issuer.GameObject.GetComponent<GameConsoleTransmission>().SendToClient(ev.Issuer.Connection,
                    $"[REPORTING] {ev.Target.Nickname} ({ev.Target.UserId}) has already been reported by {Exiled.API.Features.Player.Get(reported[ev.Target.ReferenceHub]).Nickname}",
                    "green");
                return;
            }
            if (ev.Reason.IsEmpty())
            {
                ev.Issuer.GameObject.GetComponent<GameConsoleTransmission>().SendToClient(ev.Issuer.Connection,
                    $"You have to enter a reason",
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
        private void sendDI()
        {
            try
            {
                DiscordIntegration_Plugin.ProcessSTT.SendData(CedModMain.config.ReportContent,
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
            Timing.KillCoroutines("LightsOut");
        }

        public void OnSendingRemoteAdmin(SendingRemoteAdminCommandEventArgs ev)
        {
            BanSystem.HandleRACommand(ev);
        }

        IEnumerator<float> PlayerStatsRound(RoundEndedEventArgs ev)
        {
            foreach (Exiled.API.Features.Player ply in Exiled.API.Features.Player.List)
            {
                Task.Factory.StartNew(() =>
                {
                    API.APIRequest("playerstats/addstat.php",
                        $"?rounds=1&kills=0&deaths=0&teamkills=0&alias={API.GetAlias()}&id={ply.UserId}&dnt={Convert.ToInt32(ply.ReferenceHub.serverRoles.DoNotTrack)}&ip={ply.IPAddress}&username={ply.Nickname}");
                });
            }

            yield return 0;
        }
    }
}