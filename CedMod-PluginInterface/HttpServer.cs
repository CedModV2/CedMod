using EXILED;
using EXILED.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
    private static int Port = GameCore.ConfigFile.ServerConfig.GetInt("cm_port", 8000);
    private static readonly HttpListener Listener = new HttpListener { Prefixes = { $"http://*:{Port}/" } };
    private static bool _keepGoing = true;
    private static Task _mainLoop;
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
        catch { }
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
                                        ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
                                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                        client.DownloadFile("https://api.cedmod.nl/scpplugin/index.html", "static/index.html");
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
                                using (StreamReader reader = new StreamReader(body, context.Request.ContentEncoding))
                                {
                                    //Get the data that was sent to us
                                    string json = reader.ReadToEnd();
                                    Dictionary<string, string> jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json.ToString());
                                    if (jsonData.ContainsKey("key") && jsonData.ContainsKey("user"))
                                    {
                                        if (jsonData["key"] != GameCore.ConfigFile.ServerConfig.GetString("cm_plugininterface_key", "HYGutFGytfYTF&RF67fVYT") || jsonData["user"] == null)
                                        {
                                            Log.Warn("Unauthorized connection attempt: " + context.Request.RemoteEndPoint + " request params: " + json);
                                            response.StatusCode = 401;
                                            string json1 = "{'unauthorized': 'true'}";
                                            string responseBody = JsonConvert.SerializeObject(json1);
                                            byte[] buffer1 = Encoding.UTF8.GetBytes(responseBody);
                                            response.ContentLength64 = buffer1.Length;
                                            response.OutputStream.Write(buffer1, 0, buffer1.Length);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        Log.Warn("Unauthorized connection attempt: " + context.Request.RemoteEndPoint + " request params: " + json);
                                        response.StatusCode = 401;
                                        string json1 = "{'unauthorized': 'true'}";
                                        string responseBody = JsonConvert.SerializeObject(json1);
                                        byte[] buffer1 = Encoding.UTF8.GetBytes(responseBody);
                                        response.ContentLength64 = buffer1.Length;
                                        response.OutputStream.Write(buffer1, 0, buffer1.Length);
                                        break;
                                    }
                                    Log.Info(jsonData["user"]);
                                    Log.Info(jsonData["action"]);
                                    switch (jsonData["action"])
                                    {
                                        case "broadcast":
                                            Log.Info("Broadcast recieved: " + jsonData["message"]);
                                            foreach (ReferenceHub hub in Player.GetHubs())
                                            {
                                                hub.GetComponent<Broadcast>().TargetAddElement(hub.GetComponent<Scp049PlayerScript>().connectionToClient, jsonData["message"], Convert.ToUInt16(jsonData["duration"]), false);
                                            }
                                            break;
                                        case "kick":
                                            foreach (GameObject player in PlayerManager.players)
                                            {
                                                CharacterClassManager component = player.GetComponent<CharacterClassManager>();
                                                if (component.UserId == jsonData["steamid"])
                                                {
                                                    ServerConsole.Disconnect(player, jsonData["reason"]);
                                                }
                                            }
                                            break;
                                    }
                                    response.StatusCode = 204;
                                }
                                break;
                        }
                        break;
                    case "/stats/":
                    case "/stats":
                        switch (context.Request.HttpMethod)
                        {
                            case "GET":
                                response.ContentType = "application/json";
                                string json = "{'players': '" + GetCount() + "'}";
                                string responseBody = JsonConvert.SerializeObject(json);
                                byte[] buffer = Encoding.UTF8.GetBytes(responseBody);
                                response.ContentLength64 = buffer.Length;
                                response.OutputStream.Write(buffer, 0, buffer.Length);
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
                                ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                client.DownloadFile("https://api.cedmod.nl/scpplugin/404.html", "static/404.html");
                            }
                        }
                        string error404body = File.ReadAllText(@"static\404.html");
                        byte[] buffer404 = Encoding.UTF8.GetBytes(error404body);
                        response.ContentLength64 = buffer404.Length;
                        response.OutputStream.Write(buffer404, 0, buffer404.Length);
                        response.StatusCode = 404;
                        Log.Warn("404 error served request url: " + context.Request.Url.AbsolutePath);
                        break;
                }
            }
            catch (Exception e)
            {
                response.StatusCode = 500;
                response.ContentType = "application/json";
                byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(e));
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
        }
    }
    private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
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


}