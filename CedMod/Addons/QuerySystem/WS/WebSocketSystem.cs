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
using CedMod.ApiModals;
using CedMod.Components;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using Exiled.Loader;
using Exiled.Permissions.Extensions;
using MEC;
using Mirror;
using Newtonsoft.Json;
using NWAPIPermissionSystem;
using Exiled.Permissions.Extensions;
using NWAPIPermissionSystem.Commands;
using NWAPIPermissionSystem.Models;
using PluginAPI.Core;
using RemoteAdmin;
using Serialization;
using Websocket.Client;
using UnityEngine.Networking;
using Utils;
using VoiceChat;

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
        public static WebsocketClient Socket;
        internal static object reconnectLock = new object();
        internal static Thread SendThread;
        public static ConcurrentQueue<QueryCommand> SendQueue = new ConcurrentQueue<QueryCommand>();
        internal static bool Reconnect = false;
        public static HelloMessage HelloMessage = null;
        public static DateTime LastConnection = DateTime.UtcNow;

        public static void Stop()
        {
            if (Reconnect)
                return;
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug($"ST3");
            try
            {
                Socket.Stop(WebSocketCloseStatus.NormalClosure, "Stopping").Wait();
            }
            catch (Exception e)
            {
                
            }
            Thread.Sleep(1000);
            Socket = null;
            SendThread?.Abort();
            SendThread = null;
        }

        public static async Task Start()
        {
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug($"ST4");
            if (Reconnect)
                return;
            Reconnect = true;
            LastConnection = DateTime.UtcNow;
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug($"ST5 {Socket == null}");
            try
            {
                string data1 = "";
                Dictionary<string, string> data2 = new Dictionary<string, string>();
                using (HttpClient client = new HttpClient())
                {
                    var resp = await client.SendAsync(new HttpRequestMessage()
                    {
                        Method = HttpMethod.Options,
                        RequestUri = new Uri($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Api/v3/QuerySystem/{QuerySystem.QuerySystemKey}"),
                    });
                    data1 = await resp.Content.ReadAsStringAsync();
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        Reconnect = false;
                        Log.Error($"Failed to retrieve panel location, API rejected request: {data1}, Retrying");
                        await Task.Delay(2000);
                        await Start(); //retry until we succeed or the thread gets aborted.
                        return;
                    }
                    data2 = JsonConvert.DeserializeObject<Dictionary<string, string>>(data1);
                    Log.Info($"Retrieved panel location from API, Connecting to {data1}");
                }

                QuerySystem.CurrentMaster = data2["Api"];
                QuerySystem.CurrentPanel = data2["Panel"];
                QuerySystem.CurrentMasterQuery = data2["Query"];
            }
            catch (Exception e)
            {
                Reconnect = false;
                Log.Error($"Failed to retrieve server location\n{e}");
                await Task.Delay(1000);
                await Start(); //retry until we succeed or the thread gets aborted.
                return;
            }

            try
            {
                LastConnection = DateTime.UtcNow;
                Socket = new WebsocketClient(new Uri($"ws{(QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.CurrentMasterQuery}/QuerySystem?key={QuerySystem.QuerySystemKey}&identity=SCPSL&version=3"));
                Socket.ReconnectTimeout = TimeSpan.FromSeconds(5);
                Socket.ErrorReconnectTimeout = TimeSpan.FromSeconds(5);
                Socket.IsReconnectionEnabled = false;

                Socket.DisconnectionHappened.Subscribe(i =>
                {
                    if (i.Type == DisconnectionType.Exit)
                    {
                        return;
                    }
                    if (i.CloseStatusDescription != null && i.CloseStatusDescription == "LOCATION SWITCH")
                    {
                        Log.Error($"Lost connection to CedMod Panel Instance location switched");
                        Thread.Sleep(2000);
                        lock (reconnectLock)
                        {
                            Task.Factory.StartNew(async () =>
                            { 
                                WebSocketSystem.Reconnect = false;
                                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                    Log.Debug($"ST2");
                                Stop();
                                await Task.Delay(1000);
                                await Start();
                            });
                        }
                        Socket.Dispose();
                        return;
                    }
                    else if (i.CloseStatusDescription != null && i.CloseStatusDescription == "TERMINATE RECONNECT")
                    {
                        Log.Error($"Lost connection to CedMod Panel Instance location switched");
                        Socket.Stop(WebSocketCloseStatus.NormalClosure, "Terminate Reconnect");
                    }
                    else
                    {
                        Log.Error($"Lost connection to CedMod Panel {i.CloseStatus} {i.CloseStatusDescription} {i.Type}, reconnecting in 5000ms\n{(i.Exception == null || !CedModMain.Singleton.Config.QuerySystem.Debug ? "" : i.Exception)}");
                        Thread.Sleep(5000);
                        lock (reconnectLock)
                        {
                            Task.Factory.StartNew(async () =>
                            {
                                try
                                {
                                    WebSocketSystem.Reconnect = false;
                                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                        Log.Debug($"ST1");
                                    Stop();
                                    await Task.Delay(1000);
                                    await Start();
                                }
                                catch (Exception e)
                                {
                                    Log.Error(e.ToString());
                                }
                            });
                        }
                        Socket.Dispose();
                    }
                });

                Socket.ReconnectionHappened.Subscribe(s =>
                {
                    LastConnection = DateTime.UtcNow;
                    Log.Info($"Connected successfully: {s.Type}");
                    if (SendThread == null || !SendThread.IsAlive)
                    {
                        SendThread?.Abort();
                        SendThread = new Thread(HandleSendQueue);
                        SendThread.Start();
                    }
                });

                Socket.MessageReceived.Subscribe(async s =>
                {
                    LastConnection = DateTime.UtcNow;
                    if (s.MessageType == WebSocketMessageType.Text)
                        await OnMessage(new object(), s.Text);
                });
                await Socket.Start();
                Log.Info($"Connected to cedmod panel");
            }
            catch (Exception e)
            {
                WebSocketSystem.Reconnect = false;
                Log.Error(e.ToString());
                await Task.Delay(1000);
                await Start(); //retry until we succeed or the thread gets aborted.
            }

            Reconnect = false;
        }

        public static void HandleSendQueue()
        {
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug("Started SendQueueHandler");
            while (SendThread != null && Socket != null && SendThread.IsAlive && Socket.IsRunning)
            {
                while (SendQueue.TryDequeue(out QueryCommand cmd))
                {
                    try
                    {
                        if (Socket.IsRunning && Socket.IsStarted)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug($"Handling send {Socket.IsRunning} {JsonConvert.SerializeObject(cmd)}");
                            Socket.Send(JsonConvert.SerializeObject(cmd));
                            LastConnection = DateTime.UtcNow;
                        }
                        else 
                            SendQueue.Enqueue(cmd);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Failed to handle queue message, state: {(Socket == null ? "Null Socket" : Socket.IsRunning.ToString())}\n{e}");
                        SendQueue.Enqueue(cmd);
                    }
                }

                Thread.Sleep(10);
            }
        }

        static async Task OnMessage(object sender, string ev)
        {
            try
            {
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug($"Current queue length: {SendQueue.Count}");
                if (SendThread == null || !SendThread.IsAlive)
                {
                    SendThread?.Abort();
                    SendThread = new Thread(HandleSendQueue);
                    SendThread.Start();
                }

                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug(ev);
                QueryCommand cmd = JsonConvert.DeserializeObject<QueryCommand>(ev);

                var jsonData = cmd.Data;
                string text2 = jsonData["action"];
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug(text2);
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
                                    if (staff.isLocalPlayer || !PermissionsHandler.IsPermitted(staff.serverRoles.Permissions, PlayerPermissions.PlayersManagement))
                                        continue;

                                    var plr = CedModPlayer.Get(staff);
                                    var watchlistPlr = CedModPlayer.Get(jsonData["UserId"]);
                                    if (watchlistPlr == null)
                                        continue;
                                    string msg = CedModMain.Singleton.Config.QuerySystem.PlayerGroupWatchlistJoin;
                                    if (!RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr) && !RemoteAdminModificationHandler.Singleton.Requesting.Contains(plr.UserId))
                                    {
                                        Timing.RunCoroutine(RemoteAdminModificationHandler.Singleton.ResolvePreferences(plr, () =>
                                            {
                                                if (!RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin)
                                                    msg += "\n" + CedModMain.Singleton.Config.QuerySystem.StaffReportWatchlistIngameDisabled;
                                                Broadcast.Singleton.TargetAddElement(plr.ReferenceHub.connectionToClient, msg.Replace("{playerId}", $"{watchlistPlr.PlayerId}").Replace("{userId}", watchlistPlr.UserId).Replace("{playerName}", watchlistPlr.Nickname).Replace("{reason}", jsonData["Reason"]).Replace("{groups}", jsonData["Groups"]), RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin == true ? (ushort)5 : (ushort)10, Broadcast.BroadcastFlags.AdminChat);
                                            }));
                                    }
                                    else if (RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr))
                                    {
                                        if (!RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin)
                                            msg += "\n" + CedModMain.Singleton.Config.QuerySystem.StaffReportWatchlistIngameDisabled;
                                        Broadcast.Singleton.TargetAddElement(plr.ReferenceHub.connectionToClient, msg.Replace("{playerId}", $"{watchlistPlr.PlayerId}").Replace("{userId}", watchlistPlr.UserId).Replace("{playerName}", watchlistPlr.Nickname).Replace("{reason}", jsonData["Reason"]).Replace("{groups}", jsonData["Groups"]), RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin == true ? (ushort)5 : (ushort)10, Broadcast.BroadcastFlags.AdminChat);
                                    }
                                }
                            });
                            break;
                        case "watchlistack":
                            RemoteAdminModificationHandler.UpdateWatchList();
                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                            {
                                foreach (var staff in ReferenceHub.AllHubs)
                                {
                                    if (staff.isLocalPlayer || !PermissionsHandler.IsPermitted(staff.serverRoles.Permissions, PlayerPermissions.PlayersManagement))
                                        continue;

                                    var plr = CedModPlayer.Get(staff);
                                    var watchlistPlr = CedModPlayer.Get(jsonData["UserId"]);
                                    if (watchlistPlr == null)
                                        continue;
                                    string msg = CedModMain.Singleton.Config.QuerySystem.PlayerWatchlistJoin;
                                    if (!RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr) && !RemoteAdminModificationHandler.Singleton.Requesting.Contains(plr.UserId))
                                    {
                                        Timing.RunCoroutine(RemoteAdminModificationHandler.Singleton.ResolvePreferences(plr, () =>
                                            {
                                                if (!RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin)
                                                    msg += "\n" + CedModMain.Singleton.Config.QuerySystem.StaffReportWatchlistIngameDisabled;
                                                Broadcast.Singleton.TargetAddElement(plr.ReferenceHub.connectionToClient, msg.Replace("{playerId}", $"{watchlistPlr.PlayerId}").Replace("{playerName}", watchlistPlr.Nickname).Replace("{reason}", jsonData["Reason"]).Replace("{userId}", watchlistPlr.UserId), RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin == true ? (ushort)5 : (ushort)10, Broadcast.BroadcastFlags.AdminChat);
                                            }));
                                    }
                                    else if (RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr))
                                    {
                                        if (!RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin)
                                            msg += "\n" + CedModMain.Singleton.Config.QuerySystem.StaffReportWatchlistIngameDisabled;
                                        Broadcast.Singleton.TargetAddElement(plr.ReferenceHub.connectionToClient, msg.Replace("{playerId}", $"{watchlistPlr.PlayerId}").Replace("{playerName}", watchlistPlr.Nickname).Replace("{reason}", jsonData["Reason"]).Replace("{userId}", watchlistPlr.UserId).Replace("{userId}", watchlistPlr.UserId), RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin == true ? (ushort)5 : (ushort)10, Broadcast.BroadcastFlags.AdminChat);
                                    }
                                }
                            });
                            break;
                        case "updateuserpref":
                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                            {
                                var toRefresh = CedModPlayer.Get(jsonData["UserId"]);
                                if (toRefresh == null && RemoteAdminModificationHandler.Singleton.Requesting.Contains(toRefresh.UserId))
                                    return;
                                Timing.RunCoroutine(RemoteAdminModificationHandler.Singleton.ResolvePreferences(toRefresh, null));
                            });
                            break;
                        case "reportstateack":
                        case "reportack":
                            Log.Info("Updating Reports list from panel ack.");
                            Task.Factory.StartNew(() =>
                            {
                                RemoteAdminModificationHandler.UpdateReportList();
                                if (text2 == "reportack")
                                {
                                    ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                                    {
                                        foreach (var staff in ReferenceHub.AllHubs)
                                        {
                                            if (staff.isLocalPlayer || !PermissionsHandler.IsPermitted(staff.serverRoles.Permissions, PlayerPermissions.PlayersManagement))
                                                continue;

                                            var plr = CedModPlayer.Get(staff);
                                            string msg = CedModMain.Singleton.Config.QuerySystem.StaffReportNotification;
                                            if (!RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr) && !RemoteAdminModificationHandler.Singleton.Requesting.Contains(plr.UserId))
                                            {
                                                Timing.RunCoroutine(RemoteAdminModificationHandler.Singleton.ResolvePreferences(plr, () =>
                                                {
                                                    if (!RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowReportsInRemoteAdmin) 
                                                        msg += "\n" + CedModMain.Singleton.Config.QuerySystem.StaffReportNotificationIngameDisabled;
                                                    Broadcast.Singleton.TargetAddElement(plr.ReferenceHub.connectionToClient, msg.Replace("{reporterName}", $"{jsonData["ReporterName"]} {jsonData["Reporter"]}").Replace("{reportedName}", $"{jsonData["ReportedName"]} {jsonData["Reported"]}").Replace("{checkType}", RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowReportsInRemoteAdmin ? "RemoteAdmin" : "Discord"), RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowReportsInRemoteAdmin == true ? (ushort)5 : (ushort)10, Broadcast.BroadcastFlags.AdminChat); 
                                                }));
                                            }
                                            else if (RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr))
                                            {
                                                if (!RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowReportsInRemoteAdmin) 
                                                    msg += "\n" + CedModMain.Singleton.Config.QuerySystem.StaffReportNotificationIngameDisabled;
                                                Broadcast.Singleton.TargetAddElement(plr.ReferenceHub.connectionToClient, msg.Replace("{reporterName}", $"{jsonData["ReporterName"]} {jsonData["Reporter"]}").Replace("{reportedName}", $"{jsonData["ReportedName"]} {jsonData["Reported"]}").Replace("{checkType}", RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowReportsInRemoteAdmin ? "RemoteAdmin" : "Discord"), RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowReportsInRemoteAdmin == true ? (ushort)5 : (ushort)10, Broadcast.BroadcastFlags.AdminChat);
                                            }
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
                                    });
                                }
                            });
                            break;
                        case "ping":
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug("IsPing");
                            
                            Socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                            {
                                Recipient = cmd.Recipient,
                                Data = new Dictionary<string, string>()
                                {
                                    { "Message", "PONG" }
                                }
                            }));
                            return;
                        case "custom":
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug("CustomCommand");
                            if (!jsonData.ContainsKey("command"))
                            {
                                Socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                                {
                                    Recipient = cmd.Recipient,
                                    Data = new Dictionary<string, string>()
                                    {
                                        { "Message", "Missing argument" }
                                    }
                                }));
                            }
                            else
                            {
                                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                    Log.Debug("CustomCommand"); 
                                string[] array = jsonData["command"].Split(new char[]
                                {
                                    ' '
                                });
                                if (jsonData["command"].ToUpper().Contains("REQUEST_DATA AUTH") ||
                                    jsonData["command"].ToUpper().Contains("SUDO QUIT"))
                                {
                                    Socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                                    {
                                        Recipient = cmd.Recipient,
                                        Data = new Dictionary<string, string>()
                                        {
                                            { "Message", "This command is disabled" }
                                        }
                                    }));
                                }
                                else if (CedModMain.Singleton.Config.QuerySystem.DisallowedWebCommands.Contains(array[0].ToUpper()))
                                {
                                    Socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                                    {
                                        Recipient = cmd.Recipient,
                                        Data = new Dictionary<string, string>()
                                        {
                                            { "Message", "This command is disabled" }
                                        }
                                    }));
                                }

                                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                    Log.Debug("CustomCommandCheckPerm");
                                if (ServerStatic.PermissionsHandler._members.ContainsKey(jsonData["user"]))
                                {
                                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                        Log.Debug("CustomCommandPermGood"); 
                                    string name = ServerStatic.PermissionsHandler._members[jsonData["user"]];
                                    UserGroup ugroup = ServerStatic.PermissionsHandler.GetGroup(name);
                                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                        Log.Debug("CustomCommandPermGood, dispatching");
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
                                        Log.Debug("CustomCommandPermBad, returning");
                                    
                                    Socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                                    {
                                        Recipient = cmd.Recipient,
                                        Data = new Dictionary<string, string>()
                                        {
                                            { "Message", "Userid not present in RA config, permission denied" }
                                        }
                                    }));
                                }
                            }

                            break;
                        case "kicksteamid":
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug("KickCmd");
                            foreach (CedModPlayer player in Player.GetPlayers<CedModPlayer>())
                            {
                                CharacterClassManager component = player.ReferenceHub.characterClassManager;
                                if (component.UserId == jsonData["steamid"])
                                {
                                    ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>  Timing.RunCoroutine(API.StrikeBad(player, jsonData["reason"] + "\n" + CedModMain.Singleton.Config.CedMod.AdditionalBanMessage)));
                                    Socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                                    {
                                        Recipient = cmd.Recipient,
                                        Data = new Dictionary<string, string>()
                                        {
                                            { "Message", "User kicked" }
                                        }
                                    }));
                                    return;
                                }
                            }

                            Socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                            {
                                Recipient = cmd.Recipient,
                                Data = new Dictionary<string, string>()
                                {
                                    { "Message", "User not found" }
                                }
                            }));
                            break;
                        case "mutesteamid":
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug("MuteCmd");
                            var plr = CedModPlayer.Get(jsonData["steamid"]);
                            if (plr != null)
                            {
                                Log.Info($"user: {plr.UserId} muted trough panel, issuing mute...");
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
                                    VoiceChatMutes.SetFlags(plr.ReferenceHub,  VcMuteFlags.LocalRegular);
                                }
                    
                                if (muteType == MuteType.Intercom)
                                {
                                    VoiceChatMutes.SetFlags(plr.ReferenceHub, VcMuteFlags.LocalIntercom);
                                }

                                plr.CustomInfo = CedModMain.Singleton.Config.CedMod.MuteCustomInfo.Replace("{type}", muteType.ToString());
                                Socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                                {
                                    Recipient = cmd.Recipient,
                                    Data = new Dictionary<string, string>()
                                    {
                                        { "Message", "User muted" }
                                    }
                                }));
                                return;
                            }
                            
                            Socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                            {
                                Recipient = cmd.Recipient,
                                Data = new Dictionary<string, string>()
                                {
                                    { "Message", "User not found" }
                                }
                            }));
                            break;
                        case "hello":
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug($"Received hello: {ev}");
                            HelloMessage = JsonConvert.DeserializeObject<HelloMessage>(jsonData["data"]);
                            break;
                        case "ImportRA":
                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                            {
                                List<SLPermissionEntry> Groups = new List<SLPermissionEntry>();
                                foreach (var group in ServerStatic.GetPermissionsHandler()._groups)
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
                                        Permissions = (PlayerPermissions) group.Value.Permissions,
                                        ExiledPermissions = new List<string>(),
                                        //perms.Value == null ? new List<string>() : perms.Value.CombinedPermissions
                                    });
                                }

                                SendQueue.Enqueue(new QueryCommand()
                                {
                                    Recipient = cmd.Recipient,
                                    Data = new Dictionary<string, string>()
                                    {
                                        {"Message", "ImportRAResponse"},
                                        {"Data", JsonConvert.SerializeObject(Groups)}
                                    }
                                });
                            });
                            break;
                        case "ApplyRA":
                            Log.Info($"Panel requested AutoSlPerms reload: {jsonData["reason"]}");
                            new Thread(ApplyRa).Start();
                            break;
                        case "FetchApiKey":
                            Log.Info($"Panel requested refresh of api key: {jsonData["Reason"]}");
                            using (HttpClient client = new HttpClient())
                            {
                                var response = await client.PostAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Api/v3/FetchKey/{QuerySystem.QuerySystemKey}", new StringContent(JsonConvert.SerializeObject(new
                                {
                                    Hash = jsonData["Hash"]
                                }), Encoding.Default, "application/json"));

                                if (response.IsSuccessStatusCode)
                                {
                                    CedModMain.Singleton.Config.CedMod.CedModApiKey = await response.Content.ReadAsStringAsync();
#if !EXILED
                                    File.WriteAllText(Path.Combine(CedModMain.PluginConfigFolder, "config.yml"), YamlParser.Serializer.Serialize(CedModMain.Singleton.Config));
#else
                                    var pluginConfigs = ConfigManager.LoadSorted(ConfigManager.Read());
                                    Config cedModCnf = pluginConfigs[CedModMain.Singleton.Prefix] as Config;
                                    cedModCnf.CedMod.CedModApiKey = await response.Content.ReadAsStringAsync();
                                    ConfigManager.Save(pluginConfigs);
#endif
                                    Log.Info($"Successfully saved apikey");
                                }
                                else
                                {
                                    Log.Error($"Failed to fetch key: {await response.Content.ReadAsStringAsync()}");
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        public static bool UseRa = true;
        public static object LockObj = new object();

        public static void ApplyRa()
        {
            lock (LockObj)
            {
                AutoSlPermsSlRequest permsSlRequest = null;
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
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
                            Log.Error($"Failed to request RA: {responsePerms.Content.ReadAsStringAsync().Result}");
                            responsePerms.EnsureSuccessStatusCode();
                        }
                        permsSlRequest = JsonConvert.DeserializeObject<AutoSlPermsSlRequest>(responsePerms.Content.ReadAsStringAsync().Result);
                    }
                    if (permsSlRequest.PermissionEntries.Count == 0)
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
                        Log.Error($"Failed to fetch RA from panel, using cache...\n{e}");
                        permsSlRequest = JsonConvert.DeserializeObject<AutoSlPermsSlRequest>(File.ReadAllText(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "autoSlPermCache.json")));
                    }
                    else
                    {
                        UseRa = true;
                        Log.Error($"Failed to fetch RA from panel, using RA...\n{e}");
                        return;
                    }
                }

                try
                {
                    var handler = ServerStatic.GetPermissionsHandler();
                    var oldMembers = new Dictionary<string, string>(handler._members);
                    var oldGroups = new Dictionary<string, UserGroup>(handler._groups);
                    handler._groups.Clear();
                    handler._members.Clear();
                    if (AppDomain.CurrentDomain.GetAssemblies().Any(s => s.GetName().Name == "NWAPIPermissionSystem"))
                    {
                        ClearNWApiPermissions();
                        HandleDefault(permsSlRequest.DefaultPermissions);
                    }
#if EXILED
                    Permissions.Groups.Clear();
                    var epGroup = new Exiled.Permissions.Features.Group();
                    epGroup.Permissions.AddRange(permsSlRequest.DefaultPermissions);
                    epGroup.CombinedPermissions.AddRange(permsSlRequest.DefaultPermissions);
                    epGroup.Inheritance.Clear();
                    epGroup.IsDefault = true;
                    Permissions.Groups.Add("default", epGroup);
#endif
                    foreach (var perm in permsSlRequest.PermissionEntries)
                    {
                        handler._groups.Add(perm.Name, new UserGroup()
                        {
                            BadgeColor = perm.BadgeColor,
                            BadgeText = perm.BadgeText,
                            Cover = perm.Cover,
                            HiddenByDefault = perm.Hidden,
                            KickPower = (byte)perm.KickPower,
                            Permissions = (ulong)perm.Permissions,
                            RequiredKickPower = (byte)perm.RequiredKickPower,
                            Shared = false
                        });

                        if (AppDomain.CurrentDomain.GetAssemblies().Any(s => s.GetName().Name == "NWAPIPermissionSystem"))
                        {
                            ProcessNWPermissions(perm);
                        }

#if EXILED                        
                        var epGroup1 = new Exiled.Permissions.Features.Group();
                        epGroup1.Permissions.AddRange(perm.ExiledPermissions);
                        epGroup1.CombinedPermissions.AddRange(perm.ExiledPermissions);
                        epGroup1.Inheritance.Clear();
                        epGroup1.IsDefault = false;
                        Permissions.Groups.Add(perm.Name, epGroup1);
#endif
                    }

                    foreach (var member in permsSlRequest.MembersList)
                    {
                        if (member.ReservedSlot && !QuerySystem.ReservedSlotUserids.Contains(member.UserId))
                            QuerySystem.ReservedSlotUserids.Add(member.UserId);
                        handler._members.Add(member.UserId, member.Group);
                        
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
                                        var hidden = player.PlayerInfo.IsBadgeHidden;
                                        player.ReferenceHub.serverRoles.SetGroup(handler._groups[member.Group], false);
                                        Timing.CallDelayed(0.1f, () =>
                                        {
                                            player.PlayerInfo.IsBadgeHidden = hidden;
                                        });
                                        Log.Info($"Refreshed Permissions from {member.UserId} as they were present and had changes in the AutoSlPerms response while ingame");
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error($"Failed to apply permission at realtime for {player.UserId} - {player.Nickname}");
                        }
                    }

                    foreach (var member in oldMembers)
                    {
                        if (permsSlRequest.MembersList.All(s => s.UserId != member.Key) && QuerySystem.ReservedSlotUserids.Contains(member.Key))
                        {
                            QuerySystem.ReservedSlotUserids.Remove(member.Key);
                        }
                        
                        if (CedModPlayer.Get(member.Key) != null)
                        {
                            if (permsSlRequest.MembersList.All(s => s.UserId != member.Key))
                            {
                                Log.Info(member.Key + " 3");
                                CedModPlayer.Get(member.Key).ReferenceHub.serverRoles.SetGroup(null, false);
                                Log.Info($"Removed Permissions from {member.Key} as they were no longer present in the AutoSlPerms response while ingame");
                            }
                        }
                    }
                    Log.Info($"Successfully applied {permsSlRequest.PermissionEntries.Count} Groups and {permsSlRequest.MembersList.Count} members for AutoSlPerms");
                }
                catch (Exception e)
                {
                    UseRa = true;
                    Log.Error($"Failed to fetch RA from panel, using RA...\n{e}");
                    ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                }
            }
        }

        private static void HandleDefault(List<string> defaultPermissions)
        {
            var epGroup = new Group();
            epGroup.Permissions.AddRange(defaultPermissions);
            epGroup.CombinedPermissions.AddRange(defaultPermissions);
            epGroup.IsDefault = true;
            PermissionHandler.PermissionGroups.Add("default", epGroup);
        }

        private static void ClearNWApiPermissions()
        {
            PermissionHandler.PermissionGroups.Clear();
        }

        private static void ProcessNWPermissions(SLPermissionEntry perm)
        {
            var epGroup = new Group();
            epGroup.Permissions.AddRange(perm.ExiledPermissions);
            epGroup.CombinedPermissions.AddRange(perm.ExiledPermissions);
            epGroup.InheritedGroups.Clear();
            epGroup.IsDefault = false;
            PermissionHandler.PermissionGroups.Add(perm.Name, epGroup);
        }
    }

    public class CmSender : CommandSender
    {
        public const int chunksize = 1000;
        public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay)
        {
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug($"RA Reply: {text} Suc: {success}");
            SendText(text);
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug("Added RA message to queue");
        }

        public override void Print(string text)
        {
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug($"RA Print: {text}"); 
            SendText(text);
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug("Added RA message to queue");
        }

        public void SendText(string text)
        {
            if (text.Length >= chunksize)
            {
                IEnumerable<string> chunks = Enumerable.Range(0, text.Length / chunksize).Select(i => text.Substring(i * chunksize, chunksize));
                foreach (var str in chunks) //WS becomes unstable if we send a large chunk of text
                {
                    WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
                    {
                        Recipient = Ses,
                        Data = new Dictionary<string, string>()
                        {
                            {"Message", str}
                        }
                    });
                }
            }
            else
            {
                WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
                {
                    Recipient = Ses,
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", text}
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

        public override void Respond(string text, bool success = true)
        {
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug($"RA Response: {text} Suc: {success}");
            SendText(text);
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug("Added RA message to queue");
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