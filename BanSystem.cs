using CedMod.INIT;
using EXILED;
using EXILED.Extensions;
using Mirror;
using Newtonsoft.Json;
using RemoteAdmin;
using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace CedMod
{
    public class BanSystem
    {
        public bool LastAPIRequestSuccessfull = false;
        public Plugin plugin;
        public BanSystem(Plugin plugin) => this.plugin = plugin;
        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            if (!ev.Player.gameObject.GetComponent<ServerRoles>().BypassStaff)
            {
                if (!ev.Player.characterClassManager.isLocalPlayer)
                {
                    foreach (string b in GameCore.ConfigFile.ServerConfig.GetStringList("cm_nicknamefilter"))
                    {
                        if (ev.Player.nicknameSync.MyNick.Contains(b))
                        {
                            ev.Player.nicknameSync.MyNick = ev.Player.nicknameSync.MyNick.Replace(b, "BOBBA(filtered word)");
                            ev.Player.nicknameSync.Network_myNickSync = ev.Player.nicknameSync.Network_myNickSync.Replace(b, "BOBBA(filtered word)");
                        }
                    }
                    string Bancheck = CheckBanExpired(ev.Player);
                    Initializer.logger.Debug("BANSYSTEM", "Checking ban status of user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " Response from API: " + Bancheck);
                    if (Bancheck == "1")
                    {
                        Initializer.logger.Info("BANSYSTEM", "user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " Ban expired attempting unban");
                        Unban(ev.Player);
                        foreach (GameObject o in PlayerManager.players)
                        {
                            ReferenceHub rh = o.GetComponent<ReferenceHub>();
                            if (rh.serverRoles.RemoteAdmin)
                            { 
                                rh.Broadcast(10u, "WARNING Player: " + ev.Player.nicknameSync.MyNick + " " + ev.Player.characterClassManager.UserId + " has been recently been unbanned due to ban expiery", false);
                            }
                        }
                    }
                    string BanReason = "0";
                    BanReason = GetBandetails(ev.Player);
                    if (BanReason != "0" && LastAPIRequestSuccessfull)
                    {
                        Initializer.logger.Info("BANSYSTEM", "user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " attempted connection with active ban disconnecting");
                        string Reason = "No reason specified please contact ced777ric#0001 on the discord of this server. This error should not be possible";
                        if (BanReason.StartsWith("[CEDMOD.Bansystem.Message]"))
                        {
                            Reason = BanReason;
                        }
                        else
                        {
                            if (BanReason.StartsWith("[CEDMOD.Bansystem.BanHandler]"))
                            {
                                Reason = BanReason + " You can fill in a ban appeal here: " + GameCore.ConfigFile.ServerConfig.GetString("bansystem_banappealurl", "none");
                            }
                            else
                            {
                                Initializer.logger.Error("BANSYSTEM", "Wait a second this message shouldn't even be possible. Something went terribly wrong");
                            }
                        }
                        ServerConsole.Disconnect(ev.Player.GetComponent<CharacterClassManager>().gameObject, Reason);
                    }
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
                                Ban(gameObject, Convert.ToInt16(Command[2]), ev.Sender.Nickname, text17);
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
            if (Command[0].ToUpper() == "PRIORBANS")
            {
                ev.Allow = false;
                if (Command.Length < 1)
                {
                    ev.Sender.RaReply(Command[0].ToUpper() + "#To run this program, type at least 1 arguments! (some parameters are missing)", false, true, "");
                    return;
                }
                foreach (GameObject gameObject in PlayerManager.players)
                {
                    if (Convert.ToInt16(Command[1]) == gameObject.GetComponent<QueryProcessor>().PlayerId)
                    {
                        ev.Sender.RaReply(Command[0].ToUpper() + "#" + GetPriors(gameObject.GetComponent<ReferenceHub>()).ToString(), true, true, "");
                    }
                }
            }
            if (Command[0].ToUpper() == "TOTALBANS")
            {
                ev.Allow = false;
                if (Command.Length < 1)
                {
                    ev.Sender.RaReply(Command[0].ToUpper() + "#To run this program, type at least 1 arguments! (some parameters are missing)", false, true, "");
                    return;
                }
                foreach (GameObject gameObject in PlayerManager.players)
                {
                    if (Convert.ToInt16(Command[1]) == gameObject.GetComponent<QueryProcessor>().PlayerId)
                    {
                        ev.Sender.RaReply(Command[0].ToUpper() + "#" + GetTotalBans(gameObject.GetComponent<ReferenceHub>()), true, true, "");
                    }
                }
            }
        }
        public string GetBandetails(ReferenceHub Player)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                using (WebClient webClient3 = new WebClient())
                {
                    webClient3.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", ""), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", ""));
                    webClient3.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    string text3 = webClient3.DownloadString("https://api.cedmod.nl/scpserverbans/scpplugin/reason_requestV2.php?id=" + Player.GetComponent<CharacterClassManager>().UserId + "&ip=" + Player.GetComponent<NetworkIdentity>().connectionToClient.address + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none"));
                    Initializer.logger.Debug("BANSYSTEM", "Checking ban status of user: " + Player.GetComponent<CharacterClassManager>().UserId + " Response from API: " + text3);
                    if (text3 == "0")
                    {
                        Initializer.logger.Debug("BANSYSTEM", "User is not banned");
                    }
                    LastAPIRequestSuccessfull = true;
                    return text3;
                }
            }
            catch (WebException ex)
            {
                Initializer.logger.Error("BANSYSTEN", "Unable to propperly connect to CedMod API: " + ex.Status + " | " + ex.Message);
                LastAPIRequestSuccessfull = false;
                return "0";
            }
        }
        public string CheckBanExpired(ReferenceHub Player)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                    webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    string text2 = webClient.DownloadString("https://api.cedmod.nl/scpserverbans/scpplugin/checkV2.php?id=" + Player.GetComponent<CharacterClassManager>().UserId + "&ip=" + Player.GetComponent<NetworkIdentity>().connectionToClient.address + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none"));
                    Initializer.logger.Info("BANSYSTEM", "checking ban status for user: " + Player.GetComponent<CharacterClassManager>().UserId + " Response from API: " + text2);
                    LastAPIRequestSuccessfull = true;
                    return text2;
                }
            }
            catch (WebException ex)
            {
                LastAPIRequestSuccessfull = false;
                Initializer.logger.Error("BANSYSTEN", "Unable to propperly connect to CedMod API: " + ex.Status + " | " + ex.Message);
                return "0";
            }
        }
        public string Unban(ReferenceHub Player)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                    webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    string text2 = webClient.DownloadString("https://api.cedmod.nl/scpserverbans/scpplugin/unbanV2.php?id=" + Player.GetComponent<CharacterClassManager>().UserId + "&ip=" + Player.GetComponent<NetworkIdentity>().connectionToClient.address + "&reason=Expired&aname=Server&webhook=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_webhook", "none") + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none"));
                    Initializer.logger.Info("BANSYSTEM", "user: " + Player.GetComponent<CharacterClassManager>().UserId + " unban, Response from API: " + text2);
                    LastAPIRequestSuccessfull = true;
                    return text2;
                }
            }
            catch (WebException ex)
            {
                Initializer.logger.Error("BANSYSTEN", "Unable to propperly connect to CedMod API: " + ex.Status + " | " + ex.Message);
                LastAPIRequestSuccessfull = false;
                return "0";
            }
        }
        public static object GetPriors(ReferenceHub Player)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                    webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    string text2 = webClient.DownloadString("https://api.cedmod.nl/scpserverbans/scpplugin/userdetails.php?id=" + Player.GetComponent<CharacterClassManager>().UserId + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none") + "&priors=1");
                    object json = JsonConvert.DeserializeObject(text2);
                    return json;
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("BANSYSTEN Unable to propperly connect to CedMod API: " + ex.Status + " | " + ex.Message);
                return null;
            }
        }
        public static string GetTotalBans(ReferenceHub Player)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                    webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    string text2 = webClient.DownloadString("https://api.cedmod.nl/scpserverbans/scpplugin/userdetails.php?id=" + Player.GetComponent<CharacterClassManager>().UserId + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none") + "&total=1");
                    if (text2 == "")
                    {
                        text2 = "0";
                    }
                    return text2;
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("BANSYSTEN Unable to propperly connect to CedMod API: " + ex.Status + " | " + ex.Message);
                return null;
            }
        }
        public static void Ban(GameObject player, int duration, string sender, string reason)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (duration >= 1)
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
                         "https://api.cedmod.nl/scpserverbans/scpplugin/banV2.php?id=",
                         player.GetComponent<CharacterClassManager>().UserId,
                         "&ip=",
                         player.GetComponent<NetworkIdentity>().connectionToClient.address,
                         "&reason=",
                         reason,
                         "&aname=",
                         sender,
                         "&bd=",
                         duration,
                         "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none") + "&webhook=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_webhook", "none"),
                        }));
                        Log.Info(string.Concat(new object[]
                        {
                         "User: ",
                         player.GetComponent<CharacterClassManager>().UserId,
                         " has been banned by: ",
                         sender,
                         " for the reason: ",
                         reason,
                         " duration: ",
                         duration
                        }));
                        Log.Info("BANSYSTEM: Response from ban API: " + text);
                        ServerConsole.Disconnect(player, text);
                    }
                }
                catch (WebException ex)
                {
                    Initializer.logger.Error("BANSYSTEM", string.Concat(new object[]
                    {
                      "An error occured: ",
                      ex.Message,
                      " ",
                      ex.Status,
                      " Adding to UserIDBans instead"
                    }));
                    //long issuanceTime = TimeBehaviour.CurrentTimestamp();
                    //long banExpieryTime = TimeBehaviour.GetBanExpieryTime((uint)duration);
                    //BanHandler.IssueBan(new BanDetails
                    //{
                        //OriginalName = player.GetComponent<NicknameSync>().MyNick,
                        //Id = player.GetComponent<CharacterClassManager>().UserId,
                        //IssuanceTime = issuanceTime,
                        //Expires = banExpieryTime,
                        //Reason = reason,
                        //Issuer = sender
                    //}, BanHandler.BanType.UserId);
                    //ServerConsole.Disconnect(player, reason);
                }//hm
            }
            else
            {
                if (duration <= 0)
                {
                    string text3;
                    text3 = "Reason: " + reason;
                    ServerConsole.Disconnect(player, text3);
                }
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
        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (error == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            Console.WriteLine("X509Certificate [{0}] Policy Error: '{1}'",
                cert.Subject,
                error.ToString());

            return false;
        }
    }
}