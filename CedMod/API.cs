using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Exiled.API.Features;
using GameCore;
using Newtonsoft.Json;
using Log = Exiled.API.Features.Log;

namespace CedMod
{
    public static class API
    {
        public static readonly Uri APIUrl = new Uri("https://api.cedmod.nl/");

        public static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (error == SslPolicyErrors.None) return true;
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
                    client.DefaultRequestHeaders.Add("ApiKey", CedModMain.config.CedModApiKey);
                    response = client.GetAsync(APIUrl + endpoint + arguments).Result.Content.ReadAsStringAsync().Result;
                }

                if (type == "POST")
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("ApiKey", CedModMain.config.CedModApiKey);
                    response = client.PostAsync(APIUrl + endpoint, new StringContent(arguments, Encoding.UTF8, "application/json")).Result.Content.ReadAsStringAsync().Result;
                }
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
                Log.Error($"API request failed: {response} | {ex.Message}");
                return null;
            }
        }

        public static void Ban(Player player, long duration, string sender, string reason, bool bc = true)
        {
            double realduration = TimeSpan.FromSeconds(duration).TotalMinutes;
            if (duration >= 1)
            {
                string json = "{\"Userid\": \"" + player.UserId + "\"," +
                              "\"Ip\": \"" + player.IPAddress+"\"," +
                              "\"AdminName\": \"" + sender.Replace("\"", "'") + "\"," +
                              "\"BanDuration\": "+realduration+"," +
                              "\"BanReason\": \""+reason.Replace("\"", "'")+"\"}";
                Dictionary<string, string> result = (Dictionary<string, string>) APIRequest("Auth/Ban", json, false, "POST"); 
                ServerConsole.Disconnect(player.GameObject, result["preformattedmessage"]);
                if (bc)
                    Map.Broadcast((ushort) ConfigFile.ServerConfig.GetInt("broadcast_ban_duration", 5), ConfigFile.ServerConfig.GetString("broadcast_ban_text", "%nick% has been banned from this server.").Replace("%nick%", player.Nickname), Broadcast.BroadcastFlags.Normal);
            }
            else
            {
                if (duration <= 0)
                {
                    ServerConsole.Disconnect(player.GameObject, reason + "\n" + CedModMain.config.AdditionalBanMessage);
                }
            }
        }
    }
}
