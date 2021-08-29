using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using MEC;
using Exiled.Events.EventArgs;
using Newtonsoft.Json;

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
            Timing.RunCoroutine(RemoveFromReportList(ev.Target.ReferenceHub));
            
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            if (GameCore.ConfigFile.ServerConfig.GetString("report_discord_webhook_url", "PleaseSetWebhookUrlHere") != "PleaseSetWebhookUrlHere")
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var hh = client.PostAsync(
                            GameCore.ConfigFile.ServerConfig.GetString("report_discord_webhook_url",
                                "PleaseSetWebhookUrlHere"),
                            new StringContent(JsonConvert.SerializeObject(new Dictionary<string, string>()
                                {{"content", CedModMain.config.ReportMessage}}), Encoding.Default, "application/json")).Result;
                        Log.Debug(hh.Content.ReadAsStringAsync().Result, CedModMain.config.ShowDebug);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                });
            }
        }

        public IEnumerator<float> RemoveFromReportList(ReferenceHub target)
        {
            yield return Timing.WaitForSeconds(60f);
            reported.Remove(target);
        }
        
        public void OnRoundRestart()
        {
            FriendlyFireAutoban.Teamkillers.Clear();
            Timing.KillCoroutines("LightsOut");
        }
    }
}