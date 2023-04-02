using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MEC;
using Newtonsoft.Json;
using PluginAPI.Core;
using VoiceChat;
using VoiceChat.Networking;

namespace CedMod
{
    public class BanSystem
    {
        public static Dictionary<string, Dictionary<string, string>> CachedStates = new Dictionary<string, Dictionary<string, string>>();

        public static readonly object Banlock = new object();
        public static async Task HandleJoin(CedModPlayer player)
        {
            if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                Log.Debug("Join");
            try
            {
                if (player.ReferenceHub.serverRoles.BypassStaff || player.ReferenceHub.isLocalPlayer)
                    return;

                Dictionary<string, string> info = new Dictionary<string, string>();
                bool req = false;
                lock (CachedStates)
                {
                    if (CachedStates.ContainsKey(player.UserId))
                        info = CachedStates[player.UserId];
                    else
                        req = true;
                }
                
                if (req)
                    info = (Dictionary<string, string>) await API.APIRequest("Auth/", $"{player.UserId}&{player.IpAddress}");

                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    Log.Debug(JsonConvert.SerializeObject(info));
                
                string reason;
                if (info["success"] == "true" && info["vpn"] == "true" && info["isbanned"] == "false")
                {
                    reason = info["reason"];
                    Log.Info($"user: {player.UserId} attempted connection with blocked ASN/IP/VPN/Hosting service");
                    int count = 5;
                    while (count <= 0)
                    {
                        await Task.Delay(100);
                        count--;
                        try
                        {
                            player.Disconnect(reason);
                        }
                        catch (Exception e)
                        {
                        }
                            
                        break;
                    }
                }
                else
                {
                    if (info["success"] == "true" && info["vpn"] == "false" && info["isbanned"] == "true")
                    {
                        reason = info["preformattedmessage"];
                        Log.Info($"user: {player.UserId} attempted connection with active ban disconnecting");
                        int count = 5;
                        while (count <= 0)
                        {
                            await Task.Delay(100);
                            count--;
                            try
                            {
                                player.Disconnect(reason + "\n" + CedModMain.Singleton.Config.CedMod.AdditionalBanMessage);
                            }
                            catch (Exception e)
                            {
                            }
                            
                            break;
                        }
                    }
                    else
                    {
                        if (info["success"] == "true" && info["vpn"] == "false" && info["isbanned"] == "false" &&
                            info["iserror"] == "true")
                        {
                            Log.Info($"Message from CedMod server: {info["error"]}");
                        }
                    }
                }

                if (info["success"] == "true" && info.ContainsKey("mute") && info.ContainsKey("mutereason") && info.ContainsKey("muteduration"))
                {
                    Log.Info($"user: {player.UserId} joined while muted, issuing mute...");
                    Enum.TryParse(info["mute"], out MuteType muteType);
                    player.SendConsoleMessage(CedModMain.Singleton.Config.CedMod.MuteMessage.Replace("{type}", muteType.ToString()).Replace("{duration}", info["muteduration"]).Replace("{reason}", info["mutereason"]), "red");
                    Broadcast.Singleton.TargetAddElement(player.Connection, CedModMain.Singleton.Config.CedMod.MuteMessage.Replace("{type}", muteType.ToString()).Replace("{duration}", info["muteduration"]).Replace("{reason}", info["mutereason"]), 5, Broadcast.BroadcastFlags.Normal);
                    // if (muteType == MuteType.Global)
                    //     player.Mute(true);
                    //
                    // if (muteType == MuteType.Intercom)
                    //     player.IntercomMute(true);

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

                    player.CustomInfo = CedModMain.Singleton.Config.CedMod.MuteCustomInfo.Replace("{type}", muteType.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
    
    public enum MuteType
    {
        Intercom,
        Global
    }
}