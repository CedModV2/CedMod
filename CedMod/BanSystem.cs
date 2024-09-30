using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using CedMod.Addons.QuerySystem.WS;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;
using Newtonsoft.Json;
using VoiceChat;

namespace CedMod
{
    public class BanSystem
    {
        public static Dictionary<string, Dictionary<string, string>> CachedStates = new Dictionary<string, Dictionary<string, string>>();
        public static List<ReferenceHub> Authenticating { get; set; } = new List<ReferenceHub>();
        public static Dictionary<ReferenceHub, Tuple<string, string>> CedModAuthTokens = new Dictionary<ReferenceHub, Tuple<string, string>>();

        public static readonly object Banlock = new object();
        public static async Task HandleJoin(Player player, int attempt = 0)
        {
            try
            {
                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    Logger.Debug("Join");

                if (player.ReferenceHub.authManager.AuthenticationResponse.AuthToken != null && player.ReferenceHub.authManager.AuthenticationResponse.AuthToken.BypassBans || player.ReferenceHub.isLocalPlayer)
                    return;
                
                if (attempt <= 0)
                    ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => Authenticating.Add(player.ReferenceHub));
                
                ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                {
                    Timing.CallDelayed(8f, () =>
                    {
                        Authenticating.Remove(player.ReferenceHub);
                    });
                });

                Dictionary<string, string> info = new Dictionary<string, string>();
                bool req = false;
                lock (CachedStates)
                {
                    if (CachedStates.ContainsKey(player.UserId))
                        info = CachedStates[player.UserId];
                    else
                        req = true;
                }

                //Sensitive information of the authentication token concerns information that is already handled by the CedMod Api Player IP
                //Therefore we have concluded in discussion with NW's Security Advisor that it is of no issue to use the authentication token to validate API calls to ensure one cannot cause altprevention bans by performing malicious activities
                Dictionary<string, string> authToken = new Dictionary<string, string>()
                {
                    { "Type", "Auth" },
                    { "Token", player.ReferenceHub.authManager.AuthenticationResponse.SignedAuthToken.token },
                    { "Signature", player.ReferenceHub.authManager.AuthenticationResponse.SignedAuthToken.signature },
                };
                
                if (req)
                    info = (Dictionary<string, string>) await API.APIRequest($"Auth/{player.UserId}&{player.IpAddress}?banLists={string.Join(",", ServerPreferences.Prefs.BanListReadBans.Select(s => s.Id))}&banListMutes={string.Join(",", ServerPreferences.Prefs.BanListReadMutes.Select(s => s.Id))}&server={Uri.EscapeDataString(WebSocketSystem.HelloMessage == null ? "Unknown" : WebSocketSystem.HelloMessage.Identity)}&r=1", JsonConvert.SerializeObject(authToken), false, "POST");

                if (player.ReferenceHub.authManager.AuthenticationResponse.AuthToken != null && player.IpAddress != player.ReferenceHub.authManager.AuthenticationResponse.AuthToken.RequestIp && info != null && info.ContainsKey("success") && info["success"] == "true" && info["vpn"] == "false" && info["isbanned"] == "false")
                {
                    Logger.Debug("Ip address mismatch, performing request again", CedModMain.Singleton.Config.CedMod.ShowDebug);
                    var info2 = (Dictionary<string, string>) await API.APIRequest($"Auth/{player.UserId}&{player.ReferenceHub.authManager.AuthenticationResponse.AuthToken.RequestIp}?banLists={string.Join(",", ServerPreferences.Prefs.BanListReadBans.Select(s => s.Id))}&banListMutes={string.Join(",", ServerPreferences.Prefs.BanListReadMutes.Select(s => s.Id))}&server={Uri.EscapeDataString(WebSocketSystem.HelloMessage == null ? "Unknown" : WebSocketSystem.HelloMessage.Identity)}&r=2", JsonConvert.SerializeObject(authToken), false, "POST");
                    if (info2 != null && info2.ContainsKey("success") && info2["success"] == "true" && (info2["vpn"] == "true" || info2["isbanned"] == "true"))
                        info = info2;
                }

                if (info != null)
                {
                    if (info.ContainsKey("token") && info.ContainsKey("signature"))
                    {
                        CedModAuthTokens[player.ReferenceHub] = new Tuple<string, string>(info["token"], info["signature"]);
                    }
                }

