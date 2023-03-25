using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CedMod.Components;
using HarmonyLib;
using MEC;
using NorthwoodLib.Pools;
using RemoteAdmin;
using RemoteAdmin.Communication;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(RaPlayerAuth), nameof(RaPlayerAuth.ReceiveData), typeof(CommandSender), typeof(string))]
    public static class RaPlayerAuthPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var list = ListPool<CodeInstruction>.Shared.Rent(instructions);

            var label = generator.DefineLabel();
            var source = generator.DeclareLocal(typeof(string[]));
            list[0].labels.Add(label);

            // replace the other split call with the source variable
            int splitIndex = list.FindIndex(i => i.operand is MethodInfo {Name: nameof(string.Split)});
            int splitStart = splitIndex - 7;
            int splitEnd = splitIndex + 1;

            var splitLabels = list[splitStart].ExtractLabels();

            list.RemoveRange(splitStart, splitEnd - splitStart);
            list.Insert(splitStart, new CodeInstruction(OpCodes.Ldloc, source.LocalIndex).WithLabels(splitLabels));

            // main functionality
            list.InsertRange(0, new[]
            {
                // source = string.Split(' ');
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Newarr, typeof(char)),
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Ldc_I4_0),
                new CodeInstruction(OpCodes.Ldc_I4_S, 32),
                new CodeInstruction(OpCodes.Stelem_I2),
                CodeInstruction.Call(typeof(string), nameof(string.Split), new[]
                {
                    typeof(char[])
                }),
                new CodeInstruction(OpCodes.Stloc, source.LocalIndex),

                // return if custom processing was successful
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldloc, source.LocalIndex),
                CodeInstruction.Call(typeof(RaPlayerAuthPatch), nameof(TryHandleReportRequest)),
                new CodeInstruction(OpCodes.Brfalse, label),
                new CodeInstruction(OpCodes.Ret)
            });

            foreach (var codeInstruction in list)
                yield return codeInstruction;

            ListPool<CodeInstruction>.Shared.Return(list);

        }

        public static bool TryHandleReportRequest(CommandSender sender, string[] source)
        {
            if (source[0].StartsWith("-1") && CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayersManagement))
            {
                Timing.RunCoroutine(HandleUnhandledReport(sender));
                return true;
            }

            if (source[0].StartsWith("-2") && CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayersManagement))
            {
                Timing.RunCoroutine(HandleInProgressReport(sender));
                return true;
            }

            return false;
        }
        private static IEnumerator<float> HandleUnhandledReport(CommandSender sender)
        {
            var player = CedModPlayer.Get(sender.SenderId);
            var open = RemoteAdminModificationHandler.ReportsList.Where(s => s.Status == HandleStatus.NoResponse).ToList();
            if (!RemoteAdminModificationHandler.ReportUnHandledState.ContainsKey(player))
            {
                sender.RaReply(string.Format("${0} {1}", 1, "Please use 'Request' to select a report first."), true, true, string.Empty);
                yield break;
            }
            var currently = RemoteAdminModificationHandler.ReportUnHandledState[player];
            var report = open.FirstOrDefault(s => s.Id == currently.Item1);
            if (report == null)
            {
                sender.RaReply(string.Format("${0} {1}", 1, "Please use 'Request' to select a report first."), true, true, string.Empty);
                yield break;
            }

            var resp = RemoteAdminModificationHandler.UpdateReport(report.Id.ToString(), sender.SenderId, HandleStatus.InProgress, "");
            yield return Timing.WaitUntilTrue(() => resp.IsCompleted);
            Timing.RunCoroutine(RaPlayerPatch.HandleReportType1(sender, player, new string[]
            {
                "0",
                "-1"
            }, $"<color=green>Report {report.Id} Now inprogress, please use the Inprogress tab to complete</color>"));
        }

        private static IEnumerator<float> HandleInProgressReport(CommandSender sender)
        {
            var player = CedModPlayer.Get(sender.SenderId);
            var open = RemoteAdminModificationHandler.ReportsList.Where(s => s.Status == HandleStatus.InProgress).ToList();
            if (!RemoteAdminModificationHandler.ReportInProgressState.ContainsKey(player))
            {
                sender.RaReply(string.Format("${0} {1}", 1, "Please use 'Request' to select a report first."), true, true, string.Empty);
                yield break;
            }
            var currently = RemoteAdminModificationHandler.ReportInProgressState[player];
            var report = open.FirstOrDefault(s => s.Id == currently.Item1);
            if (report == null)
            {
                sender.RaReply(string.Format("${0} {1}", 1, "Please use 'Request' to select a report first."), true, true, string.Empty);
                yield break;
            }
            var resp = RemoteAdminModificationHandler.UpdateReport(report.Id.ToString(), sender.SenderId, HandleStatus.Handled, "Handled using ingame RemoteAdmin");
            yield return Timing.WaitUntilTrue(() => resp.IsCompleted);
            Timing.RunCoroutine(RaPlayerPatch.HandleReportType2(sender, player, new string[]
            {
                "0",
                "-2"
            }, $"<color=green>Report {report.Id} Completed</color>"));
        }
    }
}