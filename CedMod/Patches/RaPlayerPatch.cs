using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using CedMod.Addons.QuerySystem;
using CedMod.Components;
using HarmonyLib;
using MEC;
using Mirror;
using Newtonsoft.Json;
using NorthwoodLib.Pools;
using PluginAPI.Core;
using RemoteAdmin;
using RemoteAdmin.Communication;
using UnityEngine.Networking;
#if !EXILED
using NWAPIPermissionSystem;
#else
using Exiled.Permissions.Extensions;
#endif

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(RaPlayer), nameof(RaPlayer.ReceiveData), typeof(CommandSender), typeof(string))]
    public static class RaPlayerPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var list = ListPool<CodeInstruction>.Shared.Rent(instructions);

            int handleReportsIndex = list.FindIndex(i => i.opcode == OpCodes.Ldloc_1);
            var defaultFunctionality = generator.DefineLabel();
            var instruction = list[handleReportsIndex];

            list.InsertRange(handleReportsIndex, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_1).MoveLabelsFrom(instruction),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldloc_0),
                CodeInstruction.Call(typeof(RaPlayerPatch), nameof(TryHandleReportRequest)),
                new CodeInstruction(OpCodes.Brfalse, defaultFunctionality),
                new CodeInstruction(OpCodes.Ret)
            });

            instruction.labels.Add(defaultFunctionality);

            int endAppendIndex = list.FindLastIndex(i => i.operand is "</color>") - 1;
            int returnIndex = list.FindLastIndex(i => i.opcode == OpCodes.Ret);
            var labels = list[endAppendIndex].ExtractLabels();

            list.RemoveRange(endAppendIndex, returnIndex - endAppendIndex);
            list.InsertRange(endAppendIndex, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0).WithLabels(labels),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldloc, 8),
                new CodeInstruction(OpCodes.Ldloc, 10),
                new CodeInstruction(OpCodes.Ldloc, 13),
                new CodeInstruction(OpCodes.Ldloc, 11),
                new CodeInstruction(OpCodes.Ldloc, 6),
                CodeInstruction.Call(typeof(RaPlayerPatch), nameof(AppendDataFromCedModApiAndSend)),
                CodeInstruction.Call(typeof(Timing), nameof(Timing.RunCoroutine), new[]
                {
                    typeof(IEnumerator<float>)
                }),
                new CodeInstruction(OpCodes.Pop)
            });

            foreach (var codeInstruction in list)
                yield return codeInstruction;

            ListPool<CodeInstruction>.Shared.Return(list);

        }

        public static bool TryHandleReportRequest(CommandSender sender,string data, string[] source)
        {
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug($"Received: {data} : {source[1]}");
            var player = CedModPlayer.Get(sender.SenderId);

            if (source[1].StartsWith("-1") && CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayersManagement))
            {
                Timing.RunCoroutine(HandleReportType1(sender, player, source));
                return true;
            }
            
            if (source[1].StartsWith("-2") && CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayersManagement))
            {
                Timing.RunCoroutine(HandleReportType2(sender, player, source));
                return true;
            }

            return false;
        }

        public static IEnumerator<float> AppendDataFromCedModApiAndSend(RaPlayer instance, CommandSender sender, CharacterClassManager characterClassManager, NetworkConnectionToClient connectionToClient, StringBuilder stringBuilder, ServerRoles serverRoles, ReferenceHub referenceHub)
        {
            Log.Debug($"Has permissions: {sender.CheckPermission("cedmod.requestdata")}", CedModMain.Singleton.Config.QuerySystem.Debug);
            if (sender.CheckPermission("cedmod.requestdata"))
            {
                sender.RaReply(string.Format("${0} {1}", instance.DataId, "Loading from CedMod API, please wait..."), true, true, string.Empty);
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
                    string.Format("${0} {1}", instance.DataId, "Loading from Panel API, please wait..."), true,
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
                                stringBuilder.Append("\n<color=#00FFF6>Use ExternalLookup for more info</color>");
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
            sender.RaReply(string.Format("${0} {1}", instance.DataId, StringBuilderPool.Shared.ToStringReturn(stringBuilder)), true, true, string.Empty);
            RaPlayerQR.Send(sender, false,
                string.IsNullOrEmpty(characterClassManager.UserId) ? "(no User ID)" : characterClassManager.UserId);
        }

        public static IEnumerator<float> HandleReportType1(CommandSender sender, CedModPlayer player, string[] source, string additional = "")
        {
            try
            {
                    var open = RemoteAdminModificationHandler.ReportsList.Where(s => s.Status == HandleStatus.NoResponse).ToList();
            //report handling
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug("Report handle");
            if (open.Count == 0)
            {
                StringBuilder stringBuilder = StringBuilderPool.Shared.Rent("<color=white>");
                stringBuilder.AppendLine("<color=green>There are currently no Unhandled Reports</color>");
                int count = 16;
                while (count >= 1)
                {
                    count--;
                    stringBuilder.AppendLine("");
                }

                stringBuilder.AppendLine(
                    "The text below serve as an indicator to what the buttons below the text do.");
                stringBuilder.AppendLine(
                    "<color=red>Next Report</color>            <color=red>Previous Report</color>            <color=red>Claim Report</color>              <color=red>Ignore Report</color>");

                stringBuilder.Append("</color>");
                sender.RaReply(string.Format("${0} {1}", 1, StringBuilderPool.Shared.ToStringReturn(stringBuilder)), true, true, string.Empty);
            }
            else
            {
                StringBuilder stringBuilder = StringBuilderPool.Shared.Rent("<color=white>");
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
                            Log.Debug("Rept 1");
                        canBackward = false;
                        canForward = false;
                    }

                    if (index == 0)
                    {
                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Log.Debug("Rept 2");
                        canBackward = false;
                    }

                    if (index + 1 == open.Count)
                    {
                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Log.Debug("Rept 3");
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
                            Log.Debug("Rept 6");
                        canBackward = false;
                    }
                    else
                    {
                        canBackward = true;
                    }

                    if (index + 1 == open.Count)
                    {
                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Log.Debug("Rept 7");
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
                        "<color=yellow>Warning: This report is marked as Cheater report, this means that NorthWood was notified of this report.\nIf this report is an actual cheating report please do not interfere with NorthWood global moderators so they can effectively ban cheaters.</color>");
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
                    stringBuilder.AppendLine("");
                }

                stringBuilder.AppendLine(
                    "The text below serve as an indicator to what the buttons below the text do.");
                stringBuilder.AppendLine(
                    $"<color={(canForward ? "blue" : "red")}>Next Report</color>            <color={(canBackward ? "blue" : "red")}>Previous Report</color>            <color=blue>Claim Report</color>              <color=blue>Ignore Report</color>");

                stringBuilder.Append("</color>");
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug("Report handle complete send");
                sender.RaReply(
                    string.Format("${0} {1}", 1,
                        StringBuilderPool.Shared.ToStringReturn(stringBuilder)), true, true, string.Empty);
                yield break;
            }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                sender.RaReply(string.Format("${0} {1}", 1, "An Exception occured while trying to display reports"), true, true, string.Empty);
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
                    Log.Debug("Report handle");
                if (inProgress.Count == 0)
                {
                    StringBuilder stringBuilder = StringBuilderPool.Shared.Rent("<color=white>");
                    stringBuilder.AppendLine("<color=green>There are currently no InProgress Reports</color>");
                    int count = 16;
                    while (count >= 1)
                    {
                        count--;
                        stringBuilder.AppendLine("");
                    }

                    stringBuilder.AppendLine(
                        "The text below serve as an indicator to what the buttons below the text do.");
                    stringBuilder.AppendLine(
                        "<color=red>Next Report</color>            <color=red>Previous Report</color>          <color=red>Complete Report</color>         <color=red>Ignore Report</color>");

                    stringBuilder.Append("</color>");
                    sender.RaReply(string.Format("${0} {1}", 1, StringBuilderPool.Shared.ToStringReturn(stringBuilder)), true, true, string.Empty);
                }
                else
                {
                    StringBuilder stringBuilder = StringBuilderPool.Shared.Rent("<color=white>");
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
                                Log.Debug("Rept 1");
                            canBackward = false;
                            canForward = false;
                        }

                        if (index == 0)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug("Rept 2");
                            canBackward = false;
                        }

                        if (index + 1 == inProgress.Count)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug("Rept 3");
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
                                Log.Debug("Rept 6");
                            canBackward = false;
                        }
                        else
                        {
                            canBackward = true;
                        }

                        if (index + 1 == inProgress.Count)
                        {
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug("Rept 7");
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
                            "<color=yellow>Warning: This report is marked as Cheater report, this means that NorthWood was notified of this report.\nIf this report is an actual cheating report please do not interfere with NorthWood global moderators so they can effectively ban cheaters.</color>");
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
                        stringBuilder.AppendLine("");
                    }

                    stringBuilder.AppendLine(
                        "The text below serve as an indicator to what the buttons below the text do.");
                    stringBuilder.AppendLine(
                        $"<color={(canForward ? "blue" : "red")}>Next Report</color>            <color={(canBackward ? "blue" : "red")}>Previous Report</color>          <color=blue>Complete Report</color>         <color=blue>Ignore Report</color>");

                    stringBuilder.Append("</color>");
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug("Report handle complete send");
                    sender.RaReply(
                        string.Format("${0} {1}", 1,
                            StringBuilderPool.Shared.ToStringReturn(stringBuilder)), true, true, string.Empty);
                    yield break;
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                sender.RaReply(
                    string.Format("${0} {1}", 1,
                        "An Exception occured while trying to display reports"), true, true, string.Empty);
            }
        }
    }
}