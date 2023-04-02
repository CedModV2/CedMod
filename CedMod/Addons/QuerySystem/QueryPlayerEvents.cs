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
        public void OnPlayerLeave(CedModPlayer player)
        {
            ThreadDispatcher.SendHeartbeatMessage(true);
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", player.UserId},
                    {"UserName", player.Nickname},
                    {"Type", nameof(OnPlayerLeave)},
                    {"Message", player.Nickname + " - " + player.UserId + " has left the server."}
                }
            });
        }

        [PluginEvent(ServerEventType.PlayerInteractElevator)]
        public void OnElevatorInteraction(CedModPlayer player, ElevatorChamber elevatorChamber)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", player.UserId},
                    {"UserName", player.Nickname},
                    {"Type", nameof(OnElevatorInteraction)},
                    {"Message", player.Nickname + " - " + player.UserId + $" has interacted with elevator {elevatorChamber.AssignedGroup}."}
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
        public void OnPlayerHurt(CedModPlayer target, CedModPlayer player, DamageHandlerBase damageHandler)
        {
            if (target == null)
                return;

            if (player != null)
            {
                WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"UserId", target.UserId},
                        {"UserName", target.Nickname},
                        {"Class", target.Role.ToString()},
                        {"AttackerClass", player.Role.ToString()},
                        {"AttackerId", player.UserId},
                        {"AttackerName", player.Nickname},
                        {"Weapon", damageHandler.ToString()},
                        {"Type", nameof(OnPlayerHurt)},
                        {
                            "Message", string.Format(
                                "{0} - {1} (<color={2}>{3}</color>) hurt {4} - {5} (<color={6}>{7}</color>) with {8}.",
                                new object[]
                                {
                                    player.Nickname,
                                    player.UserId,
                                    Misc.ToHex(player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                    player.Role,
                                    target.Nickname,
                                    target.UserId,
                                    Misc.ToHex(target.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                    target.Role,
                                    damageHandler.ToString()
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
                        {"UserId", target.UserId},
                        {"UserName", target.Nickname},
                        {"Class", target.Role.ToString()},
                        {"Weapon", damageHandler.ToString()},
                        {"Type", nameof(OnPlayerHurt)},
                        {
                            "Message", string.Format(
                                "Server - () hurt {0} - {1} (<color={2}>{3}</color>) with {4}.",
                                new object[]
                                {
                                    target.Nickname,
                                    target.UserId,
                                    Misc.ToHex(target.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                    target.Role,
                                    damageHandler.ToString()
                                })
                        }
                    }
                });
            }
        }

        [PluginEvent(ServerEventType.PlayerDying)]
        public void OnPlayerDeath(CedModPlayer target, CedModPlayer player, DamageHandlerBase damageHandler)
        {
            if (player == null || target == null)
                return;
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug("plrdeath");
            if (FriendlyFireAutoban.IsTeamKill(target, player, damageHandler))
            {
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug("istk");
                TeamkillData data = new TeamkillData();
                RoomIdentifier killerRoom = RoomIdUtils.RoomAtPosition(player.Position);
                RoomIdentifier targetRoom = RoomIdUtils.RoomAtPosition(target.Position);
                data.PlayersOnScene.Add(new UsersOnScene()
                {
                    CurrentHealth = player.Health,
                    Distance = 0,
                    Position = player.Position.ToString(),
                    RoleType = player.Role,
                    UserId = player.UserId,
                    Killer = true,
                    Room = killerRoom.name
                });

                data.PlayersOnScene.Add(new UsersOnScene()
                {
                    CurrentHealth = player.Health,
                    Distance = Vector3.Distance(player.Position, target.Position),
                    Position = target.Position.ToString(),
                    RoleType = target.Role,
                    UserId = target.UserId,
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
                    if (bystanders.Role == RoleTypeId.Spectator || player.Role == RoleTypeId.Overwatch || player.Role == RoleTypeId.None)
                        continue;

                    var distance = Vector3.Distance(player.Position, bystanders.Position);
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug($"Checking distance on killer {distance} from {player.Nickname} to {bystanders.Nickname} {data.PlayersOnScene.All(plrs => plrs.UserId != bystanders.UserId)}");
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
                            Distance = Vector3.Distance(player.Position, bystanders.Position),
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
                            var response = await client.PostAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.CurrentMaster}/Api/v3/Teamkill/{QuerySystem.QuerySystemKey}?v=2", new StringContent(JsonConvert.SerializeObject(data), Encoding.Default,
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
                        {"UserId", target.UserId},
                        {"UserName", target.Nickname},
                        {"Class", target.Role.ToString()},
                        {"AttackerClass", player.Role.ToString()},
                        {"AttackerId", player.UserId},
                        {"AttackerName", player.Nickname},
                        {"Weapon", damageHandler.ToString()},
                        {"Type", nameof(OnPlayerDeath)},
                        {
                            "Message", string.Format(
                                "Teamkill ⚠: {0} - {1} (<color={2}>{3}</color>) killed {4} - {5} (<color={6}>{7}</color>) with {8}.",
                                new object[]
                                {
                                    target.Nickname,
                                    target.UserId,
                                    Misc.ToHex(target.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                    target.Role,
                                    player.Nickname,
                                    player.UserId,
                                    Misc.ToHex(target.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                    player.Role,
                                    damageHandler.ToString()
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
                        {"UserId", target.UserId},
                        {"UserName", target.Nickname},
                        {"Class", target.Role.ToString()},
                        {"AttackerClass", player.Role.ToString()},
                        {"AttackerId", player.UserId},
                        {"AttackerName", player.Nickname},
                        {"Weapon", damageHandler.ToString()},
                        {"Type", nameof(OnPlayerDeath)},
                        {
                            "Message", string.Format(
                                "{0} - {1} (<color={2}>{3}</color>) killed {4} - {5} (<color={6}>{7}</color>) with {8}.",
                                new object[]
                                {
                                    player.Nickname,
                                    player.UserId,
                                    Misc.ToHex(player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                    player.Role,
                                    target.Nickname,
                                    target.UserId,
                                    Misc.ToHex(target.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                    target.Role,
                                    damageHandler.ToString()
                                })
                        }
                    }
                });
            }
        }

        [PluginEvent(ServerEventType.PlayerThrowItem)]
        public void OnGrenadeThrown(CedModPlayer player, ItemBase item, Rigidbody rigidbody)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", player.UserId},
                    {"UserName", player.Nickname},
                    {"Type", nameof(OnGrenadeThrown)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) threw an item {4}.", new object[]
                            {
                                player.Nickname,
                                player.UserId,
                                Misc.ToHex(player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                player.Role,
                                item.ItemTypeId
                            })
                    }
                }
            });
        }
        
        [PluginEvent(ServerEventType.PlayerThrowProjectile)]
        public void OnThrowProjectile(CedModPlayer player, ThrowableItem item, ThrowableItem.ProjectileSettings settings, bool fullForce)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", player.UserId},
                    {"UserName", player.Nickname},
                    {"Type", nameof(OnThrowProjectile)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) threw a projectile {4}.", new object[]
                            {
                                player.Nickname,
                                player.UserId,
                                Misc.ToHex(player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                player.Role,
                                item.ItemTypeId
                            })
                    }
                }
            });
        }

        [PluginEvent(ServerEventType.PlayerUseItem)]
        public void OnUsedItem(CedModPlayer player, UsableItem item)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", player.UserId},
                    {"UserName", player.Nickname},
                    {"ItemType", item.ItemTypeId.ToString()},
                    {"Type", nameof(OnUsedItem)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) threw an item {4}.", new object[]
                            {
                                player.Nickname,
                                player.UserId,
                                Misc.ToHex(player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                player.Role,
                                item.ItemTypeId
                            })
                    }
                }
            });
        }

        [PluginEvent(ServerEventType.PlayerChangeRole)]
        public void OnSetClass(CedModPlayer player, PlayerRoleBase oldrole, RoleTypeId newrole, RoleChangeReason reason)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", player.UserId},
                    {"UserName", player.Nickname},
                    {"Reason", reason.ToString()},
                    {"Type", nameof(OnSetClass)},
                    {
                        "Message", string.Format("{0} - {1}'s role has been changed to {2}",
                            new object[]
                            {
                                player.Nickname,
                                player.UserId,
                                newrole
                            })
                    }
                }
            });

            if (LevelerStore.TrackingEnabled && LevelerStore.InitialPlayerRoles.ContainsKey(player))
            {
                if (reason != RoleChangeReason.Escaped)
                    LevelerStore.InitialPlayerRoles.Remove(player);
                else
                    LevelerStore.InitialPlayerRoles[player] = newrole;
            }
        }

        [PluginEvent(ServerEventType.PlayerJoined)]
        public void OnPlayerJoin(CedModPlayer player)
        {
            ThreadDispatcher.SendHeartbeatMessage(true);
            if (CommandHandler.Synced.Contains(player.UserId))
            {
                if (ServerStatic.GetPermissionsHandler()._members.ContainsKey(player.UserId))
                    ServerStatic.GetPermissionsHandler()._members.Remove(player.UserId);
                player.ReferenceHub.serverRoles.RefreshPermissions();
                CommandHandler.Synced.Remove(player.UserId);
            }

            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", player.UserId},
                    {"UserName", player.Nickname},
                    {"PlayerId", player.PlayerId.ToString()},
                    {"Type", nameof(OnPlayerJoin)},
                    {
                        "Message", string.Format("({0}) {1} - {2} joined the game.",
                            player.PlayerId, player.Nickname, player.UserId)
                    }
                }
            });
        }

        [PluginEvent(ServerEventType.PlayerRemoveHandcuffs)]
        public void OnPlayerFreed(CedModPlayer player, CedModPlayer target)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", target.UserId},
                    {"UserName", target.Nickname},
                    {"Type", nameof(OnPlayerFreed)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has been freed by {4} - {5} (<color={6}>{7}</color>).",
                            new object[]
                            {
                                target.Nickname,
                                target.UserId,
                                Misc.ToHex(target.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                target.Role,
                                player.Nickname,
                                player.UserId,
                                Misc.ToHex(player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                player.Role
                            })
                    }
                }
            });
        }

        [PluginEvent(ServerEventType.PlayerHandcuff)]
        public void OnPlayerHandcuffed(CedModPlayer player, CedModPlayer target)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", target.UserId},
                    {"UserName", target.Nickname},
                    {"Type", nameof(OnPlayerHandcuffed)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has been cuffed by {4} - {5} (<color={6}>{7}</color>).",
                            new object[]
                            {
                                target.Nickname,
                                target.UserId,
                                Misc.ToHex(target.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                target.Role,
                                player.Nickname,
                                player.UserId,
                                Misc.ToHex(player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                player.Role
                            })
                    }
                }
            });
        }

        [PluginEvent(ServerEventType.PlayerEscape)]
        public void OnEscape(CedModPlayer player, RoleTypeId newrole)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", player.UserId},
                    {"UserName", player.Nickname},
                    {"Time", Statistics.Round.Duration.ToString()},
                    {"Type", nameof(OnEscape)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has escaped, Cuffed: {4}",
                            new object[]
                            {
                                player.Nickname,
                                player.UserId,
                                Misc.ToHex(player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                player.Role,
                                player.ReferenceHub.inventory.IsDisarmed()
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
                        {"UserId", player.UserId},
                        {"RoleType", player.Role.ToString()},
                        {"Cuffed", player.ReferenceHub.inventory.IsDisarmed().ToString()}
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