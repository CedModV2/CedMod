using System;
using System.Collections.Generic;
using System.Linq;
using EXILED;
using Grenades;
using System.Net;
using MEC;
using RemoteAdmin;
using UnityEngine;
using CedMod.INIT;

namespace CedMod
{
    public class BanSystem
    {
        public Plugin plugin;
        public BanSystem(Plugin plugin) => this.plugin = plugin;
        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            if (!ev.Player.characterClassManager.isLocalPlayer)
            {
                string text2;
                using (WebClient webClient = new WebClient())
                {
                    webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                    webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    text2 = webClient.DownloadString("http://83.82.126.185/scpserverbans/scpplugin/check.php?id=" + ev.Player.GetComponent<CharacterClassManager>().UserId + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none"));
                }
                Initializer.logger.Debug("BANSYSTEM", "Checking ban status of user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " Response from API: " + text2);
                if (text2 == "1")
                {
                    using (WebClient webClient2 = new WebClient())
                    {
                        webClient2.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                        webClient2.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                        string str = webClient2.DownloadString(string.Concat(new string[]
                        {
                    "http://83.82.126.185//scpserverbans/scpplugin/unban.php?id=",
                    ev.Player.GetComponent<CharacterClassManager>().UserId,
                    "&reason=Expired&aname=Server&webhook=",
                    GameCore.ConfigFile.ServerConfig.GetString("bansystem_webhook", "none"),
                    "&alias=",
                    GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none")
                        }));
                        Initializer.logger.Info("BANSYSTEM", "user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " Ban expired attempting unban, Response from API: " + str);
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
                        text3 = webClient3.DownloadString("http://83.82.126.185/scpserverbans/scpplugin/reason_request.php?id=" + ev.Player.GetComponent<CharacterClassManager>().UserId + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none"));
                        Initializer.logger.Debug("BANSYSTEM", "Checking ban status of user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " Response from API: " + text3);
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
                    Initializer.logger.Debug("BANSYSTEM", "Data found for " + ev.Player.GetComponent<CharacterClassManager>().UserId + "setting banned value to true");
                    flag2 = true;
                }
                if (flag2)
                {
                    Initializer.logger.Info("BANSYSTEM", "user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " attempted connection with active ban disconnecting");
                    ServerConsole.Disconnect(ev.Player.GetComponent<CharacterClassManager>().gameObject, " " + text3 + " You can fill in a ban appeal here: " + GameCore.ConfigFile.ServerConfig.GetString("bansystem_banappealurl", "none"));
                }
            }
        }
        public void OnCommand(ref RACommandEvent ev)
        {
            string[] Command = ev.Command.Split(new char[]
            {
                ' '
            });
            if (Command[0].ToUpper() == "BAN")
            {
                ev.Allow = false;
                if (Command.Length < 3)
                {
                    ev.Sender.RaReply(Command[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, "");
                    return;
                }
                string text17 = string.Empty;
                if (Command.Length > 3)
                {
                    text17 = Command.Skip(3).Aggregate((string current, string n) => current + " " + n);
                }
                if (text17.Contains("&"))
                {
                    ev.Sender.RaReply("The ban reason must not contain a & or else shit will hit the fan", false, false, "");
                    return;
                }
                if (text17 == "")
                {
                    ev.Sender.RaReply(string.Concat(new string[]
                    {
                        Command[0].ToUpper(),
                        "#To run this program, you must specify a reason use the text based RA console to do so, Autocorrection:   ban ",
                        Command[1],
                        " ",
                        Command[2],
                        " ReasonHere"
                    }), false, true, "");
                    return;
                }
                ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(new string[]
                {
                      ev.Sender.Nickname,
                      " ran the ban command (duration: ",
                      Command[2],
                      " min) on ",
                      Command[1],
                      " players. Reason: ",
                      (text17 == string.Empty) ? "(none)" : text17,
                      "."
                }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                foreach (GameObject gameObject in PlayerManager.players)
                {
                    if (Convert.ToInt16(Command[1]) == gameObject.GetComponent<QueryProcessor>().PlayerId)
                    {
                        if (!gameObject.GetComponent<ServerRoles>().BypassStaff)
                        {
                            if (Convert.ToInt16(Command[2]) >= 1)
                            {
                                try
                                {
                                    string text;
                                    using (WebClient webClient = new WebClient())
                                    {
                                        webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                                        webClient.Headers.Add("user-agent", "Cedmod Client build: " + CedMod.INIT.Initializer.GetCedModVersion());
                                        text = webClient.DownloadString(string.Concat(new object[]
                                        {
                                            "http://83.82.126.185/scpserverbans/scpplugin/ban.php?id=",
                                            gameObject.GetComponent<CharacterClassManager>().UserId,
                                            "&reason=",
                                            text17,
                                            "&aname=",
                                            ev.Sender.Nickname,
                                            "&bd=",
                                            Convert.ToInt16(Command[2]),
                                            "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none") + "&webhook=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_webhook", "none")
                                        }));
                                        Plugin.Info(string.Concat(new object[]
                                        {
                                            "User: ",
                                            gameObject.GetComponent<CharacterClassManager>().UserId,
                                            " has been banned by: ",
                                            ev.Sender.Nickname,
                                            " for the reason: ",
                                            text17,
                                            " duration: ",
                                            Convert.ToInt16(Command[2])
                                        }));
                                        Plugin.Info("BANSYSTEM: Response from ban API: " + text);
                                        ServerConsole.Disconnect(gameObject, text);
                                    }
                                }
                                catch (WebException ex)
                                {
                                    HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
                                    Plugin.Error("BANSYSTEM: " + string.Concat(new object[]
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
                                if (!string.IsNullOrEmpty(text17) && Convert.ToInt16(Command[2]) <= 0)
                                {
                                    string text3;
                                    text3 = " Reason: " + text17;
                                    ServerConsole.Disconnect(gameObject, text3);
                                }
                            }
                        }
                    }
                }

            }
            if (Command[0].ToUpper() == "UNBAN")
            {
                ev.Allow = false;
                ev.Sender.RaReply(Command[0] + "#Use the Webinterface for unbanning", false, true, "");
                return;
            }
        }
        private static bool CheckPermissions(CommandSender sender, string queryZero, PlayerPermissions perm, string replyScreen = "", bool reply = true)
        {
            if (ServerStatic.IsDedicated && sender.FullPermissions)
            {
                return true;
            }
            if (PermissionsHandler.IsPermitted(sender.Permissions, perm))
            {
                return true;
            }
            if (reply)
            {
                sender.RaReply(queryZero + "#You don't have permissions to execute this command.\nMissing permission: " + perm, false, true, replyScreen);
            }
            return false;
        }
    }
}