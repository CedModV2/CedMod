using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CedMod.INIT;
using Exiled.Events.EventArgs;
using GameCore;
using Hints;
using MEC;
using RemoteAdmin;

namespace CedMod
{
    public static class FriendlyFireAutoban
    {
        public static Dictionary<string, int> Teamkillers = new Dictionary<string, int>();
        public static Dictionary<string, string> Victims = new Dictionary<string, string>();
        public static bool AdminDisabled = false;
        public static void HandleKill(DyingEventArgs ev)
        {
            Initializer.Logger.Debug("FFA", "Check 1");
            if (RoundSummary.RoundInProgress() == false && ConfigFile.ServerConfig.GetBool("ffa_enable") == false && AdminDisabled == true)
                return;
            Initializer.Logger.Debug("FFA", "Check 2");
            if (ev.Killer == ev.Target)
                return;
            Initializer.Logger.Debug("FFA", "Check 3");
            if (!IsTeakill(ev))
                return;
            string ffatext1 = "<size=25><b><color=yellow>You teamkilled: </color></b><color=red>" +
                              ev.Target.ReferenceHub.gameObject.GetComponent<NicknameSync>().MyNick +
                              "</color><color=yellow><b> If you continue teamkilling it will result in a ban</b></color></size>";
            ev.Killer.ReferenceHub.hints.Show(new TextHint(ffatext1
                ,
                new HintParameter[] {new StringHintParameter("")}, null, 20f));
            ev.Killer.SendConsoleMessage(ffatext1, "white");
            string ffatext = string.Concat(
                "<size=25><b><color=yellow>You have been teamkilled by: </color></b></size>",
                "<color=red><size=25>", ev.Killer.ReferenceHub.gameObject.GetComponent<NicknameSync>().MyNick, " (",
                ev.Killer.ReferenceHub.gameObject.GetComponent<CharacterClassManager>().UserId,
                "), " + ev.Killer.ReferenceHub.characterClassManager.CurClass + " You were a " + ev.Target.ReferenceHub.characterClassManager.CurClass + " </size></color>" +
                Environment.NewLine,
                "<size=25><b><color=yellow> Use this as a screenshot as evidence for a report " +
                Environment.NewLine +
                "</color></b></size>",
                "<size=25><i><color=yellow> Note: if he continues to teamkill the server will ban him</color></i></size>");
            ev.Target.ReferenceHub.hints.Show(new TextHint(ffatext,
                new HintParameter[] {new StringHintParameter("")}, null, 20f));
            ev.Target.SendConsoleMessage(ffatext, "white");
            if (!Victims.ContainsKey(ev.Target.UserId))
            {
                Victims.Add(ev.Target.UserId, ev.Killer.UserId);
                //Timing.RunCoroutine(RemoveVictim(victim.UserId, victim), "timer" + victim.UserId);
            }

            if (Teamkillers.ContainsKey(ev.Killer.UserId))
                Teamkillers[ev.Killer.UserId]++;
            else
                Teamkillers.Add(ev.Killer.UserId, 1);
            foreach (KeyValuePair<string, int> s in Teamkillers)
            {
                Initializer.Logger.Debug("CMFFA" + s.Key, s.Value.ToString());
            }
            foreach (KeyValuePair<string, int> s in Teamkillers)
            {
                if (s.Key == ev.Killer.UserId)
                {
                    if (s.Value >= ConfigFile.ServerConfig.GetInt("ffa_ammountoftkbeforeban", 3))
                    {
                        Initializer.Logger.Info("FFA",
                            string.Concat("Player: ",
                                ev.Killer.Nickname, " ",
                                ev.Killer.ReferenceHub.queryProcessor.PlayerId.ToString(),
                                " ", ev.Killer.UserId,
                                " exeeded teamkill limit"));
                        Task.Factory.StartNew(() => { API.Ban(ev.Killer.ReferenceHub.gameObject,
                            ConfigFile.ServerConfig.GetInt("ffa_banduration", 4320),
                            "Server.Module.FriendlyFireAutoban",
                            ConfigFile.ServerConfig.GetString("ffa_banreason",
                                "You have teamkilled too many people"), false); });
                        QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcAddElement(
                            "<size=25><b><color=yellow>user: </color></b><color=red>" +
                            ev.Killer.Nickname +
                            "</color><color=yellow><b> got banned for teamkilling, dont be like this user please</b></color></size>",
                            20, Broadcast.BroadcastFlags.Normal);
                    }
                }
            }
        }

        public static bool IsTeakill(DyingEventArgs ev)
        {
            bool result = false;
            if (ev.Killer == ev.Target)
                return false;
            switch (ev.Killer.Team)
            {
                case Team.CDP when ev.Target.Team == Team.CHI:
                case Team.CHI when ev.Target.Team == Team.CDP:
                case Team.RSC when ev.Target.Team == Team.MTF:
                case Team.MTF when ev.Target.Team == Team.RSC:
                case Team.MTF when ev.Target.Team == Team.CDP && ev.Target.IsCuffed && !ConfigFile.ServerConfig.GetBool("ffa_killingdisarmedclassdsallowed"):
                case Team.CHI when ev.Target.Team == Team.RSC && ev.Target.IsCuffed && !ConfigFile.ServerConfig.GetBool("ffa_killingdisarmedscientistallowed"):
                    result = true;
                    break;
                case Team.CDP when ev.Target.Team == Team.CDP && !ConfigFile.ServerConfig.GetBool("ffa_dclassvsdclasstk", true):
                default:
                    result = false;
                    break;
            }
            if (ev.Killer.Team == ev.Target.Team)
                result = true;

            return result;
        }
    }
}