using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.Addons.Audio;
using CedMod.Addons.QuerySystem;
using CedMod.Addons.Sentinal;
using CedMod.Addons.Sentinal.Patches;
using CedMod.Components;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using LabApi.Features.Permissions;
using MEC;
using Mirror;
using Newtonsoft.Json;
using NorthwoodLib.Pools;
using RemoteAdmin.Communication;

namespace CedMod.Handlers
{
    public class Server: CustomEventsHandler
    {
        public static Dictionary<ReferenceHub, ReferenceHub> reported = new Dictionary<ReferenceHub, ReferenceHub>();
        public static string ReportAdditionalText = "";

        public override void OnServerRoundRestarted()
        {
            FpcServerPositionDistributorPatch.VisiblePlayers.Clear();
            SentinalBehaviour.GunshotSource.Clear();
            SentinalBehaviour.Stalking106.Clear();
            Scp939LungePatch.LungeTime.Clear();
            FriendlyFireAutoban.Teamkillers.Clear();
            FriendlyFireAutoban.AdminDisabled = false;
            foreach (var fake in AudioCommand.FakeConnections)
            {
                NetworkServer.Destroy(fake.Value.gameObject);
            }
            AudioCommand.FakeConnections.Clear();
            AudioCommand.FakeConnectionsIds.Clear();
            RemoteAdminModificationHandler.IngameUserPreferencesMap.Clear();
            RemoteAdminModificationHandler.Singleton.Requesting.Clear();
            Task.Run(() =>
            {
                lock (BanSystem.CachedStates)
                {
                    BanSystem.CachedStates.Clear();
                }
            });
            BanSystem.Authenticating.Clear();
            VoicePacketPacket.Floats.Clear();
            VoicePacketPacket.OpusDecoders.Clear();
        }

        public override void OnPlayerRequestingRaPlayerList(PlayerRequestingRaPlayerListEventArgs ev)
        {
            if (ev.Player.HasPermission(PlayerPermissions.AdminChat))
            {
                if (!RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(ev.Player) && !RemoteAdminModificationHandler.Singleton.Requesting.Contains(ev.Player.UserId))
                {
                    RemoteAdminModificationHandler.Singleton.ResolvePreferences(ev.Player, null);
                }
                
                if (RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(ev.Player) && RemoteAdminModificationHandler.IngameUserPreferencesMap[ev.Player].ShowReportsInRemoteAdmin)
                {
                    var openCount = RemoteAdminModificationHandler.ReportsList.Count(s => s.Status == HandleStatus.NoResponse);
                    var inProgressCount = RemoteAdminModificationHandler.ReportsList.Count(s => s.Status == HandleStatus.InProgress);

                    if (openCount == 0)
                    {
                        ev.ListBuilder.Append("<size=0>(").Append(-1).Append(")</size>");
                        ev.ListBuilder.Append("<color=green>[No Open Reports]</color>");
                    }
                    else
                    {
                        ev.ListBuilder.Append("<size=0>(").Append(-1).Append(")</size>");
                        ev.ListBuilder.Append($"{(RemoteAdminModificationHandler.UiBlink ? "[<color=yellow>⚠</color>] " : " ")}<color=red>[{openCount} Open Report{(openCount == 1 ? "" : "s")}]</color>");
                    }

                    ev.ListBuilder.AppendLine();

                    if (inProgressCount == 0)
                    {
                        ev.ListBuilder.Append("<size=0>(").Append(-2).Append(")</size>");
                        ev.ListBuilder.Append("<color=green>[No InProgress Reports]</color>");
                    }
                    else
                    {
                        ev.ListBuilder.Append("<size=0>(").Append(-2).Append(")</size>");
                        ev.ListBuilder.Append($"{(RemoteAdminModificationHandler.UiBlink ? "[<color=yellow>⚠</color>] " : " ")}<color=orange>[{inProgressCount} Report{(inProgressCount == 1 ? "" : "s")} Inprogress]</color>");
                    }

                    ev.ListBuilder.AppendLine();
                }
            }
            
            base.OnPlayerRequestingRaPlayerList(ev);
        }
        
