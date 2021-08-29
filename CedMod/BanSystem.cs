using System;
using System.Collections.Generic;
using Exiled.Events.EventArgs;
using Newtonsoft.Json;
using Log = Exiled.API.Features.Log;

namespace CedMod
{
    public class BanSystem
    {
        public static object banlock = new object();
        public static void HandleJoin(VerifiedEventArgs ev)
        {
            Log.Debug("Join", CedModMain.config.ShowDebug);
            try
            {
                ReferenceHub Player = ev.Player.ReferenceHub;
                if (ev.Player.ReferenceHub.serverRoles.BypassStaff || Player.characterClassManager.isLocalPlayer)
                    return;

                Dictionary<string, string> info = (Dictionary<string, string>) API.APIRequest("Auth/", $"{Player.characterClassManager.UserId}&{ev.Player.IPAddress}");
                
                Log.Debug(JsonConvert.SerializeObject(info), CedModMain.config.ShowDebug);
                
                string reason;
                if (info["success"] == "true" && info["vpn"] == "true" && info["isbanned"] == "false")
                {
                    reason = info["reason"];
                    Log.Info($"user: {Player.characterClassManager.UserId} attempted connection with blocked ASN/IP/VPN/Hosting service");
                    ev.Player.Disconnect(reason);
                }
                else
                {
                    if (info["success"] == "true" && info["vpn"] == "false" && info["isbanned"] == "true")
                    {
                        reason = info["preformattedmessage"];
                        Log.Info($"user: {Player.characterClassManager.UserId} attempted connection with active ban disconnecting");
                        ev.Player.Disconnect(reason + "\n" + CedModMain.config.AdditionalBanMessage);
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
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}