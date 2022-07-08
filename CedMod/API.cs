using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CedMod.Addons.QuerySystem;
using Exiled.API.Features;
using GameCore;
using InventorySystem.Items.Firearms;
using MEC;
using Newtonsoft.Json;
using PlayerStatsSystem;
using Log = Exiled.API.Features.Log;

namespace CedMod
{
    /// <summary>
    /// Used to manage communication with the panel.
    /// </summary>
    public static class API
    {
        /// <summary>
        /// The url of the api.
        /// </summary>
        public static readonly Uri APIUrl = new Uri("https://api.cedmod.nl/");
        
        /// <summary>
        /// Sends an api request to the panel.
        /// </summary>
        /// <param name="endpoint">The api endpoint.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="returnstring">Whether or not to deserialize the respone.</param>
        /// <param name="type">The type of the request.</param>
        /// <returns>The response. Maybe be <see langword="null"/></returns>
        public static object APIRequest(string endpoint, string arguments, bool returnstring = false, string type = "GET")
        {
            string response = "";  
            try
            {
                HttpResponseMessage resp = null;
                if (type == "GET")
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("ApiKey", CedModMain.Singleton.Config.CedMod.CedModApiKey);
                    resp = client.GetAsync(APIUrl + endpoint + arguments).Result;
                    response = resp.Content.ReadAsStringAsync().Result;
                }
                
                if (type == "DELETE")
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("ApiKey", CedModMain.Singleton.Config.CedMod.CedModApiKey);
                    resp = client.DeleteAsync(APIUrl + endpoint + arguments).Result;
                    response = resp.Content.ReadAsStringAsync().Result;
                }

                if (type == "POST")
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("ApiKey", CedModMain.Singleton.Config.CedMod.CedModApiKey);
                    resp = client.PostAsync(APIUrl + endpoint, new StringContent(arguments, Encoding.UTF8, "application/json")).Result;
                    response = resp.Content.ReadAsStringAsync().Result;
                }

