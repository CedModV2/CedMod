using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using CedMod.Components;
using CommandSystem;
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
    [HarmonyPatch(typeof(ExternalLookupCommand), nameof(ExternalLookupCommand.Execute))]
    public static class ExternalLookupCommandPatch
    {
        public static bool Prefix(ExternalLookupCommand __instance, bool __result, ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            try
            {
                if (arguments.Count == 0)
                {
                    response = "";
                    return true;
                }
                if (arguments.At(0) == "-1")
                {
                    response = "";
                    Timing.RunCoroutine(RaPlayerCoRoutine(__instance, sender as CommandSender, arguments));
                    return false;
                }
                else if (arguments.At(0) == "-2")
                {
                    response = "";
                    Timing.RunCoroutine(RaPlayerCoRoutine(__instance, sender as CommandSender, arguments));
                    return false;
                }
                else
                {
                    response = "";
                    return true;
                }
            }
            catch (Exception e)
            {
                response = "";
                return true;
            }
        }

        public static IEnumerator<float> RaPlayerCoRoutine(ExternalLookupCommand __instance, CommandSender sender, ArraySegment<string>  data)
        {
            string[] source = data.ToArray();
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

                var resp = RemoteAdminModificationHandler.UpdateReport(report.Id.ToString(), sender.SenderId, HandleStatus.Ignored, "Ignored using ingame RemoteAdmin");
                yield return Timing.WaitUntilTrue(() => resp.IsCompleted);
                Timing.RunCoroutine(RaPlayerPatch.HandleReportType1(sender, player, source = new string[] { "0", "-1" }, $"<color=green>Report {report.Id} Ignored</color>"));
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
                var resp = RemoteAdminModificationHandler.UpdateReport(report.Id.ToString(), sender.SenderId, HandleStatus.Ignored, "Ignored using ingame RemoteAdmin");
                yield return Timing.WaitUntilTrue(() => resp.IsCompleted);
                Timing.RunCoroutine(RaPlayerPatch.HandleReportType2(sender, player, source = new string[] { "0", "-2" }, $"<color=green>Report {report.Id} Ignored</color>"));
            }
        }
    }
}