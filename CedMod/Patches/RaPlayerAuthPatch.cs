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
    [HarmonyPatch(typeof(RaPlayerAuth), nameof(RaPlayerAuth.ReceiveData), new Type[] { typeof(CommandSender), typeof(string) })]
    public static class RaPlayerAuthPatch
    {
        public static bool Prefix(RaPlayerAuth __instance, CommandSender sender, string data)
        {
            Timing.RunCoroutine(RaPlayerCoRoutine(__instance, sender, data));
            return false;
        }

        public static IEnumerator<float> RaPlayerCoRoutine(RaPlayerAuth __instance, CommandSender sender, string data)
        {
            string[] source = data.Split(' ');
            if (source[0].StartsWith("-1") && CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayersManagement))
            {
                var player = CedModPlayer.Get(sender.SenderId);
                var open = RemoteAdminModificationHandler.ReportsList.Where(s => s.Status == HandleStatus.NoResponse).ToList();
                if (!RemoteAdminModificationHandler.ReportUnHandledState.ContainsKey(player))
                {
                    sender.RaReply(string.Format("${0} {1}", (object)1, (object)"Please use 'Request' to select a report first."), true, true, string.Empty);
                    yield break;
                }
                var currently = RemoteAdminModificationHandler.ReportUnHandledState[player];
                var report = open.FirstOrDefault(s => s.Id == currently.Item1);
                if (report == null)
                {
                    sender.RaReply(string.Format("${0} {1}", (object)1, (object)"Please use 'Request' to select a report first."), true, true, string.Empty);
                    yield break;
                }

                var resp = RemoteAdminModificationHandler.UpdateReport(report.Id.ToString(), sender.SenderId, HandleStatus.InProgress, "");
                yield return Timing.WaitUntilTrue(() => resp.IsCompleted);
                Timing.RunCoroutine(RaPlayerPatch.HandleReportType1(sender, player, source = new string[] { "0", "-1" }, $"<color=green>Report {report.Id} Now inprogress, please use the Inprogress tab to complete</color>"));
                yield break;
            }
            
            if (source[0].StartsWith("-2") && CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayersManagement))
            {
                var player = CedModPlayer.Get(sender.SenderId);
                var open = RemoteAdminModificationHandler.ReportsList.Where(s => s.Status == HandleStatus.InProgress).ToList();
                if (!RemoteAdminModificationHandler.ReportInProgressState.ContainsKey(player))
                {
                    sender.RaReply(string.Format("${0} {1}", (object)1, (object)"Please use 'Request' to select a report first."), true, true, string.Empty);
                    yield break;
                }
                var currently = RemoteAdminModificationHandler.ReportInProgressState[player];
                var report = open.FirstOrDefault(s => s.Id == currently.Item1);
                if (report == null)
                {
                    sender.RaReply(string.Format("${0} {1}", (object)1, (object)"Please use 'Request' to select a report first."), true, true, string.Empty);
                    yield break;
                }
                var resp = RemoteAdminModificationHandler.UpdateReport(report.Id.ToString(), sender.SenderId, HandleStatus.Handled, "Handled using ingame RemoteAdmin");
                yield return Timing.WaitUntilTrue(() => resp.IsCompleted);
                Timing.RunCoroutine(RaPlayerPatch.HandleReportType2(sender, player, source = new string[] { "0", "-2" }, $"<color=green>Report {report.Id} Completed</color>"));
                yield break;
            }
            
            if (sender is PlayerCommandSender playerCommandSender && !playerCommandSender.ServerRoles.Staff && !CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayerSensitiveDataAccess))
                yield break;
            List<ReferenceHub> referenceHubList = RAUtils.ProcessPlayerIdOrNamesList(new ArraySegment<string>(data.Split(' ')), 0, out string[] _);
            if (referenceHubList.Count == 0 || referenceHubList.Count > 1)
                yield break;
            if (string.IsNullOrEmpty(referenceHubList[0].characterClassManager.AuthToken))
            {
                sender.RaReply("PlayerInfo#Can't obtain auth token. Is server using offline mode or you selected the host?", false, true, "PlayerInfo");
            }
            else
            {
                ServerLogs.AddLog(ServerLogs.Modules.DataAccess, string.Format("{0} accessed authentication token of player {1} ({2}).", (object) sender.LogName, (object) referenceHubList[0].PlayerId, (object) referenceHubList[0].nicknameSync.MyNick), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                sender.RaReply(string.Format("PlayerInfo#<color=white>Authentication token of player {0} ({1}):\n{2}</color>", (object) referenceHubList[0].nicknameSync.MyNick, (object) referenceHubList[0].PlayerId, (object) referenceHubList[0].characterClassManager.AuthToken), true, true, "null");
                RaPlayerQR.Send(sender, true, referenceHubList[0].characterClassManager.AuthToken);
            }
        }
    }
}