using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using CedMod.INIT;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using GameCore;
using Mirror;
using Newtonsoft.Json;
using RemoteAdmin;
using ServerOutput;
using UnityEngine;
using Console = System.Console;
using Object = UnityEngine.Object;

namespace CedMod.PluginInterface
{
    internal static class WebService
    {
        public static string GetCount()
        {
            int p = 0;
            foreach (GameObject pl in PlayerManager.players)
            {
                if (!pl.GetComponent<CharacterClassManager>().isLocalPlayer)
                    p++;
            }

            string playersammount = p.ToString();
            return playersammount;
        }

        private static int _port = ConfigFile.ServerConfig.GetInt("cm_port", 8000);
        private static readonly HttpListener Listener = new HttpListener {Prefixes = {$"http://*:{_port}/"}};
        private static bool _keepGoing = true;
        private static Task _mainLoop;
        static List<string> responses = new List<string>();
        public static List<string> queue = new List<string>();
        public static bool ClientConnected = false;

        public static void StartWebServer()
        {
            if (_mainLoop != null && !_mainLoop.IsCompleted) return;
            _mainLoop = MainLoop();
        }

        public static void StopWebServer()
        {
            _keepGoing = false;
            lock (Listener)
            {
                Listener.Stop();
            }

            try
            {
                _mainLoop.Wait();
            }
            catch (Exception ex)
            {
                Initializer.Logger.LogException(ex, "CedMod.PluginInterface", "StopWebServer");
                Initializer.Logger.Error("PluginInterface", ex.StackTrace);
            }
        }

        private static async Task MainLoop()
        {
            Listener.Start();
            while (_keepGoing)
            {
                try
                {
                    HttpListenerContext context = await Listener.GetContextAsync();
                    lock (Listener)
                    {
                        if (_keepGoing) ProcessRequest(context);
                    }
                }
                catch (Exception e)
                {
                    if (e is HttpListenerException) return;
                    Initializer.Logger.LogException(e, "CedMod.PluginInterface", "MainLoop");
                }
            }
        }

