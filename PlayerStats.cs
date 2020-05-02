using CedMod.INIT;
using EXILED;
using EXILED.Extensions;
using Grenades;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace CedMod
{
    public class PlayerStatistics
    {
        public List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();
        public Plugin plugin;
        public PlayerStatistics(Plugin plugin) => this.plugin = plugin;
        public void OnPlayerDeath(ref PlayerDeathEvent ev)
        {
            PlayerDeathEvent ev1 = ev;
            Task.Factory.StartNew(() => { OnPlayerDeathThread(ev1); });
        }
        public void OnRoundEnd()
        {
            Task.Factory.StartNew(() => { OnRoundEndThread(); });
        }
        public void OnRoundEndThread()
        {
            Thread.CurrentThread.Name = "CedModV3 queue worker";
            foreach (ReferenceHub hub in Player.GetHubs())
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
                    webClient.Headers.Add(alias, "Alias");
                    webClient.Headers.Add(ServerConsole.Port.ToString(), "Port");
                    webClient.Headers.Add(ServerConsole.Ip.ToString(), "Ip");
                    {
                        int dnt = 0;
                        if (hub.serverRoles.DoNotTrack)
                        {
                            dnt = 1;
                        }
                        else
                        {
                            dnt = 0;
                        }
                        webClient.DownloadString(new Uri("https://api.cedmod.nl/playerstats/addstat.php?rounds=1&kills=0&deaths=0&teamkills=0&alias=" + alias + "&id=" + hub.characterClassManager.UserId + "&dnt=" + dnt.ToString() + "&ip=" + hub.characterClassManager.RequestIp + "&username=" + hub.nicknameSync.MyNick));

                    }
                }
            }
        }
        public void OnPlayerDeathThread(PlayerDeathEvent ev)
        {
            if (RoundSummary.RoundInProgress() && ev.Killer != ev.Player)
            {
                ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ReferenceHub hub = ev.Player;
                using (WebClient webClient = new WebClient())
                {
                    webClient.Credentials = new NetworkCredential(GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                    webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                    string alias = GameCore.ConfigFile.ServerConfig.GetString("bansystem_alias", "none");
                    if (alias.Contains(" "))
                    {
                        alias.Replace(" ", "");
                    }
                    webClient.Headers.Add(alias, "Alias");
                    webClient.Headers.Add(ServerConsole.Port.ToString(), "Port");
                    webClient.Headers.Add(ServerConsole.Ip.ToString(), "Ip");
                    if (IsTeamkill(ev.Player.characterClassManager, ev.Killer.characterClassManager))
                    {
                        int dnt = 0;
                        if (hub.serverRoles.DoNotTrack)
                        {
                            dnt = 1;
                        }
                        else
                        {
                            dnt = 0;
                        }
                        webClient.DownloadStringAsync(new Uri("https://api.cedmod.nl/playerstats/addstat.php?rounds=0&kills=0&deaths=0&teamkills=1&alias=" + alias + "&id=" + hub.characterClassManager.UserId + "&dnt=" + dnt.ToString() + "&ip=" + hub.characterClassManager.RequestIp + "&username=" + hub.nicknameSync.MyNick));
                    }
                    else
                    {
                        hub = ev.Killer;
                        int dnt = 0;
                        if (hub.serverRoles.DoNotTrack)
                        {
                            dnt = 1;
                        }
                        else
                        {
                            dnt = 0;
                        }
                        using (WebClient webClient2 = new WebClient())
                            webClient2.DownloadStringAsync(new Uri("https://api.cedmod.nl/playerstats/addstat.php?rounds=0&kills=1&deaths=0&teamkills=0&alias=" + alias + "&id=" + hub.characterClassManager.UserId + "&dnt=" + dnt.ToString() + "&ip=" + hub.characterClassManager.RequestIp + "&username=" + hub.nicknameSync.MyNick));
                        hub = ev.Player;
                        if (hub.serverRoles.DoNotTrack)
                        {
                            dnt = 1;
                        }
                        else
                        {
                            dnt = 0;
                        }
                        using (WebClient webClient1 = new WebClient())
                            webClient1.DownloadStringAsync(new Uri("https://api.cedmod.nl/playerstats/addstat.php?rounds=0&kills=0&deaths=1&teamkills=0&alias=" + alias + "&id=" + hub.characterClassManager.UserId + "&dnt=" + dnt.ToString() + "&ip=" + hub.characterClassManager.RequestIp + "&username=" + hub.nicknameSync.MyNick));
                    }
                }
            }
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
        public bool IsTeamkill(CharacterClassManager victim, CharacterClassManager killer)
        {
            Team team = victim.Classes.SafeGet(victim.CurClass).team;
            Team team2 = killer.Classes.SafeGet(killer.CurClass).team;
            if (killer.CurClass != RoleType.Tutorial)
            {
                bool flag = false;
                if (victim.CurClass == killer.CurClass && victim.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString())
                {
                    if (victim.CurClass == RoleType.ClassD && killer.CurClass == RoleType.ClassD && GameCore.ConfigFile.ServerConfig.GetBool("ffa_dclassvsdclasstk", true))
                    {
                        flag = true;
                        Initializer.logger.Debug("FFA", "Teamkill1");
                    }
                    else if (victim.CurClass != RoleType.ClassD || killer.CurClass != RoleType.ClassD || GameCore.ConfigFile.ServerConfig.GetBool("ffa_dclassvsdclasstk", true))
                    {
                        flag = true;
                        Initializer.logger.Debug("FFA", "Teamkill1");
                    }
                }
                else if (victim.CurClass == RoleType.ClassD && killer.CurClass == RoleType.ChaosInsurgency && victim.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString())
                {
                    flag = true;
                    Initializer.logger.Debug("FFA", "Teamkill2");
                }
                else if (victim.CurClass == RoleType.ChaosInsurgency && killer.CurClass == RoleType.ClassD && victim.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString())
                {
                    flag = true;
                    Initializer.logger.Debug("FFA", "Teamkill3");
                }
                else if (team == team2 && victim.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString())
                {
                    flag = true;
                    Initializer.logger.Debug("FFA", "Teamkill4");
                }
                else if (team2 == Team.MTF && victim.CurClass == RoleType.Scientist && victim.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString())
                {
                    flag = true;
                    Initializer.logger.Debug("FFA", "Teamkill5");
                }
                else if (team == Team.MTF && killer.CurClass == RoleType.Scientist && victim.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString())
                {
                    flag = true;
                    Initializer.logger.Debug("FFA", "Teamkill6");
                }
                else if (victim.CurClass == RoleType.ClassD && victim.GetComponent<Handcuffs>().NetworkCufferId >= 1 && !GameCore.ConfigFile.ServerConfig.GetBool("ffa_killingdisarmedclassdsallowed", false))
                {
                    if (killer.CurClass != RoleType.Tutorial && team2 != Team.SCP && killer.CurClass != RoleType.ClassD)
                    {
                        flag = true;
                        Initializer.logger.Debug("FFA", "Teamkill5");
                    }
                }
                else if (victim.CurClass == RoleType.Scientist && victim.GetComponent<Handcuffs>().NetworkCufferId >= 1 && !GameCore.ConfigFile.ServerConfig.GetBool("ffa_killingdisarmedscientistallowed", false) && killer.CurClass != RoleType.Tutorial && team2 != Team.SCP && killer.CurClass != RoleType.ClassD)
                {
                    flag = true;
                    Initializer.logger.Debug("FFA", "Teamkill5");
                }
                return flag;
            }
            else
            {
                return false;
            }
        }

    }
}
