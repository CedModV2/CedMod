using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.Addons.Events;
using CedMod.Addons.QuerySystem.WS;
using Interactables.Interobjects;
using InventorySystem.Disarming;
using InventorySystem.Items;
using InventorySystem.Items.ThrowableProjectiles;
using InventorySystem.Items.Usables;
using MapGeneration;
using Newtonsoft.Json;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using UnityEngine;
using EventManager = PluginAPI.Events.EventManager;

namespace CedMod.Addons.QuerySystem
{
    public class UsersOnScene
    {
        public string UserId;
        public string Position;
        public float Distance;
        public RoleTypeId RoleType;
        public float CurrentHealth;
        public bool Killer;
        public bool Victim;
        public bool Bystander;
        public string Room;
    }
    
    public class RoomsInvolved
    {
        public string Position;
        public string RoomType; //todo roomtype??
        public string Rotation;
    }
    
    public class TeamkillData
    {
        public List<UsersOnScene> PlayersOnScene = new List<UsersOnScene>();
        public List<RoomsInvolved> RoomsInvolved = new List<RoomsInvolved>();
    }

    public class QueryPlayerEvents
    {
        [PluginEvent(ServerEventType.PlayerLeft)]
        public void OnPlayerLeave(PlayerLeftEvent ev)
        {
            ThreadDispatcher.SendHeartbeatMessage(true);
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerLeave)},
                    {"Message", ev.Player.Nickname + " - " + ev.Player.UserId + " has left the server."}
                }
            });
        }

        [PluginEvent(ServerEventType.PlayerInteractElevator)]
        public void OnElevatorInteraction(PlayerInteractElevatorEvent ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnElevatorInteraction)},
                    {"Message", ev.Player.Nickname + " - " + ev.Player.UserId + $" has interacted with elevator {ev.Elevator.AssignedGroup}."}
                }
            });
        }

        //todo implement when event present
        public void OnPocketEnter()
        {
            /*WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPocketEnter)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has entered the pocket dimension.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.Role.Type,
                                Misc.ToHex(ev.Player.Role.Color),
                                ev.Player.Role
                            })
                    }
                }
            });*/
        }

        //todo implement when event present
        public void OnPocketEscape()
        {
            /*WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPocketEscape)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has escaped the pocket dimension.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.Role.Type,
                                Misc.ToHex(ev.Player.Role.Color),
                                ev.Player.Role
                            })
                    }
                }
            });*/
        }
        
        //todo implement when event present
        public void On079Tesla()
        {
            /*WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(On079Tesla)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has activated the tesla as 079.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.Role.Color),
                                ev.Player.Role
                            })
                    }
                }
            });*/
        }
        
        [PluginEvent(ServerEventType.PlayerDamage)]
        public void OnPlayerHurt(PlayerDamageEvent ev)
        {
            ev = new PlayerDamageEvent(ev.Target == null ? null : ev.Target.ReferenceHub, ev.Player == null ? null : ev.Player.ReferenceHub, ev.DamageHandler); //todo remove once nwapi fixed
            if (ev.Target == null)
                return;

            RoomIdentifier killerRoom = RoomIdUtils.RoomAtPosition(ev.Target.Position);
            if (ev.Player != null)
            {
                WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"UserId", ev.Target.UserId},
                        {"UserName", ev.Target.Nickname},
                        {"Class", ev.Target.Role.ToString()},
                        {"AttackerClass", ev.Player.Role.ToString()},
                        {"AttackerId", ev.Player.UserId},
                        {"AttackerName", ev.Player.Nickname},
                        {"Weapon", ev.DamageHandler.ToString()},
                        {"Type", nameof(OnPlayerHurt)},
                        {
                            "Message", string.Format(
                                "{0} - {1} (<color={2}>{3}</color>) hurt {4} - {5} (<color={6}>{7}</color>) with {8} in {9}.",
                                new object[]
                                {
                                    ev.Player.Nickname,
                                    ev.Player.UserId,
                                    Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                    ev.Player.Role,
                                    ev.Target.Nickname,
                                    ev.Target.UserId,
                                    Misc.ToHex(ev.Target.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                    ev.Target.Role,
                                    ev.DamageHandler.ToString(),
                                    killerRoom == null ? "Unknown" : killerRoom.Zone.ToString()
                                })
                        }
                    }
                });
            }
            else
            {
                WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"UserId", ev.Target.UserId},
                        {"UserName", ev.Target.Nickname},
                        {"Class", ev.Target.Role.ToString()},
                        {"Weapon", ev.DamageHandler.ToString()},
                        {"Type", nameof(OnPlayerHurt)},
                        {
                            "Message", string.Format(
                                "Server - () hurt {0} - {1} (<color={2}>{3}</color>) with {4} in {5}.",
                                new object[]
                                {
                                    ev.Target.Nickname,
                                    ev.Target.UserId,
                                    Misc.ToHex(ev.Target.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                    ev.Target.Role,
                                    ev.DamageHandler.ToString(),
                                    killerRoom == null ? "Unknown" : killerRoom.Zone.ToString()
                                })
                        }
                    }
                });
            }
        }

        [PluginEvent(ServerEventType.PlayerDying)]
        public void OnPlayerDeath(PlayerDyingEvent ev)
        {
            if (ev.Player == null || ev.Attacker == null)
                return;
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug("plrdeath");
            if (FriendlyFireAutoban.IsTeamKill(CedModPlayer.Get(ev.Player.ReferenceHub), CedModPlayer.Get(ev.Attacker.ReferenceHub), ev.DamageHandler))
            {
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug("istk");
                TeamkillData data = new TeamkillData();
                RoomIdentifier killerRoom = RoomIdUtils.RoomAtPosition(ev.Attacker.Position);
                RoomIdentifier targetRoom = RoomIdUtils.RoomAtPosition(ev.Player.Position);
                data.PlayersOnScene.Add(new UsersOnScene()
                {
                    CurrentHealth = ev.Attacker.Health,
                    Distance = 0,
                    Position = ev.Attacker.Position.ToString(),
                    RoleType = ev.Attacker.Role,
                    UserId = ev.Attacker.UserId,
                    Killer = true,
                    Room = killerRoom.name
                });

                data.PlayersOnScene.Add(new UsersOnScene()
                {
                    CurrentHealth = ev.Player.Health,
                    Distance = Vector3.Distance(ev.Player.Position, ev.Player.Position),
                    Position = ev.Player.Position.ToString(),
                    RoleType = ev.Player.Role,
                    UserId = ev.Player.UserId,
                    Victim = true,
                    Room = targetRoom.name
                });
                
                 data.RoomsInvolved.Add(new RoomsInvolved()
                 {
                     Position = killerRoom.transform.position.ToString(),
                     Rotation = killerRoom.transform.rotation.ToString(),
                     RoomType = killerRoom.Name.ToString()
                 });

                 if (!data.RoomsInvolved.Any(s => s.RoomType == targetRoom.Name.ToString() && s.Position == targetRoom.transform.position.ToString()))
                 {
                     data.RoomsInvolved.Add(new RoomsInvolved()
                     {
                         Position = targetRoom.transform.position.ToString(),
                         Rotation = targetRoom.transform.rotation.ToString(),
                         RoomType = targetRoom.name.ToString()
                     });
                 }
                
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug("resolving on scene players");
                foreach (var bystanders in Player.GetPlayers<CedModPlayer>())
                {
                    if (bystanders.Role == RoleTypeId.Spectator || ev.Attacker.Role == RoleTypeId.Overwatch || ev.Attacker.Role == RoleTypeId.None)
                        continue;

                    var distance = Vector3.Distance(ev.Attacker.Position, bystanders.Position);
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug($"Checking distance on killer {distance} from {ev.Attacker.Nickname} to {bystanders.Nickname} {data.PlayersOnScene.All(plrs => plrs.UserId != bystanders.UserId)}");
                    if (distance <= 60 && data.PlayersOnScene.All(plrs => plrs.UserId != bystanders.UserId))
                    {
                        RoomIdentifier bystanderRoom = RoomIdUtils.RoomAtPosition(bystanders.Position);
                        if (!data.RoomsInvolved.Any(s => s.RoomType == targetRoom.Name.ToString() && s.Position == targetRoom.transform.position.ToString()))
                        {
                            data.RoomsInvolved.Add(new RoomsInvolved()
                            {
                                Position = bystanderRoom.transform.position.ToString(),
                                Rotation = bystanderRoom.transform.rotation.ToString(),
                                RoomType = bystanderRoom.name
                            });
                        }
                        
                        data.PlayersOnScene.Add(new UsersOnScene()
                        {
                            CurrentHealth = bystanders.Health,
                            Distance = Vector3.Distance(ev.Attacker.Position, bystanders.Position),
                            Position = bystanders.Position.ToString(),
                            RoleType = bystanders.Role,
                            UserId = bystanders.UserId,
                            Bystander = true,
                            Room = bystanderRoom.Name.ToString()
                        });
                    }
                }
                

                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug("sending WR");
                Task.Factory.StartNew(async () =>
                {
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug("Thread send");
                    if (QuerySystem.QuerySystemKey == "None")
                        return;
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug("sending WR");
                    using (HttpClient client = new HttpClient())
                    {
                        try
                        {
                            var response = await client.PostAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.CurrentMaster}/Api/v3/Teamkill/{QuerySystem.QuerySystemKey}?v=2", new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8,
                                        "application/json"));
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug(await response.Content.ReadAsStringAsync());
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.ToString());
                        }
                    }
                });

                WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"UserId", ev.Player.UserId},
                        {"UserName", ev.Player.Nickname},
                        {"Class", ev.Player.Role.ToString()},
                        {"AttackerClass", ev.Attacker.Role.ToString()},
                        {"AttackerId", ev.Attacker.UserId},
                        {"AttackerName", ev.Attacker.Nickname},
                        {"Weapon", ev.DamageHandler.ToString()},
                        {"Type", nameof(OnPlayerDeath)},
                        {
                            "Message", string.Format(
                                "Teamkill ⚠: {0} - {1} (<color={2}>{3}</color>) killed {4} - {5} (<color={6}>{7}</color>) with {8} in {9}.",
                                new object[]
                                {
                                    ev.Attacker.Nickname,
                                    ev.Attacker.UserId,
                                    Misc.ToHex(ev.Attacker.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                    ev.Attacker.Role,
                                    ev.Player.Nickname,
                                    ev.Player.UserId,
                                    Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                    ev.Player.Role,
                                    ev.DamageHandler.ToString(),
                                    killerRoom == null ? "Unknown" : killerRoom.Zone.ToString()
                                })
                        }
                    }
                });
            }
            else
            {
                RoomIdentifier killerRoom = RoomIdUtils.RoomAtPosition(ev.Player.Position);
                WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"UserId", ev.Player.UserId},
                        {"UserName", ev.Player.Nickname},
                        {"Class", ev.Player.Role.ToString()},
                        {"AttackerClass", ev.Attacker.Role.ToString()},
                        {"AttackerId", ev.Attacker.UserId},
                        {"AttackerName", ev.Attacker.Nickname},
                        {"Weapon", ev.DamageHandler.ToString()},
                        {"Type", nameof(OnPlayerDeath)},
                        {
                            "Message", string.Format(
                                "{0} - {1} (<color={2}>{3}</color>) killed {4} - {5} (<color={6}>{7}</color>) with {8} In {9}.",
                                new object[]
                                {
                                    ev.Attacker.Nickname,
                                    ev.Attacker.UserId,
                                    Misc.ToHex(ev.Attacker.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                    ev.Attacker.Role,
                                    ev.Player.Nickname,
                                    ev.Player.UserId,
                                    Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                    ev.Player.Role,
                                    ev.DamageHandler.ToString(),
                                    killerRoom == null ? "Unknown" : killerRoom.Zone.ToString()
                                })
                        }
                    }
                });
            }
        }

        [PluginEvent(ServerEventType.PlayerThrowItem)]
        public void OnGrenadeThrown(PlayerThrowItemEvent ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnGrenadeThrown)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) threw an item {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Item.ItemTypeId
                            })
                    }
                }
            });
        }
        
        [PluginEvent(ServerEventType.PlayerThrowProjectile)]
        public void OnThrowProjectile(PlayerThrowProjectileEvent ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Thrower.UserId},
                    {"UserName", ev.Thrower.Nickname},
                    {"Type", nameof(OnThrowProjectile)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) threw a projectile {4}.", new object[]
                            {
                                ev.Thrower.Nickname,
                                ev.Thrower.UserId,
                                Misc.ToHex(ev.Thrower.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Thrower.Role,
                                ev.Item.ItemTypeId
                            })
                    }
                }
            });
        }

        [PluginEvent(ServerEventType.PlayerUsedItem)]
        public void OnUsedItem(PlayerUsedItemEvent ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"ItemType", ev.Item.ItemTypeId.ToString()},
                    {"Type", nameof(OnUsedItem)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) used an item {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Item.ItemTypeId
                            })
                    }
                }
            });
        }

        [PluginEvent(ServerEventType.PlayerChangeRole)]
        public void OnSetClass(PlayerChangeRoleEvent ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Reason", ev.ChangeReason.ToString()},
                    {"Type", nameof(OnSetClass)},
                    {
                        "Message", string.Format("{0} - {1}'s role has been changed to {2}",
                            new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                ev.NewRole
                            })
                    }
                }
            });

            var plr = CedModPlayer.Get(ev.Player.ReferenceHub);
            if (LevelerStore.TrackingEnabled && LevelerStore.InitialPlayerRoles.ContainsKey(plr))
            {
                if (ev.ChangeReason != RoleChangeReason.Escaped)
                    LevelerStore.InitialPlayerRoles.Remove(plr);
                else
                    LevelerStore.InitialPlayerRoles[plr] = ev.NewRole;
            }
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        public void OnPlayerJoin(PlayerJoinedEvent ev)
        {
            ThreadDispatcher.SendHeartbeatMessage(true);
            if (CommandHandler.Synced.Contains(ev.Player.UserId))
            {
                if (ServerStatic.GetPermissionsHandler()._members.ContainsKey(ev.Player.UserId))
                    ServerStatic.GetPermissionsHandler()._members.Remove(ev.Player.UserId);
                ev.Player.ReferenceHub.serverRoles.RefreshPermissions();
                CommandHandler.Synced.Remove(ev.Player.UserId);
            }

            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"PlayerId", ev.Player.PlayerId.ToString()},
                    {"Type", nameof(OnPlayerJoin)},
                    {
                        "Message", string.Format("({0}) {1} - {2} joined the game.",
                            ev.Player.PlayerId, ev.Player.Nickname, ev.Player.UserId)
                    }
                }
            });
        }

        [PluginEvent(ServerEventType.PlayerRemoveHandcuffs)]
        public void OnPlayerFreed(PlayerRemoveHandcuffsEvent ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Target.UserId},
                    {"UserName", ev.Target.Nickname},
                    {"Type", nameof(OnPlayerFreed)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has been freed by {4} - {5} (<color={6}>{7}</color>).",
                            new object[]
                            {
                                ev.Target.Nickname,
                                ev.Target.UserId,
                                Misc.ToHex(ev.Target.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Target.Role,
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role
                            })
                    }
                }
            });
        }

        [PluginEvent(ServerEventType.PlayerHandcuff)]
        public void OnPlayerHandcuffed(PlayerHandcuffEvent ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Target.UserId},
                    {"UserName", ev.Target.Nickname},
                    {"Type", nameof(OnPlayerHandcuffed)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has been cuffed by {4} - {5} (<color={6}>{7}</color>).",
                            new object[]
                            {
                                ev.Target.Nickname,
                                ev.Target.UserId,
                                Misc.ToHex(ev.Target.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Target.Role,
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role
                            })
                    }
                }
            });
        }

        [PluginEvent(ServerEventType.PlayerEscape)]
        public void OnEscape(PlayerEscapeEvent ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Time", Statistics.Round.Duration.ToString()},
                    {"Type", nameof(OnEscape)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has escaped, Cuffed: {4}",
                            new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Player.ReferenceHub.inventory.IsDisarmed()
                            })
                    }
                }
            });

            if (LevelerStore.TrackingEnabled)
            {
                WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
                {
                    Recipient = "PANEL",
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", "GRANTEXP"},
                        {"GrantType", "Escape"},
                        {"UserId", ev.Player.UserId},
                        {"RoleType", ev.Player.Role.ToString()},
                        {"Cuffed", ev.Player.ReferenceHub.inventory.IsDisarmed().ToString()}
                    }
                });

                //todo
                /*if (player.Cuffer != null)
                {
                    WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
                    {
                        Recipient = "PANEL",
                        Data = new Dictionary<string, string>()
                        {
                            {"Message", "GRANTEXP"},
                            {"GrantType", "AssistEscapeCuff"},
                            {"UserId", ev.Player.Cuffer.UserId},
                            {"RoleType", ev.Player.Cuffer.Role.Type.ToString()},
                        }
                    });
                }*/
            }
        }
    }
}