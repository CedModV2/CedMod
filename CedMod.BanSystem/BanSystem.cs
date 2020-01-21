using Smod2.API;
using Smod2.EventHandlers;
using Smod2.Events;
using System.Collections.Generic;
using RemoteAdmin;
using System;
using System.Linq;
using MEC;
using System.Net;
using GameCore;
using UnityEngine;
using Smod2;
using Smod2.Commands;

namespace CedMod
{
    class BanSystem : IEventHandlerPlayerJoin, IEventHandlerBan
    {
        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            string text2;
            using (WebClient webClient = new WebClient())
            {
                webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                text2 = webClient.DownloadString("http://83.82.126.185/scpserverbans/scpplugin/check.php?id=" + ev.Player.UserId + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none"));
            }
            Initializer.logger.Debug("BANSYSTEM", "Checking ban status of user: " + ev.Player.UserId + " Response from API: " + text2);
            if (text2 == "1")
            {
                using (WebClient webClient2 = new WebClient())
                {
                    webClient2.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                    webClient2.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    string str = webClient2.DownloadString(string.Concat(new string[]
                    {
                    "http://83.82.126.185/scpserverbans/scpplugin/unban.php?id=",
                    ev.Player.UserId,
                    "&reason=Expired&aname=Server&webhook=",
                    GameCore.ConfigFile.ServerConfig.GetString("bansystem_webhook", "none"),
                    "&alias=",
                    GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none")
                    }));
                    Initializer.logger.Info("BANSYSTEM", "user: " + ev.Player.UserId + " Ban expired attempting unban, Response from API: " + str);
                }
            }
            string text3 = "0";
            bool flag = true;
            bool flag2 = false;
            try
            {
                using (WebClient webClient3 = new WebClient())
                {
                    webClient3.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", ""), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", ""));
                    webClient3.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    text3 = webClient3.DownloadString("http://83.82.126.185/scpserverbans/scpplugin/reason_request.php?id=" + ev.Player.UserId + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none"));
                    Initializer.logger.Debug("BANSYSTEM", "Checking ban status of user: " + ev.Player.UserId + " Response from API: " + text3);
                    if (text3 == "0")
                    {
                        flag = false;
                        flag2 = false;
                        Initializer.logger.Debug("BANSYSTEM", "User is not banned");
                    }
                }
            }
            catch (WebException)
            {
                flag = false;
            }
            if (flag)
            {
                Initializer.logger.Debug("BANSYSTEM", "Data found for " + ev.Player.UserId + "setting banned value to true");
                flag2 = true;
            }
            if (flag2)
            {
                Initializer.logger.Info("BANSYSTEM", "user: " + ev.Player.UserId + " attempted connection with active ban disconnecting");
                ev.Player.Disconnect(" " + text3 + " You can fill in a ban appeal here: " + GameCore.ConfigFile.ServerConfig.GetString("bansystem_banappealurl", "none"));
            }

        }
        public void OnBan(BanEvent ev)
        {
            bool kek = true;
            ev.AllowBan = false;
            if (ev.Reason.Contains("&"))
            {
                ev.Admin.PersonalBroadcast(10, "The ban reason must not contain a & or else shit will hit the fan", false);
                kek = false;
                return;
            }
            if (ev.Reason == "")
            {
                ev.Admin.PersonalBroadcast(10, "You must specify a reason", false);
                kek = false;
                return;
            }
            if (kek && ev.Duration >= 1)
            {
                string text;
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                        webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                        text = webClient.DownloadString(string.Concat(new object[]
                        {
                                "http://83.82.126.185/scpserverbans/scpplugin/ban.php?id=",
                                ev.Player.UserId,
                                "&reason=",
                                ev.Reason,
                                "&aname=",
                                ev.Admin.Name,
                                "&bd=",
                                ev.Duration,
                                "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none") + "&webhook=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_webhook", "none")
                        }));
                        Initializer.logger.Info("BANSYSTEM", string.Concat(new object[]
                        {
                                "User: ",
                                ev.Player.UserId,
                                " has been banned by: ",
                                ev.Admin.Name,
                                " for the reason: ",
                                ev.Reason,
                                " duration: ",
                                ev.Duration
                        }));
                        Initializer.logger.Debug("BANSYSTEM", "Response from ban API: " + text);
                    }
                }
                catch (WebException ex)
                {
                    HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                    Initializer.logger.Error("BANSYSTEM", string.Concat(new object[]
                    {
                            "An error occured: ",
                            ex.Message,
                            " ",
                            ex.Status
                    }));
                }
            }
            else
            {
                if (ev.Duration <= 0)
                {
                    ev.Player.Disconnect(ev.Reason);
                }
            }
        }
    }
    public class SUnbanCommand : ICommandHandler
    {
        private Plugin plugin;

        public SUnbanCommand(Plugin plugin) => this.plugin = plugin;

        public string GetUsage() => "SUNBAN [SteamID] [REASON separate the words with - eg ban-appealed]";

        public string GetCommandDescription() => "Server bans users that are offline";

        public string[] OnCall(ICommandSender sender, string[] args)
        {
            if (sender is Player player)
            {
                try
                {
                    if (args.Length < 2) return new string[] { GetUsage() };
                    string IssuingPlayer = player.Name;
                    string input = args[0];
                    string reason = (args.Length >= 1) ? args[1] : "";
                    if (args.Length == 3)
                    {

                        using (WebClient webClient = new WebClient())
                        {
                            webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                            webClient.DownloadString(string.Concat(new string[]
                            {
                            "http://83.82.126.185//scpserverbans/scpplugin/unban.php?id=",
                            input,
                            "&reason=",
                            reason,
                            "&aname=",
                            IssuingPlayer,
                            "&webhook=",
                            GameCore.ConfigFile.ServerConfig.GetString("bansystem_webhook", "none"),
                            "&alias=",
                            GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none")
                            }));
                        }
                        global::ServerConsole.AddLog("User " + input + " Unbanned by RA user " + IssuingPlayer);
                        string response = "\n" +
                            "SteamID64: " + input.Trim() + "\n" +
                            "By: " + IssuingPlayer + "\n" +
                            "with the reason: " + reason;

                        if (IssuingPlayer != "Server")
                            plugin.Info(response);
                        return new string[] { response };
                    }
                    else
                        return new string[] { "SteamID not in correct format!", GetUsage() };
                }
                catch (Exception e)
                {
                    plugin.Error(e.StackTrace);
                    return new string[] { e.StackTrace };
                }
            }
            else
            {
                return new string[] { "kek" };
            }
        }
    }
}
