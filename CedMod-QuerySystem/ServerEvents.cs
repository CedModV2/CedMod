using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.Patches;
using CedMod.QuerySystem.Patches;
using CedMod.QuerySystem.WS;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using Newtonsoft.Json;

namespace CedMod.QuerySystem
{
    public class ServerEvents
    {
        public IEnumerator<float> SyncStart()
        {
            yield return Timing.WaitForSeconds(3);
            if (QuerySystem.Singleton.Config.SecurityKey != "None")
            {
                Log.Debug("Checking configs", CedModMain.Singleton.Config.ShowDebug);
                if (QuerySystem.Singleton.Config.EnableExternalLookup)
                {
                    Log.Debug("Setting lookup mode", CedModMain.Singleton.Config.ShowDebug);
                    ServerConfigSynchronizer.Singleton.NetworkRemoteAdminExternalPlayerLookupMode = "fullauth";
                    ServerConfigSynchronizer.Singleton.NetworkRemoteAdminExternalPlayerLookupURL =
                        $"https://{QuerySystem.PanelUrl}/Api/Lookup/";
                    ServerConfigSynchronizer.Singleton.RemoteAdminExternalPlayerLookupToken =
                        QuerySystem.Singleton.Config.SecurityKey;
                }

                Task.Factory.StartNew(() =>
                {
                    Log.Debug("Checking configs", CedModMain.Singleton.Config.ShowDebug);
                    if (QuerySystem.Singleton.Config.EnableBanreasonSync)
                    {
                        Log.Debug("Enabling ban reasons", CedModMain.Singleton.Config.ShowDebug);
                        ServerConfigSynchronizer.Singleton.NetworkEnableRemoteAdminPredefinedBanTemplates = true;
                        Log.Debug("Clearing ban reasons", CedModMain.Singleton.Config.ShowDebug);
                        ServerConfigSynchronizer.Singleton.RemoteAdminPredefinedBanTemplates.Clear();
                        HttpClient client = new HttpClient();
                        Log.Debug("Downloading ban reasons", CedModMain.Singleton.Config.ShowDebug);
                        var response =
                            client.GetAsync(
                                $"https://{QuerySystem.PanelUrl}/Api/BanReasons/{QuerySystem.Singleton.Config.SecurityKey}");
                        Log.Debug("Addding ban reasons", CedModMain.Singleton.Config.ShowDebug);
                        foreach (var dict in JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(response
                            .Result
                            .Content.ReadAsStringAsync().Result))
                        {
                            Log.Debug($"Addding ban reason {JsonConvert.SerializeObject(dict)}",
                                CedModMain.Singleton.Config.ShowDebug);

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
                                    DurationNice = durationNice,
                                    Reason = dict["Reason"]
                                });
                        }
                    }

                    HttpClient client1 = new HttpClient();
                    Log.Debug("Downloading syncs", CedModMain.Singleton.Config.ShowDebug);
                    var response1 =
                        client1.GetAsync(
                            $"https://{QuerySystem.PanelUrl}/Api/ReservedSlotUsers/{QuerySystem.Singleton.Config.SecurityKey}");
                    Log.Debug($"Downloaded Reserved slots: {response1.Result.Content.ReadAsStringAsync().Result}",
                        CedModMain.Singleton.Config.ShowDebug);
                    QuerySystem.ReservedSlotUserids =
                        JsonConvert.DeserializeObject<List<string>>(response1.Result.Content.ReadAsStringAsync()
                            .Result);
                });
            }
        }

        public void OnWaitingForPlayers()
        {
            Timing.RunCoroutine(SyncStart());

            Task.Factory.StartNew(delegate
            {
                if (!WebSocketSystem.Socket.IsAlive)
                {
                    CedMod.QuerySystem.WS.WebSocketSystem.Stop();
                    CedMod.QuerySystem.WS.WebSocketSystem.Start();
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

        public void OnRoundStart()
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnRoundStart)},
                    {"Message", "Round is restarting."}
                }
            });
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            BulletHolePatch.HoleCreators.Clear();
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnRoundEnd)},
                    {"Message", "Round is restarting."}
                }
            });
        }

        public void OnCheaterReport(ReportingCheaterEventArgs ev)
        {
            Log.Debug("sending report WR", CedModMain.Singleton.Config.ShowDebug);
            Task.Factory.StartNew(() =>
            {
                Log.Debug("Thread report send", CedModMain.Singleton.Config.ShowDebug);
                if (QuerySystem.Singleton.Config.SecurityKey == "None")
                    return;
                Log.Debug("sending report WR", CedModMain.Singleton.Config.ShowDebug);
                HttpClient client = new HttpClient();
                try
                {
                    var response = client
                        .PostAsync(
                            $"https://{QuerySystem.PanelUrl}/Api/Reports/{QuerySystem.Singleton.Config.SecurityKey}",
                            new StringContent(JsonConvert.SerializeObject(new Dictionary<string, string>()
                                {
                                    {"reporter", ev.Issuer.UserId},
                                    {"reported", ev.Issuer.UserId},
                                    {"reason", ev.Reason},
                                }), Encoding.Default,
                                "application/json")).Result;
                    Log.Debug(response.Content.ReadAsStringAsync().Result, CedModMain.Singleton.Config.ShowDebug);
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
                    {"Type", nameof(OnCheaterReport)},
                    {"UserId", ev.Issuer.UserId},
                    {"UserName", ev.Issuer.Nickname},
                    {
                        "Message", string.Concat(new string[]
                        {
                            "ingame: ",
                            ev.Issuer.UserId,
                            " report ",
                            ev.Issuer.UserId,
                            " for ",
                            ev.Reason,
                            "."
                        })
                    }
                }
            });
        }

        public void OnRespawn(RespawningTeamEventArgs ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnRespawn)},
                    {"Message", string.Format("Respawn: {0} as {1}.", ev.Players.Count, ev.NextKnownTeam)}
                }
            });
        }

        public void OnReport(LocalReportingEventArgs ev)
        {
            Log.Debug("sending report WR", CedModMain.Singleton.Config.ShowDebug);
            Task.Factory.StartNew(() =>
            {
                Log.Debug("Thread report send", CedModMain.Singleton.Config.ShowDebug);
                if (QuerySystem.Singleton.Config.SecurityKey == "None")
                    return;
                Log.Debug("sending report WR", CedModMain.Singleton.Config.ShowDebug);
                HttpClient client = new HttpClient();
                try
                {
                    var response = client
                        .PostAsync(
                            $"https://{QuerySystem.PanelUrl}/Api/Reports/{QuerySystem.Singleton.Config.SecurityKey}",
                            new StringContent(JsonConvert.SerializeObject(new Dictionary<string, string>()
                                {
                                    {"reporter", ev.Issuer.UserId},
                                    {"reported", ev.Target.UserId},
                                    {"reason", ev.Reason},
                                }), Encoding.Default,
                                "application/json")).Result;
                    Log.Debug(response.Content.ReadAsStringAsync().Result, CedModMain.Singleton.Config.ShowDebug);
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
                    {"Type", nameof(OnReport)},
                    {"UserId", ev.Target.UserId},
                    {"UserName", ev.Target.Nickname},
                    {
                        "Message", string.Concat(new string[]
                        {
                            "ingame: ",
                            ev.Issuer.UserId,
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