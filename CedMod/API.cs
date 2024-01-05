using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using CedMod.Addons.QuerySystem.WS;
using Footprinting;
using GameCore;
using InventorySystem.Items.Firearms;
using MEC;
using Newtonsoft.Json;
using PlayerStatsSystem;
using PluginAPI.Core;
using Console = System.Console;
using Log = PluginAPI.Core.Log;

namespace CedMod
{
    public static class API
    {
        public static bool HasLoaded { get; set; }
        public static string DevUri = "api.dev.cedmod.nl";
        public static string APIUrl
        {
            get
            {
                return QuerySystem.IsDev ? DevUri : "api.cedmod.nl";
            }
        }

        public static async Task<object> APIRequest(string endpoint, string arguments, bool returnstring = false, string type = "GET")
        {
            if (!VerificationChallenge.CompletedChallenge)
            {
                Log.Error($"API request failed: Challenge not complete");
                return null;
            }
            string response = "";  
            try
            {
                HttpResponseMessage resp = null;
                if (type == "GET")
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("X-ServerIp", Server.ServerIpAddress);
                        await VerificationChallenge.AwaitVerification();
                        client.DefaultRequestHeaders.Add("ApiKey", CedModMain.Singleton.Config.CedMod.CedModApiKey);
                        resp = await client.GetAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + APIUrl + "/" + endpoint + arguments);
                        response = await resp.Content.ReadAsStringAsync();
                    }
                }
                
