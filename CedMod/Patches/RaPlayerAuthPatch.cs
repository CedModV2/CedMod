using System;
using System.Collections.Generic;
using System.Linq;
using CedMod.Components;
using CedMod.Handlers;
using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using MEC;
using NorthwoodLib.Pools;
using RemoteAdmin;
using RemoteAdmin.Communication;
using Utils;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(RaPlayerAuth), nameof(RaPlayerAuth.ReceiveData), new Type[] { typeof(CommandSender), typeof(string) })]
    public static class RaPlayerAuthPatch
    {
        public static Dictionary<string, CoroutineHandle> Handles = new Dictionary<string, CoroutineHandle>();
        
        public static bool Prefix(RaPlayerAuth __instance, CommandSender sender, string data)
        {
            if (Handles.ContainsKey(sender.SenderId) && Handles[sender.SenderId].IsRunning)
                return false;
            
            Handles[sender.SenderId] = Timing.RunCoroutine(RaPlayerCoRoutine(__instance, sender, data));
            return false;
        }

        public static IEnumerator<float> RaPlayerCoRoutine(RaPlayerAuth __instance, CommandSender sender, string data)
        {
            string[] source = data.Split(' ');
            if (source[0].StartsWith("-1") && CommandProcessor.CheckPermissions(sender, PlayerPermissions.BanningUpToDay))
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
                Server.ReportAdditionalText = $"<color=green>Report {report.Id} Now inprogress, please use the Inprogress tab to complete</color>";
                var pool = StringBuilderPool.Shared.Rent();
                CedModMain.Singleton.ServerEvents.OnPlayerRequestedCustomRaInfo(new PlayerRequestedCustomRaInfoEventArgs(sender, new ArraySegment<string>(source), false, pool));
                Server.ReportAdditionalText = string.Empty;
                sender.RaReply($"$1 {StringBuilderPool.Shared.ToStringReturn(pool)}", true, false, string.Empty);
                yield break;
            }
            
            if (source[0].StartsWith("-2") && CommandProcessor.CheckPermissions(sender, PlayerPermissions.BanningUpToDay))
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
                Server.ReportAdditionalText = $"<color=green>Report {report.Id} Completed</color>";
                var pool = StringBuilderPool.Shared.Rent();
                CedModMain.Singleton.ServerEvents.OnPlayerRequestedCustomRaInfo(new PlayerRequestedCustomRaInfoEventArgs(sender, new ArraySegment<string>(source), false, pool));
                Server.ReportAdditionalText = string.Empty;
                sender.RaReply($"$1 {StringBuilderPool.Shared.ToStringReturn(pool)}", true, false, string.Empty);
                yield break;
            }
            
            if (sender is PlayerCommandSender playerCommandSender && !playerCommandSender.ReferenceHub.authManager.NorthwoodStaff && !CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayerSensitiveDataAccess))
                yield break;
            List<ReferenceHub> referenceHubList = RAUtils.ProcessPlayerIdOrNamesList(new ArraySegment<string>(data.Split(' ')), 0, out string[] _);
            if (referenceHubList.Count == 0 || referenceHubList.Count > 1)
                yield break;
            if (string.IsNullOrEmpty(referenceHubList[0].authManager.GetAuthToken()))
            {
                sender.RaReply("PlayerInfo#Can't obtain auth token. Is server using offline mode or you selected the host?", false, true, "PlayerInfo");
            }
            else
            {
                ServerLogs.AddLog(ServerLogs.Modules.DataAccess, string.Format("{0} accessed authentication token of player {1} ({2}).", (object) sender.LogName, (object) referenceHubList[0].PlayerId, (object) referenceHubList[0].nicknameSync.MyNick), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                sender.RaReply(string.Format("PlayerInfo#<color=white>Authentication token of player {0} ({1}):\n{2}</color>", (object) referenceHubList[0].nicknameSync.MyNick, (object) referenceHubList[0].PlayerId, (object) referenceHubList[0].authManager.GetAuthToken()), true, true, "null");
                RaPlayerQR.Send(sender, true, referenceHubList[0].authManager.GetAuthToken());
            }
        }
    }
}