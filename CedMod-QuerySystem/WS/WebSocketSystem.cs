using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Exiled.API.Features;
using Newtonsoft.Json;
using RemoteAdmin;
using WebSocketSharp;
using WebSocketSharp.Server;

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
        public static WebSocket socket = null;
        private static object reconnectLock = new object();

        public static void Stop()
        {
            socket.Close();
            socket = null;
        }
        public static void Start()
        {
            socket = new WebSocket($"wss://communitymanagementpanel.cedmod.nl/QuerySystem?key={QuerySystem.config.SecurityKey}&identity={QuerySystem.config.Identifier}");
            socket.Connect();
            socket.OnMessage += OnMessage;
            socket.OnClose += (sender, args) =>
            {
                lock (reconnectLock)
                {
                    Log.Error($"Lost connection to CedMod Panel {args.Reason}, reconnecting in 1000ms");
                    Thread.Sleep(1000);
                    Log.Info("Reconnecting...");
                    socket.Connect();
                    Log.Debug("Connect", CedModMain.config.ShowDebug);
                }
            };
            socket.OnOpen += (sender, args) =>
            {
                Log.Info("Connected.");
            };
            socket.OnError += (sender, args) =>
            {
                Log.Error(args.Message);
                Log.Error(args.Exception);
            };

        }

        static void OnMessage(object sender, MessageEventArgs ev)
        {
            try
            {
                Log.Debug(ev.Data, CedModMain.config.ShowDebug);
                QueryCommand cmd = JsonConvert.DeserializeObject<QueryCommand>(ev.Data);

                var jsonData = cmd.Data;
                string text2 = jsonData["action"];
                if (text2 != null)
                {
                    if (!(text2 == "kicksteamid"))
                    {
                        if (text2 == "ping")
                        {
                            socket.Send(JsonConvert.SerializeObject(new QueryCommand()
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
                                socket.Send(JsonConvert.SerializeObject(new QueryCommand()
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
                                    socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                                    {
                                        Recipient = cmd.Recipient,
                                        Data = new Dictionary<string, string>()
                                        {
                                            {"Message", "This command is disabled"}
                                        }
                                    }));
                                }
                                else if (QuerySystem.config.DisallowedWebCommands.Contains(
                                    array[0].ToUpper()))
                                {
                                    socket.Send(JsonConvert.SerializeObject(new QueryCommand()
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
                                        MainThreadDispatcher.Dispatch(delegate()
                                        {
                                            CommandProcessor.ProcessQuery(jsonData["command"],
                                                new CmSender(cmd.Recipient, jsonData["user"], jsonData["user"], ugroup));
                                        }, 0);
                                    }
                                    else
                                    {
                                        socket.Send(JsonConvert.SerializeObject(new QueryCommand()
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
                    }
                    else
                    {
                        foreach (Player player in Player.List)
                        {
                            CharacterClassManager component = player.ReferenceHub.characterClassManager;
                            if (component.UserId == jsonData["steamid"])
                            {
                                ServerConsole.Disconnect(player.GameObject, jsonData["reason"]);
                            }
                        }

                        socket.Send(JsonConvert.SerializeObject(new QueryCommand()
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
                Exiled.API.Features.Log.Error(ex.ToString());
                //socket.Close();
            }
        }
    }

    internal class CmSender : CommandSender
    {
        public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay)
        {
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
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
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
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
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
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