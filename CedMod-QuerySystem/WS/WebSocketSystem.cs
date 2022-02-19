using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CedMod.EventManager.Commands;
using CedMod.QuerySystem.Commands;
using Exiled.API.Features;
using Newtonsoft.Json;
using RemoteAdmin;
using UnityEngine;
using WebSocketSharp;

namespace CedMod.QuerySystem.WS
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
            Socket = new WebSocket($"wss://{QuerySystem.PanelUrl}/QuerySystem?key={QuerySystem.Singleton.Config.SecurityKey}&identity={QuerySystem.Singleton.Config.Identifier}");
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
                    Log.Debug("Connect", QuerySystem.Singleton.Config.Debug);
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
            Log.Debug("Started SendQueueHandler", QuerySystem.Singleton.Config.Debug);
            while (Socket.IsAlive)
            {
                while (SendQueue.TryDequeue(out QueryCommand cmd))
                {
                    try
                    {
                        Log.Debug($"Handling send {JsonConvert.SerializeObject(cmd)}", QuerySystem.Singleton.Config.Debug);
                        Socket.Send(JsonConvert.SerializeObject(cmd));
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Failed to handle queue message, state: {(Socket == null ? "Null Socket" : Socket.ReadyState)}\n{e}");
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
                Log.Debug($"Current queue length: {SendQueue.Count}", QuerySystem.Singleton.Config.Debug);
                if (SendThread == null || !SendThread.IsAlive)
                {
                    SendThread?.Abort();
                    SendThread = new Thread(HandleSendQueue);
                    SendThread.Start();
                }
                Log.Debug(ev.Data, QuerySystem.Singleton.Config.Debug);
                QueryCommand cmd = JsonConvert.DeserializeObject<QueryCommand>(ev.Data);

                var jsonData = cmd.Data;
                string text2 = jsonData["action"];
                Log.Debug(text2, QuerySystem.Singleton.Config.Debug);
                if (text2 != null)
                {
                    if (text2 == "ping")
                    {
                        Log.Debug("IsPing", QuerySystem.Singleton.Config.Debug);
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
                        Log.Debug("CustomCommand", QuerySystem.Singleton.Config.Debug);
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
                            Log.Debug("CustomCommand", QuerySystem.Singleton.Config.Debug);
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
                            else if (QuerySystem.Singleton.Config.DisallowedWebCommands.Contains(array[0].ToUpper()))
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

                            Log.Debug("CustomCommandCheckPerm", QuerySystem.Singleton.Config.Debug);
                            if (ServerStatic.PermissionsHandler._members.ContainsKey(jsonData["user"]))
                            {
                                Log.Debug("CustomCommandPermGood", QuerySystem.Singleton.Config.Debug);
                                string name = ServerStatic.PermissionsHandler._members[jsonData["user"]];
                                UserGroup ugroup = ServerStatic.PermissionsHandler.GetGroup(name);
                                Log.Debug("CustomCommandPermGood, dispatching", QuerySystem.Singleton.Config.Debug);
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
                                            try
                                            {
                                                //have to do this in a separate method as C# will get angry if the referenced plugin is not installed and this method runs
                                                EventsListCommand(cmd, jsonData["command"], jsonData["user"], ugroup);
                                            }
                                            catch
                                            {
                                            }
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
                                Log.Debug("CustomCommandPermBad, returning", QuerySystem.Singleton.Config.Debug);
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
                        Log.Debug("KickCmd", QuerySystem.Singleton.Config.Debug);
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

        private static void EventsListCommand(QueryCommand cmd, string s, string user, UserGroup userGroup)
        {
            new ListEvents().Execute(
                new ArraySegment<string>(s.Split(' ').Skip(1).ToArray()),
                new CmSender(cmd.Recipient, user, user, userGroup),
                out string response1);
            SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = cmd.Recipient,
                Data = new Dictionary<string, string>()
                {
                    {
                        "Message", 
                        $"EVENTS#{response1}"
                    }
                }
            });
        }
    }

    internal class CmSender : CommandSender
    {
        public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay)
        {
            Log.Debug($"RA Reply: {text} Suc: {success}", QuerySystem.Singleton.Config.Debug);
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = Ses,
                Data = new Dictionary<string, string>()
                {
                    {"Message", text}
                }
            });
            Log.Debug("Added RA message to queue", QuerySystem.Singleton.Config.Debug);
        }

        public override void Print(string text)
        {
            Log.Debug($"RA Print: {text}", QuerySystem.Singleton.Config.Debug);
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = Ses,
                Data = new Dictionary<string, string>()
                {
                    {"Message", text}
                }
            });
            Log.Debug("Added RA message to queue", QuerySystem.Singleton.Config.Debug);
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
            Log.Debug($"RA Response: {text} Suc: {success}", QuerySystem.Singleton.Config.Debug);
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = Ses,
                Data = new Dictionary<string, string>()
                {
                    {
                        "Message", 
                        text
                    }
                }
            });
            Log.Debug("Added RA message to queue", QuerySystem.Singleton.Config.Debug);
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