using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CedMod.Addons.Events.Commands;
using CedMod.Addons.QuerySystem.Commands;
using CedMod.ApiModals;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Exiled.Permissions.Features;
using Newtonsoft.Json;
using RemoteAdmin;
using WebSocketSharp;

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
        public static WebSocket Socket;
        private static object reconnectLock = new object();
        private static Thread SendThread;
        public static ConcurrentQueue<QueryCommand> SendQueue = new ConcurrentQueue<QueryCommand>();
        private static bool Reconnect = true;
        public static HelloMessage HelloMessage = null;

        public static void Stop()
        {
            Reconnect = false;
            Socket.Close();
            Socket = null;
            SendThread?.Abort();
        }

        public static void Start()
        {
            try
            {
                HttpClient client = new HttpClient();
                var resp = client.SendAsync(new HttpRequestMessage()
                {
                    Method = HttpMethod.Options,
                    RequestUri = new Uri("https://" + QuerySystem.CurrentMaster + $"/Api/QuerySystem/{CedModMain.Singleton.Config.QuerySystem.SecurityKey}"),
                }).Result;
                string data1 = resp.Content.ReadAsStringAsync().Result;
                if (resp.StatusCode != HttpStatusCode.OK)
                {
                    Log.Error($"Failed to retrieve panel location, API rejected request: {data1}, Retrying");
                    Thread.Sleep(2000);
                    Start(); //retry until we succeed or the thread gets aborted.
                    return;
                }
                Log.Info($"Retrieved panel location from API, Connecting to {data1} as {CedModMain.Singleton.Config.QuerySystem.Identifier}");
                QuerySystem.PanelUrl = data1;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to retrieve server location\n{e}");
                return;
            }
            Reconnect = true;
            Socket = new WebSocket($"wss://{QuerySystem.PanelUrl}/QuerySystem?key={CedModMain.Singleton.Config.QuerySystem.SecurityKey}&identity={CedModMain.Singleton.Config.QuerySystem.Identifier}&version=2");
            Socket.Connect();
            Socket.OnMessage += OnMessage;
            Socket.OnClose += (sender, args) =>
            {
                if (!Reconnect)
                    return;
                if (args.Reason == "LOCATION SWITCH")
                {
                    Log.Error($"Lost connection to CedMod Panel Instance location switched");
                    Task.Factory.StartNew(() =>
                    { 
                        WebSocketSystem.Stop();
                        WebSocketSystem.Start();
                    });
                    return;
                }
                else
                {
                    lock (reconnectLock)
                    {
                        Log.Error($"Lost connection to CedMod Panel {args.Reason}, reconnecting in 5000ms");
                        Thread.Sleep(5000);
                        Log.Info("Reconnecting...");
                        Socket.Connect();
                        Log.Debug("Connect", CedModMain.Singleton.Config.QuerySystem.Debug);
                    }
                }
            };
            Socket.OnOpen += (sender, args) => { Log.Info("Connected."); };
            Socket.OnError += (sender, args) =>
            {
                Log.Error(args.Message);
                Log.Error(args.Exception);
            };

            SendThread?.Abort();
            SendThread = new Thread(HandleSendQueue);
            SendThread.Start();
        }

        public static void HandleSendQueue()
        {
            Log.Debug("Started SendQueueHandler", CedModMain.Singleton.Config.QuerySystem.Debug);
            while (Socket.IsAlive)
            {
                while (SendQueue.TryDequeue(out QueryCommand cmd))
                {
                    try
                    {
                        Log.Debug($"Handling send {JsonConvert.SerializeObject(cmd)}", CedModMain.Singleton.Config.QuerySystem.Debug);
                        Socket.Send(JsonConvert.SerializeObject(cmd));
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Failed to handle queue message, state: {(Socket == null ? "Null Socket" : Socket.ReadyState.ToString())}\n{e}");
                        SendQueue.Enqueue(cmd);
                    }
                }

                Thread.Sleep(10);
            }
        }

        static void OnMessage(object sender, MessageEventArgs ev)
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

                Log.Debug(ev.Data, CedModMain.Singleton.Config.QuerySystem.Debug);
                QueryCommand cmd = JsonConvert.DeserializeObject<QueryCommand>(ev.Data);

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
                                else if (CedModMain.Singleton.Config.QuerySystem.DisallowedWebCommands.Contains(
                                             array[0].ToUpper()))
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
                                                CommandProcessor.ProcessQuery(jsonData["command"],
                                                    new CmSender(cmd.Recipient, jsonData["user"], jsonData["user"],
                                                        ugroup));
                                            });
                                            break;
                                    }
                                }
                                else
                                {
                                    Log.Debug("CustomCommandPermBad, returning",
                                        CedModMain.Singleton.Config.QuerySystem.Debug);
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
                                    ServerConsole.Disconnect(player.GameObject, jsonData["reason"]);
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
                        case "hello":
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

        public static void ApplyRa()
        {
            AutoSlPermsSlRequest permsSlRequest = null;
            try
            {
                HttpClient client = new HttpClient();
                var responsePerms = client.SendAsync(new HttpRequestMessage()
                {
                    Method = HttpMethod.Options,
                    RequestUri = new Uri("https://" + QuerySystem.CurrentMaster + $"/Api/GetPermissions/{CedModMain.Singleton.Config.QuerySystem.SecurityKey}"),
                }).Result;
                if (!responsePerms.IsSuccessStatusCode)
                {
                    Log.Error($"Failed to request RA: {responsePerms.Content.ReadAsStringAsync().Result}");
                    responsePerms.EnsureSuccessStatusCode();
                }
                permsSlRequest = JsonConvert.DeserializeObject<AutoSlPermsSlRequest>(responsePerms.Content.ReadAsStringAsync().Result);
                if (permsSlRequest.PermissionEntries.Count == 0)
                    return;
                if (!Directory.Exists(Path.Combine(Paths.Configs, "CedMod")))
                    Directory.CreateDirectory(Path.Combine(Paths.Configs, "CedMod"));
                File.WriteAllText(Path.Combine(Paths.Configs, "CedMod", "autoSlPermCache.json"), JsonConvert.SerializeObject(permsSlRequest));
            }
            catch (Exception e)
            {
                if (!Directory.Exists(Path.Combine(Paths.Configs, "CedMod")))
                    Directory.CreateDirectory(Path.Combine(Paths.Configs, "CedMod"));
                if (File.Exists(Path.Combine(Paths.Configs, "CedMod", "autoSlPermCache.json")))
                {
                    Log.Error($"Failed to fetch RA from panel, using cache...\n{e}");
                    permsSlRequest = JsonConvert.DeserializeObject<AutoSlPermsSlRequest>(File.ReadAllText(Path.Combine(Paths.Configs, "CedMod", "autoSlPermCache.json")));
                }
                else
                {
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
                    epGroup.Permissions.AddRange(perm.ExiledPermissions);
                    epGroup.Inheritance.Clear();
                    epGroup.IsDefault = false;
                    Permissions.Groups.Add(perm.Name, epGroup);
                }

                foreach (var member in permsSlRequest.MembersList)
                {
                    if (member.ReservedSlot && !QuerySystem.ReservedSlotUserids.Contains(member.UserId))
                        QuerySystem.ReservedSlotUserids.Add(member.UserId);
                    handler._members.Add(member.UserId, member.Group);

                    if (Player.Get(member.UserId) != null)
                    {
                        Player.Get(member.UserId).Group = handler._groups[member.Group];
                        Log.Info($"Refreshed Permissions from {member.UserId} as they were present in the AutoSlPerms response while ingame");
                    }
                }

                foreach (var member in oldMembers)
                {
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
                Log.Error($"Failed to fetch RA from panel, using RA...\n{e}");
                ServerStatic.PermissionsHandler = ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
            }
        }
    }

    internal class CmSender : CommandSender
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