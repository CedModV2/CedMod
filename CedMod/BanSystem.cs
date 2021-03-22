using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CedMod.Commands;
using CedMod.INIT;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Permissions.Extensions;
using GameCore;
using MEC;
using Mirror;
using Newtonsoft.Json;
using RemoteAdmin;
using UnityEngine;
using Console = System.Console;

namespace CedMod
{
    public class BanSystem
    {
        public static object banlock = new object();
        public static void HandleJoin(VerifiedEventArgs ev)
        {
            try
            {
                ReferenceHub Player = ev.Player.ReferenceHub;
                if (ev.Player.ReferenceHub.serverRoles.BypassStaff)
                    return;
                if (Player.characterClassManager.isLocalPlayer)
                    return;

                Dictionary<string, string> info = (Dictionary<string, string>) API.APIRequest("Auth/",
                    $"{Player.characterClassManager.UserId}&{ev.Player.IPAddress}");
                string reason;
                if (info["success"] == "true" && info["vpn"] == "true" && info["isbanned"] == "false")
                {
                    reason = info["reason"];
                    Player.characterClassManager.TargetConsolePrint(Player.characterClassManager.connectionToClient,
                        "CedMod.BANSYSTEM Message from CedMod server (VPN/Proxy detected): " + info,
                        "yellow");
                    Initializer.Logger.Info("BANSYSTEM",
                        "user: " + Player.characterClassManager.UserId +
                        " attempted connection with blocked ASN/IP/VPN/Hosting service");
                    ev.Player.Disconnect(reason);
                }
                else
                {
                    if (info["success"] == "true" && info["vpn"] == "false" && info["isbanned"] == "true")
                    {
                        reason = info["preformattedmessage"] +
                                 " You can fill in a ban appeal here: " +
                                 ConfigFile.ServerConfig.GetString("bansystem_banappealurl", "none");
                        Initializer.Logger.Info("BANSYSTEM",
                            "user: " + Player.characterClassManager.UserId +
                            " attempted connection with active ban disconnecting");
                        Player.characterClassManager.TargetConsolePrint(
                            Player.characterClassManager.connectionToClient,
                            "CedMod.BANSYSTEM Active ban: " + info["preformattedmessage"],
                            "yellow");
                        ev.Player.Disconnect(reason);
                    }
                    else
                    {
                        if (info["success"] == "true" && info["vpn"] == "false" && info["isbanned"] == "false" &&
                            info["iserror"] == "true")
                        {
                            Player.characterClassManager.TargetConsolePrint(
                                Player.characterClassManager.connectionToClient,
                                "CedMod.BANSYSTEM Message from CedMod server: " + info["error"],
                                "yellow");
                            Initializer.Logger.Info("BANSYSTEM",
                                "Message from CedMod server: " + info["error"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Initializer.Logger.LogException(ex, "CedMod", "BanSystem.HandleJoin");
            }
        }

        public static void HandleRACommand(SendingRemoteAdminCommandEventArgs ev)
        {
            ReferenceHub sender = ev.Sender.Nickname == "SERVER CONSOLE" || ev.Sender.Nickname == "GAME CONSOLE"
                ? ReferenceHub.LocalHub
                : ReferenceHub.GetHub(ev.Sender.Id);
            if (ev.Name.ToUpper() == "BAN")
            {
                ev.IsAllowed = false;
                var num4 = Convert.ToInt64(ev.Arguments[1]);
                if ((num4 == 0 && !CommandProcessor.CheckPermissions(ev.CommandSender, "BAN", new PlayerPermissions[3]
                {
                    PlayerPermissions.KickingAndShortTermBanning,
                    PlayerPermissions.BanningUpToDay,
                    PlayerPermissions.LongTermBanning
                })) || (num4 > 0 && num4 <= 3600 && !CommandProcessor.CheckPermissions(ev.CommandSender, "BAN", PlayerPermissions.KickingAndShortTermBanning)) || (num4 > 3600 && num4 <= 86400 && !CommandProcessor.CheckPermissions(ev.CommandSender, "BAN", PlayerPermissions.BanningUpToDay)) || (num4 > 86400 && !CommandProcessor.CheckPermissions(ev.CommandSender, "BAN", PlayerPermissions.LongTermBanning)))
                {
                    ev.CommandSender.Respond("No permission", false);
                    return;
                }
                if (ev.Arguments.Count < 2)
                {
                    ev.CommandSender.Respond(
                        "To run this program, type at least 3 arguments! (some parameters are missing)", false);
                    return;
                }

                string text17 = string.Empty;
                if (ev.Arguments.Count > 2)
                {
                    text17 = ev.Arguments.Skip(2).Aggregate((current, n) => current + " " + n);
                }

                if (text17.Contains("&"))
                {
                    ev.CommandSender.Respond("The ban reason must not contain a & or else shit will hit the fan",
                        false);
                    return;
                }

                if (text17 == "")
                {
                    ev.CommandSender.Respond(
                        $"To run this program, you must specify a reason use the text based RA console to do so, Autocorrection:   ban  {ev.Arguments[0]} {ev.Arguments[1]} ReasonHere",
                        false);
                    return;
                }

                ServerLogs.AddLog(ServerLogs.Modules.Administrative,
                    string.Concat(ev.Sender.Nickname, " ran the ban command (duration: ", ev.Arguments[1], " min) on ",
                        ev.Arguments[0], " players. Reason: ", (text17 == string.Empty) ? "(none)" : text17, "."),
                    ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                List<int> list = new List<int>();
                string[] source = ev.Arguments[0].Split('.');
                list.AddRange((from item in source
                    where !string.IsNullOrEmpty(item)
                    select item).Select(int.Parse));
                foreach (int num2 in list)
                {
                    foreach (Player player in Player.List)
                    {
                        if (num2 == player.ReferenceHub.queryProcessor.PlayerId)
                        {
                            if (!player.ReferenceHub.serverRoles.BypassStaff)
                            {
                                if (Convert.ToInt64(ev.Arguments[1]) >= 1)
                                {
                                    string sender1 = ev.Sender.Nickname;

                                    Task.Factory.StartNew(() =>
                                    {
                                        lock (banlock) //so theres only 1 ban at a time
                                        {
                                            API.Ban(player, Convert.ToInt64(ev.Arguments[1]), sender1, text17);
                                        }
                                    });
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(text17) && Convert.ToInt32(ev.Arguments[1]) <= 0)
                                    {
                                        string text3;
                                        text3 = " Reason: " + text17;
                                        ServerConsole.Disconnect(player.GameObject, text3);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (ev.Name.ToUpper() == "UNBAN")
            {
                ev.IsAllowed = false;
                ev.CommandSender.Respond("Use the Webinterface for unbanning", false);
            }

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
        }
    }
}