using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CedMod.Addons.Events.Commands;
using CedMod.Addons.QuerySystem.Commands;
using Exiled.API.Features;
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

        public static void Stop()
        {
            Reconnect = false;
            Socket.Close();
            Socket = null;
            SendThread?.Abort();
        }
        public static void Start()
        {
            Reconnect = true;
            Socket = new WebSocket($"wss://{QuerySystem.PanelUrl}/QuerySystem?key={CedModMain.Singleton.Config.QuerySystem.SecurityKey}&identity={CedModMain.Singleton.Config.QuerySystem.Identifier}");
            Socket.Connect();
            Socket.OnMessage += OnMessage;
            Socket.OnClose += (sender, args) =>
            {
                if (!Reconnect)
                    return;
                lock (reconnectLock)
                {
                    Log.Error($"Lost connection to CedMod Panel {args.Reason}, reconnecting in 5000ms");
                    Thread.Sleep(5000);
                    Log.Info("Reconnecting...");
                    Socket.Connect();
                    Log.Debug("Connect", CedModMain.Singleton.Config.QuerySystem.Debug);
                }
            };
            Socket.OnOpen += (sender, args) =>
            {
                Log.Info("Connected.");
            };
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
                    if (text2 == "ping")
                    {
                        Log.Debug("IsPing", CedModMain.Singleton.Config.QuerySystem.Debug);
                        Socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                        {
                            Recipient = cmd.Recipient,
                            Data = new Dictionary<string, string>()
                            {
                                {"Message", "PONG"}
                            }
                        }));
                        return;
                    }

                    if (text2 == "custom")
                    {
                        Log.Debug("CustomCommand", CedModMain.Singleton.Config.QuerySystem.Debug);
                        if (!jsonData.ContainsKey("command"))
                        {
                            Socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                            {
                                Recipient = cmd.Recipient,
                                Data = new Dictionary<string, string>()
                                {
                                    {"Message", "Missing argument"}
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
                                        {"Message", "This command is disabled"}
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
                                        {"Message", "This command is disabled"}
                                    }
                                }));
                            }

                            Log.Debug("CustomCommandCheckPerm", CedModMain.Singleton.Config.QuerySystem.Debug);
                            if (ServerStatic.PermissionsHandler._members.ContainsKey(jsonData["user"]))
                            {
                                Log.Debug("CustomCommandPermGood", CedModMain.Singleton.Config.QuerySystem.Debug);
                                string name = ServerStatic.PermissionsHandler._members[jsonData["user"]];
                                UserGroup ugroup = ServerStatic.PermissionsHandler.GetGroup(name);
                                Log.Debug("CustomCommandPermGood, dispatching", CedModMain.Singleton.Config.QuerySystem.Debug);
                                switch (jsonData["command"].Split(' ')[0].ToUpper())
                                {
                                    case "PLAYERLISTCOLORED":
                                        new PlayersCommand().Execute(
                                            new ArraySegment<string>(jsonData["command"].Split(' ').Skip(1).ToArray()),
                                            new CmSender(cmd.Recipient, jsonData["user"], jsonData["user"], ugroup),
                                            out string response);
                                        SendQueue.Enqueue(new QueryCommand()
                                        {
                                            Recipient = cmd.Recipient,
                                            Data = new Dictionary<string, string>()
                                            {
                                                {
                                                    "Message", 
                                                    $"PLAYERLISTCOLORED#{response}"
                                                }
                                            }
                                        });
                                        
                                        break;
                                    case "PLAYERLISTCOLOREDSTEAMID":
                                        new PlayerSteamidsCommand().Execute(
                                            new ArraySegment<string>(jsonData["command"].Split(' ').Skip(1).ToArray()),
                                            new CmSender(cmd.Recipient, jsonData["user"], jsonData["user"], ugroup),
                                            out string response1);
                                        SendQueue.Enqueue(new QueryCommand()
                                        {
                                            Recipient = cmd.Recipient,
                                            Data = new Dictionary<string, string>()
                                            {
                                                {
                                                    "Message", 
                                                    $"PLAYERLISTCOLOREDSTEAMID#{response1}"
                                                }
                                            }
                                        });
                                        
                                        break;
                                    case "EVENTS":
                                        if (jsonData["command"].Split(' ').Length != 0 && jsonData["command"].Split(' ')[1].ToUpper() == "LIST")
                                        {
                                            new ListEvents().Execute(
                                                new ArraySegment<string>(jsonData["command"].Split(' ').Skip(1).ToArray()),
                                                new CmSender(cmd.Recipient, jsonData["user"], jsonData["user"], ugroup),
                                                out string response2);
                                            SendQueue.Enqueue(new QueryCommand()
                                            {
                                                Recipient = cmd.Recipient,
                                                Data = new Dictionary<string, string>()
                                                {
                                                    {
                                                        "Message", 
                                                        $"EVENTS#{response2}"
                                                    }
                                                }
                                            });
                                        }
                                        else
                                        {
                                            ThreadDispatcher.ThreadDispatchQueue.Enqueue(delegate
                                            {
                                                CommandProcessor.ProcessQuery(jsonData["command"],
                                                    new CmSender(cmd.Recipient, jsonData["user"], jsonData["user"], ugroup));
                                            });
                                        }
                                        break;
                                    default:
                                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(delegate
                                        {
                                            CommandProcessor.ProcessQuery(jsonData["command"],
                                                new CmSender(cmd.Recipient, jsonData["user"], jsonData["user"], ugroup));
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
                                        {"Message", "Userid not present in RA config, permission denied"}
                                    }
                                }));
                            }
                        }
                    }

                    if (text2 == "kicksteamid")
                    {
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
                                        {"Message", "User kicked"}
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
                                {"Message", "User not found"}
                            }
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
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