        public override void OnPlayerRaPlayerListAddingPlayer(PlayerRaPlayerListAddingPlayerEventArgs ev)
        {
            if (RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(ev.Player) && RemoteAdminModificationHandler.IngameUserPreferencesMap[ev.Player].ShowWatchListUsersInRemoteAdmin)
            {
                if (RemoteAdminModificationHandler.GroupWatchlist.Any(s => s.UserIds.Contains(ev.Player.UserId)))
                {
                    if (RemoteAdminModificationHandler.GroupWatchlist.Count(s => s.UserIds.Contains(ev.Player.UserId)) >= 2)
                    {
                        ev.Prefix += $"<size=15><color=#00FFF6>[WMG{RemoteAdminModificationHandler.GroupWatchlist.Count(s => s.UserIds.Contains(ev.Player.UserId))}]</color></size> ";
                    }
                    else
                    {
                        ev.Prefix += $"<size=15><color=#00FFF6>[WG{RemoteAdminModificationHandler.GroupWatchlist.FirstOrDefault(s => s.UserIds.Contains(ev.Player.UserId)).Id}]</color></size> ";
                    }
                }
                else if (RemoteAdminModificationHandler.Watchlist.Any(s => s.Userid == ev.Player.UserId))
                {
                    ev.Prefix += $"<size=15><color=#00FFF6>[WL]</color></size> ";
                }
            }
            base.OnPlayerRaPlayerListAddingPlayer(ev);
        }

