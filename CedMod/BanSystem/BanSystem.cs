using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CedMod.BadgeController;
using CedMod.INIT;
using Exiled.Events.EventArgs;
using GameCore;
using MEC;
using Mirror;
using RemoteAdmin;
using UnityEngine;
using Console = System.Console;
using Map = Exiled.API.Features.Map;
using Permissions = Exiled.Permissions.Extensions.Permissions;
using Player = Exiled.API.Features.Player;

namespace CedMod.BanSystem
{
    public class CmUser
    {
        public bool hasbadge;
        public string[] Badge;
    }
    public class BanSystem
    {
        public static Dictionary<ReferenceHub, CmUser> Users = new Dictionary<ReferenceHub, CmUser>();
        public static List<string> Testusers = new List<string>();
        public static bool LastApiRequestSuccessfull = false;
        public void OnPlayerJoinThread(JoinedEventArgs ev)
        {
            try
            {
                Thread.CurrentThread.Name = "CedModV3 queue worker";
                Initializer.Logger.Debug("BANSYSTEM", Thread.CurrentThread.Name);
                ReferenceHub Player = ev.Player.ReferenceHub;
                if (Player.characterClassManager.UserId.Contains("@northwood"))
                {
                    return;
                }

                if (!ev.Player.ReferenceHub.serverRoles.BypassStaff)
                {
                    if (!Player.characterClassManager.isLocalPlayer)
                    {
                        foreach (string b in ConfigFile.ServerConfig.GetStringList("cm_nicknamefilter"))
                        {
                            if (Player.nicknameSync.MyNick.ToUpper().Contains(b.ToUpper()))
                            {
                                Player.nicknameSync.MyNick = Player.nicknameSync.MyNick.Replace(b.ToUpper(), "");
                                Player.nicknameSync.Network_myNickSync =
                                    Player.nicknameSync.Network_myNickSync.Replace(b.ToUpper(), "");
                            }
                        }

                        Dictionary<string, string> bancheck = Functions.CheckBanExpired(Player);

                        Initializer.Logger.Debug("BANSYSTEM",
                            "Checking ban status of user: " + Player.GetComponent<CharacterClassManager>().UserId +
                            " Response from API: " + bancheck);
                        if (bancheck["banexpired"] == "true" && bancheck["success"] == "true")
                        {
                            Initializer.Logger.Info("BANSYSTEM",
                                "user: " + Player.GetComponent<CharacterClassManager>().UserId +
                                " Ban expired attempting unban");
                            Functions.Unban(Player);
                            Map.Broadcast(20,
                                "WARNING Player: " + Player.nicknameSync.MyNick + " " +
                                Player.characterClassManager.UserId +
                                " has been recently been unbanned due to ban expiery",
                                Broadcast.BroadcastFlags.AdminChat);
                        }

                        Dictionary<string, string> banReason = Functions.GetBandetails(Player);
                        Badge.HandleBadge(banReason["badge"], Player);
                        if (banReason != null && LastApiRequestSuccessfull)
                        {
                            string reason;
                            if (banReason["success"] == "true" && banReason["vpn"] == "true" &&
                                banReason["geo"] == "false" && banReason["isbanned"] == "false")
                            {
                                reason = banReason["reason"];
                                Player.characterClassManager.TargetConsolePrint(
                                    Player.GetComponent<NetworkIdentity>().connectionToClient,
                                    "CedMod.BANSYSTEM Message from CedMod server (VPN/Proxy detected): " + banReason,
                                    "yellow");
                                Initializer.Logger.Info("BANSYSTEM",
                                    "user: " + Player.GetComponent<CharacterClassManager>().UserId +
                                    " attempted connection with blocked ASN/IP/VPN/Hosting service");
                                ev.Player.Disconnect(reason);
                            }
                            else
                            {
                                if (banReason["success"] == "true" && banReason["vpn"] == "false" &&
                                    banReason["geo"] == "true" && banReason["isbanned"] == "false")
                                {
                                    reason = banReason["reason"];
                                    Player.characterClassManager.TargetConsolePrint(
                                        Player.GetComponent<NetworkIdentity>().connectionToClient,
                                        "CedMod.BANSYSTEM Message from CedMod server (GEO Restriction): " + banReason,
                                        "yellow");
                                    Initializer.Logger.Info("BANSYSTEM",
                                        "user: " + Player.GetComponent<CharacterClassManager>().UserId +
                                        " attempted connection from blocked country");
                                    ev.Player.Disconnect(reason);
                                }
                                else
                                {
                                    if (banReason["success"] == "true" && banReason["vpn"] == "false" &&
                                        banReason["geo"] == "false" && banReason["isbanned"] == "true")
                                    {
                                        reason = banReason["preformattedmessage"] +
                                                 " You can fill in a ban appeal here: " +
                                                 ConfigFile.ServerConfig.GetString("bansystem_banappealurl", "none");
                                        Initializer.Logger.Info("BANSYSTEM",
                                            "user: " + Player.GetComponent<CharacterClassManager>().UserId +
                                            " attempted connection with active ban disconnecting");
                                        Player.characterClassManager.TargetConsolePrint(
                                            Player.GetComponent<NetworkIdentity>().connectionToClient,
                                            "CedMod.BANSYSTEM Active ban: " + banReason["preformattedmessage"],
                                            "yellow");
                                        ev.Player.Disconnect(reason);
                                    }
                                    else
                                    {
                                        if (banReason["success"] == "true" && banReason["vpn"] == "false" &&
                                            banReason["geo"] == "false" && banReason["isbanned"] == "false" &&
                                            banReason["iserror"] == "true")
                                        {
                                            Player.characterClassManager.TargetConsolePrint(
                                                Player.GetComponent<NetworkIdentity>().connectionToClient,
                                                "CedMod.BANSYSTEM Message from CedMod server: " + banReason["error"],
                                                "yellow");
                                            Initializer.Logger.Info("BANSYSTEM",
                                                "Message from CedMod server: " + banReason["error"]);
                                        }
                                    }
                                }
                            }
                        }

                        string authtype = Testusers.Contains(Player.characterClassManager.UserId)
                            ? "Test API"
                            : "Main API";
                        Player.characterClassManager.TargetConsolePrint(
                            Player.GetComponent<NetworkIdentity>().connectionToClient,
                            "CedMod.BANSYSTEM You have been authed by the CedMod: " + authtype, "green");
                    }
                }
            }
            catch (Exception ex)
            {
                Initializer.Logger.Error("BANSYSTEM", ex.Message);
                Initializer.Logger.Error("BANSYSTEM", ex.Source);
                Initializer.Logger.Error("BANSYSTEM", ex.StackTrace);
            }
        }
        public void OnPlayerJoin(JoinedEventArgs ev)
        {
            Task.Factory.StartNew(() => { OnPlayerJoinThread(ev); });
            if (!RoundSummary.RoundInProgress() && ConfigFile.ServerConfig.GetBool("cm_customloadingscreen", true))
            {
                Timing.RunCoroutine(Playerjoinhandle(ev));
            }
        }
        public IEnumerator<float> Playerjoinhandle(JoinedEventArgs ev)
        {
            ReferenceHub Player = ev.Player.ReferenceHub;
            yield return Timing.WaitForSeconds(0.5f);
            if (!RoundSummary.RoundInProgress())
            {
                Player.characterClassManager.SetPlayersClass(RoleType.Tutorial, Player.gameObject);
                ev.Player.IsGodModeEnabled = false;
                ItemType item = Functions.GetRandomItem();
                ev.Player.Inventory.AddNewItem(item);
                yield return Timing.WaitForSeconds(0.2f);
                ev.Player.Position = (new Vector3(-20f, 1020, -43));
            }
            yield return 1f;
        }
        public void OnCommand(SendingRemoteAdminCommandEventArgs ev)
        {
            try
            {
                ReferenceHub sender = ev.Sender.Nickname == "SERVER CONSOLE" || ev.Sender.Nickname == "GAME CONSOLE" ? ReferenceHub.LocalHub : ReferenceHub.GetHub(ev.Sender.Id);
                if (ev.Name.ToUpper() == "BAN")
                {
                    ev.IsAllowed = false;
                    if (ev.Arguments.Count < 2)
                    {
                        ev.CommandSender.Respond("To run this program, type at least 3 arguments! (some parameters are missing)", false);
                        return;
                    }
                    string text17 = string.Empty;
                    if (ev.Arguments.Count > 2)
                    {
                        text17 = ev.Arguments.Skip(2).Aggregate((current, n) => current + " " + n);
                    }
                    if (text17.Contains("&"))
                    {
                        ev.CommandSender.Respond("The ban reason must not contain a & or else shit will hit the fan", false);
                        return;
                    }
                    if (text17 == "")
                    {
                        ev.CommandSender.Respond($"To run this program, you must specify a reason use the text based RA console to do so, Autocorrection:   ban  {ev.Arguments[0]} {ev.Arguments[1]} ReasonHere", false);
                        return;
                    }
                    ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(ev.Sender.Nickname, " ran the ban command (duration: ", ev.Arguments[1], " min) on ", ev.Arguments[0], " players. Reason: ", (text17 == string.Empty) ? "(none)" : text17, "."), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                    List<int> list = new List<int>();
                    string[] source = ev.Arguments[0].Split('.');
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
                                    if (Convert.ToInt64(ev.Arguments[1]) >= 1)
                                    {
                                        string sender1 = ev.Sender.Nickname;
                                        Task.Factory.StartNew(() => { Functions.Ban(gameObject, Convert.ToInt64(ev.Arguments[1]), sender1, text17); });
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(text17) && Convert.ToInt32(ev.Arguments[1]) <= 0)
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
                if (ev.Name.ToUpper() == "UNBAN")
                {
                    ev.IsAllowed = false;
                    ev.CommandSender.Respond("Use the Webinterface for unbanning", false);
                    return;
                }
                if (ev.Name.ToUpper() == "PRIORBANS")
                {
                    ev.IsAllowed = false;
                    if (ev.Arguments.Count < 0)
                    {
                        ev.CommandSender.Respond("To run this program, type at least 1 arguments! (some parameters are missing}", false);
                        return;
                    }
                    foreach (GameObject gameObject in PlayerManager.players)
                    {
                        if (Convert.ToInt64(ev.Arguments[0]) == gameObject.GetComponent<QueryProcessor>().PlayerId)
                        {
                            ev.CommandSender.Respond(Functions.GetPriors(gameObject.GetComponent<ReferenceHub>()).ToString(), true);
                        }
                    }
                }
                if (ev.Name.ToUpper() == "TOTALBANS")
                {
                    ev.IsAllowed = false;
                    if (ev.Arguments.Count < 0)
                    {
                        ev.CommandSender.Respond("To run this program, type at least 1 arguments! (some parameters are missing)", false);
                        return;
                    }
                    foreach (GameObject gameObject in PlayerManager.players)
                    {
                        if (Convert.ToInt64(ev.Arguments[0]) == gameObject.GetComponent<QueryProcessor>().PlayerId)
                        {
                            ev.CommandSender.Respond(Functions.GetTotalBans(gameObject.GetComponent<ReferenceHub>()), true);
                        }
                    }
                }
                if (ev.Name.ToUpper() == "ENABLETEST")
                {
                    ev.IsAllowed = false;
                    if (ev.Arguments.Count < 0)
                    {
                        ev.CommandSender.Respond("To run this program, type at least 1 arguments! (some parameters are missing)", false);
                        return;
                    }
                    foreach (GameObject gameObject in PlayerManager.players)
                    {
                        if (Convert.ToInt64(ev.Arguments[0]) == gameObject.GetComponent<QueryProcessor>().PlayerId && !Testusers.Contains(gameObject.GetComponent<CharacterClassManager>().UserId))
                        {
                            Testusers.Add(gameObject.GetComponent<CharacterClassManager>().UserId);
                            ev.CommandSender.Respond("Done", true);
                            gameObject.GetComponent<ReferenceHub>().characterClassManager.TargetConsolePrint(gameObject.GetComponent<ReferenceHub>().GetComponent<NetworkIdentity>().connectionToClient, "You have been added to the test user list from now on until you are removed you will authenticate using the Test API this may contain experimental code.", "yellow");
                        }
                        else
                        {
                            if (Testusers.Contains(gameObject.GetComponent<CharacterClassManager>().UserId))
                            {
                                ev.CommandSender.Respond("User already in test list", false);
                            }
                        }
                    }
                }
                if (ev.Name.ToUpper() == "DISABLETEST")
                {
                    ev.IsAllowed = false;
                    if (ev.Arguments.Count < 0)
                    {
                        ev.CommandSender.Respond("To run this program, type at least 1 arguments! (some parameters are missing)", false);
                        return;
                    }
                    foreach (GameObject gameObject in PlayerManager.players)
                    {
                        if (Convert.ToInt64(ev.Arguments[0]) == gameObject.GetComponent<QueryProcessor>().PlayerId && Testusers.Contains(gameObject.GetComponent<CharacterClassManager>().UserId))
                        {
                            Testusers.Remove(gameObject.GetComponent<CharacterClassManager>().UserId);
                            ev.CommandSender.Respond("Done", true);
                            gameObject.GetComponent<ReferenceHub>().characterClassManager.TargetConsolePrint(gameObject.GetComponent<ReferenceHub>().GetComponent<NetworkIdentity>().connectionToClient, "You have been removed from the test user list.", "green");
                        }
                        else
                        {
                            if (!Testusers.Contains(gameObject.GetComponent<CharacterClassManager>().UserId))
                            {
                                ev.CommandSender.Respond("User not in list", false);
                            }
                        }
                    }
                }
                if (ev.Name.ToUpper() == "JAIL")
                {
                    ev.IsAllowed = false;
                    if (ev.Arguments.Count < 1)
                    {
                        return;
                    }
                    if (!Permissions.CheckPermission(Player.Get(sender), "at.jail"))
                    {
                        return;
                    }

                    string filter = ev.Arguments[0];
                    Player p = Player.Get(filter);
                    ReferenceHub target = p.ReferenceHub;
                    ev.CommandSender.Respond(Functions.GetPriors(target).ToString(), true);
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