using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using GameCore;
using Hints;
using InventorySystem.Disarming;
using MEC;
using RemoteAdmin;
using Log = Exiled.API.Features.Log;

namespace CedMod
{
    public static class FriendlyFireAutoban
    {
        public static Dictionary<string, int> Teamkillers = new Dictionary<string, int>();
        public static Dictionary<string, string> Victims = new Dictionary<string, string>();
        public static bool AdminDisabled = false;
        public static void HandleKill(DyingEventArgs ev)
        {
            if (RoundSummary.RoundInProgress() == false) return;
            if (!CedModMain.config.AutobanEnabled) return;
            if (AdminDisabled) return;
            if (!IsTeakill(ev))
                return;
            string ffatext1 = $"<size=25><b><color=yellow>You teamkilled: </color></b><color=red> {ev.Target.Nickname} </color><color=yellow><b> If you continue teamkilling it will result in a ban</b></color></size>";
            ev.Killer.ReferenceHub.hints.Show(new TextHint(ffatext1, new HintParameter[] {new StringHintParameter("")}, null, 20f));
            ev.Killer.SendConsoleMessage(ffatext1, "white");
            string ffatext = $"<size=25><b><color=yellow>You have been teamkilled by: </color></b></size><color=red><size=25> {ev.Killer.Nickname} ({ev.Killer.UserId} {ev.Killer.ReferenceHub.characterClassManager.CurClass} You were a {ev.Target.ReferenceHub.characterClassManager.CurClass}</size></color>\n<size=25><b><color=yellow> Use this as a screenshot as evidence for a report</color></b></size><size=25><i><color=yellow> Note: if he continues to teamkill the server will ban him</color></i></size>";
            if (ev.Killer.DoNotTrack)
                ffatext = ffatext.Replace(ev.Killer.UserId, "DNT");
            
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
                if (s.Key == ev.Killer.UserId)
                {
                    if (s.Value >= CedModMain.config.AutobanThreshold)
                    {
                        Log.Info( $"Player: {ev.Killer.Nickname} {ev.Killer.ReferenceHub.queryProcessor.PlayerId.ToString()} {ev.Killer.UserId}  exeeded teamkill limit");
                        Task.Factory.StartNew(() =>
                        {
                            API.Ban(ev.Killer, CedModMain.config.AutobanDuration, "Server.Module.FriendlyFireAutoban", CedModMain.config.AutobanReason);
                        });
                        Map.Broadcast(20,
                            $"<size=25><b><color=yellow>user: </color></b><color=red> {ev.Killer.Nickname} </color><color=yellow><b> got banned for teamkilling, dont be like this user please</b></color></size>", Broadcast.BroadcastFlags.Normal);
                    }
                }
            }
        }

        public static bool IsTeakill(DyingEventArgs ev)
        {
            if (!RoundSummary.RoundInProgress())
                return false;
            bool result = false;
            if (ev.Killer == ev.Target)
                return false;
            switch (ev.Killer.Team)
            {
                case Team.CDP when ev.Target.Team == Team.CHI:
                case Team.CHI when ev.Target.Team == Team.CDP:
                case Team.RSC when ev.Target.Team == Team.MTF:
                case Team.MTF when ev.Target.Team == Team.RSC:
                case Team.MTF when ev.Target.Team == Team.CDP && ev.Target.Inventory.IsDisarmed() && CedModMain.config.AutobanDisarmedClassDTk:
                case Team.CHI when ev.Target.Team == Team.RSC && ev.Target.Inventory.IsDisarmed() && CedModMain.config.AutobanDisarmedScientistDTk:
                    result = true;
                    break;
                case Team.CDP when ev.Target.Team == Team.CDP && CedModMain.config.AutobanClassDvsClassD:
                default:
                    result = false;
                    break;
            }
            if (ev.Killer.Team == ev.Target.Team)
                result = true;
            if (ev.Killer.Team == Team.TUT)
                result = false;

            return result;
        }
    }
}