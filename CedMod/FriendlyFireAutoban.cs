using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Hints;
using InventorySystem.Disarming;
using Log = Exiled.API.Features.Log;

namespace CedMod
{
    public static class FriendlyFireAutoban
    {
        public static Dictionary<string, int> Teamkillers = new Dictionary<string, int>();
        public static bool AdminDisabled = false;
        public static void HandleKill(DyingEventArgs ev)
        {
            if (!RoundSummary.RoundInProgress() || !CedModMain.config.AutobanEnabled || AdminDisabled || !IsTeamKill(ev))
                return;
            
            string ffaTextKiller = $"<size=25><b><color=yellow>You teamkilled: </color></b><color=red> {ev.Target.Nickname} </color><color=yellow><b> If you continue teamkilling it will result in a ban</b></color></size>";
            ev.Killer.ReferenceHub.hints.Show(new TextHint(ffaTextKiller, new HintParameter[] {new StringHintParameter("")}, null, 20f));
            ev.Killer.SendConsoleMessage(ffaTextKiller, "white");
            string ffaTextVictim = $"<size=25><b><color=yellow>You have been teamkilled by: </color></b></size><color=red><size=25> {ev.Killer.Nickname} ({ev.Killer.UserId} {ev.Killer.ReferenceHub.characterClassManager.CurClass} You were a {ev.Target.ReferenceHub.characterClassManager.CurClass}</size></color>\n<size=25><b><color=yellow> Use this as a screenshot as evidence for a report</color></b>\n{CedModMain.config.AutobanExtraMessage}\n</size><size=25><i><color=yellow> Note: if they continues to teamkill the server will ban them</color></i></size>";
            if (ev.Killer.DoNotTrack)
                ffaTextVictim = ffaTextVictim.Replace(ev.Killer.UserId, "DNT");
            
            ev.Target.ReferenceHub.hints.Show(new TextHint(ffaTextVictim,
                new HintParameter[] {new StringHintParameter("")}, null, 20f));
            ev.Target.SendConsoleMessage(ffaTextVictim, "white");
    
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
                        Log.Info( $"Player: {ev.Killer.Nickname} {ev.Killer.ReferenceHub.queryProcessor.PlayerId.ToString()} {ev.Killer.UserId} exceeded teamkill limit");
                        Task.Factory.StartNew(() =>
                        {
                            API.Ban(ev.Killer, (long) TimeSpan.FromMinutes(CedModMain.config.AutobanDuration).TotalSeconds, "Server.Module.FriendlyFireAutoban", CedModMain.config.AutobanReason);
                        });
                        Map.Broadcast(20, $"<size=25><b><color=yellow>user: </color></b><color=red> {ev.Killer.Nickname} </color><color=yellow><b> has been automatically banned for teamkilling</b></color></size>", Broadcast.BroadcastFlags.Normal);
                    }
                }
            }
        }

        public static bool IsTeamKill(DyingEventArgs ev)
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
                case Team.CDP when ev.Target.Team == Team.CDP && CedModMain.config.AutobanClassDvsClassD:
                    result = true;
                    break;
                case Team.CDP when ev.Target.Team == Team.CDP && !CedModMain.config.AutobanClassDvsClassD:
                default:
                    result = false;
                    break;
            }
            if (ev.Killer.Team == ev.Target.Team && (ev.Killer.Team != Team.CDP && ev.Target.Team != Team.CDP))
                result = true;
            if (ev.Killer.Team == Team.TUT)
                result = false;

            return result;
        }
    }
}