using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using CedMod.INIT;
using Exiled.API.Features;
using GameCore;
using Mirror;
using Newtonsoft.Json;
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
        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain,
            SslPolicyErrors error)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (error == SslPolicyErrors.None) return true;

            Console.WriteLine("X509Certificate [{0}] Policy Error: '{1}'",
                cert.Subject,
                error.ToString());

            return false;
        }
        public static Dictionary<string, string> APIRequest(string endpoint, string arguments)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                WebClient webClient = new WebClient();
                webClient.Credentials = new NetworkCredential(ConfigFile.ServerConfig.GetString("bansystem_apikey"), ConfigFile.ServerConfig.GetString("bansystem_apikey"));
                webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                webClient.Headers.Add("Alias", GetAlias());
                webClient.Headers.Add("Port", ServerConsole.Port.ToString());
                webClient.Headers.Add("Ip", ServerConsole.Ip);
                string response;  
                if (Initializer.TestApiOnly)
                    response = webClient.DownloadString(TestAPIUrl + endpoint + arguments);
                else
                    response = webClient.DownloadString(APIUrl + endpoint + arguments);
                Initializer.Logger.Info("BANSYSTEM",
                    "Response from API: "+  response);
                var jsonObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                return jsonObj;
            }
            catch (WebException ex)
            {
                Initializer.Logger.Error("API",
                    "API request failed: " + ex.Status + " | " + ex.Message);
                return null;
            }
        }

        public static IEnumerator<float> Ban(GameObject player, long duration, string sender, string reason, bool bc = true)
        {
            if (duration >= 1)
            {
                if (bc)
                    Map.Broadcast(9, player.GetComponent<NicknameSync>().MyNick + " Has been banned from the server",
                        Broadcast.BroadcastFlags.Normal);
                Dictionary<string, string> result = APIRequest("banning/ban.php", string.Concat("?id=",
                    player.GetComponent<CharacterClassManager>().UserId, "&ip=",
                    player.GetComponent<NetworkIdentity>().connectionToClient.address, "&reason=", reason,
                    "&aname=", sender, "&bd=", duration,
                    "&alias=" + GetAlias() + "&webhook=" +
                    ConfigFile.ServerConfig.GetString("bansystem_webhook", "none")));
                ServerConsole.Disconnect(player, result["preformattedmessage"]);
            }
            else
            {
                if (duration <= 0)
                {
                    ServerConsole.Disconnect(player, reason);
                }
            }

            yield return 1f;
        }
    }
}