        private static void ProcessRequest(HttpListenerContext context)
        {
            using (HttpListenerResponse response = context.Response)
            {
                try
                {
                    switch (context.Request.Url.AbsolutePath)
                    {
                        case "/":
                            switch (context.Request.HttpMethod)
                            {
                                case "GET":
                                    response.ContentType = "text/html";
                                    if (!Directory.Exists("static"))
                                    {
                                        Directory.CreateDirectory("static");
                                    }

                                    if (!File.Exists("static/index.html"))
                                    {
                                        using (WebClient client = new WebClient())
                                        {
                                            ServicePointManager.ServerCertificateValidationCallback +=
                                                ValidateRemoteCertificate;
                                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                            client.DownloadFile("https://api.cedmod.nl/scpplugin/index.html",
                                                "static/index.html");
                                        }
                                    }

                                    string responseBody1 = File.ReadAllText(@"static\index.html");
                                    //Write it to the response stream
                                    byte[] buffer = Encoding.UTF8.GetBytes(responseBody1);
                                    response.ContentLength64 = buffer.Length;
                                    response.OutputStream.Write(buffer, 0, buffer.Length);
                                    break;

                                case "POST":
                                    using (Stream body = context.Request.InputStream)
                                    using (StreamReader reader =
                                        new StreamReader(body, context.Request.ContentEncoding))
                                    {
                                        //Get the data that was sent to us
                                        string json = reader.ReadToEnd();
                                        Dictionary<string, string> jsonData =
                                            JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                                        if (jsonData.ContainsKey("key") && jsonData.ContainsKey("user") &&
                                            jsonData.ContainsKey("action"))
                                        {
                                            if (jsonData["key"] != CedModPluginInterface.SecurityKey ||
                                                jsonData["user"] == null)
                                            {
                                                Initializer.Logger.Warn("PluginInterface",
                                                    "Unauthorized connection attempt: " +
                                                    context.Request.RemoteEndPoint + " request params: " + json);
                                                throw new UnauthorizedAccessException("Login is required");
                                            }
                                        }
                                        else
                                        {
                                            Initializer.Logger.Warn("PluginInterface",
                                                "Unauthorized connection attempt: " + context.Request.RemoteEndPoint +
                                                " request params: " + json);
                                            throw new ArgumentException("Missing arguments");
                                        }

                                        Initializer.Logger.Warn("PluginInterface", "Command recieved: " + json);
                                        ;
                                        switch (jsonData["action"])
                                        {
                                            case "kicksteamid":
                                                foreach (GameObject player in PlayerManager.players)
                                                {
                                                    CharacterClassManager component =
                                                        player.GetComponent<CharacterClassManager>();
                                                    if (component.UserId == jsonData["steamid"])
                                                    {
                                                        ServerConsole.Disconnect(player, jsonData["reason"]);
                                                    }
                                                }

                                                Dictionary<string, string> json1 = new Dictionary<string, string>();
                                                json1.Add("success", "true");
                                                json1.Add("error", "none");
                                                string responseBody = JsonConvert.SerializeObject(json1);
                                                byte[] buffer1 = Encoding.UTF8.GetBytes(responseBody);
                                                response.ContentLength64 = buffer1.Length;
                                                response.OutputStream.Write(buffer1, 0, buffer1.Length);
                                                response.StatusCode = 200;

                                                break;
                                            case "custom":
                                                if (!jsonData.ContainsKey("command"))
                                                    throw new ArgumentException("Missing argument");
                                                string[] command = jsonData["command"].Split(' ');
                                                if (jsonData["command"].ToUpper().Contains("REQUEST_DATA AUTH"))
                                                    throw new UnauthorizedAccessException(
                                                        "Command disabled due to security concerns");
                                                if (CedModPluginInterface.config.DisallowedWebCommands.Contains(
                                                    command[0].ToUpper()))
                                                    throw new UnauthorizedAccessException(
                                                        "This command is disabled by a server aministrator.");
                                                if (jsonData.ContainsKey("userid"))
                                                {
                                                    string group =
                                                        ServerStatic.PermissionsHandler._members[jsonData["userid"]];
                                                    UserGroup ugroup = ServerStatic.PermissionsHandler.GetGroup(group);
                                                    CommandProcessor.ProcessQuery(jsonData["command"],
                                                        new CmSender(jsonData["user"], jsonData["userid"], ugroup));
                                                }
                                                else
                                                {
                                                    CommandProcessor.ProcessQuery(jsonData["command"],
                                                        new CmSender(jsonData["user"]));
                                                }

                                                Dictionary<string, string> json11 = new Dictionary<string, string>();
                                                json11.Add("success", "true");
                                                string responsess = "";
                                                foreach (string res in responses)
                                                {
                                                    responsess = responsess + Environment.NewLine + res;
                                                }

                                                responses.Clear();
                                                json11.Add("message", responsess);
                                                json11.Add("error", "none");
                                                string responseBody11 = JsonConvert.SerializeObject(json11);
                                                byte[] buffer11 = Encoding.UTF8.GetBytes(responseBody11);
                                                response.ContentLength64 = buffer11.Length;
                                                response.OutputStream.Write(buffer11, 0, buffer11.Length);
                                                response.StatusCode = 200;
                                                break;
                                        }
                                    }

                                    break;
                            }
                            break;
                        default:
                            response.ContentType = "text/html";
                            if (!Directory.Exists("static"))
                            {
                                Directory.CreateDirectory("static");
                            }

                            if (!File.Exists("static/404.html"))
                            {
                                using (WebClient client = new WebClient())
                                {
                                    ServicePointManager.ServerCertificateValidationCallback +=
                                        ValidateRemoteCertificate;
                                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                    client.DownloadFile("https://api.cedmod.nl/scpplugin/404.html", "static/404.html");
                                }
                            }
                            string error404Body = File.ReadAllText(@"static\404.html");
                            byte[] buffer404 = Encoding.UTF8.GetBytes(error404Body);
                            response.ContentLength64 = buffer404.Length;
                            response.OutputStream.Write(buffer404, 0, buffer404.Length);
                            response.StatusCode = 404;
                            Initializer.Logger.Warn("PluginInterface",
                                "404 error served request url: " + context.Request.Url.AbsolutePath);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.GetType() == typeof(ArgumentException))
                        context.Response.StatusCode = 400;
                    else
                        context.Response.StatusCode = 500;
                    if (ex.GetType() == typeof(UnauthorizedAccessException))
                        context.Response.StatusCode = 401;
                    Dictionary<string, string> json1 = new Dictionary<string, string>();
                    json1.Add("success", "false");
                    json1.Add("message", ex.Message);
                    string responseBody = JsonConvert.SerializeObject(json1);
                    byte[] buffer1 = Encoding.UTF8.GetBytes(responseBody);
                    response.ContentLength64 = buffer1.Length;
                    response.OutputStream.Write(buffer1, 0, buffer1.Length);
                    Initializer.Logger.LogException(ex, "CedMod.PluginInterface", "ProcessRequest");
                }
            }
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain,
            SslPolicyErrors error)
        {
            if (error == SslPolicyErrors.None)
            {
                return true;
            }

            Console.WriteLine("X509Certificate [{0}] Policy Error: '{1}'",
                cert.Subject,
                error.ToString());

            return false;
        }

        
        class CmSender : CommandSender
        {
            public override void RaReply(string text, bool success, bool logToConsole, string overrideDisplay)
            {
                responses.Add(text);
            }

            public override void Print(string text)
            {
                responses.Add(text);
            }

            public string Name;
            public string senderId;
            public ulong permissions;
            public bool fullPermissions;
            public CmSender(string name, string userid, UserGroup group)
            {
                Name = name;
                senderId = userid;
                permissions = group.Permissions;
                fullPermissions = false;
            }
            public CmSender(string name)
            {
                Name = name;
                senderId = "SERVER CONSOLE";
                permissions = ServerStatic.PermissionsHandler.FullPerm;
                fullPermissions = true;
            }

            public override void Respond(string text, bool success = true)
            {
                responses.Add(text);
            }
            

            public override string SenderId => senderId;
            public override string Nickname => Name;
            public override ulong Permissions => permissions;
            public override byte KickPower => byte.MaxValue;
            public override bool FullPermissions => fullPermissions;

            
            public override string LogName
            {
                get { return Nickname + " (" + SenderId + ")"; }
            }
        }
    }
}