        public override void OnPlayerRequestedCustomRaInfo(PlayerRequestedCustomRaInfoEventArgs ev)
        {
            if (!ev.SelectedIdentifiers[0].StartsWith("-1") && !ev.SelectedIdentifiers[0].StartsWith("-2"))
                return;
            
            bool inProgress = false;
            
            if (ev.SelectedIdentifiers[0].StartsWith("-2"))
            {
                inProgress = true;
            }
            
            ev.InfoBuilder.Append($"<color=white>");
            var reports = RemoteAdminModificationHandler.ReportsList.Where(s => s.Status == (inProgress ? HandleStatus.InProgress : HandleStatus.NoResponse)).ToList();
            if (reports.Count == 0)
            {
                ev.InfoBuilder.AppendLine("<color=green>There are currently no Unhandled Reports</color>");
                int count = 16;
                while (count >= 1)
                {
                    count--;
                    ev.InfoBuilder.AppendLine($"");
                }

                ev.InfoBuilder.AppendLine($"The text below serve as an indicator to what the buttons below the text do.");
                if (!inProgress)
                    ev.InfoBuilder.AppendLine($"<color=red>Next Report</color>            <color=red>Previous Report</color>            <color=red>Claim Report</color>              <color=red>Ignore Report</color>");
                else
                    ev.InfoBuilder.AppendLine($"<color=red>Next Report</color>            <color=red>Previous Report</color>          <color=red>Complete Report</color>         <color=red>Ignore Report</color>");

                ev.InfoBuilder.Append("</color>");
                base.OnPlayerRequestedCustomRaInfo(ev);
                return;
            }
            
            bool canForward = true;
            bool canBackward = true;
            bool first = false;
            
            if (inProgress && !RemoteAdminModificationHandler.ReportInProgressState.ContainsKey(ev.Player))
            {
                RemoteAdminModificationHandler.ReportInProgressState.Add(ev.Player, new Tuple<int, DateTime>(reports.FirstOrDefault().Id, DateTime.UtcNow));
                first = true;
            }
            else if (!inProgress && !RemoteAdminModificationHandler.ReportUnHandledState.ContainsKey(ev.Player))
            {
                RemoteAdminModificationHandler.ReportUnHandledState.Add(ev.Player, new Tuple<int, DateTime>(reports.FirstOrDefault().Id, DateTime.UtcNow));
                first = true;
            }
            
            var currentReport = inProgress ? RemoteAdminModificationHandler.ReportInProgressState[ev.Player] : RemoteAdminModificationHandler.ReportUnHandledState[ev.Player];
            var report = reports.FirstOrDefault(s => s.Id == currentReport.Item1);
            if (report == null)
                report = reports.FirstOrDefault();
            
            if (report != null)
            {
                int index = reports.IndexOf(report);
                int targetIndex = index;
                if (!ev.IsSensitiveInfo)
                    targetIndex++;
                else
                    targetIndex--;

                if (first)
                    targetIndex = index;

                if (reports.Count == 1)
                {
                    canBackward = false;
                    canForward = false;
                }

                if (index == 0)
                    canBackward = false;

                if (index + 1 == reports.Count)
                    canForward = false;
                
                if (reports.Count > targetIndex)
                {
                    if ((ev.IsSensitiveInfo && canForward) || (!ev.IsSensitiveInfo && canBackward))
                        report = reports[targetIndex];
                }

                index = reports.IndexOf(report);
                if (index == 0)
                    canBackward = false;
                else
                    canBackward = true;

                if (index + 1 == reports.Count)
                    canForward = false;
                else
                    canForward = true;

                ev.InfoBuilder.AppendLine($"Report {index + 1} out of {reports.Count}");
                
                if (inProgress)
                    RemoteAdminModificationHandler.ReportInProgressState[ev.Player] = new Tuple<int, DateTime>(report.Id, DateTime.UtcNow);
                else 
                    RemoteAdminModificationHandler.ReportUnHandledState[ev.Player] = new Tuple<int, DateTime>(report.Id, DateTime.UtcNow);
                
                ev.InfoBuilder.AppendLine($"Report: {report.Id}");
                
                if (report.IsCheatReport)
                {
                    ev.InfoBuilder.AppendLine($"<color=yellow>Warning: This report is marked as Cheater report, this means that NorthWood was notified of this report.\nIf this report is an actual cheating report please do not interfere with NorthWood global moderators so they can effectively ban cheaters.</color>");
                }
                
                var reporter = CedModPlayer.Get(report.ReporterId);
                if (reporter != null)
                {
                    ev.InfoBuilder.AppendLine($"Reporter: (<color=#43C6DB>{reporter.PlayerId}</color>) {reporter.Nickname} - {reporter.UserId} <color=green><link=CP_IP>\uF0C5</link></color>");
                    ev.SetClipboardText(reporter.UserId, reporter.UserId, 1);
                }
                else
                {
                    ev.InfoBuilder.AppendLine($"Reporter: <color=red>(Not Ingame)</color> - {report.ReporterId} <color=green><link=CP_IP>\uF0C5</link></color>");
                    ev.SetClipboardText(report.ReporterId, report.ReporterId, 1);
                }
                
                var reported = CedModPlayer.Get(report.ReportedId);
                if (reported != null)
                {
                    ev.InfoBuilder.AppendLine($"Reported: (<color=#43C6DB>{reported.PlayerId} <color=green><link=CP_ID>\uF0C5</link></color></color>) {reported.Nickname} - {reported.UserId} <color=green><link=CP_USERID>\uF0C5</link></color>");
                    ev.SetClipboardText(reported.UserId, reported.UserId, 0);
                    ev.SetClipboardText(reported.PlayerId.ToString(), reported.PlayerId.ToString(), 2);
                }
                else
                {
                    ev.InfoBuilder.AppendLine($"Reported: <color=red>(Not Ingame)</color> - {report.ReportedId} <color=green><link=CP_USERID>\uF0C5</link></color>");
                    ev.SetClipboardText(report.ReportedId, report.ReportedId, 0);
                }
                
                ev.InfoBuilder.AppendLine($"Reason: {report.Reason}");
                ev.InfoBuilder.AppendLine($"Ingame Report: {!report.IsDiscordReport}");
                ev.InfoBuilder.AppendLine(ReportAdditionalText);
                
                int count = report.IsCheatReport ? 6 : 7;
                while (count >= 1)
                {
                    count--;
                    ev.InfoBuilder.AppendLine($"");
                }
            }
            
            ev.InfoBuilder.AppendLine($"The text below serve as an indicator to what the buttons below the text do.");
            ev.InfoBuilder.AppendLine($"<color={(canForward ? "blue" : "red")}>Next Report</color>            <color={(canBackward ? "blue" : "red")}>Previous Report</color>            <color=blue>Claim Report</color>              <color=blue>Ignore Report</color>");
            ev.InfoBuilder.Append("</color>");
            base.OnPlayerRequestedCustomRaInfo(ev);
        }

