using EXILED;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Mirror;
using System.Net;
using System.Text;
using EXILED.Extensions;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

internal static class WebService
{
    /// <summary>
    /// The port the HttpListener should listen on
    /// </summary>
    /// 
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

    /// <summary>
    /// This is the heart of the web server
    /// </summary>
    private static readonly HttpListener Listener = new HttpListener { Prefixes = { $"http://*:{Port}/" } };

    /// <summary>
    /// A flag to specify when we need to stop
    /// </summary>
    private static bool _keepGoing = true;

    /// <summary>
    /// Keep the task in a static variable to keep it alive
    /// </summary>
    private static Task _mainLoop;

    /// <summary>
    /// Call this to start the web server
    /// </summary>
    public static void StartWebServer()
    {
        if (_mainLoop != null && !_mainLoop.IsCompleted) return; //Already started
        _mainLoop = MainLoop();
    }

    /// <summary>
    /// Call this to stop the web server. It will not kill any requests currently being processed.
    /// </summary>
    public static void StopWebServer()
    {
        _keepGoing = false;
        lock (Listener)
        {
            //Use a lock so we don't kill a request that's currently being processed
            Listener.Stop();
        }
        try
        {
            _mainLoop.Wait();
        }
        catch { /* je ne care pas */ }
    }

    /// <summary>
    /// The main loop to handle requests into the HttpListener
    /// </summary>
    /// <returns></returns>
    private static async Task MainLoop()
    {
        Listener.Start();
        while (_keepGoing)
        {
            try
            {
                //GetContextAsync() returns when a new request come in
                HttpListenerContext context = await Listener.GetContextAsync();
                lock (Listener)
                {
                    if (_keepGoing) ProcessRequest(context);
                }
            }
            catch (Exception e)
            {
                if (e is HttpListenerException) return; //this gets thrown when the listener is stopped
                                                        //TODO: Log the exception
            }
        }
    }

    /// <summary>
    /// Handle an incoming request
    /// </summary>
    /// <param name="context">The context of the incoming request</param>
    private static void ProcessRequest(HttpListenerContext context)
    {
        using (HttpListenerResponse response = context.Response)
        {
            try
            {
                bool handled = false;
                switch (context.Request.Url.AbsolutePath)
                {
                    //This is where we do different things depending on the URL
                    //TODO: Add cases for each URL we want to respond to
                    
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
                                    using (var client = new WebClient())
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
                                handled = true;
                                break;

                            case "POST":
                                using (Stream body = context.Request.InputStream)
                                using (StreamReader reader = new StreamReader(body, context.Request.ContentEncoding))
                                {
                                    //Get the data that was sent to us
                                    string json = reader.ReadToEnd();
                                    Dictionary<string, string> jsonData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json.ToString());
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
                                                    ServerConsole.Disconnect(player,jsonData["reason"]);
                                                }
                                            }
                                            break;
                                    }
                                    //Return 204 No Content to say we did it successfully
                                    response.StatusCode = 204;
                                    handled = true;
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
                                string json = "{'players': '"+GetCount()+"'}";
                                string responseBody = JsonConvert.SerializeObject(json);
                                //This is what we want to send back

                                //Write it to the response stream
                                byte[] buffer = Encoding.UTF8.GetBytes(responseBody);
                                response.ContentLength64 = buffer.Length;
                                response.OutputStream.Write(buffer, 0, buffer.Length);
                                handled = true;
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
                            using (var client = new WebClient())
                            {
                                ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                client.DownloadFile("https://api.cedmod.nl/scpplugin/404.html", "static/404.html");
                            }
                        }
                        string error404body = File.ReadAllText(@"static\404.html");
                        //Write it to the response stream
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
                //Return the exception details the client - you may or may not want to do this
                response.StatusCode = 500;
                response.ContentType = "application/json";
                byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(e));
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);

                //TODO: Log the exception
            }
        }
    }
    private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
    {
        // If the certificate is a valid, signed certificate, return true.
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