                if (info == null)
                {
                    if (File.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "Internal", $"tempb-{player.UserId}")))
                    {
                        info = new Dictionary<string, string>()
                        {
                            {"success", "true"},
                            {"vpn", "false"},
                            {"isbanned", "true"},
                            {"preformattedmessage", "You are banned from this server, please check back later to see the ban reason."},
                            {"iserror", "false"}
                        };
                    }
                    else if (File.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "Internal", $"tempd-{player.UserId}")))
                    {
                        info = JsonConvert.DeserializeObject<Dictionary<string, string>>(await File.ReadAllTextAsync(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "Internal", $"tempd-{player.UserId}")));
                        File.SetLastWriteTimeUtc(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "Internal", $"tempd-{player.UserId}"), DateTime.UtcNow);
                    }
                    else
                    {
                        info = new Dictionary<string, string>()
                        {
                            {"success", "true"},
                            {"vpn", "false"},
                            {"isbanned", "false"},
                            {"iserror", "false"}
                        };
                    }

                    if (File.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "Internal", $"tempm-{player.UserId}")) && !File.Exists(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "Internal", $"tempum-{player.UserId}")))
                    {
                        info.Add("mute", "Global");
                        info.Add("mutereason", "Temporarily unavailable");
                        info.Add("muteduration", "Until revoked");
                    }
                }
                else
                {
                    if (info["isbanned"] == "true")
                    {
                        await File.WriteAllTextAsync(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "Internal", $"tempd-{player.UserId}"), JsonConvert.SerializeObject(info));
                    }
                    else
                    {
                        File.Delete(Path.Combine(CedModMain.PluginConfigFolder, "CedMod", "Internal", $"tempd-{player.UserId}"));
                    }
                }

                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    Logger.Debug(JsonConvert.SerializeObject(info));
                
                string reason;
                if (info["success"] == "true" && info["vpn"] == "true" && info["isbanned"] == "false")
                {
                    reason = info["reason"];
                    Logger.Info($"user: {player.UserId} attempted connection with blocked ASN/IP/VPN/Hosting service");
                    ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => Timing.RunCoroutine(API.NormalDisconnect(player, reason)));
                }
                else
                {
                    if (info["success"] == "true" && info["vpn"] == "false" && info["isbanned"] == "true")
                    {
                        reason = info["preformattedmessage"];
                        Logger.Info($"user: {player.UserId} attempted connection with active ban disconnecting");
                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => Timing.RunCoroutine(API.NormalDisconnect(player, reason + "\n" + CedModMain.Singleton.Config.CedMod.AdditionalBanMessage)));
                    }
                    else
                    {
                        if (info["success"] == "true" && info["vpn"] == "false" && info["isbanned"] == "false" &&
                            info["iserror"] == "true")
                        {
                            Logger.Info($"Message from CedMod server: {info["error"]}");
                        }
                    }
                }

                if (info["success"] == "true" && info.ContainsKey("mute") && info.ContainsKey("mutereason") && info.ContainsKey("muteduration"))
                {
                    Logger.Info($"user: {player.UserId} joined while muted, issuing mute...");
                    Enum.TryParse(info["mute"], out MuteType muteType);
                    // if (muteType == MuteType.Global)
                    //     player.Mute(true);
                    //
                    // if (muteType == MuteType.Intercom)
                    //     player.IntercomMute(true);

                    ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                    {
                        player.SendConsoleMessage(CedModMain.Singleton.Config.CedMod.MuteMessage.Replace("{type}", muteType.ToString()).Replace("{duration}", info["muteduration"]).Replace("{reason}", info["mutereason"]), "red");
                        Broadcast.Singleton.TargetAddElement(player.Connection, CedModMain.Singleton.Config.CedMod.MuteMessage.Replace("{type}", muteType.ToString()).Replace("{duration}", info["muteduration"]).Replace("{reason}", info["mutereason"]), 5, Broadcast.BroadcastFlags.Normal);
                        
                        Timing.CallDelayed(0.1f, () =>
                        {
                            if (muteType == MuteType.Global)
                            {
                                VoiceChatMutes.SetFlags(player.ReferenceHub, VcMuteFlags.LocalRegular);
                            }

                            if (muteType == MuteType.Intercom)
                            {
                                VoiceChatMutes.SetFlags(player.ReferenceHub, VcMuteFlags.LocalIntercom);
                            }
                        });
                    });

                    if (!string.IsNullOrEmpty(CedModMain.Singleton.Config.CedMod.MuteCustomInfo))
                    {
                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => player.CustomInfo = CedModMain.Singleton.Config.CedMod.MuteCustomInfo.Replace("{type}", muteType.ToString()));
                    }
                }
                
                ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                {
                    player.ReceiveHint("", 1); //clear authenticator hint
                    Authenticating.Remove(player.ReferenceHub);
                });
            }
            catch (Exception ex)
            {
                ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                {
                    Authenticating.Remove(player.ReferenceHub);
                    player.ReceiveHint("", 1); //clear authenticator hint
                });
                Logger.Error(ex.ToString());

                if (attempt <= 4) //we will retry 5 times
                {
                    await Task.Delay(100);
                    await HandleJoin(player, attempt + 1);
                }
            }
        }
    }
    
    public enum MuteType
    {
        Intercom,
        Global
    }
}
