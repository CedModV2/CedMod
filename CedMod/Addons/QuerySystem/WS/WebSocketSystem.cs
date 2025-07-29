using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CedMod.Addons.Events.Commands;
using CedMod.Addons.QuerySystem.Commands;
using CedMod.Addons.Sentinal;
using CedMod.Addons.Sentinal.Patches;
using CedMod.Addons.StaffInfo;
using CedMod.ApiModals;
using CedMod.Components;
using CedMod.Patches;
using Exiled.Loader;
using Exiled.Permissions.Extensions;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using Newtonsoft.Json;
using RemoteAdmin;
using Serialization;
using UnityEngine;
using UnityEngine.Networking;
using Utils;
using Utils.NonAllocLINQ;
using VoiceChat;
using Logger = LabApi.Features.Console.Logger;

namespace CedMod.Addons.QuerySystem.WS
{
    public class QueryCommand
    {
        public string Recipient;
        public string Identity;
        public Dictionary<string, string> Data = new Dictionary<string, string>();
    }

    public class WebSocketSystem
    {
        public static event MessageReceived OnMessageReceived;

        public delegate void MessageReceived(QueryCommand cmd);

        public static ClientWebSocket Socket;
        internal static object reconnectLock = new object();
        internal static Thread SendThread;
        public static ConcurrentQueue<QueryCommand> SendQueue = new ConcurrentQueue<QueryCommand>();
        internal static bool Reconnect = false;
        public static HelloMessage HelloMessage = null;
        public static DateTime LastConnection = DateTime.UtcNow;
        public static bool SuppressLog { get; set; } = false;
        public static DateTime LastServerConnection { get; set; }
        public static bool SentMap = false;

        public static void Enqueue(QueryCommand cmd)
        {
            if (!cmd.Data.ContainsKey("UFrame"))
                cmd.Data["UFrame"] = SentinalBehaviour.UFrames.ToString();
            
            if (!cmd.Data.ContainsKey("RoundId"))
                cmd.Data["RoundId"] = SentinalBehaviour.RoundGuid;
            
            WebSocketSystem.SendQueue.Enqueue(cmd);
        }

        public static async Task Stop()
        {
            if (Reconnect)
                return;
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Logger.Debug($"ST3");
            try
            {
                if (!SuppressLog)
                    Logger.Info("Attempting to stop socket");
                await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Stopping", CedModMain.CancellationToken);
                if (!SuppressLog)
                    Logger.Info("Stopped socket");
            }
            catch (Exception e)
            {
            }

            Socket = null;
            SendThread = null;
            SentMap = false;
        }

        public static async Task Start()
        {
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Logger.Debug($"ST4");
            if (Reconnect)
                return;
            Reconnect = true;
            LastConnection = DateTime.UtcNow;
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Logger.Debug($"ST5 {Socket == null}");
            try
            {
                string data1 = "";
                Dictionary<string, string> data2 = new Dictionary<string, string>();
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                    await VerificationChallenge.AwaitVerification();
                    var resp = await client.SendAsync(new HttpRequestMessage()
                    {
                        Method = HttpMethod.Options,
                        RequestUri = new Uri($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Api/v3/QuerySystem/{QuerySystem.QuerySystemKey}"),
                    });
                    data1 = await resp.Content.ReadAsStringAsync();
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        Reconnect = false;
                        SentMap = false;
                        Logger.Error($"Failed to retrieve panel location, API rejected request: {data1}, Retrying");
                        await Task.Delay(2000, CedModMain.CancellationToken);
                        await Start(); //retry until we succeed or the thread gets aborted.
                        return;
                    }

                    data2 = JsonConvert.DeserializeObject<Dictionary<string, string>>(data1);
                    if (!SuppressLog)
                        Logger.Info($"Retrieved panel location from API, Connecting to {data1}");
                }

                QuerySystem.CurrentMaster = data2["Api"];
                QuerySystem.CurrentPanel = data2["Panel"];
                QuerySystem.CurrentMasterQuery = data2["Query"];
            }
            catch (Exception e)
            {
                Reconnect = false;
                SentMap = false;
                Logger.Error($"Failed to retrieve server location\n{e}");
                await Task.Delay(1000, CedModMain.CancellationToken);
                await Start(); //retry until we succeed or the thread gets aborted.
                return;
            }

            try
            {
                Socket = new ClientWebSocket();
                await Socket.ConnectAsync(new Uri($"ws{(QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.CurrentMasterQuery}/QuerySystem?key={QuerySystem.QuerySystemKey}&identity=SCPSL&version=4"), CedModMain.CancellationToken);
                if (!SuppressLog)
                    Logger.Info($"Connected to cedmod panel");
                Reconnect = false;

                LastConnection = DateTime.UtcNow;
                LastServerConnection = DateTime.UtcNow;
                if (SendThread == null || !SendThread.IsAlive)
                {
                    SendThread?.Abort();
                    SendThread = new Thread(HandleSendQueue);
                    SendThread.Start();
                }

                SuppressLog = false;

                var messageBuffer = WebSocket.CreateClientBuffer(4096, 1024);
                using (MemoryStream stream = new MemoryStream())
                {
                    while (Socket.State == WebSocketState.Open)
                    {
                        WebSocketReceiveResult result = null;
                        while (result == null || !result.EndOfMessage)
                        {
                            result = await Socket.ReceiveAsync(messageBuffer, CancellationToken.None);
                            await stream.WriteAsync(messageBuffer.Array, messageBuffer.Offset, result.Count);
                        }

                        LastServerConnection = DateTime.UtcNow;
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            if (result.CloseStatusDescription != null && result.CloseStatusDescription == "LOCATION SWITCH")
                            {
                                Logger.Error($"Lost connection to CedMod Panel Instance location switched");
                                await Task.Delay(2000, CedModMain.CancellationToken);
                                lock (reconnectLock)
                                {
                                    Task.Run(async () =>
                                    {
                                        WebSocketSystem.Reconnect = false;
                                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                            Logger.Debug($"ST2");
                                        await Stop();
                                        await Task.Delay(1000, CedModMain.CancellationToken);
                                        await Start();
                                    });
                                }

                                Socket.Dispose();
                                return;
                            }
                            else if (result.CloseStatusDescription != null && result.CloseStatusDescription == "TERMINATE RECONNECT")
                            {
                                Logger.Error($"Lost connection to CedMod Panel Connection terminated, if Watchdog does not create a reconnect restart the server");
                                await Stop();
                                Socket = null;
                                SendThread?.Abort();
                                SendThread = null;
                            }
                            else
                            {
                                Logger.Error($"Lost connection to CedMod Panel {result.CloseStatus} {result.CloseStatusDescription}, reconnecting in 1000ms");
                                await Task.Delay(5000, CedModMain.CancellationToken);
                                lock (reconnectLock)
                                {
                                    Task.Run(async () =>
                                    {
                                        try
                                        {
                                            WebSocketSystem.Reconnect = false;
                                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                                Logger.Debug($"ST1");
                                            await Stop();
                                            await Task.Delay(1000, CedModMain.CancellationToken);
                                            await Start();
                                        }
                                        catch (Exception e)
                                        {
                                            Logger.Error(e.ToString());
                                        }
                                    });
                                }

                                Socket.Dispose();
                            }
                        }
                        else if (result.MessageType == WebSocketMessageType.Text)
                        {
                            var message = Encoding.UTF8.GetString(stream.ToArray());
                            await OnMessage(message);
                        }

                        stream.Seek(0, SeekOrigin.Begin);
                        stream.SetLength(0);
                        stream.Position = 0;
                    }
                }

