using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using UnityEngine;

namespace CedMod.Addons.AdminSitSystem
{
    public class AdminSitHandler: MonoBehaviour
    {
        public static AdminSitHandler Singleton { get; set; }
        public List<AdminSit> Sits { get; set; } = new List<AdminSit>();
        public List<AdminSitLocation> AdminSitLocations = new List<AdminSitLocation>()
        {
            new AdminSitLocation()
            {
                InUse = false,
                SpawnPosition = new Vector3(130.847f, 994.308f, 21.027f)
            },
            new AdminSitLocation()
            {
                InUse = false,
                SpawnPosition = new Vector3(38.558f, 1014.676f, -32.289f)
            },
            new AdminSitLocation()
            {
                InUse = false,
                SpawnPosition = new Vector3(15.156f, 1014.676f, -32.289f)
            },
            new AdminSitLocation()
            {
                InUse = false,
                SpawnPosition = new Vector3(161.468f, 1019.952f, -13.727f)
            }
        };
        public List<string> BannedUserIds = new List<string>();

        public void Start()
        {
            Singleton = this;
        }

        [PluginEvent(ServerEventType.RoundRestart)]
        public void RoundRestart()
        {
            Singleton.Sits.Clear();
            foreach (var loc in Singleton.AdminSitLocations)
            {
                loc.InUse = false;
            }
        }

        public void Update()
        {
            foreach (var sit in Sits)
            {
                foreach (var plr in sit.Players)
                {
                    if (plr.Player == null)
                        plr.Player = CedModPlayer.Get(plr.UserId);

                    if (RoundSummary.RoundInProgress() && !RoundSummary.singleton._roundEnded && RoundSummary.roundTime >= 10 && plr.Player == null && plr.PlayerType == AdminSitPlayerType.Offender && sit.Type == AdminSitType.BanOffenderOnLeave)
                    {
                        if (!BannedUserIds.Contains(plr.UserId))
                        {
                            BannedUserIds.Add(plr.UserId);
                            new Thread(() =>
                            {
                                lock (BanSystem.Banlock)
                                {
                                    API.BanId(plr.UserId, sit.InitialDuration, sit.Players.FirstOrDefault(s => s.PlayerType == AdminSitPlayerType.Handler).UserId, sit.InitialDuration + "Leaving an active Admin sit").Wait();
                                }
                            }).Start();
                        }
                    }
                }
            }
        }
    }
}