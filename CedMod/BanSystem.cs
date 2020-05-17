using CedMod.INIT;
using EXILED;
using EXILED.Extensions;
using Mirror;
using Newtonsoft.Json;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace CedMod
{


    public class BanSystem
    {
        public List<string> testusers = new List<string>();
        public bool LastAPIRequestSuccessfull = false;
        public Plugin plugin;
        public BanSystem(Plugin plugin) => this.plugin = plugin;
        public void OnPlayerJoinThread(PlayerJoinEvent ev)
        {
            Thread.CurrentThread.Name = "CedModV3 queue worker";
            INIT.Initializer.logger.Debug("BANSYSTEM1", Thread.CurrentThread.Name.ToString());
            if (ev.Player.GetUserId().Contains("@northwood"))
            {
                return;
            }
            if (!ev.Player.gameObject.GetComponent<ServerRoles>().BypassStaff)
            {
                if (!ev.Player.characterClassManager.isLocalPlayer)
                {
                    foreach (string b in GameCore.ConfigFile.ServerConfig.GetStringList("cm_nicknamefilter"))
                    {
                        if (ev.Player.nicknameSync.MyNick.ToUpper().Contains(b.ToUpper()))
                        {
                            ev.Player.nicknameSync.MyNick = ev.Player.nicknameSync.MyNick.Replace(b.ToUpper(), "");
                            ev.Player.nicknameSync.Network_myNickSync = ev.Player.nicknameSync.Network_myNickSync.Replace(b.ToUpper(), "");
                        }
                    }
                    Dictionary<string, string> Bancheck = CheckBanExpired(ev.Player);
                    if (Bancheck["success"] == "false")
                    {
                        if (Bancheck["error"] == "[CEDMOD.Main.Message] CedMod license expired, if you see this contact the server owner.")
                        {
                            INIT.Initializer.logger.Info("CEDMOD-LICENSING", "CedMod license has expired CedMod.dll will now be deleted and the server will be force restarted due to security reasons");
                            string path = EXILED.PluginManager.PluginsDirectory;
                            Initializer.logger.Info("CEDMOD-LICENSING", path);
                            Initializer.logger.Info("CEDMOD-LICENSING", "Deleting files.");
                            if (FileManager.FileExists(path + "/CedMod.dll"))
                            {
                                FileManager.DeleteFile(path + "/CedMod.dll");
                            }
                            if (FileManager.FileExists(path + "/Survival.dll"))
                            {
                                FileManager.DeleteFile(path + "/Survival.dll");
                            }
                            if (FileManager.FileExists(path + "/FrikanwebPlugin.dll"))
                            {
                                FileManager.DeleteFile(path + "/FrikanwebPlugin.dll");
                            }
                            using (WebClient webClient31 = new WebClient())
                            {
                                ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                                webClient31.DownloadFile("https://admin.cedmod.nl/doei.txt", Application.dataPath + "../../houdoe-LEESMIJ-van-CEDMOD-De-profetie-komt-uit-pepegaplap.txt");
                            }
                            Initializer.logger.Info("CEDMOD-LICENSING", "Restarting server.");
                            Application.Quit();
                        }
                    }
                    Initializer.logger.Debug("BANSYSTEM", "Checking ban status of user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " Response from API: " + Bancheck);
                    if (Bancheck["banexpired"] == "true" && Bancheck["success"] == "true")
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
                    Dictionary<string, string> BanReason = GetBandetails(ev.Player);
                    if (BanReason != null && LastAPIRequestSuccessfull)
                    {
                        Initializer.logger.Info("BANSYSTEM", "user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " attempted connection with active ban disconnecting");
                        string Reason = "No reason specified please contact ced777ric#0001 on the discord of this server. This error should not be possible";
                        if (BanReason["success"] == "true" && BanReason["vpn"] == "true" && BanReason["geo"] == "false" && BanReason["isbanned"] == "false")
                        {
                            Reason = BanReason["reason"];
                            ev.Player.characterClassManager.TargetConsolePrint(ev.Player.GetComponent<NetworkIdentity>().connectionToClient, "CedMod.BANSYSTEM Message from CedMod server (VPN/Proxy detected): " + BanReason, "yellow");
                            ServerConsole.Disconnect(ev.Player.GetComponent<CharacterClassManager>().gameObject, Reason);
                        }
                        else
                        {
                            if (BanReason["success"] == "true" && BanReason["vpn"] == "false" && BanReason["geo"] == "true" && BanReason["isbanned"] == "false")
                            {
                                Reason = BanReason["reason"];
                                ev.Player.characterClassManager.TargetConsolePrint(ev.Player.GetComponent<NetworkIdentity>().connectionToClient, "CedMod.BANSYSTEM Message from CedMod server (GEO Restriction): " + BanReason, "yellow");
                                ServerConsole.Disconnect(ev.Player.GetComponent<CharacterClassManager>().gameObject, Reason);
                            }
                            else
                            {
                                if (BanReason["success"] == "true" && BanReason["vpn"] == "false" && BanReason["geo"] == "false" && BanReason["isbanned"] == "true")
                                {
                                    Reason = BanReason["preformattedmessage"] + " You can fill in a ban appeal here: " + GameCore.ConfigFile.ServerConfig.GetString("bansystem_banappealurl", "none");
                                    ev.Player.characterClassManager.TargetConsolePrint(ev.Player.GetComponent<NetworkIdentity>().connectionToClient, "CedMod.BANSYSTEM Active ban: " + BanReason["preformattedmessage"], "yellow");
                                    ServerConsole.Disconnect(ev.Player.GetComponent<CharacterClassManager>().gameObject, Reason);
                                }
                                else
                                {
                                    if (BanReason["success"] == "true" && BanReason["vpn"] == "false" && BanReason["geo"] == "false" && BanReason["isbanned"] == "false" && BanReason["iserror"] == "true")
                                    {
                                        ev.Player.characterClassManager.TargetConsolePrint(ev.Player.GetComponent<NetworkIdentity>().connectionToClient, "CedMod.BANSYSTEM Message from CedMod server: " + BanReason["error"], "yellow");
                                        Initializer.logger.Info("BANSYSTEM", "Message from CedMod server: " + BanReason["error"]);
                                    }
                                }
                            }
                        }
                    }
                    string authtype = testusers.Contains(ev.Player.characterClassManager.UserId) ? "Test API" : "Main API";
                    ev.Player.characterClassManager.TargetConsolePrint(ev.Player.GetComponent<NetworkIdentity>().connectionToClient, "CedMod.BANSYSTEM You have been authed by the CedMod: " + authtype, "green");
                }
            }
        }
        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            Task.Factory.StartNew(() => { OnPlayerJoinThread(ev); });
            if (!RoundSummary.RoundInProgress() && GameCore.ConfigFile.ServerConfig.GetBool("cm_customloadingscreen", true))
            {
                MEC.Timing.RunCoroutine(Playerjoinhandle(ev));
            }
        }
        public IEnumerator<float> Playerjoinhandle(PlayerJoinEvent ev)
        {
            yield return MEC.Timing.WaitForSeconds(0.5f);
            if (!RoundSummary.RoundInProgress())
            {
                ev.Player.characterClassManager.SetPlayersClass(RoleType.Tutorial, ev.Player.gameObject);
                ev.Player.SetGodMode(false);
                ev.Player.inventory.AddNewItem(ItemType.MicroHID);
                yield return MEC.Timing.WaitForSeconds(0.2f);
                ev.Player.SetPosition(new Vector3(-20f, 1020, -43));
            }
            yield return 1f;
        }
        public void OnCommand(ref RACommandEvent ev)
        {
            try
            {
                ReferenceHub sender = ev.Sender.SenderId == "SERVER CONSOLE" || ev.Sender.SenderId == "GAME CONSOLE" ? PlayerManager.localPlayer.GetPlayer() : Player.GetPlayer(ev.Sender.SenderId);
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
                    List<int> list = new List<int>();
                    string[] source = Command[1].Split(new char[]
                    {
                        '.'
                    });
                    list.AddRange((from item in source
                                   where !string.IsNullOrEmpty(item)
                                   select item).Select(new Func<string, int>(int.Parse)));
                    foreach (int num2 in list)
                    {
                        foreach (GameObject gameObject in PlayerManager.players)
                        {
                            if (num2 == gameObject.GetComponent<QueryProcessor>().PlayerId)
                            {
                                if (!gameObject.GetComponent<ServerRoles>().BypassStaff)
                                {
                                    if (Convert.ToInt64(Command[2]) >= 1)
                                    {
                                        string sender1 = ev.Sender.Nickname;
                                        Task.Factory.StartNew(() => { Ban(gameObject, Convert.ToInt64(Command[2]), sender1, text17); });
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(text17) && Convert.ToInt32(Command[2]) <= 0)
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
                        if (Convert.ToInt64(Command[1]) == gameObject.GetComponent<QueryProcessor>().PlayerId)
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
                        if (Convert.ToInt64(Command[1]) == gameObject.GetComponent<QueryProcessor>().PlayerId)
                        {
                            ev.Sender.RaReply(Command[0].ToUpper() + "#" + GetTotalBans(gameObject.GetComponent<ReferenceHub>()), true, true, "");
                        }
                    }
                }
                if (Command[0].ToUpper() == "ENABLETEST")
                {
                    ev.Allow = false;
                    if (Command.Length < 1)
                    {
                        ev.Sender.RaReply(Command[0].ToUpper() + "#To run this program, type at least 1 arguments! (some parameters are missing)", false, true, "");
                        return;
                    }
                    foreach (GameObject gameObject in PlayerManager.players)
                    {
                        if (Convert.ToInt64(Command[1]) == gameObject.GetComponent<QueryProcessor>().PlayerId && !testusers.Contains(gameObject.GetComponent<CharacterClassManager>().UserId))
                        {
                            testusers.Add(gameObject.GetComponent<CharacterClassManager>().UserId);
                            ev.Sender.RaReply(Command[0].ToUpper() + "#Done", true, true, "");
                            gameObject.GetComponent<ReferenceHub>().characterClassManager.TargetConsolePrint(gameObject.GetComponent<ReferenceHub>().GetComponent<NetworkIdentity>().connectionToClient, "You have been added to the test user list from now on until you are removed you will authenticate using the Test API this may contain experimental code.", "yellow");
                        }
                        else
                        {
                            if (testusers.Contains(gameObject.GetComponent<CharacterClassManager>().UserId))
                            {
                                ev.Sender.RaReply(Command[0].ToUpper() + "#User already in test list", true, false, "");
                            }
                        }
                    }
                }
                if (Command[0].ToUpper() == "DISABLETEST")
                {
                    ev.Allow = false;
                    if (Command.Length < 1)
                    {
                        ev.Sender.RaReply(Command[0].ToUpper() + "#To run this program, type at least 1 arguments! (some parameters are missing)", false, true, "");
                        return;
                    }
                    foreach (GameObject gameObject in PlayerManager.players)
                    {
                        if (Convert.ToInt64(Command[1]) == gameObject.GetComponent<QueryProcessor>().PlayerId && testusers.Contains(gameObject.GetComponent<CharacterClassManager>().UserId))
                        {
                            testusers.Remove(gameObject.GetComponent<CharacterClassManager>().UserId);
                            ev.Sender.RaReply(Command[0].ToUpper() + "#Done", true, true, "");
                            gameObject.GetComponent<ReferenceHub>().characterClassManager.TargetConsolePrint(gameObject.GetComponent<ReferenceHub>().GetComponent<NetworkIdentity>().connectionToClient, "You have been removed from the test user list.", "green");
                        }
                        else
                        {
                            if (!testusers.Contains(gameObject.GetComponent<CharacterClassManager>().UserId))
                            {
                                ev.Sender.RaReply(Command[0].ToUpper() + "#User not in list", true, false, "");
                            }
                        }
                    }
                }
                if (Command[0].ToUpper() == "JAIL")
                {
                    ev.Allow = false;
                    if (Command.Length < 2)
                    {
                        return;
                    }
                    if (!sender.CheckPermission("at.jail"))
                    {
                        return;
                    }
                    IEnumerable<string> array = Command.Where(a => a != Command[0]);
                    string filter = null;
                    foreach (string s in array)
                        filter += s;
                    ReferenceHub target = Player.GetPlayer(filter);
                    ev.Sender.RAMessage(GetPriors(target).ToString(), true, "CedModV2");
                }
            }
            catch (Exception ex)
            {
                Initializer.logger.Error("BANSYSTEM", "Source: " + ex.Source);
                Initializer.logger.Error("BANSYSTEM", "Stacktrace: " + ex.StackTrace);
                Initializer.logger.Error("BANSYSTEM", "Message: " + ex.Message);
                Initializer.logger.Error("BANSYSTEM", "Data: " + ex.Data);
            }
        }
        string GEOString = "";
        public Dictionary<string, string> GetBandetails(ReferenceHub Player, bool usegeoifenabled = true)
        {
            if (usegeoifenabled)
            {
                List<string> GEOList = GameCore.ConfigFile.ServerConfig.GetStringList("bansystem_geo");
                if (GEOString == "")
                {
                    foreach (string s in GEOList)
                    {

                        GEOString = GEOString + s + "+";
                    }
                }
                if (GEOList != null)
                {
                    ServerConsole.AccessRestriction = true;
                }
            }
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                using (WebClient webClient3 = new WebClient())
                {
                    webClient3.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", ""), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", ""));
                    webClient3.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    string alias = GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none");
                    if (alias.Contains(" "))
                    {
                        alias.Replace(" ", "");
                    }
                    webClient3.Headers.Add("Alias", alias);
                    webClient3.Headers.Add("Port", ServerConsole.Port.ToString());
                    webClient3.Headers.Add("Ip", ServerConsole.Ip.ToString());
                    string text3 = testusers.Contains(Player.characterClassManager.UserId) || Initializer.TestApiOnly
                        ? webClient3.DownloadString("https://test.cedmod.nl/auth/auth.php?id=" + Player.GetUserId() + "&ip=" + Player.GetComponent<NetworkIdentity>().connectionToClient.address + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none") + "&geo=" + GEOString)
                        : webClient3.DownloadString("https://api.cedmod.nl/auth/auth.php?id=" + Player.GetUserId() + "&ip=" + Player.GetComponent<NetworkIdentity>().connectionToClient.address + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none") + "&geo=" + GEOString);
                    Initializer.logger.Info("BANSYSTEM", "Checking ban status of user: " + Player.GetComponent<CharacterClassManager>().UserId + " Response from API: " + text3);
                    LastAPIRequestSuccessfull = true;
                    Dictionary<string, string> JSONObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(text3);
                    return JSONObj;
                }
            }
            catch (WebException ex)
            {
                Initializer.logger.Error("BANSYSTEN", "Unable to properly connect to CedMod API: " + ex.Status + " | " + ex.Message);
                LastAPIRequestSuccessfull = false;
                return null;
            }
        }
        public Dictionary<string, string> CheckBanExpired(ReferenceHub Player)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            INIT.Initializer.logger.Debug("BANSYSTEM1", Thread.CurrentThread.Name.ToString());
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                    webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    string alias = GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none");
                    if (alias.Contains(" "))
                    {
                        alias.Replace(" ", "");
                    }
                    webClient.Headers.Add("Alias", alias);
                    webClient.Headers.Add("Port", ServerConsole.Port.ToString());
                    webClient.Headers.Add("Ip", ServerConsole.Ip.ToString());
                    string text2 = testusers.Contains(Player.characterClassManager.UserId) || Initializer.TestApiOnly
                        ? webClient.DownloadString("https://test.cedmod.nl/auth/preauth.php?id=" + Player.GetComponent<CharacterClassManager>().UserId + "&ip=" + Player.GetComponent<NetworkIdentity>().connectionToClient.address + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none"))
                        : webClient.DownloadString("https://api.cedmod.nl/auth/preauth.php?id=" + Player.GetComponent<CharacterClassManager>().UserId + "&ip=" + Player.GetComponent<NetworkIdentity>().connectionToClient.address + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none"));
                    Initializer.logger.Info("BANSYSTEM", "checking ban status for user: " + Player.GetComponent<CharacterClassManager>().UserId + " Response from API: " + text2);
                    LastAPIRequestSuccessfull = true;
                    Dictionary<string, string> JSONObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(text2);
                    return JSONObj;
                }
            }
            catch (WebException ex)
            {
                LastAPIRequestSuccessfull = false;
                Initializer.logger.Error("BANSYSTEN", "Unable to properly connect to CedMod API: " + ex.Status + " | " + ex.Message);
                return null;
            }
        }
        public Dictionary<string, string> Unban(ReferenceHub Player)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                    webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    string alias = GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none");
                    if (alias.Contains(" "))
                    {
                        alias.Replace(" ", "");
                    }
                    webClient.Headers.Add("Alias", alias);
                    webClient.Headers.Add("Port", ServerConsole.Port.ToString());
                    webClient.Headers.Add("Ip", ServerConsole.Ip.ToString());
                    string text2 = testusers.Contains(Player.characterClassManager.UserId) || Initializer.TestApiOnly
                        ? webClient.DownloadString("https://test.cedmod.nl/banning/unban.php?id=" + Player.GetComponent<CharacterClassManager>().UserId + "&ip=" + Player.GetComponent<NetworkIdentity>().connectionToClient.address + "&reason=Expired&aname=Server&webhook=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_webhook", "none") + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none"))
                        : webClient.DownloadString("https://api.cedmod.nl/banning/unban.php?id=" + Player.GetComponent<CharacterClassManager>().UserId + "&ip=" + Player.GetComponent<NetworkIdentity>().connectionToClient.address + "&reason=Expired&aname=Server&webhook=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_webhook", "none") + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none"));
                    Initializer.logger.Info("BANSYSTEM", "user: " + Player.GetComponent<CharacterClassManager>().UserId + " unban, Response from API: " + text2);
                    LastAPIRequestSuccessfull = true;
                    Dictionary<string, string> JSONObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(text2);
                    return JSONObj;
                }
            }
            catch (WebException ex)
            {
                Initializer.logger.Error("BANSYSTEN", "Unable to properly connect to CedMod API: " + ex.Status + " | " + ex.Message);
                LastAPIRequestSuccessfull = false;
                return null;
            }
        }
        public object GetPriors(ReferenceHub Player)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                    webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    string alias = GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none");
                    if (alias.Contains(" "))
                    {
                        alias.Replace(" ", "");
                    }
                    webClient.Headers.Add("Alias", alias);
                    webClient.Headers.Add("Port", ServerConsole.Port.ToString());
                    webClient.Headers.Add("Ip", ServerConsole.Ip.ToString());
                    string text2 = testusers.Contains(Player.characterClassManager.UserId) || Initializer.TestApiOnly
                        ? webClient.DownloadString("https://test.cedmod.nl/banning/userdetails.php?id=" + Player.GetComponent<CharacterClassManager>().UserId + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none") + "&priors=1")
                        : webClient.DownloadString("https://api.cedmod.nl/banning/userdetails.php?id=" + Player.GetComponent<CharacterClassManager>().UserId + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none") + "&priors=1");
                    object json = JsonConvert.DeserializeObject(text2);
                    return json;
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("BANSYSTEN Unable to properly connect to CedMod API: " + ex.Status + " | " + ex.Message);
                return null;
            }
        }
        public string GetTotalBans(ReferenceHub Player)
        {
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                    webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    string alias = GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none");
                    if (alias.Contains(" "))
                    {
                        alias.Replace(" ", "");
                    }
                    webClient.Headers.Add("Alias", alias);
                    webClient.Headers.Add("Port", ServerConsole.Port.ToString());
                    webClient.Headers.Add("Ip", ServerConsole.Ip.ToString());
                    string text2 = testusers.Contains(Player.characterClassManager.UserId) || Initializer.TestApiOnly
                        ? webClient.DownloadString("https://api.cedmod.nl/banning/userdetails.php?id=" + Player.GetComponent<CharacterClassManager>().UserId + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none") + "&total=1")
                        : webClient.DownloadString("https://api.cedmod.nl/banning/userdetails.php?id=" + Player.GetComponent<CharacterClassManager>().UserId + "&alias=" + GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none") + "&total=1");
                    if (text2 == "")
                    {
                        text2 = "0";
                    }
                    return text2;
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("BANSYSTEN Unable to properly connect to CedMod API: " + ex.Status + " | " + ex.Message);
                return null;
            }
        }
        public static void Ban(GameObject player, long duration, string sender, string reason, bool bc = true)
        {
            Thread.CurrentThread.Name = "CedModV3 queue worker";
            INIT.Initializer.logger.Debug("BANSYSTEM4", Thread.CurrentThread.Name.ToString());
            ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (duration >= 1)
            {
                if (bc)
                {
                    Map.Broadcast(player.GetComponent<NicknameSync>().MyNick + " Has been banned from the server", 9, false);
                }
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                        webClient.Headers.Add("user-agent", "Cedmod Client build: " + CedMod.INIT.Initializer.GetCedModVersion());
                        string alias = GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none");
                        if (alias.Contains(" "))
                        {
                            alias.Replace(" ", "");
                        }
                        webClient.Headers.Add("Alias", alias);
                        webClient.Headers.Add("Port", ServerConsole.Port.ToString());
                        webClient.Headers.Add("Ip", ServerConsole.Ip.ToString());
                        string text = "";
                        if (Initializer.TestApiOnly)
                        {
                            text = webClient.DownloadString(string.Concat(new object[]
                      {
                         "https://test.cedmod.nl/banning/ban.php?id=",
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
                        }
                        else
                        {
                            text = webClient.DownloadString(string.Concat(new object[]
                             {
                         "https://api.cedmod.nl/banning/ban.php?id=",
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
                        }
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
                        Dictionary<string, string> JSONObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
                        ServerConsole.Disconnect(player, JSONObj["preformattedmessage"]);
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
                    long issuanceTime = TimeBehaviour.CurrentTimestamp();
                    long banExpieryTime = TimeBehaviour.GetBanExpieryTime((uint)duration);
                    BanHandler.IssueBan(new BanDetails
                    {
                        OriginalName = player.GetComponent<NicknameSync>().MyNick,
                        Id = player.GetComponent<CharacterClassManager>().UserId,
                        IssuanceTime = issuanceTime,
                        Expires = banExpieryTime,
                        Reason = reason,
                        Issuer = sender
                    }, BanHandler.BanType.UserId);
                    ServerConsole.Disconnect(player, reason);
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
            if (error == SslPolicyErrors.None)
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