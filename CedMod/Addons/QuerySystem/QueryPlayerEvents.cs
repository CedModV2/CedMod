using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.WS;
using CedMod.Addons.Sentinal.Patches;
using CentralAuth;
using CustomPlayerEffects;
using GameCore;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Disarming;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp079Events;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using Newtonsoft.Json;
using PlayerRoles;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

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

    public class QueryPlayerEvents : CustomEventsHandler
    {
        public QueryPlayerEvents()
        {
            StatusEffectBase.OnDisabled += OnPlayerDisableEffect;
            StatusEffectBase.OnEnabled += OnPlayerReceiveEffect;
            StatusEffectBase.OnIntensityChanged += OnPlayerReceiveEffectIntensity;
        }

        public override void OnPlayerActivatedGenerator(PlayerActivatedGeneratorEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerActivatedGenerator)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has activated generator {4}", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Generator.Base.netId
                            })
                    }
                }
            });
        }

        public override void OnPlayerCancelledUsingItem(PlayerCancelledUsingItemEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerCancelledUsingItem)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has cancelled Item Usage {4}", new object[]
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

        public override void OnPlayerChangedItem(PlayerChangedItemEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerChangedItem)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has changed held item to {4}", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.NewItem == null ? "None" : ev.NewItem.Type.ToString()
                            })
                    }
                }
            });
        }

        public override void OnPlayerEnteredPocketDimension(PlayerEnteredPocketDimensionEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerEnteredPocketDimension)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has entered the pocket dimension.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role
                            })
                    }
                }
            });
        }

        public override void OnPlayerLeavingPocketDimension(PlayerLeavingPocketDimensionEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Success", ev.IsSuccessful.ToString()},
                    {"Type", nameof(OnPlayerLeavingPocketDimension)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has escaped the pocket dimension {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.IsSuccessful ? "Successfully" : "Unsuccessfully"
                            })
                    }
                }
            });
        }

        public override void OnPlayerInteractedDoor(PlayerInteractedDoorEventArgs ev)
        {
            if (ev.Door.Base == null || DoorNametagExtension.NamedDoors == null)
                return;
            
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerInteractedDoor)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has {4} door {5}", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Door.IsOpened ? "Opened" : "Closed",
                                ev.Door.Base.netId + (DoorNametagExtension.NamedDoors.Any(s => s.Value != null && s.Value.TargetDoor != null && ev.Door.Base != null && s.Key != null && s.Value.TargetDoor == ev.Door.Base) ? $" {DoorNametagExtension.NamedDoors.FirstOrDefault(s => s.Value.TargetDoor != null && ev.Door.Base != null &&  s.Value.TargetDoor == ev.Door.Base && s.Value != null && s.Key != null).Key}" : "") 
                            })
                    }
                }
            });
        }

        public override void OnPlayerInteractedGenerator(PlayerInteractedGeneratorEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerInteractedGenerator)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has interacted with generator {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Generator.Base.netId
                            })
                    }
                }
            });
        }

        public override void OnPlayerInteractedLocker(PlayerInteractedLockerEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerInteractedLocker)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has interacted with locker {4}, Chamber {5}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Locker.Base.netId,
                                ev.Locker.Chambers.ToList().IndexOf(ev.Chamber),
                            })
                    }
                }
            });
        }


        public override void OnPlayerInteractedElevator(PlayerInteractedElevatorEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerInteractedElevator)},
                    {"Message", ev.Player.Nickname + " - " + ev.Player.UserId + $" has interacted with elevator {ev.Elevator.Base.AssignedGroup}."}
                }
            });
        }

        public override void OnScp079UsedTesla(Scp079UsedTeslaEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnScp079UsedTesla)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has activated the tesla as 079.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role
                            })
                    }
                }
            });
        }

        public override void OnPlayerShootingWeapon(PlayerShootingWeaponEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerShootingWeapon)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has shot a {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Player.CurrentItem.Type
                            })
                    }
                }
            });
        }

        public override void OnPlayerChangedRole(PlayerChangedRoleEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Reason", ev.ChangeReason.ToString()},
                    {"Type", nameof(OnPlayerChangedRole)},
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

            var plr = ev.Player;
            if (LevelerStore.TrackingEnabled && LevelerStore.InitialPlayerRoles.ContainsKey(plr))
            {
                if (ev.ChangeReason != RoleChangeReason.Escaped)
                    LevelerStore.InitialPlayerRoles.Remove(plr);
                else
                    LevelerStore.InitialPlayerRoles[plr] = ev.NewRole.RoleTypeId;
            }
        }


        public IEnumerator<float> RemoveFromReportList(ReferenceHub target)
        {
            yield return Timing.WaitForSeconds(60f);
            if (target == null)
                yield break;
            CedMod.Handlers.Server.reported.Remove(target);
        }


        public override void OnPlayerClosedGenerator(PlayerClosedGeneratorEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerClosedGenerator)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has closed generator {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Generator.Base.netId
                            })
                    }
                }
            });
        }

        public override void OnPlayerHurt(PlayerHurtEventArgs ev)
        {
            if (ev.Attacker == null)
                return;

            RoomIdentifier killerRoom = null;
            RoomUtils.TryGetRoom(ev.Attacker.Position, out killerRoom);
            if (ev.Player != null)
            {
                WebSocketSystem.Enqueue(new QueryCommand()
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
                        {"Type", nameof(OnPlayerHurt)},
                        {
                            "Message", string.Format(
                                "{0} - {1} (<color={2}>{3}</color>) hurt {4} - {5} (<color={6}>{7}</color>) with {8} in {9}.",
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
                WebSocketSystem.Enqueue(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"UserId", ev.Player.UserId},
                        {"UserName", ev.Player.Nickname},
                        {"Class", ev.Player.Role.ToString()},
                        {"Weapon", ev.DamageHandler.ToString()},
                        {"Type", nameof(OnPlayerHurt)},
                        {
                            "Message", string.Format(
                                "Server - () hurt {0} - {1} (<color={2}>{3}</color>) with {4} in {5}.",
                                new object[]
                                {
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

        public override void OnPlayerDeactivatedGenerator(PlayerDeactivatedGeneratorEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerDeactivatedGenerator)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has deactivated generator {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Generator.Base.netId
                            })
                    }
                }
            });
        }

        public override void OnPlayerDying(PlayerDyingEventArgs ev)
        {
            //some actual autism to prevent doing stuff for a cancelled event as onplayerdeath does not preserve the original class
            if (ev.Player == null || !ev.IsAllowed)
                return;
            
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Logger.Debug("plrdeath");
            if (ev.Attacker != null && FriendlyFireAutoban.IsTeamKill(ev.Player, ev.Attacker, ev.DamageHandler))
            {
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Logger.Debug("istk");
                TeamkillData data = new TeamkillData();
                RoomIdentifier killerRoom = null;
                RoomIdentifier targetRoom = null;
                RoomUtils.TryGetRoom(ev.Attacker.Position, out killerRoom);
                RoomUtils.TryGetRoom(ev.Player.Position, out targetRoom);
                data.PlayersOnScene.Add(new UsersOnScene()
                {
                    CurrentHealth = ev.Attacker.Health,
                    Distance = 0,
                    Position = ev.Attacker.Position.ToString(),
                    RoleType = ev.Attacker.Role,
                    UserId = ev.Attacker.UserId,
                    Killer = true,
                    Room = killerRoom == null ? "Unknown" : killerRoom.Zone.ToString()
                });

                data.PlayersOnScene.Add(new UsersOnScene()
                {
                    CurrentHealth = ev.Player.Health,
                    Distance = Vector3.Distance(ev.Player.Position, ev.Player.Position),
                    Position = ev.Player.Position.ToString(),
                    RoleType = ev.Player.Role,
                    UserId = ev.Player.UserId,
                    Victim = true,
                    Room = targetRoom == null ? "Unknown" : targetRoom.Zone.ToString()
                });

                if (targetRoom != null && !data.RoomsInvolved.Any(s => s.RoomType == targetRoom.Name.ToString() && s.Position == targetRoom.transform.position.ToString()))
                {
                    data.RoomsInvolved.Add(new RoomsInvolved() { Position = targetRoom.transform.position.ToString(), Rotation = targetRoom.transform.rotation.ToString(), RoomType = targetRoom.Name.ToString() });
                }

                if (killerRoom != null && !data.RoomsInvolved.Any(s => s.RoomType == killerRoom.Name.ToString() && s.Position == killerRoom.transform.position.ToString()))
                {
                    data.RoomsInvolved.Add(new RoomsInvolved() { Position = killerRoom.transform.position.ToString(), Rotation = killerRoom.transform.rotation.ToString(), RoomType = killerRoom.Name.ToString() });
                }

                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Logger.Debug("resolving on scene players");
                foreach (var bystanders in Player.List)
                {
                    if (bystanders.Role == RoleTypeId.Spectator || ev.Attacker.Role == RoleTypeId.Overwatch || ev.Attacker.Role == RoleTypeId.None)
                        continue;

                    var distance = Vector3.Distance(ev.Attacker.Position, bystanders.Position);
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Logger.Debug($"Checking distance on killer {distance} from {ev.Attacker.Nickname} to {bystanders.Nickname} {data.PlayersOnScene.All(plrs => plrs.UserId != bystanders.UserId)}");
                    if (distance <= 60 && data.PlayersOnScene.All(plrs => plrs.UserId != bystanders.UserId))
                    {
                        RoomIdentifier bystanderRoom = null;
                        RoomUtils.TryGetRoom(bystanders.Position, out bystanderRoom);
                        if (bystanderRoom != null && !data.RoomsInvolved.Any(s => s.RoomType == bystanderRoom.Name.ToString() && s.Position == bystanderRoom.transform.position.ToString()))
                        {
                            data.RoomsInvolved.Add(new RoomsInvolved() { Position = bystanderRoom.transform.position.ToString(), Rotation = bystanderRoom.transform.rotation.ToString(), RoomType = bystanderRoom.name });
                        }

                        data.PlayersOnScene.Add(new UsersOnScene()
                        {
                            CurrentHealth = bystanders.Health,
                            Distance = Vector3.Distance(ev.Attacker.Position, bystanders.Position),
                            Position = bystanders.Position.ToString(),
                            RoleType = bystanders.Role,
                            UserId = bystanders.UserId,
                            Bystander = true,
                            Room = bystanderRoom == null ? "Unknown" : bystanderRoom.Zone.ToString()
                        });
                    }
                }
                
                var cmd = new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        { "UserId", ev.Player.UserId },
                        { "UserName", ev.Player.Nickname },
                        { "Class", ev.Player.Role.ToString() },
                        { "AttackerClass", ev.Attacker.Role.ToString() },
                        { "AttackerId", ev.Attacker.UserId },
                        { "AttackerName", ev.Attacker.Nickname },
                        { "Weapon", ev.DamageHandler.ToString() },
                        { "Type", nameof(OnPlayerDying) },
                        { "Message", 
                            string.Format("Teamkill ⚠: {0} - {1} (<color={2}>{3}</color>) killed {4} - {5} (<color={6}>{7}</color>) with {8} in {9}.",
                                new object[]
                                {
                                    ev.Attacker.Nickname, 
                                    ev.Attacker.UserId, 
                                    Misc.ToHex(ev.Attacker.ReferenceHub.roleManager.CurrentRole.RoleColor), 
                                    ev.Attacker.Role,
                                    ev.Player.Nickname, 
                                    ev.Player.UserId, 
                                    Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor), 
                                    ev.Player.Role, ev.DamageHandler.ToString(), 
                                    killerRoom == null ? "Unknown" : killerRoom.Zone.ToString()
                                }) }
                    }
                };
                Timing.CallDelayed(0, Segment.FixedUpdate, () =>
                {
                    try
                    {
                        if (!ev.IsAllowed)
                            return;
                        
                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Logger.Debug("sending WR");
                        Task.Run(async () =>
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Logger.Debug("Thread send");
                            if (QuerySystem.QuerySystemKey == "None")
                                return;
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Logger.Debug("sending WR");
                            using (HttpClient client = new HttpClient())
                            {
                                client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                                await VerificationChallenge.AwaitVerification();
                                try
                                {
                                    var response = await client.PostAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.CurrentMaster}/Api/v3/Teamkill/{QuerySystem.QuerySystemKey}?v=2", new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
                                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                        Logger.Debug(await response.Content.ReadAsStringAsync());
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error(ex.ToString());
                                }
                            }
                        });
                        
                        WebSocketSystem.Enqueue(cmd);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.ToString());
                    }
                });
            }
            else
            {
                RoomIdentifier killerRoom = null;
                RoomUtils.TryGetRoom(ev.Player.Position, out killerRoom);

                var cmd = new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        { "UserId", ev.Player.UserId },
                        { "UserName", ev.Player.Nickname },
                        { "Class", ev.Player.Role.ToString() },
                        { "AttackerClass", ev.Attacker == null ? "None" : ev.Attacker.Role.ToString() },
                        { "AttackerId", ev.Attacker == null ? "server" : ev.Attacker.UserId },
                        { "AttackerName", ev.Attacker == null ? "Server " : ev.Attacker.Nickname },
                        { "Weapon", ev.DamageHandler.ToString() },
                        { "Type", nameof(OnPlayerDying) },
                        { "Message", string.Format("{0} - {1} (<color={2}>{3}</color>) killed {4} - {5} (<color={6}>{7}</color>) with {8} In {9}.", 
                            new object[]
                            {
                                ev.Attacker == null ? "Server " : ev.Attacker.Nickname,
                                ev.Attacker == null ? "server" : ev.Attacker.UserId,
                                ev.Attacker == null ? "white" : Misc.ToHex(ev.Attacker.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Attacker == null ? "None" : ev.Attacker.Role.ToString(),
                                ev.Player.Nickname, 
                                ev.Player.UserId, 
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role, 
                                ev.DamageHandler.ToString(), 
                                killerRoom == null ? "Unknown" : killerRoom.Zone.ToString()
                            }) }
                    }
                };
                
                Timing.CallDelayed(0, Segment.FixedUpdate, () =>
                {
                    try
                    {
                        if (!ev.IsAllowed)
                            return;
                        
                        WebSocketSystem.Enqueue(cmd);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.ToString());
                    }
                });
            }
        }

        public override void OnPlayerDroppedAmmo(PlayerDroppedAmmoEventArgs ev)
        {
            if (ev.AmmoPickup.Base == null)
                return;
            
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerDroppedAmmo)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has dropped {4} {5}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Amount,
                                ev.AmmoPickup.Base.NetworkInfo.ItemId
                            })
                    }
                }
            });
        }

        public override void OnPlayerDroppedItem(PlayerDroppedItemEventArgs ev)
        {
            if (ev.Pickup.Base == null)
                return;
            
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerDroppedItem)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has dropped {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Pickup.Base.NetworkInfo.ItemId
                            })
                    }
                }
            });
        }

        public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
        {
            ThreadDispatcher.SendHeartbeatMessage(true);
            if (CommandHandler.Synced.ContainsKey(ev.Player.UserId))
            {
                if (ServerStatic.PermissionsHandler.Members.ContainsKey(ev.Player.UserId) && CommandHandler.Synced[ev.Player.UserId] == ServerStatic.PermissionsHandler.Members[ev.Player.UserId])
                    if (ServerStatic.PermissionsHandler.Members.Remove(ev.Player.UserId));
                ev.Player.ReferenceHub.serverRoles.RefreshPermissions();
                CommandHandler.Synced.Remove(ev.Player.UserId);
            }

            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"PlayerId", ev.Player.PlayerId.ToString()},
                    {"Type", nameof(OnPlayerJoined)},
                    {
                        "Message", string.Format("({0}) {1} - {2} joined the game.",
                            ev.Player.PlayerId, ev.Player.Nickname, ev.Player.UserId)
                    }
                }
            });
        }

        public override void OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            FpcServerPositionDistributorPatch.VisibilityCache.Remove(ev.Player.ReferenceHub);
            foreach (var target in FpcServerPositionDistributorPatch.VisibilityCache)
            {
                target.Value.Remove(ev.Player.ReferenceHub);
            }
            Scp939LungePatch.LungeTime.Remove(ev.Player.ReferenceHub.netId);
            FpcSyncDataPatch.SyncDatas.Remove(ev.Player.ReferenceHub);
            BanSystem.Authenticating.Remove(ev.Player.ReferenceHub);
            BanSystem.CedModAuthTokens.Remove(ev.Player.ReferenceHub);
            ThreadDispatcher.SendHeartbeatMessage(true);
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerLeft)},
                    {"Message", ev.Player.Nickname + " - " + ev.Player.UserId + " has left the server."}
                }
            });
        }

        public override void OnPlayerPickedUpAmmo(PlayerPickedUpAmmoEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerPickedUpAmmo)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has picked up {4}", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Pickup?.Base.NetworkInfo.ItemId
                            })
                    }
                }
            });
        }

        public override void OnPlayerPickedUpArmor(PlayerPickedUpArmorEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerPickedUpArmor)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has picked up {4}", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Item.Type
                            })
                    }
                }
            });
        }

        public override void OnPlayerPickedUpItem(PlayerPickedUpItemEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerPickedUpItem)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has picked up {4}", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Item.Type
                            })
                    }
                }
            });
        }

        public void OnPlayerReceiveEffect(StatusEffectBase statusEffectBase)
        {
            if (statusEffectBase.Hub.authManager.InstanceMode != ClientInstanceMode.ReadyClient)
                return;
            
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", statusEffectBase.Hub.authManager.UserId},
                    {"UserName", statusEffectBase.Hub.nicknameSync.MyNick},
                    {"Type", nameof(OnPlayerReceiveEffect)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has received effect {4} with intensity {5}, duration {6}", new object[]
                            {
                                statusEffectBase.Hub.nicknameSync.MyNick,
                                statusEffectBase.Hub.authManager.UserId,
                                Misc.ToHex(statusEffectBase.Hub.roleManager.CurrentRole.RoleColor),
                                statusEffectBase.Hub.roleManager.CurrentRole.RoleTypeId,
                                statusEffectBase.GetType().Name,
                                statusEffectBase.Intensity,
                                statusEffectBase.Duration
                            })
                    }
                }
            });
        }

        public void OnPlayerDisableEffect(StatusEffectBase statusEffectBase)
        {
            if (statusEffectBase.Hub.authManager.InstanceMode != ClientInstanceMode.ReadyClient)
                return;
            
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", statusEffectBase.Hub.authManager.UserId},
                    {"UserName", statusEffectBase.Hub.nicknameSync.MyNick},
                    {"Type", nameof(OnPlayerDisableEffect)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) stopped effect {4}.", new object[]
                            {
                                statusEffectBase.Hub.nicknameSync.MyNick,
                                statusEffectBase.Hub.authManager.UserId,
                                Misc.ToHex(statusEffectBase.Hub.roleManager.CurrentRole.RoleColor),
                                statusEffectBase.Hub.roleManager.CurrentRole.RoleTypeId,
                                statusEffectBase.GetType().Name
                            })
                    }
                }
            });
        }

        public void OnPlayerReceiveEffectIntensity(StatusEffectBase statusEffectBase, byte b, byte arg3)
        {
            if (statusEffectBase.Hub.authManager.InstanceMode != ClientInstanceMode.ReadyClient)
                return;
            
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", statusEffectBase.Hub.authManager.UserId},
                    {"UserName", statusEffectBase.Hub.nicknameSync.MyNick},
                    {"Type", nameof(OnPlayerReceiveEffectIntensity)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has received effect intensity change {4} with intensity {5}, duration {6}", new object[]
                            {
                                statusEffectBase.Hub.nicknameSync.MyNick,
                                statusEffectBase.Hub.authManager.UserId,
                                Misc.ToHex(statusEffectBase.Hub.roleManager.CurrentRole.RoleColor),
                                statusEffectBase.Hub.roleManager.CurrentRole.RoleTypeId,
                                statusEffectBase.GetType().Name,
                                statusEffectBase.Intensity,
                                statusEffectBase.Duration
                            })
                    }
                }
            });
        }

        public override void OnPlayerReloadedWeapon(PlayerReloadedWeaponEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerReloadedWeapon)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has reloaded {4}", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Weapon.Type
                            })
                    }
                }
            });
        }

        public override void OnPlayerOpenedGenerator(PlayerOpenedGeneratorEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerOpenedGenerator)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has opened generator {4}", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Generator.Base.netId
                            })
                    }
                }
            });
        }

        public override void OnPlayerSpawnedRagdoll(PlayerSpawnedRagdollEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerSpawnedRagdoll)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has spawned ragdoll.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                            })
                    }
                }
            });
        }

        public override void OnPlayerUncuffed(PlayerUncuffedEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Target.UserId},
                    {"UserName", ev.Target.Nickname},
                    {"Type", nameof(OnPlayerUncuffed)},
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

        public override void OnPlayerReportedCheater(PlayerReportedCheaterEventArgs ev)
        {
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Logger.Debug("sending report WR");
            Task.Run(async () =>
            {
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Logger.Debug("Thread report send");
                if (QuerySystem.QuerySystemKey == "None" || !CedModMain.Singleton.Config.CedMod.EnableIngameReports)
                    return;
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Logger.Debug("sending report WR");
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                    await VerificationChallenge.AwaitVerification();
                    try
                    {
                        var response = await client.PostAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.CurrentMaster}/Api/v3/Reports/{QuerySystem.QuerySystemKey}",
                                new StringContent(JsonConvert.SerializeObject(new Dictionary<string, string>()
                                    {
                                        {"reporter", ev.Player.UserId},
                                        {"reported", ev.Target.UserId},
                                        {"reason", ev.Reason},
                                        {"cheating", "true"}
                                    }), Encoding.UTF8,
                                    "application/json"));
                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Logger.Debug(await response.Content.ReadAsStringAsync());
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.ToString());
                    }
                }
            });
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnPlayerReportedCheater)},
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {
                        "Message", string.Concat(new string[]
                        {
                            "ingame: ",
                            ev.Player.UserId,
                            " report ",
                            ev.Target.UserId,
                            " for ",
                            ev.Reason,
                            "."
                        })
                    }
                }
            });
        }

        public override void OnPlayerReportingPlayer(PlayerReportingPlayerEventArgs ev)
        {
            if (!CedModMain.Singleton.Config.CedMod.EnableIngameReports)
            {
                if (string.IsNullOrEmpty(CedModMain.Singleton.Config.CedMod.IngameReportDisabledMessage))
                {
                    ev.Player.SendConsoleMessage($"[REPORTING] Ingame reporting is disabled on this server.", "green");
                    ev.IsAllowed = false;
                    return;
                }
                else
                {
                    ev.Player.SendConsoleMessage($"[REPORTING] {CedModMain.Singleton.Config.CedMod.IngameReportDisabledMessage}", "green");
                    ev.IsAllowed = false;
                    return;
                }
            }

            if (CedModMain.Singleton.Config.QuerySystem.ReportBlacklist.Contains(ev.Player.UserId))
            {
                ev.Player.SendConsoleMessage($"[REPORTING] You are blacklisted from ingame reporting", "green");
                ev.IsAllowed = false;
                return;
            }

            if (ev.Player.UserId == ev.Target.UserId)
            {
                ev.Player.SendConsoleMessage($"[REPORTING] You can't report yourself", "green");
                ev.IsAllowed = false;
                return;
            }

            if (CedMod.Handlers.Server.reported.ContainsKey(ev.Target.ReferenceHub))
            {
                ev.Player.SendConsoleMessage($"[REPORTING] {ev.Target.Nickname} ({(ev.Target.DoNotTrack ? "DNT" : ev.Target.UserId)}) has already been reported by {CedModPlayer.Get(CedMod.Handlers.Server.reported[ev.Target.ReferenceHub]).Nickname}", "green");
                ev.IsAllowed = false;
                return;
            }

            if (ev.Target.RemoteAdminAccess && !CedModMain.Singleton.Config.QuerySystem.StaffReportAllowed)
            {
                ev.Player.SendConsoleMessage($"[REPORTING] " + CedModMain.Singleton.Config.QuerySystem.StaffReportMessage, "green");
                ev.IsAllowed = false;
                return;
            }

            if (ev.Reason.IsEmpty())
            {
                ev.Player.SendConsoleMessage($"[REPORTING] You have to enter a reason", "green");
                ev.IsAllowed = false;
                return;
            }

            CedMod.Handlers.Server.reported.Add(ev.Target.ReferenceHub, ev.Player.ReferenceHub);
            Timing.RunCoroutine(RemoveFromReportList(ev.Target.ReferenceHub));

            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Logger.Debug("sending report WR");
            Task.Run(async () =>
            {
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Logger.Debug("Thread report send");
                if (QuerySystem.QuerySystemKey == "None")
                    return;
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Logger.Debug("sending report WR");
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                    await VerificationChallenge.AwaitVerification();
                    try
                    {
                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                        {
                            ev.Player.SendConsoleMessage($"[REPORTING] Sending report to server staff...", "green");
                        });
                        var response = await client.PostAsync(
                                $"http{(QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.CurrentMaster}/Api/v3/Reports/{QuerySystem.QuerySystemKey}",
                                new StringContent(JsonConvert.SerializeObject(new Dictionary<string, string>()
                                    {
                                        { "reporter", ev.Player.UserId },
                                        { "reported", ev.Target.UserId },
                                        { "reason", ev.Reason },
                                    }), Encoding.UTF8,
                                    "application/json"));
                        if (response.IsSuccessStatusCode)
                        {
                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => { ev.Player.SendConsoleMessage($"[REPORTING] {CedModMain.Singleton.Config.QuerySystem.ReportSuccessMessage}", "green"); });
                        }
                        else
                        {
                            string textResponse = await response.Content.ReadAsStringAsync();
                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => { ev.Player.SendConsoleMessage($"[REPORTING] Failed to send report, please let server staff know: {textResponse}", "green"); });
                            Logger.Error($"Failed to send report: {textResponse}");
                        }

                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Logger.Debug(await response.Content.ReadAsStringAsync());
                    }
                    catch (Exception ex)
                    {
                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => { ev.Player.SendConsoleMessage($"[REPORTING] Failed to send report, please let server staff know: {ex}", "green"); });
                        Logger.Error(ex.ToString());
                    }
                }
            });
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnPlayerReportingPlayer)},
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {
                        "Message", string.Concat(new string[]
                        {
                            "ingame: ",
                            ev.Player.UserId,
                            " report ",
                            ev.Target.UserId,
                            " for ",
                            ev.Reason,
                            "."
                        })
                    }
                }
            });
            
            ev.IsAllowed = false;
            return;
        }

        public override void OnPlayerThrowingItem(PlayerThrowingItemEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerThrowingItem)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) threw an item {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Pickup.Base.NetworkInfo.ItemId
                            })
                    }
                }
            });
        }

        public override void OnPlayerThrewProjectile(PlayerThrewProjectileEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerThrewProjectile)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) threw a projectile {4}.", new object[]
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

        public override void OnPlayerUsedItem(PlayerUsedItemEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"ItemType", ev.Item.Type.ToString()},
                    {"Type", nameof(OnPlayerUsedItem)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) used an item {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Item.Type
                            })
                    }
                }
            });
        }

        public override void OnPlayerCuffed(PlayerCuffedEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Target.UserId},
                    {"UserName", ev.Target.Nickname},
                    {"Type", nameof(OnPlayerCuffed)},
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

        public override void OnPlayerEscaped(PlayerEscapedEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Time", RoundStart.RoundLength.ToString()},
                    {"Type", nameof(OnPlayerEscaped)},
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
                WebSocketSystem.Enqueue(new QueryCommand()
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

                foreach (DisarmedPlayers.DisarmedEntry entry in DisarmedPlayers.Entries)
                {
                    if ((int)entry.DisarmedPlayer == (int)ev.Player.NetworkId)
                    {
                        var disarmer = Player.List.FirstOrDefault(s => s.NetworkId == entry.Disarmer);
                        if (disarmer != null)
                        {
                            WebSocketSystem.Enqueue(new QueryCommand()
                            {
                                Recipient = "PANEL",
                                Data = new Dictionary<string, string>()
                                {
                                    {"Message", "GRANTEXP"},
                                    {"GrantType", "AssistEscapeCuff"},
                                    {"UserId", disarmer.UserId},
                                    {"RoleType", disarmer.Role.ToString()},
                                }
                            });
                        }
                    }
                }
            }
        }
    }
}