                if (type == "DELETE")
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("X-ServerIp", Server.ServerIpAddress);
                        await VerificationChallenge.AwaitVerification();
                        client.DefaultRequestHeaders.Add("ApiKey", CedModMain.Singleton.Config.CedMod.CedModApiKey);
                        resp = await client.DeleteAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + APIUrl + "/" + endpoint + arguments);
                        response = await resp.Content.ReadAsStringAsync();
                    }
                }

                if (type == "POST")
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("X-ServerIp", Server.ServerIpAddress);
                        await VerificationChallenge.AwaitVerification();
                        client.DefaultRequestHeaders.Add("ApiKey", CedModMain.Singleton.Config.CedMod.CedModApiKey);
                        resp = await client.PostAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + APIUrl + "/" + endpoint, new StringContent(arguments, Encoding.UTF8, "application/json"));
                        response = await resp.Content.ReadAsStringAsync();
                    }
                }

                if (!resp.IsSuccessStatusCode)
                {
                    if (resp.StatusCode == HttpStatusCode.PreconditionRequired)
                    {
                        VerificationChallenge.CompletedChallenge = false;
                        VerificationChallenge.ChallengeStarted = false;
                    }
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
        
        public static async Task Mute(Player player, string adminname, double duration, string reason, MuteType Type)
        {
            long realduration = (long)TimeSpan.FromSeconds(duration).TotalMinutes;
            if (realduration <= 3)
                return;
            
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                { "UserId", player.UserId },
                { "Type", (int)Type },
                { "AdminName", adminname },
                { "Mutereason", reason },
                { "Muteduration", (int)realduration }
            };
            
            Dictionary<string, string> result = (Dictionary<string, string>) await APIRequest($"api/Mute/{player.UserId}?banLists={string.Join(",", ServerPreferences.Prefs.BanListWriteMutes.Select(s => s.Id))}", JsonConvert.SerializeObject(data), false, "POST");
            if (result == null)
            {
                await File.WriteAllTextAsync(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "Internal", $"tempm-{player.UserId}"), JsonConvert.SerializeObject(data));
            }
        }
        
        public static async Task UnMute(Player player)
        {
            Dictionary<string, string> result = (Dictionary<string, string>) await APIRequest($"api/Mute/{player.UserId}?banLists={string.Join(",", ServerPreferences.Prefs.BanListWriteMutes.Select(s => s.Id))}", "", false, "DELETE");
            if (result == null)
            {
                await File.WriteAllTextAsync(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "Internal", $"tempum-{player.UserId}"), player.UserId);
            }
        }

        public static async Task Ban(CedModPlayer player, long duration, string sender, string reason, bool bc = true)
        {
            long realduration = (long)TimeSpan.FromSeconds(duration).TotalMinutes;
            if (duration >= 1)
            {
                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    { "Userid", player.UserId },
                    { "Ip", player.IpAddress },
                    { "AdminName", sender.Replace("\"", "'") },
                    { "BanDuration", (int)realduration },
                    { "BanReason", reason.Replace("\"", "'") }
                };
                
                Dictionary<string, string> result = (Dictionary<string, string>) await APIRequest($"Auth/Ban?banLists={string.Join(",", ServerPreferences.Prefs.BanListWriteBans.Select(s => s.Id))}", JsonConvert.SerializeObject(data), false, "POST");
                if (result == null)
                {
                    await File.WriteAllTextAsync(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "Internal", $"tempb-{player.UserId}"), JsonConvert.SerializeObject(data));
                    result = new Dictionary<string, string>()
                    {
                        { "preformattedmessage", $"You have been banned from this server: {reason}" }
                    };
                }
                WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
                {
                    Recipient = "PANEL",
                    Data = new Dictionary<string, string>()
                    {
                        { "Message", "BANISSUED" },
                        { "UserId", player.UserId }
                    }
                });
                ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>  Timing.RunCoroutine(StrikeBad(player, result.ContainsKey("preformattedmessage") ? result["preformattedmessage"] : $"Failed to execute api request {JsonConvert.SerializeObject(result)}")));
                if (bc)
                    Server.SendBroadcast(ConfigFile.ServerConfig.GetString("broadcast_ban_text", "%nick% has been banned from this server.").Replace("%nick%", player.Nickname), (ushort) ConfigFile.ServerConfig.GetInt("broadcast_ban_duration", 5), Broadcast.BroadcastFlags.Normal);
            }
            else
            {
                if (duration <= 0)
                {
                    ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>  Timing.RunCoroutine(StrikeBad(player, reason + "\n" + CedModMain.Singleton.Config.CedMod.AdditionalBanMessage)));
                }
            }
        }
        
        public static async Task BanId(string UserId, long duration, string sender, string reason, bool bc = true)
        {
            double realduration = TimeSpan.FromSeconds(duration).TotalMinutes;
            if (duration >= 1)
            {
                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    { "Userid", UserId },
                    { "Ip", "0.0.0.0" },
                    { "AdminName", sender.Replace("\"", "'") },
                    { "BanDuration", (int)realduration },
                    { "BanReason", reason.Replace("\"", "'") }
                };
                
                Dictionary<string, string> result = (Dictionary<string, string>) await APIRequest($"Auth/Ban?banLists={string.Join(",", ServerPreferences.Prefs.BanListWriteBans.Select(s => s.Id))}", JsonConvert.SerializeObject(data), false, "POST");
                if (result == null)
                {
                    await File.WriteAllTextAsync(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "Internal", $"tempb-{UserId}"), JsonConvert.SerializeObject(data));
                    result = new Dictionary<string, string>()
                    {
                        { "preformattedmessage", $"You have been banned from this server: {reason}" }
                    };
                }
                WebSocketSystem.SendQueue.Enqueue(new QueryCommand()   
                {                                                      
                    Recipient = "PANEL",                               
                    Data = new Dictionary<string, string>()            
                    {                  
                        { "Message", "BANISSUED" },
                        { "UserId", UserId }                    
                    }                                                  
                });                                                    
                CedModPlayer player = CedModPlayer.Get(UserId);
                if (player != null)
                {
                    if (player != null)
                    {
                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>  Timing.RunCoroutine(StrikeBad(player, result.ContainsKey("preformattedmessage") ? result["preformattedmessage"] : $"Failed to execute api request {JsonConvert.SerializeObject(result)}")));
                    }

                    if (bc)
                        Server.SendBroadcast(ConfigFile.ServerConfig.GetString("broadcast_ban_text", "%nick% has been banned from this server.").Replace("%nick%", player.Nickname), (ushort) ConfigFile.ServerConfig.GetInt("broadcast_ban_duration", 5), Broadcast.BroadcastFlags.Normal);
                }
            }
            else
            {
                if (duration <= 0)
                {
                    CedModPlayer player = CedModPlayer.Get(UserId);
                    if (player != null)
                    {
                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>  Timing.RunCoroutine(StrikeBad(player, reason + "\n" + CedModMain.Singleton.Config.CedMod.AdditionalBanMessage)));
                    }
                }
            }
        }

        public static IEnumerator<float> StrikeBad(CedModPlayer player, string reason)
        {
            try
            {
                player.ReferenceHub.playerStats.KillPlayer(new DisruptorDamageHandler(new Footprint(player.ReferenceHub), -1));
            }
            catch (Exception e)
            {
                Log.Error($"Failed to kill for kick: {e}");
            }
            yield return Timing.WaitForSeconds(0.1f);
            int count = 5;
            while (count >= 0)
            {
                yield return Timing.WaitForSeconds(0.2f);
                count--;
                try
                {
                    player.Disconnect(reason);
                }
                catch (Exception e)
                {
                    continue;
                }
                            
                break;
            }
        }
    }
}
