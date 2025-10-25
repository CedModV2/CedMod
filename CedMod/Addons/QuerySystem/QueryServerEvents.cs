using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.WS;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using CedMod.Addons.Sentinal;
using CedMod.Addons.Sentinal.Patches;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Enums;
using MapGeneration;
using MEC;
using Newtonsoft.Json;
using Version = GameCore.Version;

namespace CedMod.Addons.QuerySystem
{
    public class QueryServerEvents: CustomEventsHandler
    {
        private bool _first = false;

        public static void CreateMapLayout()
        {
            WebSocketSystem.SentMap = true;
            List<(FacilityZone zone, string name, string pos, string rot)> rooms = new List<(FacilityZone zone, string name, string pos, string rot)>();
            foreach (var roomIdentifier in RoomIdentifier.AllRoomIdentifiers)
            {
                rooms.Add((roomIdentifier.Zone, roomIdentifier.name, roomIdentifier.transform.position.ToString(), roomIdentifier.transform.rotation.eulerAngles.ToString()));
            }
            
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    { "Type", nameof(CreateMapLayout) },
                    { "Layout", JsonConvert.SerializeObject(rooms) },
                    { "Seed", SeedSynchronizer.Seed.ToString() }
                }
            });
        }

        public override void OnPlayerPreAuthenticating(PlayerPreAuthenticatingEventArgs ev)
        {
            if (QuerySystem.ReservedSlotUserids.Contains(ev.UserId))
                ev.CanJoin = true;
            
            Task.Run(async () =>
            {
                Dictionary<string, string> authToken = new Dictionary<string, string>()
                {
                    { "Type", "Preauth" },
                    { "Token", $"{ev.UserId};{ev.Flags};{ev.Region};{ev.Expiration}" },
                    { "Signature", Convert.ToBase64String(ev.Signature) },
                };
                            
                Dictionary<string, string> info = (Dictionary<string, string>) await API.APIRequest($"Auth/{ev.UserId}&{ev.IpAddress}?banLists={string.Join(",", ServerPreferences.Prefs.BanListReadBans.Select(s => s.Id))}&banListMutes={string.Join(",", ServerPreferences.Prefs.BanListReadMutes.Select(s => s.Id))}&server={Uri.EscapeDataString(WebSocketSystem.HelloMessage == null ? "Unknown" : WebSocketSystem.HelloMessage.Identity)}&r=1", JsonConvert.SerializeObject(authToken), false, "POST");
                lock (BanSystem.CachedStates)
                {
                    if (BanSystem.CachedStates.ContainsKey(ev.UserId))
                        BanSystem.CachedStates.Remove(ev.UserId);
                    BanSystem.CachedStates.Add(ev.UserId, info);
                }
							
            });
            
            base.OnPlayerPreAuthenticating(ev);
        }

        public override void OnServerCommandExecuted(CommandExecutedEventArgs ev)
        {
            if (ev.CommandType == CommandType.Client)
                return;
            
            try
            {
                WebSocketSystem.Enqueue(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"Type", "OnAdminCommand"},
                        {"UserId", ev.Sender?.SenderId},
                        {"UserName", ev.Sender?.Nickname},
                        {"Command", ev.CommandName + " " + string.Join(" ", ev.Arguments)},
                        {
                            "Message", string.Concat(new string[]
                            {
                                $"{ev.Sender?.Nickname} ({ev.Sender?.SenderId})",
                                " used command: ",
                                ev.CommandName,
                                " ",
                                string.Join(" ", ev.Arguments)
                            })
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
            base.OnServerCommandExecuted(ev);
        }

        public IEnumerator<float> SyncStart(bool wait = true)
        {
            if (wait)
                yield return Timing.WaitForSeconds(3);
            else
                yield return Timing.WaitForSeconds(0.2f);

            if (CedModMain.Singleton.Config.QuerySystem.RejectRemoteCommands)
            {
                Logger.Warn("You have RejectRemoteCommands enabled in the CedMod QuerySystem config, features such as RemoteCommands, EventManager, and more will not function correctly");
            }

            if (QuerySystem.QuerySystemKey != "None")
            {
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Logger.Debug("Checking configs");
                if (CedModMain.Singleton.Config.QuerySystem.EnableExternalLookup)
                {
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Logger.Debug("Setting lookup mode");
                    ServerConfigSynchronizer.Singleton.NetworkRemoteAdminExternalPlayerLookupMode = "fullauth";
                    ServerConfigSynchronizer.Singleton.NetworkRemoteAdminExternalPlayerLookupURL =
                        $"http{(QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.CurrentPanel}/Api/v3/Lookup/";
                    ServerConfigSynchronizer.Singleton.RemoteAdminExternalPlayerLookupToken =
                        QuerySystem.QuerySystemKey;
                }

                Task.Run(async () =>
                {
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Logger.Debug("Checking configs");
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                            await VerificationChallenge.AwaitVerification();
                            if (CedModMain.Singleton.Config.QuerySystem.EnableBanreasonSync)
                            {
                                ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                                {
                                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                        Logger.Debug("Enabling ban reasons");
                                    ServerConfigSynchronizer.Singleton.NetworkEnableRemoteAdminPredefinedBanTemplates = true;
                                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                        Logger.Debug("Clearing ban reasons");
                                    ServerConfigSynchronizer.Singleton.RemoteAdminPredefinedBanTemplates.Clear();
                                });

                                int cnt = 0;
                                while (ServerConfigSynchronizer.Singleton.RemoteAdminPredefinedBanTemplates.Count >= 1)
                                {
                                    if (cnt >= 100)
                                        break;

                                    cnt++;
                                    await Task.Delay(10);
                                }

                                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                    Logger.Debug("Downloading ban reasons");
                                var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.CurrentMaster}/Api/v3/BanReasons/{QuerySystem.QuerySystemKey}");
                                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                    Logger.Debug("Addding ban reasons");
                                var data = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(await response.Content.ReadAsStringAsync());
                                ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                                {
                                    foreach (var dict in data)
                                    {
                                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                            Logger.Debug($"Addding ban reason {JsonConvert.SerializeObject(dict)}");

                                        var durationNice = "";
                                        TimeSpan timeSpan = TimeSpan.FromMinutes(double.Parse(dict["Dur"]));
                                        int num2 = timeSpan.Days / 365;
                                        if (num2 > 0)
                                        {
                                            durationNice = string.Format("{0}y", num2);
                                        }
                                        else if (timeSpan.Days > 0)
                                        {
                                            durationNice = string.Format("{0}d", timeSpan.Days);
                                        }
                                        else if (timeSpan.Hours > 0)
                                        {
                                            durationNice = string.Format("{0}h", timeSpan.Hours);
                                        }
                                        else if (timeSpan.Minutes > 0)
                                        {
                                            durationNice = string.Format("{0}m", timeSpan.Minutes);
                                        }
                                        else
                                        {
                                            durationNice = string.Format("{0}s", timeSpan.Seconds);
                                        }

                                        ServerConfigSynchronizer.Singleton.RemoteAdminPredefinedBanTemplates.Add(
                                            new ServerConfigSynchronizer.PredefinedBanTemplate()
                                            {
                                                Duration = Convert.ToInt32(dict["Dur"]),
                                                FormattedDuration = durationNice,
                                                Reason = dict["Reason"]
                                            });
                                    }
                                });
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.ToString());
                    }

                    new Thread((o =>
                    {
                        try
                        {
                            if (!_first)
                            {
                                WebSocketSystem.ApplyRa(false);
                                _first = true;
                            }

                            WebSocketSystem.ApplyRa(true);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            Logger.Error(e.ToString());
                        }
                    })).Start(true);
                });
            }
        }

        public override void OnServerWaitingForPlayers()
        {
            Timing.RunCoroutine(SyncStart());

            Task.Run(async delegate
            {
                if (WebSocketSystem.Socket.State != WebSocketState.Open && WebSocketSystem.Socket.State != WebSocketState.Connecting)
                {
                    WebSocketSystem.Stop();
                    await WebSocketSystem.Start();
                }
            });
            SentinalBehaviour.RoundGuid = Guid.NewGuid().ToString();
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    { "Type", nameof(OnServerWaitingForPlayers) },
                    { "Message", "Server is waiting for players." }
                }
            });
            CreateMapLayout();
        }

        public override void OnServerRoundStarted()
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    { "Type", nameof(OnServerRoundStarted) },
                    { "Message", "Round is starting." }
                }
            });
            LevelerStore.TrackingEnabled = WebSocketSystem.HelloMessage.ExpEnabled;

            if (LevelerStore.TrackingEnabled)
            {
                Timing.CallDelayed(2, () =>
                {
                    LevelerStore.InitialPlayerRoles.Clear();
                    foreach (var plr in Player.ReadyList)
                    {
                        if (plr.ReferenceHub == null || plr.GameObject == null)
                            continue;
                        
                        LevelerStore.InitialPlayerRoles.Add(plr, plr.Role);
                    }
                });
            }
        }

        public override void OnServerRoundRestarted()
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    { "Type", nameof(OnServerRoundRestarted) },
                    { "Message", "Round is restarting." }
                }
            });
            WebSocketSystem.SentMap = false;
        }

        public override void OnServerRoundEnded(RoundEndedEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    { "Type", nameof(OnServerRoundEnded) },
                    { "Message", "Round has ended." }
                }
            });

            if (LevelerStore.TrackingEnabled)
            {
                foreach (var plr in LevelerStore.InitialPlayerRoles)
                {
                    if (plr.Key == null || plr.Key.ReferenceHub == null || !plr.Key.GameObject == null)
                        continue;
                    WebSocketSystem.Enqueue(new QueryCommand()
                    {
                        Recipient = "PANEL",
                        Data = new Dictionary<string, string>()
                        {
                            { "Message", "GRANTEXP" },
                            { "GrantType", "SurvivalEndRound" },
                            { "UserId", plr.Key.UserId },
                            { "RoleType", plr.Value.ToString() }
                        }
                    });
                }

                LevelerStore.InitialPlayerRoles.Clear();
            }
        }


        public override void OnServerWaveRespawned(WaveRespawnedEventArgs ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    { "Type", nameof(OnServerWaveRespawned) },
                    { "Message", string.Format("Respawn: {0} as {1}.", ev.Players.Count.ToString(), ev.Wave.Faction.ToString()) }
                }
            });
        }
    }

    public class MiniMapClass
    {
        public List<MiniMapElement> MapElements = new List<MiniMapElement>();
        public List<MiniMapPlayerElement> PlayerElements = new List<MiniMapPlayerElement>();
    }

    public class MiniMapElement
    {
        public string Position;
        public string Name;
        public string ZoneType;
        public string Rotation;
    }

    public class MiniMapPlayerElement
    {
        public string Position;
        public string Name;
        public string Zone;
        public string TeamColor;
    }
}