        public override void OnPlayerRequestedRaPlayerInfo(PlayerRequestedRaPlayerInfoEventArgs ev)
        {
            if (ev.Player.HasPermissions("cedmod.requestdata"))
            {
                Timing.RunCoroutine(RequestData(ev, ev.InfoBuilder.ToString()));
            }
            base.OnPlayerRequestedRaPlayerInfo(ev);
        }

        private IEnumerator<float> RequestData(PlayerRequestedRaPlayerInfoEventArgs ev, string data)
        {
            ev.Player.ReferenceHub.queryProcessor.SendToClient(string.Format("${0} {1}", 1, $"{data}\n<color=green>Loading from CedMod API, please wait...</color>"), true, true, string.Empty);
            var pam = ev.Target.ReferenceHub.authManager;
            var con = ev.Player.ConnectionToClient;
            
            var info = StringBuilderPool.Shared.Rent("\n");
            string respString = "";
            var code = HttpStatusCode.OK;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                var t = VerificationChallenge.AwaitVerification();
                yield return Timing.WaitUntilTrue(() => t.IsCompleted);
                client.DefaultRequestHeaders.Add("ApiKey", CedModMain.Singleton.Config.CedMod.CedModApiKey);
                var respTask = client.SendAsync(new HttpRequestMessage(HttpMethod.Options, $"http{(QuerySystem.UseSSL ? "s" : "")}://" + API.APIUrl + $"/Auth/{pam.UserId}&{con.address}?banLists={string.Join(",", ServerPreferences.Prefs.BanListWriteBans.Select(s => s.Id))}&banListMutes={string.Join(",", ServerPreferences.Prefs.BanListReadMutes.Select(s => s.Id))}&banListWarns={string.Join(",", ServerPreferences.Prefs.BanListReadWarns.Select(s => s.Id))}"));
                yield return Timing.WaitUntilTrue(() => respTask.IsCompleted);
                var resp = respTask.Result;
                var respStringTask = resp.Content.ReadAsStringAsync();
                yield return Timing.WaitUntilTrue(() => respStringTask.IsCompleted);
                respString = respStringTask.Result;
                code = resp.StatusCode;
            }

