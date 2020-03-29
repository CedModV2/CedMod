﻿using CedMod.INIT;
using Mirror;
using EXILED;
using System;
using EXILED.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;

namespace CedMod
{
    public class FriendlyFireAutoBan
    {
        public static bool AdminDisabled = false;
        private readonly Plugin plugin;
        public FriendlyFireAutoBan(Plugin plugin) => this.plugin = plugin;
        public void OnRoundStart()
        {
            badguylist.Clear();
            AdminDisabled = false;
        }
        static List<string> badguylist = new List<string>();
        Dictionary<string, string> victims = new Dictionary<string, string>();
        public void Ondeath(ref PlayerDeathEvent ev)
        {
            CharacterClassManager victim = ev.Player.characterClassManager;
            CharacterClassManager killer = ev.Killer.characterClassManager;
            if (GameCore.ConfigFile.ServerConfig.GetBool("ffa_enable", false) && RoundSummary.RoundInProgress() && !AdminDisabled)
            {
                bool flag = false;
                Team team = victim.Classes.SafeGet(victim.CurClass).team;
                Team team2 = killer.Classes.SafeGet(killer.CurClass).team;
                FFALog(killer, victim, team, team2);
                if (killer.CurClass != RoleType.Tutorial)
                {
                    if (victim.CurClass == killer.CurClass && victim.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString())
                    {
                        if (victim.CurClass == RoleType.ClassD && killer.CurClass == RoleType.ClassD && GameCore.ConfigFile.ServerConfig.GetBool("ffa_dclassvsdclasstk", true))
                        {
                            flag = true;
                            Initializer.logger.Debug("FFA", "Teamkill1");
                            FFALog(killer, victim, team, team2);
                        }
                        else if (victim.CurClass != RoleType.ClassD || killer.CurClass != RoleType.ClassD || GameCore.ConfigFile.ServerConfig.GetBool("ffa_dclassvsdclasstk", true))
                        {
                            flag = true;
                            Initializer.logger.Debug("FFA", "Teamkill1");
                            FFALog(killer, victim, team, team2);
                        }
                    }
                    else if (victim.CurClass == RoleType.ClassD && killer.CurClass == RoleType.ChaosInsurgency && victim.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString())
                    {
                        flag = true;
                        Initializer.logger.Debug("FFA", "Teamkill2");
                        FFALog(killer, victim, team, team2);
                    }
                    else if (victim.CurClass == RoleType.ChaosInsurgency && killer.CurClass == RoleType.ClassD && victim.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString())
                    {
                        flag = true;
                        Initializer.logger.Debug("FFA", "Teamkill3");
                        FFALog(killer, victim, team, team2);
                    }
                    else if (team == team2 && victim.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString())
                    {
                        flag = true;
                        Initializer.logger.Debug("FFA", "Teamkill4");
                        FFALog(killer, victim, team, team2);
                    }
                    else if (team2 == Team.MTF && victim.CurClass == RoleType.Scientist && victim.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString())
                    {
                        flag = true;
                        Initializer.logger.Debug("FFA", "Teamkill5");
                        FFALog(killer, victim, team, team2);
                    }
                    else if (team == Team.MTF && killer.CurClass == RoleType.Scientist && victim.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString() != killer.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString())
                    {
                        flag = true;
                        Initializer.logger.Debug("FFA", "Teamkill6");
                        FFALog(killer, victim, team, team2);
                    }
                    else if (victim.CurClass == RoleType.ClassD && victim.GetComponent<Handcuffs>().NetworkCufferId >= 1 && !GameCore.ConfigFile.ServerConfig.GetBool("ffa_killingdisarmedclassdsallowed", false))
                    {
                        if (killer.CurClass != RoleType.Tutorial && team2 != Team.SCP && killer.CurClass != RoleType.ClassD)
                        {
                            flag = true;
                            Initializer.logger.Debug("FFA", "Teamkill5");
                            FFALog(killer, victim, team, team2);
                        }
                    }
                    else if (victim.CurClass == RoleType.Scientist && victim.GetComponent<Handcuffs>().NetworkCufferId >= 1 && !GameCore.ConfigFile.ServerConfig.GetBool("ffa_killingdisarmedscientistallowed", false) && killer.CurClass != RoleType.Tutorial && team2 != Team.SCP && killer.CurClass != RoleType.ClassD)
                    {
                        flag = true;
                        Initializer.logger.Debug("FFA", "Teamkill5");
                        FFALog(killer, victim, team, team2);
                    }
                }
                if (flag)
                {
                    if (killer.GetComponent<NetworkIdentity>().connectionToClient != null)
                    {
                        RemoteAdmin.QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(killer.gameObject.GetComponent<NetworkIdentity>().connectionToClient, "<size=25><b><color=yellow>You teamkilled: </color></b><color=red>" + victim.gameObject.GetComponent<NicknameSync>().MyNick + "</color><color=yellow><b> If you continue teamkilling it will result in a ban</b></color></size>", 20U, false);
                    }
                    if (victim.GetComponent<NetworkIdentity>().connectionToClient != null)
                    {
                        RemoteAdmin.QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(victim.gameObject.GetComponent<NetworkIdentity>().connectionToClient, string.Concat(new string[]
                        {
                        string.Concat(new string[]
                        {
                            "<size=25><b><color=yellow>You have been teamkilled by: </color></b></size>",
                            "<color=red><size=25>",
                            killer.gameObject.GetComponent<NicknameSync>().MyNick,
                            " (",
                            killer.gameObject.GetComponent<CharacterClassManager>().UserId,
                            "), " + killer.CurClass + " You were a " + victim.CurClass + " </size></color>" + Environment.NewLine,
                            "<size=25><b><color=yellow> Use this as a screenshot as evidence for a report " + Environment.NewLine + " Or if you want to forgive this person open your client console with ` or ~ and type .forgive <i>You only have 30 seconds to forgive this teamkill it is not possible after that</i></color></b></size>",
                            "<size=25><i><color=yellow> Note: if he continues to teamkill the server will ban him</color></i></size>"
                        })
                        }), 20U, false);
                        if (!victims.ContainsKey(victim.UserId))
                        {
                            victims.Add(victim.UserId, killer.UserId);
                            MEC.Timing.RunCoroutine(RemoveVictim(victim.UserId, victim), "timer");
                        }
                    }
                    badguylist.Add(killer.gameObject.GetComponent<CharacterClassManager>().UserId);
                    int num = 0;
                    using (List<string>.Enumerator enumerator = badguylist.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current == killer.gameObject.GetComponent<CharacterClassManager>().UserId.ToString())
                            {
                                num++;
                                if (num >= GameCore.ConfigFile.ServerConfig.GetInt("ffa_ammountoftkbeforeban", 3) && num <= GameCore.ConfigFile.ServerConfig.GetInt("ffa_ammountoftkbeforeban", 3))
                                {
                                    Initializer.logger.Info("FFA", string.Concat(new string[]
                                    {
                                    "Player: ",
                                    killer.gameObject.GetComponent<NicknameSync>().MyNick,
                                    " ",
                                    killer.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString(),
                                    " ",
                                    killer.gameObject.GetComponent<CharacterClassManager>().UserId,
                                    " exeeded teamkill limit"
                                    }));
                                    BanSystem.Ban(killer.gameObject, GameCore.ConfigFile.ServerConfig.GetInt("ffa_banduration", 4320), "Server.Module.FriendlyFireAutoban", GameCore.ConfigFile.ServerConfig.GetString("ffa_banreason", "You have teamkilled too many people"), false);
                                    RemoteAdmin.QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcAddElement("<size=25><b><color=yellow>user: </color></b><color=red>" + killer.gameObject.GetComponent<NicknameSync>().MyNick + "</color><color=yellow><b> got banned for teamkilling, dont be like this user please</b></color></size>", 20U, false);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (AdminDisabled)
                {
                    RemoteAdmin.QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(victim.gameObject.GetComponent<NetworkIdentity>().connectionToClient, "<size=25><b><color=yellow>You have been teamkilled but FriendlyFireAutoban is disabled by an admin, reports regarding this teamkill will not be handled</color></b></size>", 10U, false);
                }
            }
        }
        public static void FFA(CharacterClassManager victim, CharacterClassManager killer)
        {

        }
        public static void FFALog(CharacterClassManager killer, CharacterClassManager victim, Team victimteam, Team killerteam)
        {
            Initializer.logger.Debug("FFA", string.Concat(new object[]
            {
            "VictimDetails: ",
            victim.gameObject.GetComponent<CharacterClassManager>().UserId,
            " ",
            victim.gameObject.GetComponent<NicknameSync>().MyNick,
            " ",
            victim.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString(),
            " ",
            victimteam,
            " ",
            victim.CurClass
            }));
            Initializer.logger.Debug("FFA", string.Concat(new object[]
            {
            "KillerDetails: ",
            killer.gameObject.GetComponent<CharacterClassManager>().UserId,
            " ",
            killer.gameObject.GetComponent<NicknameSync>().MyNick,
            " ",
            killer.gameObject.GetComponent<RemoteAdmin.QueryProcessor>().PlayerId.ToString(),
            " ",
            killerteam,
            " ",
            killer.CurClass
            }));
        }
        public IEnumerator<float> RemoveVictim(string victim, CharacterClassManager victimccm)
        {
            yield return MEC.Timing.WaitForSeconds(30f);
            victims.Remove(victim);
            Initializer.logger.Debug("FFA", "forgive timeout");
        }
        public void ConsoleCommand(ConsoleCommandEvent ev)
        {
            string[] Command = ev.Command.Split(new char[]
            {
                ' '
            });
            switch (Command[0].ToUpper())
            {
                case "FORGIVE":
                    if (victims.ContainsKey(ev.Player.characterClassManager.UserId))
                    {
                        string killeruid = victims.GetValueSafe(ev.Player.characterClassManager.UserId);
                        ReferenceHub hub1 = null;
                        foreach (ReferenceHub hub in Player.GetHubs())
                        {
                            if (hub.characterClassManager.UserId == killeruid)
                            {
                                hub1 = hub;
                            }
                        }
                        ev.Color = "yellow";
                        ev.ReturnMessage = "You have forgiven " + hub1.GetNickname() + "(" + hub1.characterClassManager.UserId + ").";
                        MEC.Timing.KillCoroutines("timer");
                        victims.Remove(ev.Player.characterClassManager.UserId);
                        hub1.Broadcast(10u, "<color=yellow>You have been forgiven by " + ev.Player.GetNickname() + "(" + ev.Player.characterClassManager.UserId + ")" + ".</color>", false);
                        badguylist.Remove(hub1.characterClassManager.UserId);
                    }
                    else
                    {
                        ev.Color = "yellow";
                        ev.ReturnMessage = "You dont have anyone to forgive.";
                    }
                    break;
            }
        }
    }
}
