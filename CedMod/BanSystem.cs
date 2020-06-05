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
        public static List<string> testusers = new List<string>();
        public static bool LastAPIRequestSuccessfull = false;
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
                    Dictionary<string, string> Bancheck = Functions.CheckBanExpired(ev.Player);
                    
                    Initializer.logger.Debug("BANSYSTEM", "Checking ban status of user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " Response from API: " + Bancheck);
                    if (Bancheck["banexpired"] == "true" && Bancheck["success"] == "true")
                    {
                        Initializer.logger.Info("BANSYSTEM", "user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " Ban expired attempting unban");
                        Functions.Unban(ev.Player);
                        foreach (GameObject o in PlayerManager.players)
                        {
                            ReferenceHub rh = o.GetComponent<ReferenceHub>();
                            if (rh.serverRoles.RemoteAdmin)
                            {
                                rh.Broadcast(10, "WARNING Player: " + ev.Player.nicknameSync.MyNick + " " + ev.Player.characterClassManager.UserId + " has been recently been unbanned due to ban expiery", false);
                            }
                        }
                    }
                    Dictionary<string, string> BanReason = Functions.GetBandetails(ev.Player);
                    if (BanReason != null && LastAPIRequestSuccessfull)
                    {
                        string Reason = "No reason specified please contact ced777ric#0001 on the discord of this server. This error should not be possible";
                        if (BanReason["success"] == "true" && BanReason["vpn"] == "true" && BanReason["geo"] == "false" && BanReason["isbanned"] == "false")
                        {
                            Reason = BanReason["reason"];
                            ev.Player.characterClassManager.TargetConsolePrint(ev.Player.GetComponent<NetworkIdentity>().connectionToClient, "CedMod.BANSYSTEM Message from CedMod server (VPN/Proxy detected): " + BanReason, "yellow");
                            Initializer.logger.Info("BANSYSTEM", "user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " attempted connection with blocked ASN/IP/VPN/Hosting service");
                            ServerConsole.Disconnect(ev.Player.GetComponent<CharacterClassManager>().gameObject, Reason);
                        }
                        else
                        {
                            if (BanReason["success"] == "true" && BanReason["vpn"] == "false" && BanReason["geo"] == "true" && BanReason["isbanned"] == "false")
                            {
                                Reason = BanReason["reason"];
                                ev.Player.characterClassManager.TargetConsolePrint(ev.Player.GetComponent<NetworkIdentity>().connectionToClient, "CedMod.BANSYSTEM Message from CedMod server (GEO Restriction): " + BanReason, "yellow");
                                Initializer.logger.Info("BANSYSTEM", "user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " attempted connection from blocked country");
                                ServerConsole.Disconnect(ev.Player.GetComponent<CharacterClassManager>().gameObject, Reason);
                            }
                            else
                            {
                                if (BanReason["success"] == "true" && BanReason["vpn"] == "false" && BanReason["geo"] == "false" && BanReason["isbanned"] == "true")
                                {
                                    Reason = BanReason["preformattedmessage"] + " You can fill in a ban appeal here: " + GameCore.ConfigFile.ServerConfig.GetString("bansystem_banappealurl", "none");
                                    Initializer.logger.Info("BANSYSTEM", "user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " attempted connection with active ban disconnecting");
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
                                        Task.Factory.StartNew(() => { Functions.Ban(gameObject, Convert.ToInt64(Command[2]), sender1, text17); });
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
                            ev.Sender.RaReply(Command[0].ToUpper() + "#" + Functions.GetPriors(gameObject.GetComponent<ReferenceHub>()).ToString(), true, true, "");
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
                            ev.Sender.RaReply(Command[0].ToUpper() + "#" + Functions.GetTotalBans(gameObject.GetComponent<ReferenceHub>()), true, true, "");
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
                    ev.Sender.RAMessage(Functions.GetPriors(target).ToString(), true, "CedModV2");
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
    }
}