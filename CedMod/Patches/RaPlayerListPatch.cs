using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using CedMod.Components;
using HarmonyLib;
using MEC;
using NorthwoodLib.Pools;
using RemoteAdmin;
using RemoteAdmin.Communication;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(RaPlayerList), nameof(RaPlayerList.ReceiveData), typeof(CommandSender), typeof(string))]
    public static class RaPlayerListPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = ListPool<CodeInstruction>.Shared.Rent(instructions);

            int createListIndex = list.FindIndex(i => i.operand is MethodInfo {Name: "Rent"}) + 2;
            list.InsertRange(createListIndex, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldloc, 8),
                CodeInstruction.Call(typeof(RaPlayerListPatch), nameof(InsertReports))
            });

            int ifStart = list.FindLastIndex(i => i.operand is MethodInfo {Name: "get_Mode"}) + 2;
            int ifEnd = list.FindLastIndex(i => i.operand is MethodInfo {Name: "AppendLine"}) + 2;

            list.RemoveRange(ifStart, ifEnd - ifStart);
            list.InsertRange(ifStart, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldloc, 8),
                new CodeInstruction(OpCodes.Ldloc, 10),
                new CodeInstruction(OpCodes.Ldloc, 5),
                new CodeInstruction(OpCodes.Ldloc, 6),
                CodeInstruction.Call(typeof(RaPlayerListPatch), nameof(AppendPlayerToList)),
            });

            foreach (var codeInstruction in list)
                yield return codeInstruction;

            ListPool<CodeInstruction>.Shared.Return(list);
        }

        public static void InsertReports(CommandSender sender, StringBuilder stringBuilder)
        {
            if (!CommandProcessor.CheckPermissions(sender, PlayerPermissions.PlayersManagement))
                return;
            var plr = CedModPlayer.Get(sender.SenderId);
            if (!RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr))
            {
                Timing.RunCoroutine(RemoteAdminModificationHandler.Singleton.ResolvePreferences(plr, null));
            }

            if (!RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr) || !RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowReportsInRemoteAdmin)
                return;
            var openCount = RemoteAdminModificationHandler.ReportsList.Count(s => s.Status == HandleStatus.NoResponse);
            var inProgressCount = RemoteAdminModificationHandler.ReportsList.Count(s => s.Status == HandleStatus.InProgress);

            if (openCount == 0)
            {
                stringBuilder.Append("<size=0>(").Append(-1).Append(")</size>");
                stringBuilder.Append("<color=green>[No Open Reports]</color>");
            }
            else
            {
                stringBuilder.Append("<size=0>(").Append(-1).Append(")</size>");
                stringBuilder.Append($"{(RemoteAdminModificationHandler.UiBlink ? "[<color=yellow>⚠</color>] " : " ")}<color=red>[{openCount} Open Report{(openCount == 1 ? "" : "s")}]</color>");
            }

            stringBuilder.AppendLine();

            if (inProgressCount == 0)
            {
                stringBuilder.Append("<size=0>(").Append(-2).Append(")</size>");
                stringBuilder.Append("<color=green>[No InProgress Reports]</color>");
            }
            else
            {
                stringBuilder.Append("<size=0>(").Append(-2).Append(")</size>");
                stringBuilder.Append($"{(RemoteAdminModificationHandler.UiBlink ? "[<color=yellow>⚠</color>] " : " ")}<color=orange>[{inProgressCount} Report{(inProgressCount == 1 ? "" : "s")} Inprogress]</color>");
            }

            stringBuilder.AppendLine();
        }
        public static void AppendPlayerToList(RaPlayerList instance, CommandSender sender, StringBuilder stringBuilder, ReferenceHub hub, bool viewHiddenBadges, bool viewHiddenGlobalBadges)
        {

            var plr = CedModPlayer.Get(sender.SenderId);
            if (!RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr))
            {
                RemoteAdminModificationHandler.IngameUserPreferencesMap[plr] = new()
                {
                    ShowReportsInRemoteAdmin = true
                };
                Timing.RunCoroutine(RemoteAdminModificationHandler.Singleton.ResolvePreferences(plr, null));
            }

            bool flag2 = false;
            stringBuilder.Append(instance.GetPrefix(hub, viewHiddenBadges, viewHiddenGlobalBadges));
            stringBuilder.Append(flag2 ? "<link=RA_OverwatchEnabled><color=white>[</color><color=#03f8fc>\uF06E</color><color=white>]</color></link> " : string.Empty);
            stringBuilder.Append("<color={RA_ClassColor}>(").Append(hub.PlayerId).Append(") ");
            if (RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr) && RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin)
            {
                if (RemoteAdminModificationHandler.GroupWatchlist.Any(s => s.UserIds.Contains(hub.characterClassManager.UserId)))
                {
                    if (RemoteAdminModificationHandler.GroupWatchlist.Count(s => s.UserIds.Contains(hub.characterClassManager.UserId)) >= 2)
                    {
                        stringBuilder.Append($"<size=15><color=#00FFF6>[WMG{RemoteAdminModificationHandler.GroupWatchlist.Count(s => s.UserIds.Contains(hub.characterClassManager.UserId))}]</color></size> ");
                    }
                    else
                    {
                        stringBuilder.Append($"<size=15><color=#00FFF6>[WG{RemoteAdminModificationHandler.GroupWatchlist.FirstOrDefault(s => s.UserIds.Contains(hub.characterClassManager.UserId)).Id}]</color></size> ");
                    }
                }
                else if (RemoteAdminModificationHandler.Watchlist.Any(s => s.Userid == hub.characterClassManager.UserId))
                {
                    stringBuilder.Append($"<size=15><color=#00FFF6>[WL]</color></size> ");
                }
            }
            stringBuilder.Append(hub.nicknameSync.CombinedName.Replace("\n", string.Empty).Replace("RA_", string.Empty)).Append("</color>");
            stringBuilder.AppendLine();
        }
    }
}