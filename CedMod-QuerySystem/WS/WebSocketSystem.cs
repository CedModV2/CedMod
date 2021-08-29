using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Exiled.API.Features;
using Newtonsoft.Json;
using RemoteAdmin;
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

        public static void Stop()
        {
            Socket.Close();
            Socket = null;
        }
        public static void Start()
        {
            Socket = new WebSocket($"wss://{QuerySystem.PanelUrl}/QuerySystem?key={QuerySystem.Singleton.Config.SecurityKey}&identity={QuerySystem.Singleton.Config.Identifier}");
            Socket.Connect();
            Socket.OnMessage += OnMessage;
            Socket.OnClose += (sender, args) =>
            {
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

        }

        static void OnMessage(object sender, MessageEventArgs ev)
        {
            try
            {
                Log.Debug(ev.Data, CedModMain.Singleton.Config.ShowDebug);
                QueryCommand cmd = JsonConvert.DeserializeObject<QueryCommand>(ev.Data);

                var jsonData = cmd.Data;
                string text2 = jsonData["action"];
                if (text2 != null)
                {
                    if (text2 == "ping")
                        {
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
                                    if (ServerStatic.PermissionsHandler._members.ContainsKey(jsonData["user"]))
                                    {
                                        string name = ServerStatic.PermissionsHandler._members[jsonData["user"]];
                                        UserGroup ugroup = ServerStatic.PermissionsHandler.GetGroup(name);
                                        MainThreadDispatcher.Dispatch(delegate
                                        {
                                            CommandProcessor.ProcessQuery(jsonData["command"],
                                                new CmSender(cmd.Recipient, jsonData["user"], jsonData["user"], ugroup));
                                        });
                                    }
                                    else
                                    {
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
                        foreach (Player player in Player.List)
                        {
                            CharacterClassManager component = player.ReferenceHub.characterClassManager;
                            if (component.UserId == jsonData["steamid"])
                            {
                                ServerConsole.Disconnect(player.GameObject, jsonData["reason"]);
                            }
                        }

                        Socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                        {
                            Recipient = cmd.Recipient,
                            Data = new Dictionary<string, string>()
                            {
                                {"Message", "User kicked"}
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
            Task.Factory.StartNew(delegate
            {
                WebSocketSystem.Socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = Ses,
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", text}
                    }
                }));
            });
        }

        public override void Print(string text)
        {
            Task.Factory.StartNew(delegate
            {
                WebSocketSystem.Socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = Ses,
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", text}
                    }
                }));
            });
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
            Task.Factory.StartNew(delegate
            {
                WebSocketSystem.Socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = Ses,
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", text}
                    }
                }));
            });
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