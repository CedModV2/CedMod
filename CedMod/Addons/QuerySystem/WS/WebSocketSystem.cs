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
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Exiled.Permissions.Features;
using MEC;
using Mirror;
using Newtonsoft.Json;
using RemoteAdmin;
using Websocket.Client;
using UnityEngine.Networking;

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
            Log.Debug($"ST3", CedModMain.Singleton.Config.QuerySystem.Debug);
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

        public static void Start()
        {
            Log.Debug($"ST4", CedModMain.Singleton.Config.QuerySystem.Debug);
            if (Reconnect)
                return;
            Reconnect = true;
            LastConnection = DateTime.UtcNow;
            Log.Debug($"ST5 {Socket == null}", CedModMain.Singleton.Config.QuerySystem.Debug);
            try
            {
                string data1 = "";
                Dictionary<string, string> data2 = new Dictionary<string, string>();
                using (HttpClient client = new HttpClient())
                {
                    var resp = client.SendAsync(new HttpRequestMessage()
                    {
                        Method = HttpMethod.Options,
                        RequestUri = new Uri("https://" + QuerySystem.CurrentMaster + $"/Api/v3/QuerySystem/{QuerySystem.QuerySystemKey}"),
                    }).Result;
                    data1 = resp.Content.ReadAsStringAsync().Result;
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        Reconnect = false;
                        Log.Error($"Failed to retrieve panel location, API rejected request: {data1}, Retrying");
                        Thread.Sleep(2000);
                        Start(); //retry until we succeed or the thread gets aborted.
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
                Thread.Sleep(2000);
                Start(); //retry until we succeed or the thread gets aborted.
                return;
            }

            try
            {
                LastConnection = DateTime.UtcNow;
                Socket = new WebsocketClient(new Uri($"wss://{QuerySystem.CurrentMasterQuery}/QuerySystem?key={QuerySystem.QuerySystemKey}&identity=SCPSL&version=3"));
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
                        Socket.Stop(WebSocketCloseStatus.NormalClosure, "Location switch");
                        Thread.Sleep(2000);
                        lock (reconnectLock)
                        {
                            Task.Factory.StartNew(() =>
                            { 
                                WebSocketSystem.Reconnect = false;
                                Log.Debug($"ST2", CedModMain.Singleton.Config.QuerySystem.Debug);
                                Stop();
                                Thread.Sleep(1000);
                                Start();
                            });
                        }
                        Socket.Dispose();
                        return;
                    }
                    else
                    {
                        Log.Error($"Lost connection to CedMod Panel {i.CloseStatus} {i.CloseStatusDescription} {i.Type}, reconnecting in 5000ms\n{(i.Exception == null || !CedModMain.Singleton.Config.QuerySystem.Debug ? "" : i.Exception)}");
                        Thread.Sleep(5000);
                        lock (reconnectLock)
                        {
                            Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    WebSocketSystem.Reconnect = false;
                                    Log.Debug($"ST1", CedModMain.Singleton.Config.QuerySystem.Debug);
                                    Stop();
                                    Thread.Sleep(1000);
                                    Start();
                                }
                                catch (Exception e)
                                {
                                    Log.Error(e);
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

                Socket.MessageReceived.Subscribe(s =>
                {
                    LastConnection = DateTime.UtcNow;
                    if (s.MessageType == WebSocketMessageType.Text)
                        OnMessage(new object(), s.Text);
                });
                Socket.Start().Wait();
                Log.Info($"Connected to cedmod panel");
            }
            catch (Exception e)
            {
                WebSocketSystem.Reconnect = false;
                Log.Error(e);
                Thread.Sleep(2000);
                Start(); //retry until we succeed or the thread gets aborted.
            }

            Reconnect = false;
        }

        public static void HandleSendQueue()
        {
            Log.Debug("Started SendQueueHandler", CedModMain.Singleton.Config.QuerySystem.Debug);
            while (SendThread != null && Socket != null && SendThread.IsAlive && Socket.IsRunning)
            {
                while (SendQueue.TryDequeue(out QueryCommand cmd))
                {
                    try
                    {
                        if (Socket.IsRunning && Socket.IsStarted)
                        {
                            Log.Debug($"Handling send {Socket.IsRunning} {JsonConvert.SerializeObject(cmd)}", CedModMain.Singleton.Config.QuerySystem.Debug);
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

        static void OnMessage(object sender, string ev)
        {
            try
            {
                Log.Debug($"Current queue length: {SendQueue.Count}", CedModMain.Singleton.Config.QuerySystem.Debug);
                if (SendThread == null || !SendThread.IsAlive)
                {
                    SendThread?.Abort();
                    SendThread = new Thread(HandleSendQueue);
                    SendThread.Start();
                }

                Log.Debug(ev, CedModMain.Singleton.Config.QuerySystem.Debug);
                QueryCommand cmd = JsonConvert.DeserializeObject<QueryCommand>(ev);

                var jsonData = cmd.Data;
                string text2 = jsonData["action"];
                Log.Debug(text2, CedModMain.Singleton.Config.QuerySystem.Debug);
                if (text2 != null)
                {
                    switch (text2)
                    {
                        case "ping":
                            Log.Debug("IsPing", CedModMain.Singleton.Config.QuerySystem.Debug);
                            
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
                            Log.Debug("CustomCommand", CedModMain.Singleton.Config.QuerySystem.Debug);
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
                                Log.Debug("CustomCommand", CedModMain.Singleton.Config.QuerySystem.Debug);
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

                                Log.Debug("CustomCommandCheckPerm", CedModMain.Singleton.Config.QuerySystem.Debug);
                                if (ServerStatic.PermissionsHandler._members.ContainsKey(jsonData["user"]))
                                {
                                    Log.Debug("CustomCommandPermGood", CedModMain.Singleton.Config.QuerySystem.Debug);
                                    string name = ServerStatic.PermissionsHandler._members[jsonData["user"]];
                                    UserGroup ugroup = ServerStatic.PermissionsHandler.GetGroup(name);
                                    Log.Debug("CustomCommandPermGood, dispatching",
                                        CedModMain.Singleton.Config.QuerySystem.Debug);
                                    switch (jsonData["command"].Split(' ')[0].ToUpper())
                                    {
                                        default:
                                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(delegate
                                            {
                                                var sender = new CmSender(cmd.Recipient, jsonData["user"], jsonData["user"], ugroup);
                                                bool removeDummy = false;
                                                if (Player.Get(jsonData["user"]) == null && CedModMain.Singleton.Config.QuerySystem.DummyExperimental)
                                                {
                                                    HandleQueryplayer.CreateDummy(sender);
                                                    removeDummy = true;
                                                    Log.Info($"Created Dummy player for {jsonData["user"]} Remote Commands Functionality");
                                                }
                                                CommandProcessor.ProcessQuery(jsonData["command"], sender);
                                                if (removeDummy)
                                                {
                                                    var dum = HandleQueryplayer.PlayerDummies.FirstOrDefault(s => s.UserId == jsonData["user"]);
                                                    Player.Dictionary.Remove(dum.GameObject);
                                                    NetworkServer.Destroy(dum.GameObject);
                                                }
                                            });
                                            break;
                                    }
                                }
                                else
                                {
                                    Log.Debug("CustomCommandPermBad, returning", CedModMain.Singleton.Config.QuerySystem.Debug);
                                    
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
                            Log.Debug("KickCmd", CedModMain.Singleton.Config.QuerySystem.Debug);
                            foreach (Player player in Player.List)
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
                            Log.Debug("MuteCmd", CedModMain.Singleton.Config.QuerySystem.Debug);
                            var plr = Player.Get(jsonData["steamid"]);
                            if (plr != null)
                            {
                                Log.Info($"user: {plr.UserId} muted trough panel, issuing mute...");
                                Enum.TryParse(jsonData["type"], out MuteType muteType);
                                plr.SendConsoleMessage(CedModMain.Singleton.Config.CedMod.MuteMessage.Replace("{type}", muteType.ToString()).Replace("{duration}", jsonData["duration"]).Replace("{reason}", jsonData["reason"]), "red");
                                plr.Broadcast(5, CedModMain.Singleton.Config.CedMod.MuteMessage.Replace("{type}", muteType.ToString()).Replace("{duration}", jsonData["duration"]).Replace("{reason}", jsonData["reason"]), Broadcast.BroadcastFlags.Normal);
                                plr.IsMuted = muteType == MuteType.Global;
                                plr.IsIntercomMuted = muteType == MuteType.Intercom;
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
                            Log.Debug($"Received hello: {ev}", CedModMain.Singleton.Config.QuerySystem.Debug);
                            HelloMessage = JsonConvert.DeserializeObject<HelloMessage>(jsonData["data"]);
                            break;
                        case "ImportRA":
                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                            {
                                List<SLPermissionEntry> Groups = new List<SLPermissionEntry>();
                                foreach (var group in ServerStatic.GetPermissionsHandler()._groups)
                                {
                                    var perms = Permissions.Groups.FirstOrDefault(s => s.Key == group.Key);
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
                                        ExiledPermissions = perms.Value == null ? new List<string>() : perms.Value.CombinedPermissions,
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
                            Task.Factory.StartNew(ApplyRa);
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
                            RequestUri = new Uri("https://" + QuerySystem.CurrentMaster + $"/Api/v3/GetPermissions/{QuerySystem.QuerySystemKey}"),
                        }).Result;
                        if (!responsePerms.IsSuccessStatusCode)
                        {
                            if (responsePerms.Content.ReadAsStringAsync().Result == "No entries defined.")
                            {
                                if (!UseRa)
                                    ServerStatic.PermissionsHandler = ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                                UseRa = true;
                                if (Directory.Exists(Path.Combine(Paths.Configs, "CedMod")))
                                {
                                    if (File.Exists(Path.Combine(Paths.Configs, "CedMod", "autoSlPermCache.json")))
                                        File.Delete(Path.Combine(Paths.Configs, "CedMod", "autoSlPermCache.json"));
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
                            ServerStatic.PermissionsHandler = ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                        UseRa = true;
                        if (Directory.Exists(Path.Combine(Paths.Configs, "CedMod")))
                        {
                            if (File.Exists(Path.Combine(Paths.Configs, "CedMod", "autoSlPermCache.json")))
                                File.Delete(Path.Combine(Paths.Configs, "CedMod", "autoSlPermCache.json"));
                        }
                        return;
                    }
                    if (!Directory.Exists(Path.Combine(Paths.Configs, "CedMod")))
                        Directory.CreateDirectory(Path.Combine(Paths.Configs, "CedMod"));
                    File.WriteAllText(Path.Combine(Paths.Configs, "CedMod", "autoSlPermCache.json"), JsonConvert.SerializeObject(permsSlRequest));
                    UseRa = false;
                }
                catch (Exception e)
                {
                    if (!Directory.Exists(Path.Combine(Paths.Configs, "CedMod")))
                        Directory.CreateDirectory(Path.Combine(Paths.Configs, "CedMod"));
                    if (File.Exists(Path.Combine(Paths.Configs, "CedMod", "autoSlPermCache.json")))
                    {
                        UseRa = false;
                        Log.Error($"Failed to fetch RA from panel, using cache...\n{e}");
                        permsSlRequest = JsonConvert.DeserializeObject<AutoSlPermsSlRequest>(File.ReadAllText(Path.Combine(Paths.Configs, "CedMod", "autoSlPermCache.json")));
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
                    handler._groups.Clear();
                    handler._members.Clear();
                    Permissions.Groups.Clear();

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

                        var epGroup = new Group();
                        epGroup.Permissions.AddRange(perm.ExiledPermissions);
                        epGroup.CombinedPermissions.AddRange(perm.ExiledPermissions);
                        epGroup.Inheritance.Clear();
                        epGroup.IsDefault = false;
                        Permissions.Groups.Add(perm.Name, epGroup);
                    }

                    foreach (var member in permsSlRequest.MembersList)
                    {
                        if (member.ReservedSlot && !QuerySystem.ReservedSlotUserids.Contains(member.UserId))
                            QuerySystem.ReservedSlotUserids.Add(member.UserId);
                        handler._members.Add(member.UserId, member.Group);

                        var player = Player.Get(member.UserId);
                        if (player != null)
                        {
                            bool hidden = player.BadgeHidden;
                            Player.Get(member.UserId).Group = handler._groups[member.Group];
                            player.BadgeHidden = hidden;
                            Log.Info($"Refreshed Permissions from {member.UserId} as they were present in the AutoSlPerms response while ingame");
                        }
                    }

                    foreach (var member in oldMembers)
                    {
                        if (permsSlRequest.MembersList.All(s => s.UserId != member.Key) && QuerySystem.ReservedSlotUserids.Contains(member.Key))
                        {
                            QuerySystem.ReservedSlotUserids.Remove(member.Key);
                        }
                        
                        if (Player.Get(member.Key) != null)
                        {
                            if (permsSlRequest.MembersList.All(s => s.UserId != member.Key))
                            {
                                Log.Info(member.Key + " 3");
                                Player.Get(member.Key).Group = null;
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
                    ServerStatic.PermissionsHandler = ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                }
            }
        }
    }

    public class CmSender : CommandSender
    {
        public const int chunksize = 1000;
        public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay)
        {
            Log.Debug($"RA Reply: {text} Suc: {success}", CedModMain.Singleton.Config.QuerySystem.Debug);
            SendText(text);
            Log.Debug("Added RA message to queue", CedModMain.Singleton.Config.QuerySystem.Debug);
        }

        public override void Print(string text)
        {
            Log.Debug($"RA Print: {text}", CedModMain.Singleton.Config.QuerySystem.Debug);
            SendText(text);
            Log.Debug("Added RA message to queue", CedModMain.Singleton.Config.QuerySystem.Debug);
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
            Log.Debug($"RA Response: {text} Suc: {success}", CedModMain.Singleton.Config.QuerySystem.Debug);
            SendText(text);
            Log.Debug("Added RA message to queue", CedModMain.Singleton.Config.QuerySystem.Debug);
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