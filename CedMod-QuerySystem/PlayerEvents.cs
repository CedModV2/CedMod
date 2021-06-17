using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CedMod.QuerySystem.WS;
using Exiled.API.Extensions;
using Exiled.Events.EventArgs;
using Newtonsoft.Json;

namespace CedMod.QuerySystem
{
    public class PlayerEvents
    {
        public void OnPlayerLeave(LeftEventArgs ev)
        {
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", ev.Player.Nickname + " - " + ev.Player.UserId + " has left the server."}
                    }
                }));
            });
        }

        public void OnElevatorInteraction(InteractingElevatorEventArgs ev)
        {
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", ev.Player.Nickname + " - " + ev.Player.UserId + " has interacted with elevator."}
                    }
                }));
            });
        }

        public void OnPocketEnter(EnteringPocketDimensionEventArgs ev)
        {
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
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
                }));
            });
        }

        public void OnPocketEscape(EscapingPocketDimensionEventArgs ev)
        {
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
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
                }));
            });
        }

        public void On079Tesla(InteractingTeslaEventArgs ev)
        {
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
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
                }));
            });
        }

        public void OnPlayerHurt(HurtingEventArgs ev)
        {
            if (ev.DamageType == DamageTypes.Scp207)
                return;
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {
                            "Message", string.Format(
                                "{0} damaged {1} - {2} (<color={3}>{4}</color>) ammount {5} with {6}.", new object[]
                                {
                                    ev.HitInformations.Attacker,
                                    ev.Target.Nickname,
                                    ev.Target.UserId,
                                    Misc.ToHex(ev.Target.Role.GetColor()),
                                    ev.Target.Role,
                                    ev.Amount,
                                    DamageTypes.FromIndex(ev.Tool).name
                                })
                        }
                    }
                }));
            });
        }

        public void OnPlayerDeath(DyingEventArgs ev)
        {
            if (FriendlyFireAutoban.IsTeakill(ev))
            {
                Task.Factory.StartNew(delegate()
                {
                    WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                    {
                        Recipient = "ALL",
                        Data = new Dictionary<string, string>()
                        {
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
                                        DamageTypes.FromIndex(ev.HitInformation.Tool).name
                                    })
                            }
                        }
                    }));
                });
            }
            else
            {
                Task.Factory.StartNew(delegate()
                {
                    WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                    {
                        Recipient = "ALL",
                        Data = new Dictionary<string, string>()
                        {
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
                                        DamageTypes.FromIndex(ev.HitInformation.Tool).name
                                    })
                            }
                        }
                    }));
                });
            }
        }

        public void OnGrenadeThrown(ThrowingGrenadeEventArgs ev)
        {
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) threw a grenade.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.Role.GetColor()),
                                ev.Player.Role
                            })}
                    }
                }));
            });
        }

        public void OnMedicalItem(UsedMedicalItemEventArgs ev)
        {
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", string.Format(
                            "{0} - {1} (<color={2}>{3}</color>) Used a {4}.", new object[]
                            {
                                ev.Player.Nickname,
                                ev.Player.UserId,
                                Misc.ToHex(ev.Player.Role.GetColor()),
                                ev.Player.Role,
                                ev.Item
                            })}
                    }
                }));
            });
        }

        public void OnSetClass(ChangingRoleEventArgs ev)
        {
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", string.Format("{0} - {1}'s role has been changed to <color={2}>{3}</color>.", new object[]
                        {
                            ev.Player.Nickname,
                            ev.Player.UserId,
                            Misc.ToHex(ev.NewRole.GetColor()),
                            ev.NewRole
                        })}
                    }
                }));
            });
        }

        public void OnPlayerJoin(VerifiedEventArgs ev)
        {
            if (CommandHandler.synced.Contains(ev.Player.UserId))
            {
                if (ServerStatic.GetPermissionsHandler()._members.ContainsKey(ev.Player.UserId))
                    ServerStatic.GetPermissionsHandler()._members.Remove(ev.Player.UserId);
                ev.Player.ReferenceHub.serverRoles.RefreshPermissions();
                CommandHandler.synced.Remove(ev.Player.UserId);
            }

            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", string.Format("({0}) {1} - {2} joined the game.",
                            ev.Player.Id, ev.Player.Nickname, ev.Player.UserId)}
                    }
                }));
            });
        }

        public void OnPlayerFreed(RemovingHandcuffsEventArgs ev)
        {
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", string.Format(
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
                            })}
                    }
                }));
            });
        }

        public void OnPlayerHandcuffed(HandcuffingEventArgs ev)
        {
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", string.Format(
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
                            })}
                    }
                }));
            });
        }
    }
}