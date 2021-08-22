using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CedMod.Commands;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Permissions.Extensions;
using Newtonsoft.Json;
using RemoteAdmin;
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
                if (ev.Player.ReferenceHub.serverRoles.BypassStaff)
                    return;
                if (Player.characterClassManager.isLocalPlayer)
                    return;

                Dictionary<string, string> info = (Dictionary<string, string>) API.APIRequest("Auth/",
                    $"{Player.characterClassManager.UserId}&{ev.Player.IPAddress}");
                
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

        /*public static void HandleRACommand(SendingRemoteAdminCommandEventArgs ev)
        {
            ReferenceHub sender = ev.Sender.Nickname == "SERVER CONSOLE" || ev.Sender.Nickname == "GAME CONSOLE"
                ? ReferenceHub.LocalHub
                : ReferenceHub.GetHub(ev.Sender.Id);

            if (ev.Name.ToUpper() == "JAIL")
            {
                if (!ev.Sender.CheckPermission("at.jail"))
                {
                    return;
                }

                if (ev.Arguments.Count != 1)
                {
                    return;
                }

                Player Ply = Player.Get(ev.Arguments[0]);
                if (Ply == null)
                {
                    return;
                }

                string response1 = (string) API.APIRequest($"api/BanLog/UserId/{Ply.UserId}", "", true);
                if (response1.Contains("\"message\":\"Specified BanLog does not exist\""))
                {
                    ev.Sender.RemoteAdminMessage("No banlogs found!", false);
                    return;
                }
                ApiBanResponse resp =
                    JsonConvert.DeserializeObject<ApiBanResponse>(response1);
                foreach (BanModel ban in resp.Message)
                {
                    ev.Sender.RemoteAdminMessage($"\nIssuer :{ban.Adminname}" +
                                   $"\nReason {ban.Banreason}" +
                                   $"\nDuration {ban.Banduration}" +
                                   $"\nTimestamp {ban.Timestamp}", true);
                }
            }
        }*/
    }
}