            try
            {
                if (code != HttpStatusCode.OK)
                {
                    Logger.Error($"Failed to RequestData CMAPI: {code} | {respString}");
                }
                else
                {
                    Dictionary<string, object> cmData = JsonConvert.DeserializeObject<Dictionary<string, object>>(respString);
                    info.Append("\n<color=#D4AF37>CedMod Added Fields:</color>");
                    if (cmData["antiVpnWhitelist"] is bool ? (bool)cmData["antiVpnWhitelist"] : false)
                        info.Append("\nActive flag: <color=#008080>AntiVpn Whitelisted</color>");

                    if (cmData["triggeredAvpnPast"] is bool ? (bool)cmData["triggeredAvpnPast"] : false)
                        info.Append("\nActive flag: <color=#red>AntiVpn Triggered</color>");

                    info.Append($"\nModeration: {cmData["warns"]} Warnings, {cmData["banLogs"]} Banlogs");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            ev.Player.ReferenceHub.queryProcessor.SendToClient(string.Format("${0} {1}", 1, $"{data}\n<color=green>Loading from Panel API, please wait...</color>"), true, true, string.Empty);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                var t = VerificationChallenge.AwaitVerification();
                yield return Timing.WaitUntilTrue(() => t.IsCompleted);
                client.DefaultRequestHeaders.Add("ApiKey", CedModMain.Singleton.Config.CedMod.CedModApiKey);
                var respTask = client.SendAsync(new HttpRequestMessage(HttpMethod.Options, $"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Api/v3/RequestData/{QuerySystem.QuerySystemKey}/{pam.UserId}"));
                yield return Timing.WaitUntilTrue(() => respTask.IsCompleted);
                var resp = respTask.Result;
                var respStringTask = resp.Content.ReadAsStringAsync();
                yield return Timing.WaitUntilTrue(() => respStringTask.IsCompleted);
                respString = respStringTask.Result;
                code = resp.StatusCode;
            }

            try
            {
                if (code != HttpStatusCode.OK)
                {
                    Logger.Error($"Failed to RequestData PanelAPI: {code} | {respString}");
                }
                else
                {
                    Dictionary<string, object> cmData = JsonConvert.DeserializeObject<Dictionary<string, object>>(respString);

                    if (!pam.DoNotTrack)
                    {
                        info.Append($"\nActivity in the last 14 days: {TimeSpan.FromMinutes(cmData["activityLast14"] is double ? (double)cmData["activityLast14"] : 0).TotalHours}. Level {cmData["level"]} ({cmData["experience"]} Exp)");
                    }

                    if (cmData["panelUser"].ToString() != "")
                        info.Append($"\nPanelUser: {cmData["panelUser"]}");

                    info.Append($"\nPossible Alt Accounts: {cmData["usersFound"]}");
                    if (RemoteAdminModificationHandler.GroupWatchlist.Any(s => s.UserIds.Contains(pam.UserId)))
                    {
                        if (RemoteAdminModificationHandler.GroupWatchlist.Count(s => s.UserIds.Contains(pam.UserId)) >= 2)
                        {
                            info.Append($"\n<color=#00FFF6>User is in {RemoteAdminModificationHandler.GroupWatchlist.Count(s => s.UserIds.Contains(pam.UserId))} Watchlist groups.\nUse ExternalLookup to view details</color>");
                        }
                        else
                        {
                            var group = RemoteAdminModificationHandler.GroupWatchlist.FirstOrDefault(s => s.UserIds.Contains(pam.UserId));
                            info.Append($"\n<color=#00FFF6>User is in Group Watchlist: {group.GroupName} ({group.Id}) \n{group.Reason} Members:</color>");
                            foreach (var member in group.UserIds.Take(4))
                            {
                                var plr = CedModPlayer.Get(member);
                                info.Append($"\n<color=#00FFF6>{(plr == null ? "Not Ingame" : $"{plr.PlayerId} - {plr.UserId}")}</color>");
                            }

                            info.Append($"\n<color=#00FFF6>Use ExternalLookup for more info</color>");
                        }
                    }
                    else if (RemoteAdminModificationHandler.Watchlist.Any(s => s.Userid == pam.UserId))
                    {
                        info.Append($"\n<color=#00FFF6>User is on watchlist:\n{RemoteAdminModificationHandler.Watchlist.FirstOrDefault(s => s.Userid == pam.UserId).Reason}\nUse ExternalLookup for more info</color>");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            data += StringBuilderPool.Shared.ToStringReturn(info);
            ev.Player.ReferenceHub.queryProcessor.SendToClient(data, true, true, string.Empty);
        }
    }
}