using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
                    Log.Error($"Lost connection to CedMod Panel {args.Reason}, reconnecting in 1000ms");
                    Thread.Sleep(1000);
                    Log.Info("Reconnecting...");
                    Socket.Connect();
                    Log.Debug("Connect", CedModMain.Singleton.Config.ShowDebug);
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
            Log.Debug("Started SendQueueHandler", CedModMain.Singleton.Config.ShowDebug);
            while (Socket.IsAlive)
            {
                while (SendQueue.TryDequeue(out QueryCommand cmd))
                {
                    try
                    {
                        Log.Debug($"Handling send {JsonConvert.SerializeObject(cmd)}", CedModMain.Singleton.Config.ShowDebug);
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
                Log.Debug($"Current queue length: {SendQueue.Count}", CedModMain.Singleton.Config.ShowDebug);
                if (SendThread == null || !SendThread.IsAlive)
                {
                    SendThread?.Abort();
                    SendThread = new Thread(HandleSendQueue);
                    SendThread.Start();
                }
                Log.Debug(ev.Data, CedModMain.Singleton.Config.ShowDebug);
                QueryCommand cmd = JsonConvert.DeserializeObject<QueryCommand>(ev.Data);

                var jsonData = cmd.Data;
                string text2 = jsonData["action"];
                Log.Debug(text2, CedModMain.Singleton.Config.ShowDebug);
                if (text2 != null)
                {
                    if (text2 == "ping")
                        {
                            Log.Debug("IsPing", CedModMain.Singleton.Config.ShowDebug);
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
                            Log.Debug("CustomCommand", CedModMain.Singleton.Config.ShowDebug);
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
                                Log.Debug("CustomCommand", CedModMain.Singleton.Config.ShowDebug);
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
                                else if (QuerySystem.Singleton.Config.DisallowedWebCommands.Contains(
                                    array[0].ToUpper()))
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


                                {
                                    Log.Debug("CustomCommandCheckPerm", CedModMain.Singleton.Config.ShowDebug);
                                    if (ServerStatic.PermissionsHandler._members.ContainsKey(jsonData["user"]))
                                    {
                                        Log.Debug("CustomCommandPermGood", CedModMain.Singleton.Config.ShowDebug);
                                        string name = ServerStatic.PermissionsHandler._members[jsonData["user"]];
                                        UserGroup ugroup = ServerStatic.PermissionsHandler.GetGroup(name);
                                        Log.Debug("CustomCommandPermGood, dispatching", CedModMain.Singleton.Config.ShowDebug);
                                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(delegate
                                        {
                                            CommandProcessor.ProcessQuery(jsonData["command"],
                                                new CmSender(cmd.Recipient, jsonData["user"], jsonData["user"], ugroup));
                                        });
                                    }
                                    else
                                    {
                                        Log.Debug("CustomCommandPermBad, returning", CedModMain.Singleton.Config.ShowDebug);
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
                        }
                    if (text2 == "kicksteamid")
                    {
                        Log.Debug("KickCmd", CedModMain.Singleton.Config.ShowDebug);
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
        public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay)
        {
            Log.Debug($"RA Reply: {text} Suc: {success}", CedModMain.Singleton.Config.ShowDebug);
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = Ses,
                Data = new Dictionary<string, string>()
                {
                    {"Message", text}
                }
            });
            Log.Debug("Added RA message to queue", CedModMain.Singleton.Config.ShowDebug);
        }

        public override void Print(string text)
        {
            Log.Debug($"RA Print: {text}", CedModMain.Singleton.Config.ShowDebug);
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = Ses,
                Data = new Dictionary<string, string>()
                {
                    {"Message", text}
                }
            });
            Log.Debug("Added RA message to queue", CedModMain.Singleton.Config.ShowDebug);
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
            Log.Debug($"RA Response: {text} Suc: {success}", CedModMain.Singleton.Config.ShowDebug);
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
            Log.Debug("Added RA message to queue", CedModMain.Singleton.Config.ShowDebug);
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