using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CedMod.INIT;
using Exiled.API.Features;
using GameCore;
using Mirror;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;
using UnityEngine;
using Console = System.Console;

namespace CedMod
{
    public static class API
    {
        public static readonly Uri APIUrl = new Uri("https://api.cedmod.nl/");
        public static readonly Uri TestAPIUrl = new Uri("https://test.cedmod.nl/");

        public static string GetAlias()
        {
            var alias = ConfigFile.ServerConfig.GetString("bansystem_alias", "none");
            alias = alias.Replace(" ", "");
            return alias;
        }

        public static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain,
            SslPolicyErrors error)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (error == SslPolicyErrors.None) return true;

            Console.WriteLine("X509Certificate [{0}] Policy Error: '{1}'",
                cert.Subject,
                error.ToString());

            return false;
        }
        public static object APIRequest(string endpoint, string arguments, bool returnstring = false, string type = "GET")
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string response = "";  
            try
            {
                if (type == "GET")
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("ApiKey", ConfigFile.ServerConfig.GetString("bansystem_apikey"));
                    if (Initializer.TestApiOnly)
                        response = client.GetAsync(TestAPIUrl + endpoint + arguments).Result.Content.ReadAsStringAsync().Result;
                    else
                        response = client.GetAsync(APIUrl + endpoint + arguments).Result.Content.ReadAsStringAsync().Result;
                }

                if (type == "POST")
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("ApiKey", ConfigFile.ServerConfig.GetString("bansystem_apikey"));
                    if (Initializer.TestApiOnly)
                        response = client.PostAsync(TestAPIUrl + endpoint, new StringContent(arguments, Encoding.UTF8, "application/json")).Result.Content.ReadAsStringAsync().Result;
                    else
                        response = client.PostAsync(APIUrl + endpoint, new StringContent(arguments, Encoding.UTF8, "application/json")).Result.Content.ReadAsStringAsync().Result;
                }
                Initializer.Logger.Info("BANSYSTEM",
                    "Response from API: "+  response);
                if (!returnstring)
                {
                    var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                    return jsonObj;
                }
                return response;
            }
            catch (WebException ex)
            {
                using (StreamReader r = new StreamReader(((HttpWebResponse)ex.Response).GetResponseStream()))
                {
                    response = r.ReadToEnd();
                }
                if (string.IsNullOrEmpty(response))
                    SentrySdk.CaptureMessage($"API-Request failed on {GetAlias()} Response code {ex.Status} {ex.Message}", SentryLevel.Warning);
                else
                    SentrySdk.CaptureMessage($"API-Request failed on {GetAlias()} Response code {ex.Status} {ex.Message} API response was {response}", SentryLevel.Warning);
                Initializer.Logger.Error("API",
                    "API request failed: " + response + " | " + ex.Message);
                return null;
            }
        }

        public static void Ban(GameObject player, long duration, string sender, string reason, bool bc = true)
        {
            if (duration >= 1)
            {
                string json = "{\"Userid\": \"" + player.GetComponent<CharacterClassManager>().UserId + "\"," +
                              "\"Ip\": \"" + player.GetComponent<CharacterClassManager>().connectionToClient.address+"\"," +
                              "\"AdminName\": \"" + sender.Replace("\"", "'") + "\"," +
                              "\"BanDuration\": "+duration+"," +
                              "\"BanReason\": \""+reason.Replace("\"", "'")+"\"}";
                Dictionary<string, string> result = (Dictionary<string, string>) APIRequest("Auth/Ban", json, false, "POST"); 
                ServerConsole.Disconnect(player, result["preformattedmessage"]);
                if (bc)
                    Map.Broadcast((ushort) ConfigFile.ServerConfig.GetInt("broadcast_ban_duration", 5), ConfigFile.ServerConfig.GetString("broadcast_ban_text", "%nick% has been banned from this server.").Replace("%nick%", player.GetComponent<NicknameSync>().MyNick),
                        Broadcast.BroadcastFlags.Normal);
            }
            else
            {
                if (duration <= 0)
                {
                    ServerConsole.Disconnect(player, reason);
                }
            }
        }
    }
}
