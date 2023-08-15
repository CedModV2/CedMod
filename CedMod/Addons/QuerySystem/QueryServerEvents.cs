using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.Patches;
using CedMod.Addons.QuerySystem.WS;
using MEC;
using Newtonsoft.Json;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using Respawning;
using UnityEngine.XR;
using Server = CedMod.Handlers.Server;

namespace CedMod.Addons.QuerySystem
{
    public class QueryServerEvents
    {
        public IEnumerator<float> SyncStart(bool wait = true)
        {
            if (wait)
                yield return Timing.WaitForSeconds(3);
            else
                yield return Timing.WaitForSeconds(0.2f);
            
            if (QuerySystem.QuerySystemKey != "None")
            {
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug("Checking configs");  
                if (CedModMain.Singleton.Config.QuerySystem.EnableExternalLookup)
                {
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug("Setting lookup mode");
                    ServerConfigSynchronizer.Singleton.NetworkRemoteAdminExternalPlayerLookupMode = "fullauth";
                    ServerConfigSynchronizer.Singleton.NetworkRemoteAdminExternalPlayerLookupURL =
                        $"http{(QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.CurrentPanel}/Api/v3/Lookup/";
                    ServerConfigSynchronizer.Singleton.RemoteAdminExternalPlayerLookupToken =
                        QuerySystem.QuerySystemKey;
                }

                Task.Run(async () =>
                {
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug("Checking configs");
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            await VerificationChallenge.AwaitVerification();
                            if (CedModMain.Singleton.Config.QuerySystem.EnableBanreasonSync)
                            {
                                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                    Log.Debug("Enabling ban reasons");  
                                ServerConfigSynchronizer.Singleton.NetworkEnableRemoteAdminPredefinedBanTemplates = true;
                                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                    Log.Debug("Clearing ban reasons");
                                ServerConfigSynchronizer.Singleton.RemoteAdminPredefinedBanTemplates.Clear();
                                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                    Log.Debug("Downloading ban reasons");
                                var response = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.CurrentMaster}/Api/v3/BanReasons/{QuerySystem.QuerySystemKey}");
                                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                    Log.Debug("Addding ban reasons");
                                foreach (var dict in JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(await response.Content.ReadAsStringAsync()))
                                {
                                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                        Log.Debug($"Addding ban reason {JsonConvert.SerializeObject(dict)}");

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
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.ToString());
                    }
                    new Thread(WebSocketSystem.ApplyRa).Start();
                });
            }
        }

        [PluginEvent(ServerEventType.WaitingForPlayers)]
        public void OnWaitingForPlayers(WaitingForPlayersEvent ev)
        {
            Timing.RunCoroutine(SyncStart());

            Task.Run(async delegate
            {
                if (!WebSocketSystem.Socket.IsRunning)
                {
                    WebSocketSystem.Stop();
                    await WebSocketSystem.Start();
                }
            });

            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnWaitingForPlayers)},
                    {"Message", "Server is waiting for players."}
                }
            });
        }

        [PluginEvent(ServerEventType.RoundStart)]
        public void OnRoundStart(RoundStartEvent ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnRoundStart)},
                    {"Message", "Round is starting."}
                }
            });
            LevelerStore.TrackingEnabled = WebSocketSystem.HelloMessage.ExpEnabled;

            if (LevelerStore.TrackingEnabled)
            {
                Timing.CallDelayed(2, () =>
                {
                    LevelerStore.InitialPlayerRoles.Clear();
                    foreach (var plr in Player.GetPlayers<CedModPlayer>())
                    {
                        LevelerStore.InitialPlayerRoles.Add(plr, plr.Role);
                    }
                });
            }
        }

        [PluginEvent(ServerEventType.RoundEnd)]
        public void OnRoundEnd(RoundEndEvent ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnRoundEnd)},
                    {"Message", "Round has ended."}
                }
            });

            if (LevelerStore.TrackingEnabled)
            {
                foreach (var plr in LevelerStore.InitialPlayerRoles)
                {
                    if (plr.Key == null || !plr.Key.GameObject == null)
                        continue;
                    WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
                    {
                        Recipient = "PANEL",
                        Data = new Dictionary<string, string>()
                        {
                            {"Message", "GRANTEXP"},
                            {"GrantType", "SurvivalEndRound"},
                            {"UserId", plr.Key.UserId},
                            {"RoleType", plr.Value.ToString()}
                        }
                    });
                }
                LevelerStore.InitialPlayerRoles.Clear();
            }
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
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
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

        [PluginEvent(ServerEventType.TeamRespawn)]
        public void OnRespawn(TeamRespawnEvent ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnRespawn)},
                    {"Message", string.Format("Respawn: {0} as {1}.", "undef", ev.Team)}
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
            if (Server.reported.ContainsKey(ev.Target.ReferenceHub))
            {
                ev.Player.SendConsoleMessage($"[REPORTING] {ev.Target.Nickname} ({(ev.Target.DoNotTrack ? "DNT" : ev.Target.UserId)}) has already been reported by {CedModPlayer.Get(Server.reported[ev.Target.ReferenceHub]).Nickname}", "green");
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
            
            Server.reported.Add(ev.Target.ReferenceHub, ev.Player.ReferenceHub);
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
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
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
        
        public IEnumerator<float> RemoveFromReportList(ReferenceHub target)
        {
            yield return Timing.WaitForSeconds(60f);
            Server.reported.Remove(target);
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