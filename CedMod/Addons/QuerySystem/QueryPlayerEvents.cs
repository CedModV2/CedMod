using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.WS;
using InventorySystem.Disarming;
using InventorySystem.Items;
using MapGeneration;
using Newtonsoft.Json;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using UnityEngine;

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
        public void OnElevatorInteraction(CedModPlayer player)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", player.UserId},
                    {"UserName", player.Nickname},
                    {"Type", nameof(OnElevatorInteraction)},
                    {"Message", player.Nickname + " - " + player.UserId + " has interacted with elevator."}
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
        public void OnPlayerHurt(CedModPlayer player, CedModPlayer target, DamageHandlerBase damageHandler)
        {
            if (player == null)
                return;

            if (target != null)
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

        [PluginEvent(ServerEventType.PlayerDeath)]
        public void OnPlayerDeath(CedModPlayer player, CedModPlayer target, DamageHandlerBase damageHandler)
        {
            if (player == null || target == null)
                return;
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug("plrdeath");
            if (FriendlyFireAutoban.IsTeamKill(player, target, damageHandler))
            {
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug("istk");
                TeamkillData data = new TeamkillData();
                data.PlayersOnScene.Add(new UsersOnScene()
                {
                    CurrentHealth = player.Health,
                    Distance = 0,
                    Position = player.Position.ToString(),
                    RoleType = player.Role,
                    UserId = player.UserId,
                    Killer = true,
                    Room = "" //todo
                });

                data.PlayersOnScene.Add(new UsersOnScene()
                {
                    CurrentHealth = player.Health,
                    Distance = Vector3.Distance(player.Position, target.Position),
                    Position = target.Position.ToString(),
                    RoleType = target.Role,
                    UserId = target.UserId,
                    Victim = true,
                    Room = "" //todo
                });
                
                //todo yes
                // data.RoomsInvolved.Add(new RoomsInvolved()
                // {
                //     Position = ev.Killer.CurrentRoom.Position.ToString(),
                //     Rotation = ev.Killer.CurrentRoom.transform.rotation.ToString(),
                //     RoomType = ev.Killer.CurrentRoom.Type
                // });

                // if (!data.RoomsInvolved.Any(s => s.RoomType == ev.Target.CurrentRoom.Type))
                // {
                //     data.RoomsInvolved.Add(new RoomsInvolved()
                //     {
                //         Position = ev.Target.CurrentRoom.Position.ToString(),
                //         Rotation = ev.Target.CurrentRoom.transform.rotation.ToString(),
                //         RoomType = ev.Target.CurrentRoom.Type
                //     });
                // }
                
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug("resolving on scene players");
                foreach (var bystanders in Player.GetPlayers<CedModPlayer>())
                {
                    if (bystanders.Role == RoleTypeId.Spectator || player.Role == RoleTypeId.None)
                        continue;

                    if (Vector3.Distance(player.Position, player.Position) <= 60 && data.PlayersOnScene.All(plrs => plrs.UserId != player.UserId))
                    {
                        // if (!data.RoomsInvolved.Any(s => s.RoomType == player.CurrentRoom.Type))
                        // {
                        //     data.RoomsInvolved.Add(new RoomsInvolved()
                        //     {
                        //         Position = player.CurrentRoom.Position.ToString(),
                        //         Rotation = player.CurrentRoom.transform.rotation.ToString(),
                        //         RoomType = player.CurrentRoom.Type
                        //     });
                        // }
                        
                        data.PlayersOnScene.Add(new UsersOnScene()
                        {
                            CurrentHealth = player.Health,
                            Distance = Vector3.Distance(player.Position, player.Position),
                            Position = player.Position.ToString(),
                            RoleType = player.Role,
                            UserId = player.UserId,
                            Bystander = true,
                            Room = "" //todo
                        });
                    }
                }
                

                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug("sending WR");
                Task.Factory.StartNew(() =>
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
                            var response = client
                                .PostAsync(
                                    $"https://{QuerySystem.CurrentMaster}/Api/v3/Teamkill/{QuerySystem.QuerySystemKey}?v=2",
                                    new StringContent(JsonConvert.SerializeObject(data), Encoding.Default,
                                        "application/json")).Result;
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug(response.Content.ReadAsStringAsync().Result);
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
        public void OnGrenadeThrown(CedModPlayer player, ItemBase item)
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

        [PluginEvent(ServerEventType.PlayerUseItem)]
        public void OnUsedItem(CedModPlayer player, ItemBase item)
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
                    {"Type", nameof(OnPlayerFreed)},
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