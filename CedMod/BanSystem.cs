using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CedMod.CedMod.INIT;
using EXILED;
using EXILED.Extensions;
using GameCore;
using MEC;
using Mirror;
using RemoteAdmin;
using UnityEngine;

namespace CedMod
{


    public class BanSystem
    { 
        public static List<string> Testusers = new List<string>();
        public static bool LastApiRequestSuccessfull = false;
        public Plugin Plugin;
        public BanSystem(Plugin plugin) => Plugin = plugin;
        public void OnPlayerJoinThread(PlayerJoinEvent ev)
        {
            Thread.CurrentThread.Name = "CedModV3 queue worker";
            Initializer.Logger.Debug("BANSYSTEM", Thread.CurrentThread.Name);
            if (ev.Player.GetUserId().Contains("@northwood"))
            {
                return;
            }
            if (!ev.Player.gameObject.GetComponent<ServerRoles>().BypassStaff)
            {
                if (!ev.Player.characterClassManager.isLocalPlayer)
                {
                    foreach (string b in ConfigFile.ServerConfig.GetStringList("cm_nicknamefilter"))
                    {
                        if (ev.Player.nicknameSync.MyNick.ToUpper().Contains(b.ToUpper()))
                        {
                            ev.Player.nicknameSync.MyNick = ev.Player.nicknameSync.MyNick.Replace(b.ToUpper(), "");
                            ev.Player.nicknameSync.Network_myNickSync = ev.Player.nicknameSync.Network_myNickSync.Replace(b.ToUpper(), "");
                        }
                    }
                    Dictionary<string, string> bancheck = Functions.CheckBanExpired(ev.Player);
                    
                    Initializer.Logger.Debug("BANSYSTEM", "Checking ban status of user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " Response from API: " + bancheck);
                    if (bancheck["banexpired"] == "true" && bancheck["success"] == "true")
                    {
                        Initializer.Logger.Info("BANSYSTEM", "user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " Ban expired attempting unban");
                        Functions.Unban(ev.Player);
                        foreach (GameObject o in PlayerManager.players)
                        {
                            ReferenceHub rh = o.GetComponent<ReferenceHub>();
                            if (rh.serverRoles.RemoteAdmin)
                            {
                                rh.Broadcast(10, "WARNING Player: " + ev.Player.nicknameSync.MyNick + " " + ev.Player.characterClassManager.UserId + " has been recently been unbanned due to ban expiery");
                            }
                        }
                    }
                    Dictionary<string, string> banReason = Functions.GetBandetails(ev.Player);
                    if (banReason != null && LastApiRequestSuccessfull)
                    {
                        string reason;
                        if (banReason["success"] == "true" && banReason["vpn"] == "true" && banReason["geo"] == "false" && banReason["isbanned"] == "false")
                        {
                            reason = banReason["reason"];
                            ev.Player.characterClassManager.TargetConsolePrint(ev.Player.GetComponent<NetworkIdentity>().connectionToClient, "CedMod.BANSYSTEM Message from CedMod server (VPN/Proxy detected): " + banReason, "yellow");
                            Initializer.Logger.Info("BANSYSTEM", "user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " attempted connection with blocked ASN/IP/VPN/Hosting service");
                            ServerConsole.Disconnect(ev.Player.GetComponent<CharacterClassManager>().gameObject, reason);
                        }
                        else
                        {
                            if (banReason["success"] == "true" && banReason["vpn"] == "false" && banReason["geo"] == "true" && banReason["isbanned"] == "false")
                            {
                                reason = banReason["reason"];
                                ev.Player.characterClassManager.TargetConsolePrint(ev.Player.GetComponent<NetworkIdentity>().connectionToClient, "CedMod.BANSYSTEM Message from CedMod server (GEO Restriction): " + banReason, "yellow");
                                Initializer.Logger.Info("BANSYSTEM", "user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " attempted connection from blocked country");
                                ServerConsole.Disconnect(ev.Player.GetComponent<CharacterClassManager>().gameObject, reason);
                            }
                            else
                            {
                                if (banReason["success"] == "true" && banReason["vpn"] == "false" && banReason["geo"] == "false" && banReason["isbanned"] == "true")
                                {
                                    reason = banReason["preformattedmessage"] + " You can fill in a ban appeal here: " + ConfigFile.ServerConfig.GetString("bansystem_banappealurl", "none");
                                    Initializer.Logger.Info("BANSYSTEM", "user: " + ev.Player.GetComponent<CharacterClassManager>().UserId + " attempted connection with active ban disconnecting");
                                    ev.Player.characterClassManager.TargetConsolePrint(ev.Player.GetComponent<NetworkIdentity>().connectionToClient, "CedMod.BANSYSTEM Active ban: " + banReason["preformattedmessage"], "yellow");
                                    ServerConsole.Disconnect(ev.Player.GetComponent<CharacterClassManager>().gameObject, reason);
                                }
                                else
                                {
                                    if (banReason["success"] == "true" && banReason["vpn"] == "false" && banReason["geo"] == "false" && banReason["isbanned"] == "false" && banReason["iserror"] == "true")
                                    {
                                        ev.Player.characterClassManager.TargetConsolePrint(ev.Player.GetComponent<NetworkIdentity>().connectionToClient, "CedMod.BANSYSTEM Message from CedMod server: " + banReason["error"], "yellow");
                                        Initializer.Logger.Info("BANSYSTEM", "Message from CedMod server: " + banReason["error"]);
                                    }
                                }
                            }
                        }
                    }
                    string authtype = Testusers.Contains(ev.Player.characterClassManager.UserId) ? "Test API" : "Main API";
                    ev.Player.characterClassManager.TargetConsolePrint(ev.Player.GetComponent<NetworkIdentity>().connectionToClient, "CedMod.BANSYSTEM You have been authed by the CedMod: " + authtype, "green");
                }
            }
        }
        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            Task.Factory.StartNew(() => { OnPlayerJoinThread(ev); });
            if (!RoundSummary.RoundInProgress() && ConfigFile.ServerConfig.GetBool("cm_customloadingscreen", true))
            {
                Timing.RunCoroutine(Playerjoinhandle(ev));
            }
        }
        public IEnumerator<float> Playerjoinhandle(PlayerJoinEvent ev)
        {
            yield return Timing.WaitForSeconds(0.5f);
            if (!RoundSummary.RoundInProgress())
            {
                ev.Player.characterClassManager.SetPlayersClass(RoleType.Tutorial, ev.Player.gameObject);
                yield return Timing.WaitForSeconds(0.2f);
                ev.Player.SetGodMode(false);
                ItemType item = Functions.GetRandomItem();
                ev.Player.inventory.AddNewItem(item);
                ev.Player.SetPosition(new Vector3(-20f, 1020, -43));
            }
            yield return 1f;
        }
        public void OnCommand(ref RACommandEvent ev)
        {
            try
            {
                ReferenceHub sender = ev.Sender.SenderId == "SERVER CONSOLE" || ev.Sender.SenderId == "GAME CONSOLE" ? PlayerManager.localPlayer.GetPlayer() : Player.GetPlayer(ev.Sender.SenderId);
                string[] command = ev.Command.Split(' ');
                if (command[0].ToUpper() == "BAN")
                {
                    ev.Allow = false;
                    if (command.Length < 3)
                    {
                        ev.Sender.RaReply(command[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, "");
                        return;
                    }
                    string text17 = string.Empty;
                    if (command.Length > 3)
                    {
                        text17 = command.Skip(3).Aggregate((current, n) => current + " " + n);
                    }
                    if (text17.Contains("&"))
                    {
                        ev.Sender.RaReply("The ban reason must not contain a & or else shit will hit the fan", false, false, "");
                        return;
                    }
                    if (text17 == "")
                    {
                        ev.Sender.RaReply(string.Concat(command[0].ToUpper(), "#To run this program, you must specify a reason use the text based RA console to do so, Autocorrection:   ban ", command[1], " ", command[2], " ReasonHere"), false, true, "");
                        return;
                    }
                    List<int> list = new List<int>();
                    string[] source = command[1].Split('.');
                    list.AddRange((from item in source
                                   where !string.IsNullOrEmpty(item)
                                   select item).Select(int.Parse));
                    foreach (int num2 in list)
                    {
                        foreach (GameObject gameObject in PlayerManager.players)
                        {
                            if (num2 == gameObject.GetComponent<QueryProcessor>().PlayerId)
                            {
                                if (!gameObject.GetComponent<ServerRoles>().BypassStaff)
                                {
                                    if (Convert.ToInt64(command[2]) >= 1)
                                    {
                                        string sender1 = ev.Sender.Nickname;
                                        Task.Factory.StartNew(() => { Functions.Ban(gameObject, Convert.ToInt64(command[2]), sender1, text17); });
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(text17) && Convert.ToInt32(command[2]) <= 0)
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
                if (command[0].ToUpper() == "UNBAN")
                {
                    ev.Allow = false;
                    ev.Sender.RaReply(command[0] + "#Use the Webinterface for unbanning", false, true, "");
                    return;
                }
                if (command[0].ToUpper() == "PRIORBANS")
                {
                    ev.Allow = false;
                    if (command.Length < 1)
                    {
                        ev.Sender.RaReply(command[0].ToUpper() + "#To run this program, type at least 1 arguments! (some parameters are missing)", false, true, "");
                        return;
                    }
                    foreach (GameObject gameObject in PlayerManager.players)
                    {
                        if (Convert.ToInt64(command[1]) == gameObject.GetComponent<QueryProcessor>().PlayerId)
                        {
                            ev.Sender.RaReply(command[0].ToUpper() + "#" + Functions.GetPriors(gameObject.GetComponent<ReferenceHub>()), true, true, "");
                        }
                    }
                }
                if (command[0].ToUpper() == "TOTALBANS")
                {
                    ev.Allow = false;
                    if (command.Length < 1)
                    {
                        ev.Sender.RaReply(command[0].ToUpper() + "#To run this program, type at least 1 arguments! (some parameters are missing)", false, true, "");
                        return;
                    }
                    foreach (GameObject gameObject in PlayerManager.players)
                    {
                        if (Convert.ToInt64(command[1]) == gameObject.GetComponent<QueryProcessor>().PlayerId)
                        {
                            ev.Sender.RaReply(command[0].ToUpper() + "#" + Functions.GetTotalBans(gameObject.GetComponent<ReferenceHub>()), true, true, "");
                        }
                    }
                }
                if (command[0].ToUpper() == "ENABLETEST")
                {
                    ev.Allow = false;
                    if (command.Length < 1)
                    {
                        ev.Sender.RaReply(command[0].ToUpper() + "#To run this program, type at least 1 arguments! (some parameters are missing)", false, true, "");
                        return;
                    }
                    foreach (GameObject gameObject in PlayerManager.players)
                    {
                        if (Convert.ToInt64(command[1]) == gameObject.GetComponent<QueryProcessor>().PlayerId && !Testusers.Contains(gameObject.GetComponent<CharacterClassManager>().UserId))
                        {
                            Testusers.Add(gameObject.GetComponent<CharacterClassManager>().UserId);
                            ev.Sender.RaReply(command[0].ToUpper() + "#Done", true, true, "");
                            gameObject.GetComponent<ReferenceHub>().characterClassManager.TargetConsolePrint(gameObject.GetComponent<ReferenceHub>().GetComponent<NetworkIdentity>().connectionToClient, "You have been added to the test user list from now on until you are removed you will authenticate using the Test API this may contain experimental code.", "yellow");
                        }
                        else
                        {
                            if (Testusers.Contains(gameObject.GetComponent<CharacterClassManager>().UserId))
                            {
                                ev.Sender.RaReply(command[0].ToUpper() + "#User already in test list", true, false, "");
                            }
                        }
                    }
                }
                if (command[0].ToUpper() == "DISABLETEST")
                {
                    ev.Allow = false;
                    if (command.Length < 1)
                    {
                        ev.Sender.RaReply(command[0].ToUpper() + "#To run this program, type at least 1 arguments! (some parameters are missing)", false, true, "");
                        return;
                    }
                    foreach (GameObject gameObject in PlayerManager.players)
                    {
                        if (Convert.ToInt64(command[1]) == gameObject.GetComponent<QueryProcessor>().PlayerId && Testusers.Contains(gameObject.GetComponent<CharacterClassManager>().UserId))
                        {
                            Testusers.Remove(gameObject.GetComponent<CharacterClassManager>().UserId);
                            ev.Sender.RaReply(command[0].ToUpper() + "#Done", true, true, "");
                            gameObject.GetComponent<ReferenceHub>().characterClassManager.TargetConsolePrint(gameObject.GetComponent<ReferenceHub>().GetComponent<NetworkIdentity>().connectionToClient, "You have been removed from the test user list.", "green");
                        }
                        else
                        {
                            if (!Testusers.Contains(gameObject.GetComponent<CharacterClassManager>().UserId))
                            {
                                ev.Sender.RaReply(command[0].ToUpper() + "#User not in list", true, false, "");
                            }
                        }
                    }
                }
                if (command[0].ToUpper() == "JAIL")
                {
                    ev.Allow = false;
                    if (command.Length < 2)
                    {
                        return;
                    }
                    if (!sender.CheckPermission("at.jail"))
                    {
                        return;
                    }
                    IEnumerable<string> array = command.Where(a => a != command[0]);
                    string filter = null;
                    foreach (string s in array)
                        filter += s;
                    ReferenceHub target = Player.GetPlayer(filter);
                    ev.Sender.RAMessage(Functions.GetPriors(target).ToString(), true, "CedModV2");
                }
            }
            catch (Exception ex)
            {
                Initializer.Logger.Error("BANSYSTEM", "Source: " + ex.Source);
                Initializer.Logger.Error("BANSYSTEM", "Stacktrace: " + ex.StackTrace);
                Initializer.Logger.Error("BANSYSTEM", "Message: " + ex.Message);
                Initializer.Logger.Error("BANSYSTEM", "Data: " + ex.Data);
            }
        }
    }
}