using System;
using System.Collections.Generic;
using System.Linq;
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
        public static bool Prefix(RaPlayer __instance, CommandSender sender, string data)
        {
            Timing.RunCoroutine(RaPlayerCoRoutine(__instance, sender, data));
            return false;
        }

        public static IEnumerator<float> RaPlayerCoRoutine(RaPlayer __instance, CommandSender sender, string data)
        {
            string[] source = data.Split(' ');
            int result;
            if (source.Length != 2 || !int.TryParse(source[0], out result))
                yield break;
            bool flag1 = result == 1;
            PlayerCommandSender playerCommandSender1 = sender as PlayerCommandSender;
            
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug($"Received: {data} : {source[1]}");
            var player = CedModPlayer.Get(sender.SenderId);

            if (source[1].StartsWith("-1") && CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayersManagement))
            {
                Timing.RunCoroutine(HandleReportType1(sender, player, source));
                yield break;
            }
            
            if (source[1].StartsWith("-2") && CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayersManagement))
            {
                Timing.RunCoroutine(HandleReportType2(sender, player, source));
                yield break;
            }
            
            if (!flag1 && playerCommandSender1 != null && !playerCommandSender1.ServerRoles.Staff && !CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayerSensitiveDataAccess))
                yield break;
            List<ReferenceHub> referenceHubList = RAUtils.ProcessPlayerIdOrNamesList(new ArraySegment<string>(((IEnumerable<string>)source).Skip<string>(1).ToArray<string>()), 0, out string[] _);
            if (referenceHubList.Count == 0)
                yield break;
            bool flag2 = PermissionsHandler.IsPermitted(sender.Permissions, 18007046UL);
            if (playerCommandSender1 != null &&
                (playerCommandSender1.ServerRoles.Staff || playerCommandSender1.ServerRoles.RaEverywhere))
                flag2 = true;
            if (referenceHubList.Count > 1)
            {
                StringBuilder stringBuilder = StringBuilderPool.Shared.Rent("<color=white>");
                stringBuilder.Append("Selecting multiple players:");
                stringBuilder.Append("\nPlayer ID: <color=green><link=CP_ID>\uF0C5</link></color>");
                stringBuilder.Append("\nIP Address: " +
                                     (!flag1 ? "<color=green><link=CP_IP>\uF0C5</link></color>" : "[REDACTED]"));
                stringBuilder.Append("\nUser ID: " +
                                     (flag2 ? "<color=green><link=CP_USERID>\uF0C5</link></color>" : "[REDACTED]"));
                stringBuilder.Append("</color>");
                string data1 = string.Empty;
                string data2 = string.Empty;
                string data3 = string.Empty;
                foreach (ReferenceHub referenceHub in referenceHubList)
                {
                    data1 = data1 + (object)referenceHub.PlayerId + ".";
                    if (!flag1)
                        data2 = data2 + (referenceHub.networkIdentity.connectionToClient.IpOverride != null
                            ? referenceHub.networkIdentity.connectionToClient.OriginalIpAddress
                            : referenceHub.networkIdentity.connectionToClient.address) + ",";
                    if (flag2)
                        data3 = data3 + referenceHub.characterClassManager.UserId + ".";
                }

                if (data1.Length > 0)
                    RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId, data1);
                if (data2.Length > 0)
                    RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, data2);
                if (data3.Length > 0)
                    RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, data3);
                sender.RaReply(string.Format("${0} {1}", (object)__instance.DataId, (object)stringBuilder), true, true,
                    string.Empty);
                StringBuilderPool.Shared.Return(stringBuilder);
            }
            else
            {
                ReferenceHub referenceHub = referenceHubList[0];
                ServerLogs.AddLog(ServerLogs.Modules.DataAccess,
                    string.Format("{0} accessed IP address of player {1} ({2}).", (object)sender.LogName,
                        (object)referenceHub.PlayerId, (object)referenceHub.nicknameSync.MyNick),
                    ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                bool flag3 = PermissionsHandler.IsPermitted(sender.Permissions, PlayerPermissions.GameplayData);
                CharacterClassManager characterClassManager = referenceHub.characterClassManager;
                NicknameSync nicknameSync = referenceHub.nicknameSync;
                NetworkConnectionToClient connectionToClient = referenceHub.networkIdentity.connectionToClient;
                ServerRoles serverRoles = referenceHub.serverRoles;
                if (sender is PlayerCommandSender playerCommandSender2)
                    playerCommandSender2.ReferenceHub.queryProcessor.GameplayData = flag3;
                StringBuilder stringBuilder = StringBuilderPool.Shared.Rent("<color=white>");
                stringBuilder.Append("Nickname: " + nicknameSync.CombinedName);
                stringBuilder.Append(string.Format("\nPlayer ID: {0} <color=green><link=CP_ID>\uF0C5</link></color>",
                    (object)referenceHub.PlayerId));
                RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId,
                    string.Format("{0}", (object)referenceHub.PlayerId));
                if (connectionToClient == null)
                    stringBuilder.Append("\nIP Address: null");
                else if (!flag1)
                {
                    stringBuilder.Append("\nIP Address: " + connectionToClient.address + " ");
                    if (connectionToClient.IpOverride != null)
                    {
                        RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip,
                            connectionToClient.OriginalIpAddress ?? "");
                        stringBuilder.Append(" [routed via " + connectionToClient.OriginalIpAddress + "]");
                    }
                    else
                        RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, connectionToClient.address ?? "");

                    stringBuilder.Append(" <color=green><link=CP_IP>\uF0C5</link></color>");
                }
                else
                    stringBuilder.Append("\nIP Address: [REDACTED]");

                stringBuilder.Append("\nUser ID: " +
                                     (flag2
                                         ? (string.IsNullOrEmpty(characterClassManager.UserId)
                                             ? "(none)"
                                             : characterClassManager.UserId +
                                               " <color=green><link=CP_USERID>\uF0C5</link></color>")
                                         : "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>"));
                if (flag2)
                {
                    RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, characterClassManager.UserId ?? "");
                    if (characterClassManager.SaltedUserId != null && characterClassManager.SaltedUserId.Contains("$"))
                        stringBuilder.Append("\nSalted User ID: " + characterClassManager.SaltedUserId);
                    if (!string.IsNullOrEmpty(characterClassManager.UserId2))
                        stringBuilder.Append("\nUser ID 2: " + characterClassManager.UserId2);
                }

                stringBuilder.Append("\nServer role: " + serverRoles.GetColoredRoleString());
                bool flag4 = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenBadges);
                bool flag5 = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenGlobalBadges);
                if (playerCommandSender1 != null && playerCommandSender1.ServerRoles.Staff)
                {
                    flag4 = true;
                    flag5 = true;
                }

                bool flag6 = !string.IsNullOrEmpty(serverRoles.HiddenBadge);
                bool flag7 = !flag6 || serverRoles.GlobalHidden & flag5 || !serverRoles.GlobalHidden & flag4;
                if (flag7)
                {
                    if (flag6)
                    {
                        stringBuilder.Append("\n<color=#DC143C>Hidden role: </color>" + serverRoles.HiddenBadge);
                        stringBuilder.Append("\n<color=#DC143C>Hidden role type: </color>" +
                                             (serverRoles.GlobalHidden ? "GLOBAL" : "LOCAL"));
                    }

                    if (serverRoles.RaEverywhere)
                        stringBuilder.Append(
                            "\nStudio Status: <color=#BCC6CC>Studio GLOBAL Staff (management or global moderation)</color>");
                    else if (serverRoles.Staff)
                        stringBuilder.Append("\nStudio Status: <color=#94B9CF>Studio Staff</color>");
                }

                int flags = (int)VoiceChatMutes.GetFlags(referenceHubList[0]);
                if (flags != 0)
                {
                    stringBuilder.Append("\nMUTE STATUS:");
                    foreach (int num in Enum.GetValues(typeof(VcMuteFlags)))
                    {
                        if (num != 0 && (flags & num) == num)
                        {
                            stringBuilder.Append(" <color=#F70D1A>");
                            stringBuilder.Append((object)(VcMuteFlags)num);
                            stringBuilder.Append("</color>");
                        }
                    }
                }

                stringBuilder.Append("\nActive flag(s):");
                if (characterClassManager.GodMode)
                    stringBuilder.Append(" <color=#659EC7>[GOD MODE]</color>");
                if (referenceHub.playerStats.GetModule<AdminFlagsStat>().HasFlag(AdminFlags.Noclip))
                    stringBuilder.Append(" <color=#DC143C>[NOCLIP ENABLED]</color>");
                else if (FpcNoclip.IsPermitted(referenceHub))
                    stringBuilder.Append(" <color=#E52B50>[NOCLIP UNLOCKED]</color>");
                if (serverRoles.DoNotTrack)
                    stringBuilder.Append(" <color=#BFFF00>[DO NOT TRACK]</color>");
                if (serverRoles.BypassMode)
                    stringBuilder.Append(" <color=#BFFF00>[BYPASS MODE]</color>");
                if (flag7 && serverRoles.RemoteAdmin)
                    stringBuilder.Append(" <color=#43C6DB>[RA AUTHENTICATED]</color>");
                if (serverRoles.IsInOverwatch)
                    stringBuilder.Append(" <color=#008080>[OVERWATCH MODE]</color>");
                else if (flag3)
                {
                    PlayerRoleBase playerRoleBase;
                    stringBuilder.Append("\nClass: ")
                        .Append(PlayerRoleLoader.AllRoles.TryGetValue(referenceHub.GetRoleId(), out playerRoleBase)
                            ? playerRoleBase.RoleName
                            : "None");
                    stringBuilder.Append(" <color=#fcff99>[HP: ")
                        .Append(CommandProcessor.GetRoundedStat<HealthStat>(referenceHub)).Append("]</color>");
                    stringBuilder.Append(" <color=green>[AHP: ")
                        .Append(CommandProcessor.GetRoundedStat<AhpStat>(referenceHub))
                        .Append("]</color>");
                    stringBuilder.Append(" <color=#977dff>[HS: ")
                        .Append(CommandProcessor.GetRoundedStat<HumeShieldStat>(referenceHub)).Append("]</color>");
                    stringBuilder.Append("\nPosition: ").Append(referenceHub.transform.position.ToPreciseString());
                }
                else
                    stringBuilder.Append(
                        "\n<color=#D4AF37>Some fields were hidden. GameplayData permission required.</color>");

                Log.Debug($"Has permissions: {sender.CheckPermission("cedmod.requestdata")}", CedModMain.Singleton.Config.QuerySystem.Debug);
                if (sender.CheckPermission("cedmod.requestdata"))
                {
                    sender.RaReply(string.Format("${0} {1}", __instance.DataId, "Loading from CedMod API, please wait..."), true, true, string.Empty);
                    UnityWebRequest www = new UnityWebRequest($"http{(QuerySystem.UseSSL ? "s" : "")}://" + API.APIUrl + $"/Auth/{characterClassManager.UserId}&{connectionToClient.address}", "OPTIONS");
                    DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
                    www.downloadHandler = dH;

                    www.SetRequestHeader("ApiKey", CedModMain.Singleton.Config.CedMod.CedModApiKey);
                    yield return Timing.WaitUntilDone(www.SendWebRequest());
                    try
                    {
                        if (www.responseCode != 200)
                        {
                            Log.Error($"Failed to RequestData CMAPI: {www.responseCode} | {www.downloadHandler.text}");
                        }
                        else
                        {
                            Dictionary<string, object> cmData =
                                JsonConvert.DeserializeObject<Dictionary<string, object>>(www.downloadHandler.text);
                            stringBuilder.Append("\n<color=#D4AF37>CedMod Added Fields:</color>");
                            if (cmData["antiVpnWhitelist"] is bool ? (bool)cmData["antiVpnWhitelist"] : false)
                                stringBuilder.Append("\nActive flag: <color=#008080>AntiVpn Whitelisted</color>");

                            if (cmData["triggeredAvpnPast"] is bool ? (bool)cmData["triggeredAvpnPast"] : false)
                                stringBuilder.Append("\nActive flag: <color=#red>AntiVpn Triggered</color>");

                            stringBuilder.Append(
                                $"\nModeration: {cmData["warns"]} Warnings, {cmData["banLogs"]} Banlogs");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    www.Dispose();
                    sender.RaReply(
                        string.Format("${0} {1}", __instance.DataId, "Loading from Panel API, please wait..."), true,
                        true, string.Empty);
                    www = new UnityWebRequest(
                        $"http{(QuerySystem.UseSSL ? "s" : "")}://" 
                        + QuerySystem.CurrentMaster 
                        + $"/Api/v3/RequestData/{QuerySystem.QuerySystemKey}/{characterClassManager.UserId}", "OPTIONS");
                    DownloadHandlerBuffer dH1 = new DownloadHandlerBuffer();
                    www.downloadHandler = dH1;
                    yield return Timing.WaitUntilDone(www.SendWebRequest());
                    try
                    {
                        if (www.responseCode != 200)
                        {
                            Log.Error(
                                $"Failed to RequestData PanelAPI: {www.responseCode} | {www.downloadHandler.text}");
                        }
                        else
                        {
                            Dictionary<string, object> cmData =
                                JsonConvert.DeserializeObject<Dictionary<string, object>>(www.downloadHandler.text);

                            if (!serverRoles.DoNotTrack)
                            {
                                stringBuilder.Append(
                                    $"\nActivity in the last 14 days: {TimeSpan.FromMinutes(cmData["activityLast14"] is double ? (double)cmData["activityLast14"] : 0).TotalHours}. Level {cmData["level"]} ({cmData["experience"]} Exp)");
                            }

                            if (cmData["panelUser"].ToString() != "")
                                stringBuilder.Append($"\nPanelUser: {cmData["panelUser"]}");

                            stringBuilder.Append($"\nPossible Alt Accounts: {cmData["usersFound"]}");
                            if (RemoteAdminModificationHandler.GroupWatchlist.Any(s => s.UserIds.Contains(referenceHub.characterClassManager.UserId)))
                            {
                                if (RemoteAdminModificationHandler.GroupWatchlist.Count(s => s.UserIds.Contains(referenceHub.characterClassManager.UserId)) >= 2)
                                {
                                    stringBuilder.Append($"\n<color=#00FFF6>User is in {RemoteAdminModificationHandler.GroupWatchlist.Count(s => s.UserIds.Contains(referenceHub.characterClassManager.UserId))} Watchlist groups.\nUse ExternalLookup to view details</color>");
                                }
                                else
                                {
                                    var group = RemoteAdminModificationHandler.GroupWatchlist.FirstOrDefault(s => s.UserIds.Contains(referenceHub.characterClassManager.UserId));
                                    stringBuilder.Append($"\n<color=#00FFF6>User is in Group Watchlist: {group.GroupName} ({group.Id}) \n{group.Reason} Members:</color>");
                                    foreach (var member in group.UserIds.Take(4))
                                    {
                                        var plr = CedModPlayer.Get(member);
                                        stringBuilder.Append($"\n<color=#00FFF6>{(plr == null ? "Not Ingame" : $"{plr.PlayerId} - {plr.UserId}")}</color>");
                                    }
                                    stringBuilder.Append($"\n<color=#00FFF6>Use ExternalLookup for more info</color>");
                                }
                            }
                            else if (RemoteAdminModificationHandler.Watchlist.Any(s => s.Userid == referenceHub.characterClassManager.UserId))
                            {
                                stringBuilder.Append($"\n<color=#00FFF6>User is on watchlist:\n{RemoteAdminModificationHandler.Watchlist.FirstOrDefault(s => s.Userid == referenceHub.characterClassManager.UserId).Reason}\nUse ExternalLookup for more info</color>");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    www.Dispose();
                }

                stringBuilder.Append("</color>");
                sender.RaReply(string.Format("${0} {1}", (object)__instance.DataId, (object)StringBuilderPool.Shared.ToStringReturn(stringBuilder)), true, true, string.Empty);
                RaPlayerQR.Send(sender, false,
                    string.IsNullOrEmpty(characterClassManager.UserId) ? "(no User ID)" : characterClassManager.UserId);
            }
        }

        public static IEnumerator<float> HandleReportType1(CommandSender sender, CedModPlayer player, string[] source, string additional = "")
        {
            try
            {
                    var open = RemoteAdminModificationHandler.ReportsList.Where(s => s.Status == HandleStatus.NoResponse).ToList();
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
                sender.RaReply(string.Format("${0} {1}", (object)1, (object)StringBuilderPool.Shared.ToStringReturn(stringBuilder)), true, true, string.Empty);
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

                int count = report.IsCheatReport ? 8 : 9;
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
                sender.RaReply(string.Format("${0} {1}", (object)1, (object)"An Exception occured while trying to display reports"), true, true, string.Empty);
            }
        }
        
        public static IEnumerator<float> HandleReportType2(CommandSender sender, CedModPlayer player, string[] source, string additional = "")
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
                    sender.RaReply(string.Format("${0} {1}", (object)1, (object)StringBuilderPool.Shared.ToStringReturn(stringBuilder)), true, true, string.Empty);
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
                            stringBuilder.AppendLine($"Handler: (<color=#43C6DB>{handler.PlayerId}</color>) {handler.Nickname} - {handler.UserId}</color>");
                        }
                        else
                        {
                            stringBuilder.AppendLine($"Handler: <color=red>(Not Ingame)</color> - {report.AssignedHandler.UserName}");
                        }
                    }
                    else
                    {
                        stringBuilder.AppendLine("Could not find Assigned Handler");
                    }

                    stringBuilder.AppendLine($"Reason: {report.Reason}");
                    stringBuilder.AppendLine($"Ingame Report: {!report.IsDiscordReport}");

                    stringBuilder.AppendLine(additional);
                    
                    int count = report.IsCheatReport ? 7 : 8;
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