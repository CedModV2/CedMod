using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.Addons.Events;
using CedMod.Addons.QuerySystem.WS;
using CedMod.Addons.Sentinal.Patches;
using CentralAuth;
using CustomPlayerEffects;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Disarming;
using InventorySystem.Items;
using InventorySystem.Items.ThrowableProjectiles;
using InventorySystem.Items.Usables;
using MapGeneration;
using MEC;
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
        public QueryPlayerEvents()
        {
            StatusEffectBase.OnDisabled += OnPlayerDisableEffect;
            StatusEffectBase.OnEnabled += OnPlayerReceiveEffect;
            StatusEffectBase.OnIntensityChanged += OnPlayerReceiveEffectIntensity;
        }
        
        [PluginEvent(ServerEventType.PlayerActivateGenerator)]
        public void OnActivateGenerator(PlayerActivateGeneratorEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnActivateGenerator)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has activated generator {4}", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Generator.netId
                            })
                    }
                }
            });
        }
        
        [PluginEvent(ServerEventType.PlayerCancelUsingItem)]
        public void OnCancelItem(PlayerCancelUsingItemEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnCancelItem)},
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
        
        [PluginEvent(ServerEventType.PlayerChangeItem)]
        public void OnChangeItem(PlayerChangeItemEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnChangeItem)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has changed held item to {4}", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Player.ReferenceHub.inventory.UserInventory.Items.ContainsKey(ev.NewItem) ? ev.Player.ReferenceHub.inventory.UserInventory.Items[ev.NewItem].ItemTypeId : ItemType.None
                            })
                    }
                }
            });
        }
        
        [PluginEvent(ServerEventType.PlayerEnterPocketDimension)]
        public void OnPocketEnter(PlayerEnterPocketDimensionEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
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
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role
                            })
                    }
                }
            });
        }

        [PluginEvent(ServerEventType.PlayerExitPocketDimension)]
        public void OnPocketEscape(PlayerExitPocketDimensionEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Success", ev.IsSuccessful.ToString()},
                    {"Type", nameof(OnPocketEscape)},
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
        
        [PluginEvent(ServerEventType.PlayerInteractDoor)]
        public void OnInteractDoor(PlayerInteractDoorEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnInteractDoor)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has interacted with door {4}", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Door.netId + (DoorNametagExtension.NamedDoors.Any(s => s.Value.TargetDoor == ev.Door) ? $" {DoorNametagExtension.NamedDoors.FirstOrDefault(s => s.Value.TargetDoor == ev.Door).Key}" : "") 
                            })
                    }
                }
            });
        }
        
        [PluginEvent(ServerEventType.PlayerInteractGenerator)]
        public void OnInteractGenerator(PlayerInteractGeneratorEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnInteractGenerator)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has interacted with generator {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Generator.netId
                            })
                    }
                }
            });
        }
        
        [PluginEvent(ServerEventType.PlayerInteractLocker)]
        public void OnInteractLocker(PlayerInteractLockerEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnInteractLocker)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has interacted with locker {4}, Chamber {5}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Locker.netId,
                                ev.Locker.Chambers.IndexOf(ev.Chamber),
                            })
                    }
                }
            });
        }
        

        [PluginEvent(ServerEventType.PlayerInteractElevator)]
        public void OnElevatorInteraction(PlayerInteractElevatorEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
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
        
        [PluginEvent(ServerEventType.Scp079UseTesla)]
        public void On079Tesla(Scp079UseTeslaEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
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
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role
                            })
                    }
                }
            });
        }

        [PluginEvent(ServerEventType.PlayerShotWeapon)]
        public void OnPlayerShoot(PlayerShotWeaponEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerShoot)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has shot a {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Player.CurrentItem.ItemTypeId
                            })
                    }
                }
            });
        }
        
        [PluginEvent(ServerEventType.PlayerChangeRole)]
        public void OnSetClass(PlayerChangeRoleEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
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

            var plr = ev.Player;
            if (LevelerStore.TrackingEnabled && LevelerStore.InitialPlayerRoles.ContainsKey(plr))
            {
                if (ev.ChangeReason != RoleChangeReason.Escaped)
                    LevelerStore.InitialPlayerRoles.Remove(plr);
                else
                    LevelerStore.InitialPlayerRoles[plr] = ev.NewRole;
            }
        }
        
        
        public IEnumerator<float> RemoveFromReportList(ReferenceHub target)
        {
            yield return Timing.WaitForSeconds(60f);
            CedMod.Handlers.Server.reported.Remove(target);
        }
        
        
        [PluginEvent(ServerEventType.PlayerCloseGenerator)]
        public void OnCloseGenerator(PlayerCloseGeneratorEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnCloseGenerator)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has closed generator {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Generator.netId
                            })
                    }
                }
            });
        }
        
        [PluginEvent(ServerEventType.PlayerDamage)]
        public void OnPlayerHurt(PlayerDamageEvent ev)
        {
            if (ev.Target == null)
                return;

            RoomIdentifier killerRoom = RoomIdUtils.RoomAtPosition(ev.Target.Position);
            if (ev.Player != null)
            {
                WebSocketSystem.Enqueue(new QueryCommand()
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
                WebSocketSystem.Enqueue(new QueryCommand()
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
        
        [PluginEvent(ServerEventType.PlayerDeactivatedGenerator)]
        public void OnDeactivateGenerator(PlayerDeactivatedGeneratorEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnDeactivateGenerator)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has deactivated generator {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Generator.netId
                            })
                    }
                }
            });
        }

        [PluginEvent(ServerEventType.PlayerDying)]
        public void OnPlayerDeath(PlayerDyingEvent ev)
        {
            if (ev.Player == null || ev.Attacker == null)
                return;
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug("plrdeath");
            if (FriendlyFireAutoban.IsTeamKill(ev.Player, ev.Attacker, ev.DamageHandler))
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
                    data.RoomsInvolved.Add(new RoomsInvolved()
                    {
                        Position = targetRoom.transform.position.ToString(),
                        Rotation = targetRoom.transform.rotation.ToString(),
                        RoomType = targetRoom.Name.ToString()
                    });
                }
                
                if (killerRoom != null && !data.RoomsInvolved.Any(s => s.RoomType == killerRoom.Name.ToString() && s.Position == killerRoom.transform.position.ToString()))
                {
                    data.RoomsInvolved.Add(new RoomsInvolved()
                    {
                        Position = killerRoom.transform.position.ToString(),
                        Rotation = killerRoom.transform.rotation.ToString(),
                        RoomType = killerRoom.Name.ToString()
                    });
                }

                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug("resolving on scene players");
                foreach (var bystanders in Player.GetPlayers())
                {
                    if (bystanders.Role == RoleTypeId.Spectator || ev.Attacker.Role == RoleTypeId.Overwatch || ev.Attacker.Role == RoleTypeId.None)
                        continue;

                    var distance = Vector3.Distance(ev.Attacker.Position, bystanders.Position);
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug($"Checking distance on killer {distance} from {ev.Attacker.Nickname} to {bystanders.Nickname} {data.PlayersOnScene.All(plrs => plrs.UserId != bystanders.UserId)}");
                    if (distance <= 60 && data.PlayersOnScene.All(plrs => plrs.UserId != bystanders.UserId))
                    {
                        RoomIdentifier bystanderRoom = RoomIdUtils.RoomAtPosition(bystanders.Position);
                        if (bystanderRoom != null && !data.RoomsInvolved.Any(s => s.RoomType == bystanderRoom.Name.ToString() && s.Position == bystanderRoom.transform.position.ToString()))
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
                            Room = bystanderRoom == null ? "Unknown" : bystanderRoom.Zone.ToString()
                        });
                    }
                }
                

                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug("sending WR");
                Task.Run(async () =>
                {
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug("Thread send");
                    if (QuerySystem.QuerySystemKey == "None")
                        return;
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug("sending WR");
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("X-ServerIp", Server.ServerIpAddress);
                        await VerificationChallenge.AwaitVerification();
                        try
                        {
                            var response = await client.PostAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.CurrentMaster}/Api/v3/Teamkill/{QuerySystem.QuerySystemKey}?v=2", new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug(await response.Content.ReadAsStringAsync());
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.ToString());
                        }
                    }
                });

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
        
        [PluginEvent(ServerEventType.PlayerDroppedAmmo)]
        public void OnPlayerDropAmmo(PlayerDroppedAmmoEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerDropAmmo)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has dropped {4} {5}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Amount,
                                ev.Item.NetworkInfo.ItemId
                            })
                    }
                }
            });
        }
        
        [PluginEvent(ServerEventType.PlayerDropedpItem)]
        public void OnPlayerDropItem(PlayerDroppedItemEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerDropItem)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has dropped {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Item.NetworkInfo.ItemId
                            })
                    }
                }
            });
        }
        
        [PluginEvent(ServerEventType.PlayerJoined)]
        public void OnPlayerJoin(PlayerJoinedEvent ev)
        {
            ThreadDispatcher.SendHeartbeatMessage(true);
            if (CommandHandler.Synced.ContainsKey(ev.Player.UserId))
            {
                if (ServerStatic.GetPermissionsHandler()._members.ContainsKey(ev.Player.UserId) && CommandHandler.Synced[ev.Player.UserId] == ServerStatic.GetPermissionsHandler()._members[ev.Player.UserId])
                    ServerStatic.GetPermissionsHandler()._members.Remove(ev.Player.UserId);
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
                    {"Type", nameof(OnPlayerJoin)},
                    {
                        "Message", string.Format("({0}) {1} - {2} joined the game.",
                            ev.Player.PlayerId, ev.Player.Nickname, ev.Player.UserId)
                    }
                }
            });
        }
        
        [PluginEvent(ServerEventType.PlayerLeft)]
        public void OnPlayerLeave(PlayerLeftEvent ev)
        {
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
                    {"Type", nameof(OnPlayerLeave)},
                    {"Message", ev.Player.Nickname + " - " + ev.Player.UserId + " has left the server."}
                }
            });
        }
        
        [PluginEvent(ServerEventType.PlayerPickupAmmo)]
        public void OnPlayerPickupAmmo(PlayerPickupAmmoEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerPickupAmmo)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has picked up {4}", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Item.NetworkInfo.ItemId
                            })
                    }
                }
            });
        }
        
        [PluginEvent(ServerEventType.PlayerPickupArmor)]
        public void OnPlayerPickupArmor(PlayerPickupArmorEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerPickupArmor)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has picked up {4}", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Item.NetworkInfo.ItemId
                            })
                    }
                }
            });
        }
        
        [PluginEvent(ServerEventType.PlayerSearchedPickup)]
        public void OnPlayerPickupItem(PlayerSearchedPickupEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerPickupItem)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has picked up {4}", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Item.NetworkInfo.ItemId
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
        
        [PluginEvent(ServerEventType.PlayerReloadWeapon)]
        public void OnPlayerReloadWeapon(PlayerReloadWeaponEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerReloadWeapon)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has reloaded {4}", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Firearm.ItemTypeId
                            })
                    }
                }
            });
        }
        
        [PluginEvent(ServerEventType.PlayerUnlockGenerator)]
        public void OnPlayerUnlockGenerator(PlayerUnlockGeneratorEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerUnlockGenerator)},
                    {
                        "Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) has unlocked generator {4}", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.ReferenceHub.roleManager.CurrentRole.RoleColor),
                                ev.Player.Role,
                                ev.Generator.netId
                            })
                    }
                }
            });
        }
        
        [PluginEvent(ServerEventType.RagdollSpawn)]
        public void OnPlayerRagdollSpawn(RagdollSpawnEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnPlayerRagdollSpawn)},
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
        
        [PluginEvent(ServerEventType.PlayerRemoveHandcuffs)]
        public void OnPlayerFreed(PlayerRemoveHandcuffsEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
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
        
        [PluginEvent(ServerEventType.PlayerCheaterReport)]
        public void OnCheaterReport(PlayerCheaterReportEvent ev)
        {
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug("sending report WR");
            Task.Run(async () =>
            {
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug("Thread report send");
                if (QuerySystem.QuerySystemKey == "None" || !CedModMain.Singleton.Config.CedMod.EnableIngameReports)
                    return;
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug("sending report WR");
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-ServerIp", PluginAPI.Core.Server.ServerIpAddress);
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
                            Log.Debug(await response.Content.ReadAsStringAsync());
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }
                
            });
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnCheaterReport)},
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

        [PluginEvent(ServerEventType.PlayerReport)]
        public bool OnReport(PlayerReportEvent ev)
        {
            if (!CedModMain.Singleton.Config.CedMod.EnableIngameReports)
            {
                if (string.IsNullOrEmpty(CedModMain.Singleton.Config.CedMod.IngameReportDisabledMessage))
                {
                    ev.Player.SendConsoleMessage($"[REPORTING] Ingame reporting is disabled on this server.", "green");
                    return false;
                }
                else
                {
                    ev.Player.SendConsoleMessage($"[REPORTING] {CedModMain.Singleton.Config.CedMod.IngameReportDisabledMessage}", "green");
                    return false;
                }
            }
            if (CedModMain.Singleton.Config.QuerySystem.ReportBlacklist.Contains(ev.Player.UserId))
            {
                ev.Player.SendConsoleMessage($"[REPORTING] You are blacklisted from ingame reporting", "green");
                return false;
            }
            if (ev.Player.UserId == ev.Target.UserId)
            {
                ev.Player.SendConsoleMessage($"[REPORTING] You can't report yourself", "green");
                return false;
            }
            if (CedMod.Handlers.Server.reported.ContainsKey(ev.Target.ReferenceHub))
            {
                ev.Player.SendConsoleMessage($"[REPORTING] {ev.Target.Nickname} ({(ev.Target.DoNotTrack ? "DNT" : ev.Target.UserId)}) has already been reported by {CedModPlayer.Get(CedMod.Handlers.Server.reported[ev.Target.ReferenceHub]).Nickname}", "green");
                return false;
            }
            if (ev.Target.RemoteAdminAccess && !CedModMain.Singleton.Config.QuerySystem.StaffReportAllowed)
            {
                ev.Player.SendConsoleMessage($"[REPORTING] " + CedModMain.Singleton.Config.QuerySystem.StaffReportMessage, "green");
                return false;
            }
            if (ev.Reason.IsEmpty())
            {
                ev.Player.SendConsoleMessage($"[REPORTING] You have to enter a reason", "green");
                return false;
            }
            
            CedMod.Handlers.Server.reported.Add(ev.Target.ReferenceHub, ev.Player.ReferenceHub);
            Timing.RunCoroutine(RemoveFromReportList(ev.Target.ReferenceHub));
            
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug("sending report WR");
            Task.Run(async () =>
            {
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug("Thread report send");    
                if (QuerySystem.QuerySystemKey == "None")
                    return;
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug("sending report WR");
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-ServerIp", PluginAPI.Core.Server.ServerIpAddress);
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
                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                            {
                                ev.Player.SendConsoleMessage($"[REPORTING] {CedModMain.Singleton.Config.QuerySystem.ReportSuccessMessage}", "green");
                            });
                        }
                        else
                        {
                            string textResponse = await response.Content.ReadAsStringAsync();
                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                            {
                                ev.Player.SendConsoleMessage($"[REPORTING] Failed to send report, please let server staff know: {textResponse}", "green");
                            });
                            Log.Error($"Failed to send report: {textResponse}");
                        }

                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Log.Debug(await response.Content.ReadAsStringAsync());
                    }
                    catch (Exception ex)
                    {
                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                        {
                            ev.Player.SendConsoleMessage($"[REPORTING] Failed to send report, please let server staff know: {ex}", "green");
                        });
                        Log.Error(ex.ToString());
                    }
                }
            });
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnReport)},
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
            
            return false;
        }

        [PluginEvent(ServerEventType.PlayerThrowItem)]
        public void OnItemThrown(PlayerThrowItemEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"UserId", ev.Player.UserId},
                    {"UserName", ev.Player.Nickname},
                    {"Type", nameof(OnItemThrown)},
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
            WebSocketSystem.Enqueue(new QueryCommand()
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
            WebSocketSystem.Enqueue(new QueryCommand()
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
        
        [PluginEvent(ServerEventType.PlayerHandcuff)]
        public void OnPlayerHandcuffed(PlayerHandcuffEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
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
            WebSocketSystem.Enqueue(new QueryCommand()
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
                        var disarmer = Player.GetPlayers().FirstOrDefault(s => s.NetworkId == entry.Disarmer);
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