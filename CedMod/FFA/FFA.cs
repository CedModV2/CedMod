using System;
using System.Collections.Generic;
using CedMod.INIT;
using Exiled.API.Features;
using GameCore;
using Exiled.Events.EventArgs;
using MEC;
using Mirror;
using RemoteAdmin;
using UnityEngine;

namespace CedMod.FFA
{
    public class FriendlyFireAutoBan
    {
        public static bool AdminDisabled;
        public void OnRoundStart()
        {
            _badguylist.Clear();
            AdminDisabled = false;
        }
        static List<string> _badguylist = new List<string>();
        Dictionary<string, string> _victims = new Dictionary<string, string>();
        public void Ondeath(DiedEventArgs ev)
        {
            CharacterClassManager victim = ev.Target.ReferenceHub.characterClassManager;
            CharacterClassManager killer = ev.Killer.ReferenceHub.characterClassManager;
            if (ConfigFile.ServerConfig.GetBool("ffa_enable") && RoundSummary.RoundInProgress() && !AdminDisabled)
            {
                bool flag = Functions.IsTeamKill(victim, killer);
                if (flag)
                {
                    if (killer.GetComponent<NetworkIdentity>().connectionToClient != null)
                    {
                        QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(killer.gameObject.GetComponent<NetworkIdentity>().connectionToClient, "<size=25><b><color=yellow>You teamkilled: </color></b><color=red>" + victim.gameObject.GetComponent<NicknameSync>().MyNick + "</color><color=yellow><b> If you continue teamkilling it will result in a ban</b></color></size>", 20, Broadcast.BroadcastFlags.Normal);
                    }
                    if (victim.GetComponent<NetworkIdentity>().connectionToClient != null)
                    {
                        QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(victim.gameObject.GetComponent<NetworkIdentity>().connectionToClient, string.Concat("<size=25><b><color=yellow>You have been teamkilled by: </color></b></size>", "<color=red><size=25>", killer.gameObject.GetComponent<NicknameSync>().MyNick, " (", killer.gameObject.GetComponent<CharacterClassManager>().UserId, "), " + killer.CurClass + " You were a " + victim.CurClass + " </size></color>" + Environment.NewLine, "<size=25><b><color=yellow> Use this as a screenshot as evidence for a report " + Environment.NewLine + " Or if you want to forgive this person open your client console with ` or ~ and type .forgive <i>You only have 30 seconds to forgive this teamkill it is not possible after that</i></color></b></size>", "<size=25><i><color=yellow> Note: if he continues to teamkill the server will ban him</color></i></size>"), 20, Broadcast.BroadcastFlags.Normal);
                        if (!_victims.ContainsKey(victim.UserId))
                        {
                            _victims.Add(victim.UserId, killer.UserId);
                            Timing.RunCoroutine(RemoveVictim(victim.UserId, victim), "timer");
                        }
                    }
                    _badguylist.Add(killer.gameObject.GetComponent<CharacterClassManager>().UserId);
                    int num = 0;
                    using (List<string>.Enumerator enumerator = _badguylist.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current == killer.gameObject.GetComponent<CharacterClassManager>().UserId)
                            {
                                num++;
                                if (num >= ConfigFile.ServerConfig.GetInt("ffa_ammountoftkbeforeban", 3) && num <= ConfigFile.ServerConfig.GetInt("ffa_ammountoftkbeforeban", 3))
                                {
                                    Initializer.Logger.Info("FFA", string.Concat("Player: ", killer.gameObject.GetComponent<NicknameSync>().MyNick, " ", killer.gameObject.GetComponent<QueryProcessor>().PlayerId.ToString(), " ", killer.gameObject.GetComponent<CharacterClassManager>().UserId, " exeeded teamkill limit"));
                                    Functions.Ban(killer.gameObject, ConfigFile.ServerConfig.GetInt("ffa_banduration", 4320), "Server.Module.FriendlyFireAutoban", ConfigFile.ServerConfig.GetString("ffa_banreason", "You have teamkilled too many people"), false);
                                    QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcAddElement("<size=25><b><color=yellow>user: </color></b><color=red>" + killer.gameObject.GetComponent<NicknameSync>().MyNick + "</color><color=yellow><b> got banned for teamkilling, dont be like this user please</b></color></size>", 20, Broadcast.BroadcastFlags.Normal);
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
                    QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(victim.gameObject.GetComponent<NetworkIdentity>().connectionToClient, "<size=25><b><color=yellow>You have been teamkilled but FriendlyFireAutoban is disabled by an admin, reports regarding this teamkill will not be handled</color></b></size>", 10, Broadcast.BroadcastFlags.Normal);
                }
            }
        }
        public IEnumerator<float> RemoveVictim(string victim, CharacterClassManager victimccm)
        {
            yield return Timing.WaitForSeconds(30f);
            _victims.Remove(victim);
            Initializer.Logger.Debug("FFA", "forgive timeout");
        }
        public void ConsoleCommand(SendingConsoleCommandEventArgs ev)
        {
            switch (ev.Name.ToUpper())
            {
                case "FORGIVE":
                    if (_victims.ContainsKey(ev.Player.ReferenceHub.characterClassManager.UserId))
                    {
                        string killeruid;
                        _victims.TryGetValue(ev.Player.ReferenceHub.characterClassManager.UserId, out killeruid);
                        ReferenceHub hub1 = null;
                        Dictionary<GameObject, ReferenceHub> hubs = ReferenceHub.GetAllHubs();
                        foreach (KeyValuePair<GameObject, ReferenceHub> keyvalue in hubs)
                        {
                            ReferenceHub hub = keyvalue.Value;
                            if (hub.characterClassManager.UserId == killeruid)
                            {
                                hub1 = hub;
                            }
                        }
                        ev.Color = "yellow";
                        if (hub1 != null)
                        {
                            ev.ReturnMessage = "You have forgiven " + hub1.nicknameSync.MyNick + "(" +
                                               hub1.characterClassManager.UserId + ").";
                            Timing.KillCoroutines("timer");
                            _victims.Remove(ev.Player.ReferenceHub.characterClassManager.UserId);
                            ev.Player.Broadcast(10,
                                "<color=yellow>You have been forgiven by " + ev.Player.Nickname + "(" +
                                ev.Player.UserId + ")" + ".</color>");
                            _badguylist.Remove(hub1.characterClassManager.UserId);
                        }
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