                SentMap = false;
                if (Socket.State == WebSocketState.Open)
                    await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "End", CedModMain.CancellationToken);
            }
            catch (Exception e)
            {
                int delay = 1000;
                if (e is WebSocketException e3 && e3.WebSocketErrorCode != WebSocketError.ConnectionClosedPrematurely)
                {
                    Reconnect = false;
                    if (e is WebSocketException e2)
                    {
                        if (e2.WebSocketErrorCode == WebSocketError.Success && e2.Message == "Unable to connect to the remote server")
                            delay = 15000;

                        Logger.Error($"Lost connection to CedMod Panel {e2.WebSocketErrorCode} {e2.Message}, reconnecting in {delay}");
                    }
                    else
                    {
                        Logger.Error(e.ToString());
                    }
                }
                else if (e is WebSocketException e4 && e4.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    SuppressLog = true;

                SentMap = false;
                WebSocketSystem.Reconnect = false;
                await Task.Delay(delay, CedModMain.CancellationToken);
                await Start(); //retry until we succeed or the thread gets aborted.
            }
        }

        public static void HandleSendQueue()
        {
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Logger.Debug("Started SendQueueHandler");
            while (SendThread != null && Socket != null && SendThread.IsAlive && (Socket.State == WebSocketState.Open || Socket.State == WebSocketState.Connecting) && !Shutdown._quitting)
            {
                while (SendQueue.TryDequeue(out QueryCommand cmd))
                {
                    try
                    {
                        if (Socket != null && Socket.State == WebSocketState.Open)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Logger.Debug($"Handling send {Socket.State} {JsonConvert.SerializeObject(cmd)}");
                            var encoded = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(cmd));
                            int bytesSent = 0;

                            while (bytesSent < encoded.Length)
                            {
                                int bytesToSend = Math.Min(1024, encoded.Length - bytesSent);
                                byte[] buffer = new byte[bytesToSend];
                                Array.Copy(encoded, bytesSent, buffer, 0, bytesToSend);
                                Socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, bytesSent + bytesToSend >= encoded.Length, CancellationToken.None).Wait();

                                bytesSent += bytesToSend;
                            }

                            LastConnection = DateTime.UtcNow;
                        }
                        else
                        {
                            SendQueue.Enqueue(cmd);
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Failed to handle queue message, state: {(Socket == null ? "Null Socket" : Socket.State.ToString())}\n{e}");
                        SendQueue.Enqueue(cmd);
                    }
                }

                Thread.Sleep(10);
            }
        }

        static async Task OnMessage(string ev)
        {
            try
            {
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Logger.Debug($"Current queue length: {SendQueue.Count}");
                if (SendThread == null || !SendThread.IsAlive)
                {
                    SendThread?.Abort();
                    SendThread = new Thread(HandleSendQueue);
                    SendThread.Start();
                }

                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Logger.Debug(ev);
                QueryCommand cmd = JsonConvert.DeserializeObject<QueryCommand>(ev);
                OnMessageReceived?.Invoke(cmd);

                var jsonData = cmd.Data;
                string text2 = jsonData["action"];
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Logger.Debug(text2);
                if (text2 != null)
                {
                    switch (text2)
                    {
                        case "watchlistgroupack":
                            RemoteAdminModificationHandler.UpdateWatchList();
                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                            {
                                foreach (var staff in ReferenceHub.AllHubs)
                                {
                                    if (staff.isLocalPlayer || !PermissionsHandler.IsPermitted(staff.serverRoles.Permissions, PlayerPermissions.AdminChat))
                                        continue;

                                    var plr = CedModPlayer.Get(staff);
                                    var watchlistPlr = CedModPlayer.Get(jsonData["UserId"]);
                                    if (watchlistPlr == null)
                                        continue;
                                    string msg = CedModMain.Singleton.Config.QuerySystem.PlayerGroupWatchlistJoin;
                                    if (!RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr) && !RemoteAdminModificationHandler.Singleton.Requesting.Contains(plr.UserId))
                                    {
                                        RemoteAdminModificationHandler.Singleton.ResolvePreferences(plr, () =>
                                        {
                                            if (RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].StreamerMode)
                                                return;
                                            
                                            if (!RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin)
                                                msg += "\n" + CedModMain.Singleton.Config.QuerySystem.StaffReportWatchlistIngameDisabled;
                                            
                                            Broadcast.Singleton.TargetAddElement(plr.ReferenceHub.connectionToClient,
                                                msg.Replace("{playerId}", $"{watchlistPlr.PlayerId}")
                                                    .Replace("{userId}", RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowUserIdsInBroadcasts ? watchlistPlr.UserId : "REDACTED")
                                                    .Replace("{playerName}", watchlistPlr.Nickname)
                                                    .Replace("{reason}", jsonData["Reason"])
                                                    .Replace("{groups}", jsonData["Groups"]),
                                                RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin == true ? (ushort)5 : (ushort)10, Broadcast.BroadcastFlags.AdminChat);
                                        });
                                    }
                                    else if (RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr))
                                    {
                                        if (!RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].StreamerMode)
                                            return;
                                                
                                        if (!RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin)
                                            msg += "\n" + CedModMain.Singleton.Config.QuerySystem.StaffReportWatchlistIngameDisabled;
                                        Broadcast.Singleton.TargetAddElement(plr.ReferenceHub.connectionToClient, msg.Replace("{playerId}", $"{watchlistPlr.PlayerId}").Replace("{userId}", RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowUserIdsInBroadcasts ? watchlistPlr.UserId : "REDACTED").Replace("{playerName}", watchlistPlr.Nickname).Replace("{reason}", jsonData["Reason"]).Replace("{groups}", jsonData["Groups"]), RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin == true ? (ushort)5 : (ushort)10, Broadcast.BroadcastFlags.AdminChat);
                                    }

                                    staff.encryptedChannelManager.TrySendMessageToClient($"0!{CedModMain.Singleton.Config.QuerySystem.PlayerWatchGrouplistJoinStaffChat.Replace("{playerId}", $"{watchlistPlr.PlayerId}").Replace("{playerName}", watchlistPlr.Nickname).Replace("{reason}", jsonData["Reason"]).Replace("{userId}", watchlistPlr.UserId).Replace("{userId}", watchlistPlr.UserId).Replace("{groups}", jsonData["Groups"])}", EncryptedChannelManager.EncryptedChannel.AdminChat);
                                }
                            });
                            break;
                        case "watchlistack":
                            RemoteAdminModificationHandler.UpdateWatchList();
                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                            {
                                foreach (var staff in ReferenceHub.AllHubs)
                                {
                                    if (staff.isLocalPlayer || !PermissionsHandler.IsPermitted(staff.serverRoles.Permissions, PlayerPermissions.AdminChat))
                                        continue;

                                    var plr = CedModPlayer.Get(staff);
                                    var watchlistPlr = CedModPlayer.Get(jsonData["UserId"]);
                                    if (watchlistPlr == null)
                                        continue;
                                    string msg = CedModMain.Singleton.Config.QuerySystem.PlayerWatchlistJoin;
                                    if (!RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr) && !RemoteAdminModificationHandler.Singleton.Requesting.Contains(plr.UserId))
                                    {
                                        RemoteAdminModificationHandler.Singleton.ResolvePreferences(plr, () =>
                                        {
                                            if (RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].StreamerMode)
                                                return;
                                            
                                            if (!RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin)
                                                msg += "\n" + CedModMain.Singleton.Config.QuerySystem.StaffReportWatchlistIngameDisabled;
                                            
                                            Broadcast.Singleton.TargetAddElement(plr.ReferenceHub.connectionToClient,
                                                msg.Replace("{playerId}", $"{watchlistPlr.PlayerId}")
                                                    .Replace("{playerName}", watchlistPlr.Nickname)
                                                    .Replace("{reason}", jsonData["Reason"])
                                                    .Replace("{userId}", RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowUserIdsInBroadcasts ? watchlistPlr.UserId : "REDACTED"),
                                                RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin == true ? (ushort)5 : (ushort)10, Broadcast.BroadcastFlags.AdminChat);
                                        });
                                    }
                                    else if (RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr))
                                    {
                                        if (!RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].StreamerMode)
                                            return;
                                        
                                        if (!RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin)
                                            msg += "\n" + CedModMain.Singleton.Config.QuerySystem.StaffReportWatchlistIngameDisabled;
                                        Broadcast.Singleton.TargetAddElement(plr.ReferenceHub.connectionToClient, msg.Replace("{playerId}", $"{watchlistPlr.PlayerId}").Replace("{playerName}", watchlistPlr.Nickname).Replace("{reason}", jsonData["Reason"]).Replace("{userId}", watchlistPlr.UserId).Replace("{userId}", RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowUserIdsInBroadcasts ? watchlistPlr.UserId : "REDACTED"), RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin == true ? (ushort)5 : (ushort)10, Broadcast.BroadcastFlags.AdminChat);
                                    }

                                    staff.encryptedChannelManager.TrySendMessageToClient($"0!{CedModMain.Singleton.Config.QuerySystem.PlayerWatchlistJoinStaffChat.Replace("{playerId}", $"{watchlistPlr.PlayerId}").Replace("{playerName}", watchlistPlr.Nickname).Replace("{reason}", jsonData["Reason"]).Replace("{userId}", watchlistPlr.UserId).Replace("{userId}", watchlistPlr.UserId)}", EncryptedChannelManager.EncryptedChannel.AdminChat);
                                }
                            });
                            break;
                        case "updateuserpref":
                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                            {
                                var toRefresh = CedModPlayer.Get(jsonData["UserId"]);
                                if (toRefresh == null || RemoteAdminModificationHandler.Singleton.Requesting.Contains(toRefresh.UserId))
                                    return;
                                RemoteAdminModificationHandler.Singleton.ResolvePreferences(toRefresh, () =>
                                {
                                    ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                                    {
                                        StaffInfoHandler.StaffData.Remove(toRefresh.UserId);
                                        StaffInfoHandler.Requested.Remove(toRefresh.UserId);
                                    });
                                });
                            });
                            break;
                        case "reportstateack":
                        case "reportack":
                            Logger.Info("Updating Reports list from panel ack.");
                            Task.Run(() =>
                            {
                                RemoteAdminModificationHandler.UpdateReportList();
                                if (text2 == "reportack")
                                {
                                    ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                                    {
                                        foreach (var staff in ReferenceHub.AllHubs)
                                        {
                                            if (staff.isLocalPlayer || !PermissionsHandler.IsPermitted(staff.serverRoles.Permissions, PlayerPermissions.AdminChat))
                                                continue;

                                            var plr = CedModPlayer.Get(staff);
                                            string msg = CedModMain.Singleton.Config.QuerySystem.StaffReportNotification;
                                            if (!RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr) && !RemoteAdminModificationHandler.Singleton.Requesting.Contains(plr.UserId))
                                            {
                                                RemoteAdminModificationHandler.Singleton.ResolvePreferences(plr, () =>
                                                {
                                                    if (!RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowReportsInRemoteAdmin)
                                                        msg += "\n" + CedModMain.Singleton.Config.QuerySystem.StaffReportNotificationIngameDisabled;
                                                    Broadcast.Singleton.TargetAddElement(plr.ReferenceHub.connectionToClient, msg.Replace("{reporterName}", $"{jsonData["ReporterName"]} {(RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowUserIdsInBroadcasts ? jsonData["Reporter"] : "REDACTED")}").Replace("{reportedName}", $"{jsonData["ReportedName"]} {(RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowUserIdsInBroadcasts ? jsonData["Reported"] : "REDACTED")}").Replace("{checkType}", RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowReportsInRemoteAdmin ? "RemoteAdmin" : "Discord"), RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowReportsInRemoteAdmin == true ? (ushort)5 : (ushort)10, Broadcast.BroadcastFlags.AdminChat);
                                                });
                                            }
                                            else if (RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr))
                                            {
                                                if (!RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowReportsInRemoteAdmin)
                                                    msg += "\n" + CedModMain.Singleton.Config.QuerySystem.StaffReportNotificationIngameDisabled;
                                                Broadcast.Singleton.TargetAddElement(plr.ReferenceHub.connectionToClient, msg.Replace("{reporterName}", $"{jsonData["ReporterName"]} {(RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowUserIdsInBroadcasts ? jsonData["Reporter"] : "REDACTED")}").Replace("{reportedName}", $"{jsonData["ReportedName"]} {(RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowUserIdsInBroadcasts ? jsonData["Reported"] : "REDACTED")}").Replace("{checkType}", RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowReportsInRemoteAdmin ? "RemoteAdmin" : "Discord"), RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowReportsInRemoteAdmin == true ? (ushort)5 : (ushort)10, Broadcast.BroadcastFlags.AdminChat);
                                            }

                                            staff.encryptedChannelManager.TrySendMessageToClient($"0!{CedModMain.Singleton.Config.QuerySystem.StaffReportAdminChatMessage.Replace("{reporterName}", $"{jsonData["ReporterName"]} {jsonData["Reporter"]}").Replace("{reportedName}", $"{jsonData["ReportedName"]} {jsonData["Reported"]}").Replace("{checkType}", RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowReportsInRemoteAdmin ? "RemoteAdmin" : "Discord").Replace("{reason}", jsonData["Reason"])}", EncryptedChannelManager.EncryptedChannel.AdminChat);
                                        }
                                    });
                                }
                                else
                                {
                                    ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                                    {
                                        var plr = CedModPlayer.Get(jsonData["ReporterId"]);
                                        if (plr != null)
                                        {
                                            var msg = CedModMain.Singleton.Config.QuerySystem.PlayerReportUpdateNotification;
                                            Broadcast.Singleton.TargetAddElement(plr.ReferenceHub.connectionToClient, msg.Replace("{reportedName}", $"{jsonData["ReportedName"]}").Replace("{reportState}", $"{jsonData["NewStatus"]}").Replace("{handlerName}", jsonData["Handler"]), 10, Broadcast.BroadcastFlags.Normal);
                                        }

                                        foreach (var staff in ReferenceHub.AllHubs)
                                        {
                                            if (staff.isLocalPlayer || !PermissionsHandler.IsPermitted(staff.serverRoles.Permissions, PlayerPermissions.PlayersManagement))
                                                continue;

                                            staff.encryptedChannelManager.TrySendMessageToClient($"0!{CedModMain.Singleton.Config.QuerySystem.StaffReportAdminChatUpdateMessage.Replace("{reporterName}", $"{jsonData["ReporterName"]} {jsonData["ReporterId"]}").Replace("{reportedName}", $"{jsonData["ReportedName"]} {jsonData["ReportedId"]}").Replace("{state}", jsonData["NewStatus"]).Replace("{reason}", jsonData.TryGetValue("Reason", out var value) ? value : "Not found").Replace("{handler}", jsonData["Handler"])}", EncryptedChannelManager.EncryptedChannel.AdminChat);
                                        }
                                    });
                                }
                            });
                            break;
                        case "ping":
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Logger.Debug("IsPing");

                            SendQueue.Enqueue(new QueryCommand()
                            {
                                Recipient = cmd.Recipient,
                                Data = new Dictionary<string, string>()
                                {
                                    { "Message", "PONG" }
                                }
                            });
                            return;
                        case "custom":
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Logger.Debug("CustomCommand");
                            if (!jsonData.ContainsKey("command"))
                            {
                                SendQueue.Enqueue(new QueryCommand()
                                {
                                    Recipient = cmd.Recipient,
                                    Data = new Dictionary<string, string>()
                                    {
                                        { "Message", "Missing argument" }
                                    }
                                });
                            }
                            else
                            {
                                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                    Logger.Debug("CustomCommand");
                                string[] array = jsonData["command"].Split(new char[]
                                {
                                    ' '
                                });

                                if (jsonData["command"].ToUpper().Contains("REQUEST_DATA AUTH") || jsonData["command"].ToUpper().Contains("SUDO QUIT"))
                                {
                                    SendQueue.Enqueue(new QueryCommand()
                                    {
                                        Recipient = cmd.Recipient,
                                        Data = new Dictionary<string, string>()
                                        {
                                            { "Message", "This command is disabled" }
                                        }
                                    });
                                    return;
                                }

                                if (CedModMain.Singleton.Config.QuerySystem.DisallowedWebCommands.Contains(array[0].ToUpper()))
                                {
                                    SendQueue.Enqueue(new QueryCommand()
                                    {
                                        Recipient = cmd.Recipient,
                                        Data = new Dictionary<string, string>()
                                        {
                                            { "Message", "This command is disabled" }
                                        }
                                    });
                                    return;
                                }

                                if (CedModMain.Singleton.Config.QuerySystem.RejectRemoteCommands)
                                {
                                    SendQueue.Enqueue(new QueryCommand()
                                    {
                                        Recipient = cmd.Recipient,
                                        Data = new Dictionary<string, string>()
                                        {
                                            { "Message", "Remote commands is disabled, features may not function as intended, to resolve, enabled RejectRemoteCommands in the QueryServer section of the CedMod plugin config" }
                                        }
                                    });
                                    return;
                                }

                                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                    Logger.Debug("CustomCommandCheckPerm");
                                if (ServerStatic.PermissionsHandler.Members.ContainsKey(jsonData["user"]))
                                {
                                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                        Logger.Debug("CustomCommandPermGood");
                                    string name = ServerStatic.PermissionsHandler.Members[jsonData["user"]];
                                    UserGroup ugroup = ServerStatic.PermissionsHandler.GetGroup(name);
                                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                        Logger.Debug("CustomCommandPermGood, dispatching");
                                    switch (jsonData["command"].Split(' ')[0].ToUpper())
                                    {
                                        default:
                                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(delegate
                                            {
                                                var sender = new CmSender(cmd.Recipient, jsonData["user"], jsonData["user"], ugroup);
                                                CommandProcessor.ProcessQuery(jsonData["command"], sender);
                                            });
                                            break;
                                    }
                                }
                                else
                                {
                                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                        Logger.Debug("CustomCommandPermBad, returning");

                                    SendQueue.Enqueue(new QueryCommand()
                                    {
                                        Recipient = cmd.Recipient,
                                        Data = new Dictionary<string, string>()
                                        {
                                            { "Message", "Userid not present in RA config, permission denied" }
                                        }
                                    });
                                }
                            }

                            break;
                        case "kicksteamid":
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Logger.Debug("KickCmd");
                            foreach (ReferenceHub player in ReferenceHub.AllHubs)
                            {
                                if (player.authManager.UserId == jsonData["steamid"])
                                {
                                    ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => Timing.RunCoroutine(API.StrikeBad(CedModPlayer.Get(player), jsonData["reason"] + "\n" + CedModMain.Singleton.Config.CedMod.AdditionalBanMessage)));
                                    SendQueue.Enqueue(new QueryCommand()
                                    {
                                        Recipient = cmd.Recipient,
                                        Data = new Dictionary<string, string>()
                                        {
                                            { "Message", "User kicked" }
                                        }
                                    });
                                    return;
                                }
                            }

                            SendQueue.Enqueue(new QueryCommand()
                            {
                                Recipient = cmd.Recipient,
                                Data = new Dictionary<string, string>()
                                {
                                    { "Message", "User not found" }
                                }
                            });
                            break;
                        case "mutesteamid":
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Logger.Debug("MuteCmd");
                            var plr = CedModPlayer.Get(jsonData["steamid"]);
                            if (plr != null)
                            {
                                ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                                {
                                    Logger.Info($"user: {plr.UserId} muted through panel, issuing mute...");
                                    Enum.TryParse(jsonData["type"], out MuteType muteType);
                                    plr.SendConsoleMessage(CedModMain.Singleton.Config.CedMod.MuteMessage.Replace("{type}", muteType.ToString()).Replace("{duration}", jsonData["duration"]).Replace("{reason}", jsonData["reason"]), "red");
                                    Broadcast.Singleton.TargetAddElement(plr.Connection, CedModMain.Singleton.Config.CedMod.MuteMessage.Replace("{type}", muteType.ToString()).Replace("{duration}", jsonData["duration"]).Replace("{reason}", jsonData["reason"]), 5, Broadcast.BroadcastFlags.Normal);
                                    // if (muteType == MuteType.Global)
                                    //     plr.Mute(true);
                                    //
                                    // if (muteType == MuteType.Intercom)
                                    //     plr.IntercomMute(true);

                                    if (muteType == MuteType.Global)
                                    {
                                        VoiceChatMutes.SetFlags(plr.ReferenceHub, VcMuteFlags.LocalRegular);
                                    }

                                    if (muteType == MuteType.Intercom)
                                    {
                                        VoiceChatMutes.SetFlags(plr.ReferenceHub, VcMuteFlags.LocalIntercom);
                                    }

                                    if (!string.IsNullOrEmpty(CedModMain.Singleton.Config.CedMod.MuteCustomInfo))
                                        plr.CustomInfo = CedModMain.Singleton.Config.CedMod.MuteCustomInfo.Replace("{type}", muteType.ToString());
                                    SendQueue.Enqueue(new QueryCommand()
                                    {
                                        Recipient = cmd.Recipient,
                                        Data = new Dictionary<string, string>()
                                        {
                                            { "Message", "User muted" }
                                        }
                                    });
                                    return;
                                });
                            }

                            SendQueue.Enqueue(new QueryCommand()
                            {
                                Recipient = cmd.Recipient,
                                Data = new Dictionary<string, string>()
                                {
                                    { "Message", "User not found" }
                                }
                            });
                            break;
                        case "hello":
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Logger.Debug($"Received hello: {ev}");
                            HelloMessage = JsonConvert.DeserializeObject<HelloMessage>(jsonData["data"]);
                            break;
                        case "ImportRA":
                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                            {
                                List<SLPermissionEntry> Groups = new List<SLPermissionEntry>();
                                foreach (var group in ServerStatic.PermissionsHandler.Groups)
                                {
                                    //var perms = Permissions.Groups.FirstOrDefault(s => s.Key == group.Key);
                                    Groups.Add(new SLPermissionEntry()
                                    {
                                        Name = group.Key,
                                        KickPower = group.Value.KickPower,
                                        RequiredKickPower = group.Value.RequiredKickPower,
                                        Hidden = group.Value.HiddenByDefault,
                                        Cover = group.Value.Cover,
                                        ReservedSlot = false,
                                        BadgeText = group.Value.BadgeText,
                                        BadgeColor = group.Value.BadgeColor,
                                        RoleId = 0,
                                        Permissions = (PlayerPermissions)group.Value.Permissions,
                                        ExiledPermissions = new List<string>(),
                                        //perms.Value == null ? new List<string>() : perms.Value.CombinedPermissions
                                    });
                                }

                                SendQueue.Enqueue(new QueryCommand()
                                {
                                    Recipient = cmd.Recipient,
                                    Data = new Dictionary<string, string>()
                                    {
                                        { "Message", "ImportRAResponse" },
                                        { "Data", JsonConvert.SerializeObject(Groups) }
                                    }
                                });
                            });
                            break;
                        case "ApplyRA":
                            //Logger.Info($"Panel requested AutoSlPerms reload: {jsonData["reason"]}");
                            new Thread(ApplyRa).Start(true);
                            //Logger.Info($"Panel requested AutoSlPerms reload: {jsonData["reason"]}");
                            new Thread(() =>
                            {
                                try
                                {
                                    ApplyRa(true);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    Logger.Error(e.ToString());
                                }
                            }).Start();
                            break;
                        case "FetchApiKey":
                            Logger.Info($"Panel requested refresh of api key: {jsonData["Reason"]}");
                            Task.Run(async () =>
                            {
                                using (HttpClient client = new HttpClient())
                                {
                                    client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                                    await VerificationChallenge.AwaitVerification();
                                    var response = await client.PostAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Api/v3/FetchKey/{QuerySystem.QuerySystemKey}",
                                        new StringContent(JsonConvert.SerializeObject(new
                                        {
                                            Hash = jsonData["Hash"]
                                        }), Encoding.UTF8, "application/json"));

                                    if (response.IsSuccessStatusCode)
                                    {
                                        CedModMain.Singleton.Config.CedMod.CedModApiKey = await response.Content.ReadAsStringAsync();
#if !EXILED
                                        File.WriteAllText(Path.Combine(CedModMain.PluginConfigFolder, "config.yml"), YamlParser.Serializer.Serialize(CedModMain.Singleton.Config));
#else
                                        var pluginConfigs = ConfigManager.LoadSorted(ConfigManager.Read());
                                        if (pluginConfigs[CedModMain.Singleton.Prefix] is Config cedModCnf)
                                        {
                                            cedModCnf.CedMod.CedModApiKey = await response.Content.ReadAsStringAsync();
                                            ConfigManager.Save(pluginConfigs);
                                        }
#endif
                                        Logger.Info($"Successfully saved apikey");
                                    }
                                    else
                                    {
                                        Logger.Error($"Failed to fetch key: {await response.Content.ReadAsStringAsync()}");
                                    }
                                }
                            });
                            break;
                        case "HideVerificationString":
                            try
                            {
                                if (!CustomNetworkManager.IsVerified || IsCheckingToken)
                                    return;
                                
                                ReloadServerNamePatch.IncludeString = false;
                                ServerConsole.ReloadServerName();
                                ServerConsole.Update = true;
                            }
                            catch (Exception e)
                            {
                                Logger.Info(e.ToString());
                            }
                            break;
                        case "VerificationCheck":
                            try
                            {
                                if (!CustomNetworkManager.IsVerified || IsCheckingToken)
                                    return;
                                IsCheckingToken = true;

                                var delay = !ServerConsole.ServerName.Contains("CedModVerification");

                                Verification.ServerId = Convert.ToInt32(jsonData["ServerId"]);
                                ReloadServerNamePatch.IncludeString = true;
                                ServerConsole.ReloadServerName();
                                ServerConsole.Update = true;

                                if (delay)
                                    Task.Run(async () =>
                                    {
                                        try
                                        {
                                            await ServerPreferences.WaitForSecond(90000, CedModMain.CancellationToken, (o) => !Shutdown._quitting && CedModMain.Singleton.CacheHandler != null);
                                            await HandleVerification();
                                        }
                                        catch (Exception e)
                                        {
                                            Logger.Error($"Panel requested verification check however check failed, {e.ToString()}");
                                            SendQueue.Enqueue(new QueryCommand()
                                            {
                                                Data = new Dictionary<string, string>()
                                                {
                                                    { "Message", "VERIFICATIONFEEDBACK" },
                                                    { "Result", e.ToString() }
                                                },
                                                Identity = "",
                                                Recipient = "PANEL",
                                            });
                                        }
                                    });
                                else
                                {
                                    await HandleVerification();
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Error($"Panel requested verification check however check failed, {e.ToString()}");
                                SendQueue.Enqueue(new QueryCommand()
                                {
                                    Data = new Dictionary<string, string>()
                                    {
                                        { "Message", "VERIFICATIONFEEDBACK" },
                                        { "Result", e.ToString() }
                                    },
                                    Identity = "",
                                    Recipient = "PANEL",
                                });
                            }
                            
                            IsCheckingToken = false;
                            break;
                        case "reloadsrvprefs":
                            await ServerPreferences.ResolvePreferences(false);
                            break;
                        case "requestHeartbeat":
                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => { ThreadDispatcher.SendHeartbeatMessage(true); });
                            break;
                        case "requestMap":
                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => { QueryServerEvents.CreateMapLayout(); });
                            break;
                        case "sentinalteslakill":
                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                            {
                                var plrKill = Player.Get(jsonData["UserId"]);
                                if (plrKill == null)
                                    return;
                            
                                if (!TeslaGateHandler.TeslaKills.ContainsKey(plrKill.UserId) || (DateTime.UtcNow - TeslaGateHandler.TeslaKills[plrKill.UserId].time).TotalSeconds >= 10)
                                    return;

                                int count = TeslaGateHandler.TeslaKills[plrKill.UserId].amount;
                                while (count > 0)
                                {
                                    count--;
                                    TeslaGateController.ServerReceiveMessage(plrKill.Connection, new TeslaHitMsg(TeslaGateHandler.TeslaKills[plrKill.UserId].gate));
                                }
                            });
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        private static async Task HandleVerification()
        {
            string result = await Verification.ConfirmId();
            if (result != string.Empty)
                Logger.Error($"Panel requested verification check however check failed, {result}");
            SendQueue.Enqueue(new QueryCommand()
            {
                Data = new Dictionary<string, string>()
                {
                    { "Message", "VERIFICATIONFEEDBACK" },
                    { "Result", $"{ServerConsole.Ip} {(ServerConsole.PortOverride == 0 ? ServerStatic.ServerPort : ServerConsole.PortOverride)} {result}" }
                },
                Identity = "",
                Recipient = "PANEL",
            });
        }

        public static bool IsCheckingToken = false;
        public static bool UseRa = true;
        public static object LockObj = new object();

        public static void ApplyRa(object? state)
        {
            if (state == null)
                state = true;

            bool request = (bool)state;
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Logger.Debug("Attempting to get permissions");

            lock (LockObj)
            {
                AutoSlPermsSlRequest permsSlRequest = null;
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                        VerificationChallenge.AwaitVerification().Wait();

                        try
                        {
                            if (!request)
                                throw new Exception("Request is false");
                            var responseWhitelist = client.SendAsync(new HttpRequestMessage()
                            {
                                Method = HttpMethod.Options,
                                RequestUri = new Uri($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Api/v3/GetWhitelist/{QuerySystem.QuerySystemKey}"),
                            }).Result;

                            if (responseWhitelist.IsSuccessStatusCode)
                            {
                                var data = responseWhitelist.Content.ReadAsStringAsync().Result;
                                List<string> whitelist = JsonConvert.DeserializeObject<List<string>>(data);
                                QuerySystem.Whitelist = whitelist;
                                QuerySystem.UseWhitelist = true;
                                ServerConsole.WhiteListEnabled = true;
                                if (Directory.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod")))
                                {
                                    File.WriteAllText(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "whitelistCache.json"), data);
                                }

                                Logger.Info("Loaded whitelist from Api");
                                CheckWhitelist();
                            }
                            else if (responseWhitelist.StatusCode == HttpStatusCode.BadRequest)
                            {
                                QuerySystem.Whitelist = new List<string>();
                                QuerySystem.UseWhitelist = false;
                                ServerConsole.WhiteListEnabled = false;
                                if (Directory.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod")))
                                {
                                    if (File.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "whitelistCache.json")))
                                    {
                                        File.Delete(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "whitelistCache.json"));
                                        Logger.Info("Removed whitelist as whitelist is inactive.");
                                    }
                                }
                            }
                            else
                            {
                                if (Directory.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod")))
                                {
                                    if (File.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "whitelistCache.json")))
                                    {
                                        List<string> whitelist = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "whitelistCache.json")));
                                        QuerySystem.Whitelist = whitelist;
                                        QuerySystem.UseWhitelist = true;
                                        ServerConsole.WhiteListEnabled = true;
                                        Logger.Info("Loaded whitelist from file");
                                        CheckWhitelist();
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (request)
                                Logger.Error($"Failed to check for whitelist {e}");

                            if (Directory.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod")))
                            {
                                if (File.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "whitelistCache.json")))
                                {
                                    List<string> whitelist = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "whitelistCache.json")));
                                    QuerySystem.Whitelist = whitelist;
                                    QuerySystem.UseWhitelist = true;
                                    Logger.Info("Loaded whitelist from file");
                                    CheckWhitelist();
                                }
                            }
                        }

                        if (!request)
                            throw new Exception("Request is false");
                        
                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Logger.Debug("Attempting to send permission request");

                        var responsePerms = client.SendAsync(new HttpRequestMessage()
                        {
                            Method = HttpMethod.Options,
                            RequestUri = new Uri($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Api/v3/GetPermissions/{QuerySystem.QuerySystemKey}"),
                        }).Result;
                        if (!responsePerms.IsSuccessStatusCode)
                        {
                            if (responsePerms.Content.ReadAsStringAsync().Result == "No entries defined.")
                            {
                                if (!UseRa)
                                    ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                                UseRa = true;
                                if (Directory.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod")))
                                {
                                    if (File.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "autoSlPermCache.json")))
                                        File.Delete(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "autoSlPermCache.json"));
                                }

                                return;
                            }

                            Logger.Error($"Failed to request RA: {responsePerms.Content.ReadAsStringAsync().Result}");
                            responsePerms.EnsureSuccessStatusCode();
                        }

                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Logger.Debug("Got permissions");
                        permsSlRequest = JsonConvert.DeserializeObject<AutoSlPermsSlRequest>(responsePerms.Content.ReadAsStringAsync().Result);
                    }

                    if (permsSlRequest.PermissionEntries.Count == 0)
                    {
                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Logger.Debug("Permission request contained no permissions");
                        if (!UseRa)
                            ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                        UseRa = true;
                        if (Directory.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod")))
                        {
                            if (File.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "autoSlPermCache.json")))
                                File.Delete(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "autoSlPermCache.json"));
                        }

                        return;
                    }

                    if (!Directory.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod")))
                        Directory.CreateDirectory(Path.Combine(CedModMain.PluginConfigFolder, "CedMod"));
                    File.WriteAllText(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "autoSlPermCache.json"), JsonConvert.SerializeObject(permsSlRequest));
                    UseRa = false;
                }
                catch (Exception e)
                {
                    if (!Directory.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod")))
                        Directory.CreateDirectory(Path.Combine(CedModMain.PluginConfigFolder, "CedMod"));
                    if (File.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "autoSlPermCache.json")))
                    {
                        UseRa = false;
                        if (request)
                            Logger.Error($"Failed to fetch RA from panel, using cache...\n{e}");
                        permsSlRequest = JsonConvert.DeserializeObject<AutoSlPermsSlRequest>(File.ReadAllText(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "autoSlPermCache.json")));
                    }
                    else
                    {
                        UseRa = true;
                        if (request && File.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "autoSlPermCache.json")))
                            Logger.Error($"Failed to fetch RA from panel, using RA...\n{e}");
                        return;
                    }
                }
                
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Logger.Debug("Attempting to invoke thread dispatacher");

                ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                {
                    PermissionProvider.Permissions = permsSlRequest;
                    try
                    {
                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Logger.Debug("Permission thread dispatched");
                        var handler = ServerStatic.PermissionsHandler;
                        var oldMembers = new Dictionary<string, string>(handler.Members);
                        var oldGroups = new Dictionary<string, UserGroup>(handler.Groups);
                        handler.Groups.Clear();
                        handler.Members.Clear();
#if EXILED
                        Permissions.Groups.Clear();
                        var epGroup = new Exiled.Permissions.Features.Group();
                        epGroup.Permissions.AddRange(permsSlRequest.DefaultPermissions);
                        epGroup.CombinedPermissions.AddRange(permsSlRequest.DefaultPermissions);
                        epGroup.Inheritance.Clear();
                        epGroup.IsDefault = true;
                        Permissions.Groups.Add("default", epGroup);
#endif
                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Logger.Debug("Attempting to add groups");
                        foreach (var perm in permsSlRequest.PermissionEntries)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Logger.Debug($"Attempting to add {perm.Name}");
                            handler.Groups.Add(perm.Name, new UserGroup()
                            {
                                BadgeColor = string.IsNullOrEmpty(perm.BadgeColor) ? "none" : perm.BadgeColor,
                                BadgeText = perm.BadgeText,
                                Cover = perm.Cover,
                                HiddenByDefault = perm.Hidden,
                                KickPower = (byte)perm.KickPower,
                                Permissions = (ulong)perm.Permissions,
                                RequiredKickPower = (byte)perm.RequiredKickPower,
                                Shared = false
                            });

#if EXILED
                            var epGroup1 = new Exiled.Permissions.Features.Group();
                            epGroup1.Permissions.AddRange(perm.ExiledPermissions);
                            epGroup1.CombinedPermissions.AddRange(perm.ExiledPermissions);
                            epGroup1.Inheritance.Clear();
                            epGroup1.IsDefault = false;
                            Permissions.Groups.Add(perm.Name, epGroup1);
#endif
                        }

                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Logger.Debug("Attempting to add members");
                        foreach (var member in permsSlRequest.MembersList)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Logger.Debug($"Attempting to add {member.UserId} - {member.Group}");
                            
                            if (member.ReservedSlot && !QuerySystem.ReservedSlotUserids.Contains(member.UserId))
                                QuerySystem.ReservedSlotUserids.Add(member.UserId);
                            handler.Members.Add(member.UserId, member.Group);

                            var player = CedModPlayer.Get(member.UserId);
                            try
                            {
                                if (player != null)
                                {
                                    if (oldMembers.ContainsKey(member.UserId))
                                    {
                                        UserGroup group = null;
                                        if (oldMembers.ContainsKey(member.UserId))
                                            group = oldGroups[oldMembers[member.UserId]];
                                        var newGroup = permsSlRequest.PermissionEntries.FirstOrDefault(s => s.Name == member.Group);

                                        if (group == null || oldMembers[member.UserId] != member.Group || group.Permissions != (ulong)newGroup.Permissions || group.BadgeText != newGroup.BadgeText || group.BadgeColor != newGroup.BadgeColor || group.KickPower != newGroup.KickPower || group.RequiredKickPower != newGroup.RequiredKickPower || group.Cover != newGroup.Cover || group.HiddenByDefault != newGroup.Hidden)
                                        {
                                            var hidden = !string.IsNullOrEmpty(player.ReferenceHub.serverRoles.HiddenBadge);
                                            player.ReferenceHub.serverRoles.SetGroup(handler.Groups[member.Group], false, !hidden);
                                            Logger.Info($"Refreshed Permissions from {member.UserId} with hidden value: {hidden} as they were present and had changes in the AutoSlPerms response while ingame");
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Error($"Failed to apply permission at realtime for {player.UserId} - {player.Nickname}");
                            }
                        }
                        
                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Logger.Debug("Attempting to process old members");

                        foreach (var member in oldMembers)
                        {
                            if (Enumerable.All(permsSlRequest.MembersList, s => s.UserId != member.Key) && QuerySystem.ReservedSlotUserids.Contains(member.Key))
                            {
                                QuerySystem.ReservedSlotUserids.Remove(member.Key);
                            }

                            if (CedModPlayer.Get(member.Key) != null)
                            {
                                if (Enumerable.All(permsSlRequest.MembersList, s => s.UserId != member.Key))
                                {
                                    Logger.Info(member.Key + " 3");
                                    CedModPlayer.Get(member.Key).ReferenceHub.serverRoles.SetGroup(null, false);
                                    Logger.Info($"Removed Permissions from {member.Key} as they were no longer present in the AutoSlPerms response while ingame");
                                }
                            }
                        }
                        //Logger.Info($"Successfully applied {permsSlRequest.PermissionEntries.Count} Groups and {permsSlRequest.MembersList.Count} members for AutoSlPerms");
                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Logger.Debug($"Successfully applied {permsSlRequest.PermissionEntries.Count} Groups and {permsSlRequest.MembersList.Count} members for AutoSlPerms");
                    }
                    catch (Exception e)
                    {
                        UseRa = true;
                        Logger.Error($"Failed to fetch RA from panel, using RA...\n{e}");
                        ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                    }
                });
            }
        }

        private static void CheckWhitelist()
        {
            try
            {
                if (QuerySystem.UseWhitelist)
                {
                    foreach (ReferenceHub plr in ReferenceHub.AllHubs)
                    {
                        if (plr.isLocalPlayer)
                            continue;

                        if (!plr.authManager.NorthwoodStaff && plr.authManager.UserId != null && !plr.authManager.UserId.EndsWith("@northwood") && !QuerySystem.Whitelist.Contains(plr.authManager.UserId) && !WhiteList.Users.Contains(plr.authManager.UserId))
                        {
                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(new Action(() =>
                            {
                                try
                                {
                                    Timing.RunCoroutine(API.StrikeBad(CedModPlayer.Get(plr),
                                        "You are not whitelisted on this server."));
                                }
                                catch (Exception e)
                                {
                                    Logger.Error($"Failed to kick user {e}");
                                }
                            }));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to check whitelist: {e}");
            }
        }
    }

    public class CmSender : CommandSender
    {
        public const int chunksize = 1000;

        public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay)
        {
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Logger.Debug($"RA Reply: {text} Suc: {success}");
            SendText(text);
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Logger.Debug("Added RA message to queue");
        }

        public override void Print(string text)
        {
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Logger.Debug($"RA Print: {text}");
            SendText(text);
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Logger.Debug("Added RA message to queue");
        }

        public void SendText(string text)
        {
            /*if (text.Length >= chunksize)
            {
                IEnumerable<string> chunks = Enumerable.Range(0, text.Length / chunksize).Select(i => text.Substring(i * chunksize, chunksize));
                foreach (var str in chunks) //WS becomes unstable if we send a large chunk of text
                {
                    WebSocketSystem.Enqueue(new QueryCommand()
                    {
                        Recipient = Ses,
                        Data = new Dictionary<string, string>()
                        {
                            {"Message", str}
                        }
                    });
                }
            }
            else*/
            {
                WebSocketSystem.Enqueue(new QueryCommand()
                {
                    Recipient = Ses,
                    Data = new Dictionary<string, string>()
                    {
                        { "Message", text }
                    }
                });
            }
        }

        public CmSender(string ses, string name, string userid, UserGroup group)
        {
            Ses = ses;
            Name = name;
            senderId = userid;
            permissions = group.Permissions;
            fullPermissions = false;
        }

        public override bool Available()
        {
            return true;
        }

        public override void Respond(string text, bool success = true)
        {
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Logger.Debug($"RA Response: {text} Suc: {success}");
            SendText(text);
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Logger.Debug("Added RA message to queue");
        }

        public override string SenderId
        {
            get { return senderId; }
        }

        public override string Nickname
        {
            get { return Name; }
        }

        public override ulong Permissions
        {
            get { return permissions; }
        }

        public override byte KickPower
        {
            get { return byte.MaxValue; }
        }

        public override bool FullPermissions
        {
            get { return fullPermissions; }
        }

        public override string LogName
        {
            get { return Nickname + " (" + SenderId + ")"; }
        }

        public string Ses;

        public string Name;

        public string senderId;

        public ulong permissions;

        public bool fullPermissions;
    }
}