                if (!resp.IsSuccessStatusCode)
                {
                    Log.Error($"API request failed: {resp.StatusCode} | {response}");
                    return null;
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
        
        /// <summary>
        /// Sends a mute request to the panel.
        /// </summary>
        /// <param name="player">The player to mute.</param>
        /// <param name="adminname">The issuer of them mute.</param>
        /// <param name="duration">The duration of the mute.</param>
        /// <param name="reason">The reason of the mute.</param>
        /// <param name="Type">The type of the mute.</param>
        public static void Mute(Player player, string adminname, double duration, string reason, MuteType Type)
        {
            long realduration = (long)TimeSpan.FromSeconds(duration).TotalMinutes;
            string req = "{\"AdminName\": \"" + adminname + "\"," +
                         "\"Muteduration\": " + realduration + "," +
                         "\"Type\": " + (int)Type + "," +
                         "\"Mutereason\": \"" + reason + "\"}";
            Dictionary<string, string> result = (Dictionary<string, string>) APIRequest($"api/Mute/{player.UserId}", req, false, "POST");
        }
        
        /// <summary>
        /// Sends a unmute request to the panel.
        /// </summary>
        /// <param name="player">The player to unmute.</param>
        public static void UnMute(Player player)
        {
            Dictionary<string, string> result = (Dictionary<string, string>) APIRequest($"api/Mute/{player.UserId}", "", false, "DELETE");
        }

        /// <summary>
        /// Sends a ban request to the panel.
        /// </summary>
        /// <param name="player">The player to ban.</param>
        /// <param name="duration">The duration of the ban.</param>
        /// <param name="sender">The issues of the ban.</param>
        /// <param name="reason">The reason of the ban.</param>
        /// <param name="bc">Whether or not to broadcast the ban</param>
        public static void Ban(Player player, long duration, string sender, string reason, bool bc = true)
        {
            long realduration = (long)TimeSpan.FromSeconds(duration).TotalMinutes;
            if (duration >= 1)
            {
                string json = "{\"Userid\": \"" + player.UserId + "\"," +
                              "\"Ip\": \"" + player.IPAddress+"\"," +
                              "\"AdminName\": \"" + sender.Replace("\"", "'") + "\"," +
                              "\"BanDuration\": "+realduration+"," +
                              "\"BanReason\": \""+reason.Replace("\"", "'")+"\"}";
                Dictionary<string, string> result = (Dictionary<string, string>) APIRequest("Auth/Ban", json, false, "POST"); 
                ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>  Timing.RunCoroutine(StrikeBad(player, result.ContainsKey("preformattedmessage") ? result["preformattedmessage"] : $"Failed to execute api request {JsonConvert.SerializeObject(result)}")));
                if (bc)
                    Map.Broadcast((ushort) ConfigFile.ServerConfig.GetInt("broadcast_ban_duration", 5), ConfigFile.ServerConfig.GetString("broadcast_ban_text", "%nick% has been banned from this server.").Replace("%nick%", player.Nickname));
            }
            else
            {
                if (duration <= 0)
                {
                    ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>  Timing.RunCoroutine(StrikeBad(player, reason + "\n" + CedModMain.Singleton.Config.CedMod.AdditionalBanMessage)));
                }
            }
        }
        
        /// <summary>
        /// Sends a ban request to the panel using the players UserID.
        /// </summary>
        /// <param name="UserId">The players UserID.</param>
        /// <param name="duration">The duration of the ban.</param>
        /// <param name="sender">The issuer of the ban.</param>
        /// <param name="reason">The reason of the ban.</param>
        /// <param name="bc">Whether or not to broadcast the ban.</param>
        /// <see cref="Ban(Player, long, string, string, bool)"/>
        public static void BanId(string UserId, long duration, string sender, string reason, bool bc = true)
        {
            double realduration = TimeSpan.FromSeconds(duration).TotalMinutes;
            if (duration >= 1)
            {
                string json = "{\"Userid\": \"" + UserId + "\"," +
                              "\"Ip\": \"0.0.0.0\"," +
                              "\"AdminName\": \"" + sender.Replace("\"", "'") + "\"," +
                              "\"BanDuration\": "+realduration+"," +
                              "\"BanReason\": \""+reason.Replace("\"", "'")+"\"}";
                Dictionary<string, string> result = (Dictionary<string, string>) APIRequest("Auth/Ban", json, false, "POST");
                Player player = Player.Get(UserId);
                if (player != null)
                {
                    if (player != null)
                    {
                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>  Timing.RunCoroutine(StrikeBad(player, result.ContainsKey("preformattedmessage") ? result["preformattedmessage"] : $"Failed to execute api request {JsonConvert.SerializeObject(result)}")));
                    }
                    
                    if (bc)
                        Map.Broadcast((ushort) ConfigFile.ServerConfig.GetInt("broadcast_ban_duration", 5), ConfigFile.ServerConfig.GetString("broadcast_ban_text", "%nick% has been banned from this server.").Replace("%nick%", player.Nickname));
                }
            }
            else
            {
                if (duration <= 0)
                {
                    Player player = Player.Get(UserId);
                    if (player != null)
                    {
                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>  Timing.RunCoroutine(StrikeBad(player, reason + "\n" + CedModMain.Singleton.Config.CedMod.AdditionalBanMessage)));
                    }
                }
            }
        }

        /// <summary>
        /// Kills and kicks a player.
        /// </summary>
        /// <param name="player">The player to kill and kick.</param>
        /// <param name="reason">The reason for doing so.</param>
        public static IEnumerator<float> StrikeBad(Player player, string reason)
        {
            player.ReferenceHub.playerStats.KillPlayer(new DisruptorDamageHandler(player.Footprint, -1));
            yield return Timing.WaitForSeconds(0.1f);
            ServerConsole.Disconnect(player.GameObject, reason);
        }
    }
}
