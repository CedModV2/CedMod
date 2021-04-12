using System.Collections.Generic;
using Exiled.Events;
using MEC;
using Exiled.Events.EventArgs;

namespace CedMod.Handlers
{
    public class Server
    {
        Dictionary<ReferenceHub, ReferenceHub> reported = new Dictionary<ReferenceHub, ReferenceHub>();
        public void OnReport(LocalReportingEventArgs ev)
        {
            if (CedModMain.config.ReportBlacklist.Contains(ev.Issuer.UserId))
            {
                ev.IsAllowed = false;
                ev.Issuer.SendConsoleMessage($"[REPORTING] You are banned from ingame reports", "green");
                return;
            }
            if (ev.Issuer.UserId == ev.Target.UserId)
            {
                ev.IsAllowed = false;
                ev.Issuer.SendConsoleMessage($"[REPORTING] You can't report yourself", "green");
                return;
            }
            if (reported.ContainsKey(ev.Target.ReferenceHub))
            {
                ev.IsAllowed = false;
                ev.Issuer.SendConsoleMessage($"[REPORTING] {ev.Target.Nickname} ({ev.Target.UserId}) has already been reported by {Exiled.API.Features.Player.Get(reported[ev.Target.ReferenceHub]).Nickname}", "green");
                return;
            }
            if (ev.Target.RemoteAdminAccess && !CedModMain.config.StaffReportAllowed)
            {
                ev.IsAllowed = false;
                ev.Issuer.SendConsoleMessage($"[REPORTING] " + CedModMain.config.StaffReportMessage, "green");
                return;
            }
            if (ev.Reason.IsEmpty())
            {
                ev.IsAllowed = false;
                ev.Issuer.SendConsoleMessage($"[REPORTING] You have to enter a reason", "green");
                return;
            }
            
            reported.Add(ev.Target.ReferenceHub, ev.Issuer.ReferenceHub);
            Timing.RunCoroutine(removefromlist(ev.Target.ReferenceHub));
        }

        public IEnumerator<float> removefromlist(ReferenceHub target)
        {
            yield return Timing.WaitForSeconds(60f);
            reported.Remove(target);
        }
        
        public void OnRoundRestart()
        {
            FriendlyFireAutoban.Teamkillers.Clear();
            FriendlyFireAutoban.Victims.Clear();
            Timing.KillCoroutines("LightsOut");
        }

        public void OnSendingRemoteAdmin(SendingRemoteAdminCommandEventArgs ev)
        {
            BanSystem.HandleRACommand(ev);
        }
    }
}