using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdminToys;
using InventorySystem.Items;
using MEC;
using PlayerRoles;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using UnityEngine;

namespace CedMod.Addons.AdminSitSystem
{
    public class AdminSitHandler: MonoBehaviour
    {
        public static AdminSitHandler Singleton { get; set; }
        public List<AdminSit> Sits { get; set; } = new List<AdminSit>();
        public Dictionary<string, string> LeftPlayers = new Dictionary<string, string>();

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

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        public void WaitingForPlayers(WaitingForPlayersEvent ev)
        {
            Singleton.Sits.Clear();
            foreach (var loc in Singleton.AdminSitLocations)
            {
                loc.InUse = false;
            }
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        public void OnPlayerJoin(PlayerJoinedEvent ev)
        {
            var sit = Singleton.Sits.FirstOrDefault(s => s.Players.Any(s => s.UserId == ev.Player.UserId));
            if (sit != null)
            {
                Timing.CallDelayed(0.1f, () =>
                {
                    ev.Player.SetRole(RoleTypeId.Tutorial, RoleChangeReason.RemoteAdmin);
                    Timing.CallDelayed(0.1f, () => {
                    {
                        ev.Player.Position = sit.Location.SpawnPosition;
                    }});
                });
                return;
            }

            if (Singleton.LeftPlayers.TryGetValue(ev.Player.UserId, out var leftPlayer))
            {
                sit = Singleton.Sits.FirstOrDefault(s => s.Id == leftPlayer);
                if (sit == null)
                {
                    var loc = AdminSitHandler.Singleton.AdminSitLocations.FirstOrDefault(s => !s.InUse);
                    loc.InUse = true;
                    sit = new AdminSit()
                    {
                        Id = leftPlayer,
                        AssociatedReportId = 0,
                        InitialDuration = 0,
                        InitialReason = "",
                        Location = loc,
                        SpawnedObjects = new List<AdminToyBase>(),
                        Players = new List<AdminSitPlayer>()
                    };
                    AdminSitHandler.Singleton.Sits.Add(sit);
                }
                
                Timing.CallDelayed(0.1f, () =>
                {
                    sit.Players.Add(new AdminSitPlayer()
                    {
                        Player = CedModPlayer.Get(ev.Player.ReferenceHub),
                        PlayerType = AdminSitPlayerType.Handler,
                        UserId = ev.Player.UserId,
                        Ammo = new Dictionary<ItemType, ushort>(ev.Player.ReferenceHub.inventory.UserInventory.ReserveAmmo),
                        Health = ev.Player.Health,
                        Items = new Dictionary<ushort, ItemBase>(ev.Player.ReferenceHub.inventory.UserInventory.Items),
                        Position = ev.Player.Position,
                        Role = ev.Player.Role,
                    });
                    ev.Player.SetRole(RoleTypeId.Tutorial, RoleChangeReason.RemoteAdmin);
                    Timing.CallDelayed(0.1f, () => {
                    {
                        ev.Player.Position = sit.Location.SpawnPosition;
                    }});
                });
            }
        }

        [PluginEvent(ServerEventType.PlayerLeft)]
        public void OnPlayerLeft(PlayerLeftEvent ev)
        {
            var sit = Singleton.Sits.FirstOrDefault(s => s.Players.Any(s => s.UserId == ev.Player.UserId));
            if (sit != null)
            {
                Singleton.LeftPlayers.TryAdd(ev.Player.UserId, sit.Id);
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