using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.QuerySystem.WS;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using InventorySystem.Items.Usables;
using Newtonsoft.Json;
using UnityEngine;
using Scp207 = CustomPlayerEffects.Scp207;

namespace CedMod.QuerySystem
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

    public class PlayerEvents
    {
        public void OnPlayerLeave(LeftEventArgs ev)
        {
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
            Log.Debug("plrdeath", QuerySystem.Singleton.Config.Debug);
            if (FriendlyFireAutoban.IsTeamKill(ev))
            {
                Log.Debug("istk", QuerySystem.Singleton.Config.Debug);
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
                Log.Debug("resolving on scene players", QuerySystem.Singleton.Config.Debug);
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

                Log.Debug("sending WR", QuerySystem.Singleton.Config.Debug);
                Task.Factory.StartNew(() =>
                {
                    Log.Debug("Thread send", QuerySystem.Singleton.Config.Debug);
                    if (QuerySystem.Singleton.Config.SecurityKey == "None")
                        return;
                    Log.Debug("sending WR", QuerySystem.Singleton.Config.Debug);
                    HttpClient client = new HttpClient();
                    try
                    {
                        var response = client
                            .PostAsync(
                                $"https://{QuerySystem.PanelUrl}/Api/Teamkill/{QuerySystem.Singleton.Config.SecurityKey}",
                                new StringContent(JsonConvert.SerializeObject(playersOnScene), Encoding.Default,
                                    "application/json")).Result;
                        Log.Debug(response.Content.ReadAsStringAsync().Result, QuerySystem.Singleton.Config.Debug);
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
        }

        public void OnPlayerJoin(VerifiedEventArgs ev)
        {
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
    }
}