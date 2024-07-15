using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using CedMod.Components;
using CommandSystem.Commands.RemoteAdmin;
using HarmonyLib;
using MEC;
using Mirror;
using Newtonsoft.Json;
using NorthwoodLib.Pools;
#if !EXILED
using NWAPIPermissionSystem;
#else
using Exiled.Permissions.Extensions;
#endif
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Events;
using RemoteAdmin;
using RemoteAdmin.Communication;
using UnityEngine;
using UnityEngine.Networking;
using Utils;
using VoiceChat;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(RaPlayer), nameof(RaPlayer.ReceiveData), new Type[] { typeof(CommandSender), typeof(string) })]
    public static class RaPlayerPatch
    {
        public static Dictionary<string, CoroutineHandle> Handles = new Dictionary<string, CoroutineHandle>();

        public static bool Prefix(RaPlayer __instance, CommandSender sender, string data)
        {
            if (Handles.ContainsKey(sender.SenderId) && Handles[sender.SenderId].IsRunning)
                return false;

            Handles[sender.SenderId] = Timing.RunCoroutine(RaPlayerCoRoutine(__instance, sender, data));
            return false;
        }

        public static IEnumerator<float> RaPlayerCoRoutine(RaPlayer __instance, CommandSender sender, string data)
        {
            string[] spData = data.Split(' ');
			if (spData.Length != 2) 
                yield break;

            int result;
            if (spData.Length != 2 || !int.TryParse(spData[0], out result))
                yield break;
            
			bool isShort = result == 1;

			var playerSender = sender as PlayerCommandSender;

            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug($"Received: {data} : {spData[1]}");
            var cplayer = CedModPlayer.Get(sender.SenderId);

            if (spData[1].StartsWith("-1") && CommandProcessor.CheckPermissions(sender, PlayerPermissions.AdminChat))
            {
                Timing.RunCoroutine(HandleReportType1(sender, cplayer, spData));
                yield break;
            }

            if (spData[1].StartsWith("-2") && CommandProcessor.CheckPermissions(sender, PlayerPermissions.AdminChat))
            {
                Timing.RunCoroutine(HandleReportType2(sender, cplayer, spData));
                yield break;
            }
            
			if (!isShort && playerSender != null && !playerSender.ReferenceHub.authManager.RemoteAdminGlobalAccess && !playerSender.ReferenceHub.authManager.BypassBansFlagSet && !CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayerSensitiveDataAccess))
                yield break;

			ArraySegment<string> playersData = new ArraySegment<string>(spData.Skip(1).ToArray());

			var players = RAUtils.ProcessPlayerIdOrNamesList(playersData, 0, out _);

			//Players not found
			if (players.Count == 0)
                yield break;

			bool userIdPerms;

			if (playerSender != null && playerSender.ReferenceHub.authManager.NorthwoodStaff)
				userIdPerms = true;
			else
				userIdPerms = PermissionsHandler.IsPermitted(sender.Permissions, ServerRoles.UserIdPerms);

			//Multiple select
			if (players.Count > 1)
			{
				var info2 = StringBuilderPool.Shared.Rent();
				info2.AppendFormat("${0} ", __instance.DataId);
				info2.Append("<color=white>Multiple players selected:");
				info2.Append("\nPlayer ID: <color=green><link=CP_ID>\uf0c5</link></color>");
				info2.AppendFormat("\nIP Address: {0}", !isShort ? "<color=green><link=CP_IP>\uf0c5</link></color>" : "[REDACTED]");
				info2.AppendFormat("\nUser ID: {0}", userIdPerms ? "<color=green><link=CP_USERID>\uf0c5</link></color>" : "[REDACTED]");
				info2.Append("</color>");

				StringBuilder ids = StringBuilderPool.Shared.Rent();
				StringBuilder ips = !isShort ? StringBuilderPool.Shared.Rent() : null;
				StringBuilder userids = userIdPerms ? StringBuilderPool.Shared.Rent() : null;

				foreach (ReferenceHub item in players)
				{
					ids.Append(item.PlayerId);
					ids.Append(", ");

					if (!isShort)
					{
						ServerLogs.AddLog(ServerLogs.Modules.DataAccess, $"{sender.LogName} accessed IP address of player {item.PlayerId} ({item.nicknameSync.MyNick}).", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);

						ips.Append(item.networkIdentity.connectionToClient.address);
						ips.Append(", ");
					}

					if (userIdPerms)
					{
						userids.Append(item.authManager.UserId);
						userids.Append(", ");
					}
				}

				RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId, StringBuilderPool.Shared.ToStringReturn(ids));
				RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, ips == null ? string.Empty : StringBuilderPool.Shared.ToStringReturn(ips));
				RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, userids == null ? string.Empty : StringBuilderPool.Shared.ToStringReturn(userids));

				sender.RaReply(StringBuilderPool.Shared.ToStringReturn(info2), true, true, string.Empty);
                yield break;;
			}

			var player = players[0];

			var gameplayData = PermissionsHandler.IsPermitted(sender.Permissions, PlayerPermissions.GameplayData);

			var ccm = player.characterClassManager;
			var pam = player.authManager;
			var nms = player.nicknameSync;
			var con = player.networkIdentity.connectionToClient;
			var sr = player.serverRoles;

			if (sender is PlayerCommandSender commandSender)
				commandSender.ReferenceHub.queryProcessor.GameplayData = gameplayData;

			var info = StringBuilderPool.Shared.Rent();
			info.AppendFormat("${0} ", __instance.DataId);
			info.AppendFormat("<color=white>Nickname: {0}", nms.CombinedName);
			info.AppendFormat("\nPlayer ID: {0} <color=green><link=CP_ID>\uf0c5</link></color>", player.PlayerId);
			RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId, $"{player.PlayerId}");
			if (con == null)
			{
				RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, string.Empty);
				info.Append("\nIP Address: null");
			}
			else if (!isShort)
			{
				ServerLogs.AddLog(ServerLogs.Modules.DataAccess, $"{sender.LogName} accessed IP address of player {player.PlayerId} ({player.nicknameSync.MyNick}).", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);

				string address = con.address;

				info.AppendFormat("\nIP Address: {0} ", address);
				RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, address);

				if (con.IpOverride != null)
					info.AppendFormat(" [routed via {0}]", con.OriginalIpAddress);

				info.Append(" <color=green><link=CP_IP>\uf0c5</link></color>");
			}
			else
			{
				RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, string.Empty);
				info.Append("\nIP Address: [REDACTED]");
			}

			info.Append("\nUser ID: ");
			if (userIdPerms)
			{
				if (string.IsNullOrEmpty(pam.UserId))
					info.Append("(none)");
				else
					info.AppendFormat("{0} <color=green><link=CP_USERID>\uf0c5</link></color>", pam.UserId);

				RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, pam.UserId ?? string.Empty);
				if (pam.SaltedUserId != null && pam.SaltedUserId.Contains("$", StringComparison.Ordinal))
					info.AppendFormat("\nSalted User ID: {0}", pam.SaltedUserId);
			}
			else
			{
				info.Append("<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>");
				RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, string.Empty);
			}

			info.Append("\nServer role: ");
			info.Append(sr.GetColoredRoleString());

			bool vhb = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenBadges);
			bool ghb = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenGlobalBadges);

			if (playerSender != null)
			{
				vhb = true;
				ghb = true;
			}

			bool hidden = !string.IsNullOrEmpty(sr.HiddenBadge);
			bool show = !hidden || sr.GlobalHidden && ghb || !sr.GlobalHidden && vhb;

			if (show)
			{
				if (hidden)
				{
					info.AppendFormat("\n<color=#DC143C>Hidden role: </color>{0}", sr.HiddenBadge);
					info.AppendFormat("\n<color=#DC143C>Hidden role type: </color>{0}", (sr.GlobalHidden ? "GLOBAL" : "LOCAL"));
				}

				if (player.authManager.RemoteAdminGlobalAccess)
					info.Append("\nStudio Status: <color=#BCC6CC>Studio GLOBAL Staff (management or global moderation)</color>");
				else if (player.authManager.NorthwoodStaff)
					info.Append("\nStudio Status: <color=#94B9CF>Studio Staff</color>");
			}

			VcMuteFlags muteFlags = VoiceChatMutes.GetFlags(players[0]);

			if (muteFlags != 0)
			{
				info.Append("\nMUTE STATUS:");

				foreach (VcMuteFlags flag in EnumUtils<VcMuteFlags>.Values)
				{
					if (flag == 0 || (muteFlags & flag) != flag)
						continue;

					info.Append(" <color=#F70D1A>");
					info.Append(flag);
					info.Append("</color>");
				}
			}

			info.Append("\nActive flag(s):");

			if (ccm.GodMode)
				info.Append(" <color=#659EC7>[GOD MODE]</color>");

			if (player.playerStats.GetModule<AdminFlagsStat>().HasFlag(AdminFlags.Noclip))
				info.Append(" <color=#DC143C>[NOCLIP ENABLED]</color>");
			else if (FpcNoclip.IsPermitted(player))
				info.Append(" <color=#E52B50>[NOCLIP UNLOCKED]</color>");

			if (pam.DoNotTrack)
				info.Append(" <color=#BFFF00>[DO NOT TRACK]</color>");

			if (sr.BypassMode)
				info.Append(" <color=#BFFF00>[BYPASS MODE]</color>");

			if (show && sr.RemoteAdmin)
				info.Append(" <color=#43C6DB>[RA AUTHENTICATED]</color>");

			if (sr.IsInOverwatch)
				info.Append(" <color=#008080>[OVERWATCH MODE]</color>");
			else if (gameplayData)
			{
				info.Append("\nClass: ").Append(PlayerRoleLoader.AllRoles.TryGetValue(player.GetRoleId(), out PlayerRoleBase cl) ? cl.RoleTypeId : "None");
				info.Append(" <color=#fcff99>[HP: ").Append(CommandProcessor.GetRoundedStat<HealthStat>(player)).Append("]</color>");
				info.Append(" <color=green>[AHP: ").Append(CommandProcessor.GetRoundedStat<AhpStat>(player)).Append("]</color>");
				info.Append(" <color=#977dff>[HS: ").Append(CommandProcessor.GetRoundedStat<HumeShieldStat>(player)).Append("]</color>");
				info.Append("\nPosition: ").Append(player.transform.position.ToPreciseString());
			}
			else
				info.Append("\n<color=#D4AF37>Some fields were hidden. GameplayData permission required.</color>");

			
            Log.Debug($"Has permissions: {sender.CheckPermission("cedmod.requestdata")}",
                CedModMain.Singleton.Config.QuerySystem.Debug);
            if (sender.CheckPermission("cedmod.requestdata"))
            {
                sender.RaReply(string.Format("${0} {1}", __instance.DataId, "Loading from CedMod API, please wait..."), true, true, string.Empty);
                string respString = "";
                var code = HttpStatusCode.OK;
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-ServerIp", Server.ServerIpAddress);
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
                        Log.Error($"Failed to RequestData CMAPI: {code} | {respString}");
                    }
                    else
                    {
                        Dictionary<string, object> cmData =
                            JsonConvert.DeserializeObject<Dictionary<string, object>>(respString);
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

                sender.RaReply(string.Format("${0} {1}", __instance.DataId, "Loading from Panel API, please wait..."), true, true, string.Empty);

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-ServerIp", Server.ServerIpAddress);
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
                        Log.Error(
                            $"Failed to RequestData PanelAPI: {code} | {respString}");
                    }
                    else
                    {
                        Dictionary<string, object> cmData = JsonConvert.DeserializeObject<Dictionary<string, object>>(respString);

                        if (!player.authManager.DoNotTrack)
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
            }
            
            info.Append("</color>");

			sender.RaReply(StringBuilderPool.Shared.ToStringReturn(info), true, true, string.Empty);
			RaPlayerQR.Send(sender, false, !userIdPerms || string.IsNullOrEmpty(pam.UserId) ? string.Empty : pam.UserId);
        }

        public static IEnumerator<float> HandleReportType1(CommandSender sender, CedModPlayer player, string[] source,
            string additional = "")
        {
            try
            {
                var open = RemoteAdminModificationHandler.ReportsList.Where(s => s.Status == HandleStatus.NoResponse)
                    .ToList();
                //report handling
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug($"Report handle");
                if (open.Count == 0)
                {
                    StringBuilder stringBuilder = StringBuilderPool.Shared.Rent($"<color=white>");
                    stringBuilder.AppendLine("<color=green>There are currently no Unhandled Reports</color>");
                    int count = 16;
                    while (count >= 1)
                    {
                        count--;
                        stringBuilder.AppendLine($"");
                    }

                    stringBuilder.AppendLine(
                        $"The text below serve as an indicator to what the buttons below the text do.");
                    stringBuilder.AppendLine(
                        $"<color=red>Next Report</color>            <color=red>Previous Report</color>            <color=red>Claim Report</color>              <color=red>Ignore Report</color>");

                    stringBuilder.Append("</color>");
                    sender.RaReply(
                        string.Format("${0} {1}", (object)1,
                            (object)StringBuilderPool.Shared.ToStringReturn(stringBuilder)), true, true, string.Empty);
                }
                else
                {
                    StringBuilder stringBuilder = StringBuilderPool.Shared.Rent($"<color=white>");
                    bool canForward = true;
                    bool canBackward = true;
                    bool first = false;

                    if (!RemoteAdminModificationHandler.ReportUnHandledState.ContainsKey(player))
                    {
                        first = true;
                        RemoteAdminModificationHandler.ReportUnHandledState.Add(player,
                            new Tuple<int, DateTime>(open.FirstOrDefault().Id, DateTime.UtcNow));
                    }

                    var currently = RemoteAdminModificationHandler.ReportUnHandledState[player];
                    var report = open.FirstOrDefault(s => s.Id == currently.Item1);
                    if (report == null)
                        report = open.FirstOrDefault();
                    if (report != null)
                    {
                        int index = open.IndexOf(report);
                        int targetIndex = index;
                        if (source[0] == "1")
                            targetIndex++;
                        else if (source[0] == "0")
                            targetIndex--;

                        if (first)
                            targetIndex = index;

                        if (open.Count == 1)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug($"Rept 1");
                            canBackward = false;
                            canForward = false;
                        }

                        if (index == 0)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug($"Rept 2");
                            canBackward = false;
                        }

                        if (index + 1 == open.Count)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug($"Rept 3");
                            canForward = false;
                        }

                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Log.Debug($"Rept 4 {index} {targetIndex}");

                        if (open.Count > targetIndex)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug($"Rept 5 {index} {targetIndex}");
                            if ((source[0] == "1" && canForward) || (source[0] == "0" && canBackward))
                                report = open[targetIndex];
                        }

                        index = open.IndexOf(report);
                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Log.Debug($"Rept 8 {index} {targetIndex}");

                        if (index == 0)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug($"Rept 6");
                            canBackward = false;
                        }
                        else
                        {
                            canBackward = true;
                        }

                        if (index + 1 == open.Count)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug($"Rept 7");
                            canForward = false;
                        }
                        else
                        {
                            canForward = true;
                        }

                        stringBuilder.AppendLine($"Report {index + 1} out of {open.Count}");
                    }

                    RemoteAdminModificationHandler.ReportUnHandledState[player] =
                        new Tuple<int, DateTime>(report.Id, DateTime.UtcNow);


                    stringBuilder.AppendLine($"Report: {report.Id}");

                    if (report.IsCheatReport)
                    {
                        stringBuilder.AppendLine(
                            $"<color=yellow>Warning: This report is marked as Cheater report, this means that NorthWood was notified of this report.\nIf this report is an actual cheating report please do not interfere with NorthWood global moderators so they can effectively ban cheaters.</color>");
                    }

                    var reporter = CedModPlayer.Get(report.ReporterId);
                    if (reporter != null)
                    {
                        stringBuilder.AppendLine(
                            $"Reporter: (<color=#43C6DB>{reporter.PlayerId}</color>) {reporter.Nickname} - {reporter.UserId} <color=green><link=CP_IP>\uF0C5</link></color>");
                        RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, reporter.UserId);
                    }
                    else
                    {
                        stringBuilder.AppendLine(
                            $"Reporter: <color=red>(Not Ingame)</color> - {report.ReporterId} <color=green><link=CP_IP>\uF0C5</link></color>");
                        RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, report.ReporterId);
                    }

                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug($"Checking list {string.Join(", ", Player.PlayersUserIds.Select(s => s.Key))}");

                    var reported = CedModPlayer.Get(report.ReportedId);
                    if (reported != null)
                    {
                        stringBuilder.AppendLine(
                            $"Reported: (<color=#43C6DB>{reported.PlayerId} <color=green><link=CP_ID>\uF0C5</link></color></color>) {reported.Nickname} - {reported.UserId} <color=green><link=CP_USERID>\uF0C5</link></color>");
                        RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, reported.UserId);
                        RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId, reported.PlayerId.ToString());
                    }
                    else
                    {
                        stringBuilder.AppendLine(
                            $"Reported: <color=red>(Not Ingame)</color> - {report.ReportedId} <color=green><link=CP_USERID>\uF0C5</link></color>");
                        RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, report.ReportedId);
                    }

                    stringBuilder.AppendLine($"Reason: {report.Reason}");
                    stringBuilder.AppendLine($"Ingame Report: {!report.IsDiscordReport}");

                    stringBuilder.AppendLine(additional);

                    int count = report.IsCheatReport ? 6 : 7;
                    while (count >= 1)
                    {
                        count--;
                        stringBuilder.AppendLine($"");
                    }

                    stringBuilder.AppendLine(
                        $"The text below serve as an indicator to what the buttons below the text do.");
                    stringBuilder.AppendLine(
                        $"<color={(canForward ? "blue" : "red")}>Next Report</color>            <color={(canBackward ? "blue" : "red")}>Previous Report</color>            <color=blue>Claim Report</color>              <color=blue>Ignore Report</color>");

                    stringBuilder.Append("</color>");
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug($"Report handle complete send");
                    sender.RaReply(
                        string.Format("${0} {1}", (object)1,
                            (object)StringBuilderPool.Shared.ToStringReturn(stringBuilder)), true, true, string.Empty);
                    yield break;
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                sender.RaReply(
                    string.Format("${0} {1}", (object)1,
                        (object)"An Exception occured while trying to display reports"), true, true, string.Empty);
            }
        }

        public static IEnumerator<float> HandleReportType2(CommandSender sender, CedModPlayer player, string[] source,
            string additional = "")
        {
            try
            {
                //report handling
                var inProgress = RemoteAdminModificationHandler.ReportsList
                    .Where(s => s.Status == HandleStatus.InProgress).ToList();
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug($"Report handle");
                if (inProgress.Count == 0)
                {
                    StringBuilder stringBuilder = StringBuilderPool.Shared.Rent($"<color=white>");
                    stringBuilder.AppendLine("<color=green>There are currently no InProgress Reports</color>");
                    int count = 16;
                    while (count >= 1)
                    {
                        count--;
                        stringBuilder.AppendLine($"");
                    }

                    stringBuilder.AppendLine(
                        $"The text below serve as an indicator to what the buttons below the text do.");
                    stringBuilder.AppendLine(
                        $"<color=red>Next Report</color>            <color=red>Previous Report</color>          <color=red>Complete Report</color>         <color=red>Ignore Report</color>");

                    stringBuilder.Append("</color>");
                    sender.RaReply(
                        string.Format("${0} {1}", (object)1,
                            (object)StringBuilderPool.Shared.ToStringReturn(stringBuilder)), true, true, string.Empty);
                }
                else
                {
                    StringBuilder stringBuilder = StringBuilderPool.Shared.Rent($"<color=white>");
                    bool canForward = true;
                    bool canBackward = true;
                    bool first = false;

                    if (!RemoteAdminModificationHandler.ReportInProgressState.ContainsKey(player))
                    {
                        RemoteAdminModificationHandler.ReportInProgressState.Add(player,
                            new Tuple<int, DateTime>(inProgress.FirstOrDefault().Id, DateTime.UtcNow));
                        first = true;
                    }

                    var currently = RemoteAdminModificationHandler.ReportInProgressState[player];
                    var report = inProgress.FirstOrDefault(s => s.Id == currently.Item1);
                    if (report == null)
                        report = inProgress.FirstOrDefault();
                    if (report != null)
                    {
                        int index = inProgress.IndexOf(report);
                        int targetIndex = index;
                        if (source[0] == "1")
                            targetIndex++;
                        else if (source[0] == "0")
                            targetIndex--;

                        if (first)
                            targetIndex = index;

                        if (inProgress.Count == 1)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug($"Rept 1");
                            canBackward = false;
                            canForward = false;
                        }

                        if (index == 0)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug($"Rept 2");
                            canBackward = false;
                        }

                        if (index + 1 == inProgress.Count)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug($"Rept 3");
                            canForward = false;
                        }

                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Log.Debug($"Rept 4 {index} {targetIndex}");

                        if (inProgress.Count > targetIndex)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug($"Rept 5 {index} {targetIndex}");
                            if ((source[0] == "1" && canForward) || (source[0] == "0" && canBackward))
                                report = inProgress[targetIndex];
                        }

                        index = inProgress.IndexOf(report);

                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Log.Debug($"Rept 8 {index} {targetIndex}");

                        if (index == 0)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug($"Rept 6");
                            canBackward = false;
                        }
                        else
                        {
                            canBackward = true;
                        }

                        if (index + 1 == inProgress.Count)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug($"Rept 7");
                            canForward = false;
                        }
                        else
                        {
                            canForward = true;
                        }

                        stringBuilder.AppendLine($"Report {index + 1} out of {inProgress.Count}");
                    }

                    RemoteAdminModificationHandler.ReportInProgressState[player] =
                        new Tuple<int, DateTime>(report.Id, DateTime.UtcNow);

                    stringBuilder.AppendLine($"Report: {report.Id}");

                    if (report.IsCheatReport)
                    {
                        stringBuilder.AppendLine(
                            $"<color=yellow>Warning: This report is marked as Cheater report, this means that NorthWood was notified of this report.\nIf this report is an actual cheating report please do not interfere with NorthWood global moderators so they can effectively ban cheaters.</color>");
                    }

                    var reporter = CedModPlayer.Get(report.ReporterId);
                    if (reporter != null)
                    {
                        stringBuilder.AppendLine(
                            $"Reporter: (<color=#43C6DB>{reporter.PlayerId}</color>) {reporter.Nickname} - {reporter.UserId} <color=green><link=CP_IP>\uF0C5</link></color>");
                        RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, reporter.UserId);
                    }
                    else
                    {
                        stringBuilder.AppendLine(
                            $"Reporter: <color=red>(Not Ingame)</color> - {report.ReporterId} <color=green><link=CP_IP>\uF0C5</link></color>");
                        RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, report.ReporterId);
                    }

                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug($"Checking list {string.Join(", ", Player.PlayersUserIds.Select(s => s.Key))}");

                    var reported = CedModPlayer.Get(report.ReportedId);
                    if (reported != null)
                    {
                        stringBuilder.AppendLine(
                            $"Reported: (<color=#43C6DB>{reported.PlayerId} <color=green><link=CP_ID>\uF0C5</link></color></color>) {reported.Nickname} - {reported.UserId} <color=green><link=CP_USERID>\uF0C5</link></color>");
                        RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, reported.UserId);
                        RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId, reported.PlayerId.ToString());
                    }
                    else
                    {
                        stringBuilder.AppendLine(
                            $"Reported: <color=red>(Not Ingame)</color> - {report.ReportedId} <color=green><link=CP_USERID>\uF0C5</link></color>");
                        RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, report.ReportedId);
                    }

                    if (report.AssignedHandler != null)
                    {
                        var handler = CedModPlayer.Get(report.AssignedHandler.SteamId + "@steam");
                        if (handler == null)
                            CedModPlayer.Get(report.AssignedHandler.DiscordId + "@discord");
                        else if (handler == null && report.AssignedHandler.AdditionalId != "")
                            CedModPlayer.Get(report.AssignedHandler.AdditionalId);
                        if (handler != null)
                        {
                            stringBuilder.AppendLine(
                                $"Handler: (<color=#43C6DB>{handler.PlayerId}</color>) {handler.Nickname} - {handler.UserId}</color>");
                        }
                        else
                        {
                            stringBuilder.AppendLine(
                                $"Handler: <color=red>(Not Ingame)</color> - {report.AssignedHandler.UserName}");
                        }
                    }
                    else
                    {
                        stringBuilder.AppendLine("Could not find Assigned Handler");
                    }

                    stringBuilder.AppendLine($"Reason: {report.Reason}");
                    stringBuilder.AppendLine($"Ingame Report: {!report.IsDiscordReport}");

                    stringBuilder.AppendLine(additional);

                    int count = report.IsCheatReport ? 5 : 6;
                    while (count >= 1)
                    {
                        count--;
                        stringBuilder.AppendLine($"");
                    }

                    stringBuilder.AppendLine(
                        $"The text below serve as an indicator to what the buttons below the text do.");
                    stringBuilder.AppendLine(
                        $"<color={(canForward ? "blue" : "red")}>Next Report</color>            <color={(canBackward ? "blue" : "red")}>Previous Report</color>          <color=blue>Complete Report</color>         <color=blue>Ignore Report</color>");

                    stringBuilder.Append("</color>");
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug($"Report handle complete send");
                    sender.RaReply(
                        string.Format("${0} {1}", (object)1,
                            (object)StringBuilderPool.Shared.ToStringReturn(stringBuilder)), true, true, string.Empty);
                    yield break;
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                sender.RaReply(
                    string.Format("${0} {1}", (object)1,
                        (object)"An Exception occured while trying to display reports"), true, true, string.Empty);
            }
        }
    }
}