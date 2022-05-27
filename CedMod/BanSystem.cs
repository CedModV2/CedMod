using System;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Dissonance;
using Exiled.Events.EventArgs;
using Newtonsoft.Json;
using Log = Exiled.API.Features.Log;

namespace CedMod
{
    public class BanSystem
    {
        public static readonly object Banlock = new object();
        public static void HandleJoin(VerifiedEventArgs ev)
        {
            Log.Debug("Join", CedModMain.Singleton.Config.CedMod.ShowDebug);
            try
            {
                ReferenceHub player = ev.Player.ReferenceHub;
                if (player.serverRoles.BypassStaff || player.characterClassManager.isLocalPlayer)
                    return;

                Dictionary<string, string> info = (Dictionary<string, string>) API.APIRequest("Auth/", $"{player.characterClassManager.UserId}&{ev.Player.IPAddress}");
                
                Log.Debug(JsonConvert.SerializeObject(info), CedModMain.Singleton.Config.CedMod.ShowDebug);
                
                string reason;
                if (info["success"] == "true" && info["vpn"] == "true" && info["isbanned"] == "false")
                {
                    reason = info["reason"];
                    Log.Info($"user: {player.characterClassManager.UserId} attempted connection with blocked ASN/IP/VPN/Hosting service");
                    ev.Player.Disconnect(reason);
                }
                else
                {
                    if (info["success"] == "true" && info["vpn"] == "false" && info["isbanned"] == "true")
                    {
                        reason = info["preformattedmessage"];
                        Log.Info($"user: {player.characterClassManager.UserId} attempted connection with active ban disconnecting");
                        ev.Player.Disconnect(reason + "\n" + CedModMain.Singleton.Config.CedMod.AdditionalBanMessage);
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
                    Log.Info($"user: {player.characterClassManager.UserId} joined while muted, issuing mute...");
                    Enum.TryParse(info["mute"], out MuteType muteType);
                    ev.Player.SendConsoleMessage(CedModMain.Singleton.Config.CedMod.MuteMessage.Replace("{type}", muteType.ToString()).Replace("{duration}", info["muteduration"]).Replace("{reason}", info["mutereason"]), "red");
                    ev.Player.Broadcast(5, CedModMain.Singleton.Config.CedMod.MuteMessage.Replace("{type}", muteType.ToString()).Replace("{duration}", info["muteduration"]).Replace("{reason}", info["mutereason"]), Broadcast.BroadcastFlags.Normal);
                    ev.Player.IsMuted = muteType == MuteType.Global;
                    ev.Player.IsIntercomMuted = muteType == MuteType.Intercom;
                    ev.Player.CustomInfo = CedModMain.Singleton.Config.CedMod.MuteCustomInfo.Replace("{type}", muteType.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
    
    public enum MuteType
    {
        Intercom,
        Global
    }
}