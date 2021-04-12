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
	public class WebSocketSystemBehavior : WebSocketBehavior
    {
	    private void DieIfNotAuthed()
        {
            Thread.Sleep(2000);
            if (!authed)
            {
                Context.WebSocket.Send("Failed to authenticate in time");
                Context.WebSocket.Close();
            }
        }
	    
        protected override void OnOpen()
        {
	        WebSocketSystem.Clients.Add(this);
            Task.Factory.StartNew(delegate() { DieIfNotKepAlive(); });
            Task.Factory.StartNew(delegate() { DieIfNotAuthed(); });
        }
        
        protected override void OnClose(CloseEventArgs e)
        {
	        WebSocketSystem.Clients.Remove(this);
        }
        
        public void DieIfNotKepAlive()
        {
            Thread.Sleep(5000);
            if (DateTime.Now.Subtract(this.lastkeepalivetime) >= TimeSpan.FromSeconds(5.0))
            {
                Context.WebSocket.Close();
            }

            DieIfNotKepAlive();
        }
        
        protected override void OnMessage(MessageEventArgs ev)
        {
            if (ev.Data.StartsWith("STAYALIVE"))
            {
                lastkeepalivetime = DateTime.Now;
                return;
            }

            try
            {
                if (ev.Data.StartsWith("CMAUTHENTICATE"))
                {
                    if (!authed)
                    {
                        string securityKey = QuerySystem.SecurityKey;
                        SHA256 sha = SHA256.Create();
                        byte[] iv = new byte[16];
                        byte[] key = sha.ComputeHash(Encoding.ASCII.GetBytes(securityKey));
                        string text = QuerySystem.DecryptString(ev.Data.Split(new char[]
                        {
                            ' '
                        })[1], key, iv);
                        if (text.Split(new char[]
                        {
                            ':'
                        })[0] == "youcantrustme")
                        {
                            authed = true;
                            return;
                        }

                        Send("Authentication failed");
                        Context.WebSocket.Close();
                    }
                    else
                    {
                        Send("You are already authenticated");
                    }
                }

                if (authed)
                {
                    Dictionary<string, string> jsonData =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(ev.Data);
                    string securityKey2 = QuerySystem.SecurityKey;
                    SHA256 sha2 = SHA256.Create();
                    byte[] iv2 = new byte[16];
                    byte[] key2 = sha2.ComputeHash(Encoding.ASCII.GetBytes(securityKey2));
                    string decrypted = QuerySystem.DecryptString(jsonData["key"], key2, iv2);
                    if (decrypted.Split(new char[]
                    {
                        ':'
                    })[0] == "youcantrustme")
                    {
                        string text2 = jsonData["action"];
                        if (text2 != null)
                        {
                            if (!(text2 == "kicksteamid"))
                            {
                                if (text2 == "custom")
                                {
                                    if (!jsonData.ContainsKey("command"))
                                    {
                                        Send("Missing argument");
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
                                            Send("This command is disabled");
                                        }
                                        else if (QuerySystem.config.DisallowedWebCommands.Contains(
                                            array[0].ToUpper()))
                                        {
                                            Send("This command is disabled");
                                        }
                                        else if (!string.IsNullOrWhiteSpace(decrypted.Split(new char[]
                                        {
                                            ':'
                                        })[1]))
                                        {
                                            string name = ServerStatic.PermissionsHandler._members[decrypted.Split(
                                                new char[]
                                                {
                                                    ':'
                                                })[1]];
                                            UserGroup ugroup = ServerStatic.PermissionsHandler.GetGroup(name);
                                            MainThreadDispatcher.Dispatch(delegate()
                                            {
                                                CommandProcessor.ProcessQuery(jsonData["command"],
                                                    new CmSender(this, jsonData["user"], decrypted.Split(new char[]
                                                    {
                                                        ':'
                                                    })[1], ugroup));
                                            }, 0);
                                        }
                                        else
                                        {
                                            MainThreadDispatcher.Dispatch(
                                                delegate()
                                                {
                                                    CommandProcessor.ProcessQuery(jsonData["command"],
                                                        new CmSender(this, jsonData["user"]));
                                                }, 0);
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

                                Send("User kicked");
                            }
                        }
                    }
                    else
                    {
                        Send("Authentication failed");
                        authed = false;
                        Context.WebSocket.Close();
                    }
                }
                else
                {
                    Send("Authentication failed");
                    Context.WebSocket.Close();
                }
            }
            catch (Exception ex)
            {
                Exiled.API.Features.Log.Error(ex.ToString());
                Context.WebSocket.Close();
            }
        }
        
        private DateTime lastkeepalivetime;
        
        public bool authed;
    }

    public class WebSocketSystem
    {
	    public static void Start()
        {
            Stop();
            Server = new WebSocketServer(QuerySystem.config.Port, false);
            Server.AddWebSocketService<WebSocketSystemBehavior>("/");
            Server.Start();
            Log.Info($"Server started {Server.Address} {Server.Port}");
        }
	    
        public static void Stop()
        {
            if (Server != null)
            {
                Server.Stop();
                Server = null;
            }
            Log.Info("Server stopped");
        }
        
        public static WebSocketServer Server;
        
        public static List<WebSocketSystemBehavior> Clients = new List<WebSocketSystemBehavior>();
    }
    
    internal class CmSender : CommandSender
	{
		public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay)
		{
			Task.Factory.StartNew(delegate()
			{
				Ses.Context.WebSocket.Send(text);
			});
		}
		
		public override void Print(string text)
		{
			Task.Factory.StartNew(delegate()
			{
				Ses.Context.WebSocket.Send(text);
			});
		}
		
		public CmSender(WebSocketSystemBehavior ses, string name, string userid, UserGroup group)
		{
			Ses = ses;
			Name = name;
			senderId = userid;
			permissions = group.Permissions;
			fullPermissions = false;
		}
		
		public CmSender(WebSocketSystemBehavior ses, string name)
		{
			Ses = ses;
			Name = name;
			senderId = "SERVER CONSOLE";
			permissions = ServerStatic.PermissionsHandler.FullPerm;
			fullPermissions = true;
		}
		
		public override void Respond(string text, bool success = true)
		{
			Ses.Context.WebSocket.Send(text);
		}
		
		public override string SenderId
		{
			get
			{
				return senderId;
			}
		}
		public override string Nickname
		{
			get
			{
				return Name;
			}
		}
		
		public override ulong Permissions
		{
			get
			{
				return permissions;
			}
		}

		public override byte KickPower
		{
			get
			{
				return byte.MaxValue;
			}
		}

		public override bool FullPermissions
		{
			get
			{
				return fullPermissions;
			}
		}
		
		public override string LogName
		{
			get
			{
				return Nickname + " (" + SenderId + ")";
			}
		}
		
		public WebSocketSystemBehavior Ses;
		
		public string Name;
		
		public string senderId;
		
		public ulong permissions;
		
		public bool fullPermissions;
	}
}