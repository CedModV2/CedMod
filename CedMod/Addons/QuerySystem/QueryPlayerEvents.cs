using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.WS;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Newtonsoft.Json;
using UnityEngine;

namespace CedMod.Addons.QuerySystem
{
    public class UsersOnScene
    {
        public string UserId;
        public string Position;
        public float Distance;
        public RoleType RoleType;
        public float CurrentHealth;
        public bool Killer;
        public bool Victim;
        public bool Bystander;
        public string Room;
    }

    public class QueryPlayerEvents
    {
        public void OnPlayerLeave(LeftEventArgs ev)
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

        public void OnElevatorInteraction(InteractingElevatorEventArgs ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnElevatorInteraction)},
                    {"Message", ev.Player.Nickname + " - " + ev.Player.UserId + " has interacted with elevator."}
                }
            });
        }

        public void OnPocketEnter(EnteringPocketDimensionEventArgs ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
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
                                ev.Player.Role,
                                Misc.ToHex(ev.Player.Role.GetColor()),
                                ev.Player.Role
                            })
                    }
                }
            });
        }

        public void OnPocketEscape(EscapingPocketDimensionEventArgs ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
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
                                ev.Player.Role,
                                Misc.ToHex(ev.Player.Role.GetColor()),
                                ev.Player.Role
                            })
                    }
                }
            });
        }

        public void On079Tesla(InteractingTeslaEventArgs ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
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
                                Misc.ToHex(ev.Player.Role.GetColor()),
                                ev.Player.Role
                            })
                    }
                }
            });
        }

        public void OnPlayerDeath(DyingEventArgs ev)
        {
            if (ev.Killer == null || ev.Target == null)
                return;
            Log.Debug("plrdeath", CedModMain.Singleton.Config.QuerySystem.Debug);
            if (FriendlyFireAutoban.IsTeamKill(ev))
            {
                Log.Debug("istk", CedModMain.Singleton.Config.QuerySystem.Debug);
                List<UsersOnScene> playersOnScene = new List<UsersOnScene>();
                playersOnScene.Add(new UsersOnScene()
                {
                    CurrentHealth = ev.Killer.Health,
                    Distance = 0,
                    Position = ev.Killer.Position.ToString(),
                    RoleType = ev.Killer.Role,
                    UserId = ev.Killer.UserId,
                    Killer = true,
                    Room = ev.Killer.CurrentRoom.Name
                });

                playersOnScene.Add(new UsersOnScene()
                {
                    CurrentHealth = ev.Target.Health,
                    Distance = Vector3.Distance(ev.Killer.Position, ev.Target.Position),
                    Position = ev.Target.Position.ToString(),
                    RoleType = ev.Target.Role,
                    UserId = ev.Target.UserId,
                    Victim = true,
                    Room = ev.Target.CurrentRoom.Name
                });
                Log.Debug("resolving on scene players", CedModMain.Singleton.Config.QuerySystem.Debug);
                foreach (var player in Player.List)
                {
                    if (player.Role == RoleType.Spectator || player.Role == RoleType.None)
                        continue;

                    if (Vector3.Distance(ev.Killer.Position, player.Position) <= 20 &&
                        playersOnScene.All(plrs => plrs.UserId != player.UserId))
                    {
                        playersOnScene.Add(new UsersOnScene()
                        {
                            CurrentHealth = player.Health,
                            Distance = Vector3.Distance(ev.Killer.Position, player.Position),
                            Position = player.Position.ToString(),
                            RoleType = player.Role,
                            UserId = player.UserId,
                            Bystander = true,
                            Room = player.CurrentRoom.Name
                        });
                    }
                }

                Log.Debug("sending WR", CedModMain.Singleton.Config.QuerySystem.Debug);
                Task.Factory.StartNew(() =>
                {
                    Log.Debug("Thread send", CedModMain.Singleton.Config.QuerySystem.Debug);
                    if (CedModMain.Singleton.Config.QuerySystem.SecurityKey == "None")
                        return;
                    Log.Debug("sending WR", CedModMain.Singleton.Config.QuerySystem.Debug);
                    HttpClient client = new HttpClient();
                    try
                    {
                        var response = client
                            .PostAsync(
                                $"https://{QuerySystem.PanelUrl}/Api/Teamkill/{CedModMain.Singleton.Config.QuerySystem.SecurityKey}",
                                new StringContent(JsonConvert.SerializeObject(playersOnScene), Encoding.Default,
                                    "application/json")).Result;
                        Log.Debug(response.Content.ReadAsStringAsync().Result, CedModMain.Singleton.Config.QuerySystem.Debug);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                });

                WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"UserId", ev.Target.UserId},
                        {"UserName", ev.Target.Nickname},
                        {"Class", ev.Target.Role.ToString()},
                        {"AttackerClass", ev.Killer.Role.ToString()},
                        {"AttackerId", ev.Killer.UserId},
                        {"AttackerName", ev.Killer.Nickname},
                        {"Weapon", ev.Handler.Type.ToString()},
                        {"Type", nameof(OnPlayerDeath)},
                        {
                            "Message", string.Format(
                                "Teamkill ⚠: {0} - {1} (<color={2}>{3}</color>) killed {4} - {5} (<color={6}>{7}</color>) with {8}.",
                                new object[]
                                {
                                    ev.Killer.Nickname,
                                    ev.Killer.UserId,
                                    Misc.ToHex(ev.Killer.Role.GetColor()),
                                    ev.Killer.Role,
                                    ev.Target.Nickname,
                                    ev.Target.UserId,
                                    Misc.ToHex(ev.Target.Role.GetColor()),
                                    ev.Target.Role,
                                    ev.Handler.Type.ToString()
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
                        {"AttackerClass", ev.Killer.Role.ToString()},
                        {"AttackerId", ev.Killer.UserId},
                        {"AttackerName", ev.Killer.Nickname},
                        {"Weapon", ev.Handler.Type.ToString()},
                        {"Type", nameof(OnPlayerDeath)},
                        {
                            "Message", string.Format(
                                "{0} - {1} (<color={2}>{3}</color>) killed {4} - {5} (<color={6}>{7}</color>) with {8}.",
                                new object[]
                                {
                                    ev.Killer.Nickname,
                                    ev.Killer.UserId,
                                    Misc.ToHex(ev.Killer.Role.GetColor()),
                                    ev.Killer.Role,
                                    ev.Target.Nickname,
                                    ev.Target.UserId,
                                    Misc.ToHex(ev.Target.Role.GetColor()),
                                    ev.Target.Role,
                                    ev.Handler.Type.ToString()
                                })
                        }
                    }
                });
            }
        }

        public void OnGrenadeThrown(ThrowingItemEventArgs ev)
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
                            "{0} - {1} (<color={2}>{3}</color>) threw a grenade.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.Role.GetColor()),
                                ev.Player.Role
                            })
                    }
                }
            });
        }

        public void OnUsedItem(UsedItemEventArgs ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"ItemType", ev.Item.Type.ToString()},
                    {"Type", nameof(OnUsedItem)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) Used a {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.Role.GetColor()),
                                ev.Player.Role,
                                ev.Item
                            })
                    }
                }
            });
        }

        public void OnSetClass(ChangingRoleEventArgs ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Reason", ev.Reason.ToString()},
                    {"Type", nameof(OnSetClass)},
                    {
                        "Message", string.Format("{0} - {1}'s role has been changed to <color={2}>{3}</color>.",
                            new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.NewRole.GetColor()),
                                ev.NewRole
                            })
                    }
                }
            });

            if (LevelerStore.TrackingEnabled && LevelerStore.InitialPlayerRoles.ContainsKey(ev.Player))
            {
                if (ev.Reason != SpawnReason.Escaped)
                    LevelerStore.InitialPlayerRoles.Remove(ev.Player);
                else
                    LevelerStore.InitialPlayerRoles[ev.Player] = ev.NewRole;
            }
        }

        public void OnPlayerJoin(VerifiedEventArgs ev)
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
                    {"PlayerId", ev.Player.Id.ToString()},
                    {"Type", nameof(OnPlayerJoin)},
                    {
                        "Message", string.Format("({0}) {1} - {2} joined the game.",
                            ev.Player.Id, ev.Player.Nickname, ev.Player.UserId)
                    }
                }
            });
        }

        public void OnPlayerFreed(RemovingHandcuffsEventArgs ev)
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
                                Misc.ToHex(ev.Target.Role.GetColor()),
                                ev.Target.Role,
                                ev.Cuffer.Nickname,
                                ev.Cuffer.UserId,
                                Misc.ToHex(ev.Cuffer.Role.GetColor()),
                                ev.Cuffer.Role
                            })
                    }
                }
            });
        }

        public void OnPlayerHandcuffed(HandcuffingEventArgs ev)
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
                                Misc.ToHex(ev.Target.Role.GetColor()),
                                ev.Target.Role,
                                ev.Cuffer.Nickname,
                                ev.Cuffer.UserId,
                                Misc.ToHex(ev.Cuffer.Role.GetColor()),
                                ev.Cuffer.Role
                            })
                    }
                }
            });
        }

        public void OnEscape(EscapingEventArgs ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Time", Round.ElapsedTime.ToString()},
                    {"Type", nameof(OnPlayerHandcuffed)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has escaped, Cuffed: {4}",
                            new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.RoleColor),
                                ev.Player.Role,
                                ev.Player.IsCuffed
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
                        {"Cuffed", ev.Player.IsCuffed.ToString()}
                    }
                });

                if (ev.Player.Cuffer != null)
                {
                    WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
                    {
                        Recipient = "PANEL",
                        Data = new Dictionary<string, string>()
                        {
                            {"Message", "GRANTEXP"},
                            {"GrantType", "AssistEscapeCuff"},
                            {"UserId", ev.Player.Cuffer.UserId},
                            {"RoleType", ev.Player.Cuffer.Role.ToString()},
                        }
                    });
                }
            }
        }
    }
}