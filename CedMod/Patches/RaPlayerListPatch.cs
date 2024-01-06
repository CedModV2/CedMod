using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using CedMod.Components;
using CentralAuth;
using HarmonyLib;
using MEC;
using Mirror;
using Newtonsoft.Json;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using PluginAPI.Core;
using RemoteAdmin;
using RemoteAdmin.Communication;
using UnityEngine;
using UnityEngine.Networking;
using Utils;
using VoiceChat;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(RaPlayerList), nameof(RaPlayerList.ReceiveData), new Type[] { typeof(CommandSender), typeof(string) })]
    public static class RaPlayerListPatch
    {
        public static Dictionary<string, CoroutineHandle> Handles = new Dictionary<string, CoroutineHandle>();

        public static bool Prefix(RaPlayerList __instance, CommandSender sender, string data)
        {
            if (Handles.ContainsKey(sender.SenderId) && Handles[sender.SenderId].IsRunning)
                return false;
            
            Handles[sender.SenderId] = Timing.RunCoroutine(RaPlayerCoRoutine(__instance, sender, data));
            return false;
        }

        public static IEnumerator<float> RaPlayerCoRoutine(RaPlayerList __instance, CommandSender sender, string data)
        {
            string[] strArray = data.Split(' ');
            int result1;
            int result2;
            if (strArray.Length != 3 || !int.TryParse(strArray[0], out result1) || !int.TryParse(strArray[1], out result2) || !Enum.IsDefined(typeof (RaPlayerList.PlayerSorting), (object) result2))
                yield break;
            bool flag1 = result1 == 1;
            int num = strArray[2].Equals("1") ? 1 : 0;
            RaPlayerList.PlayerSorting sortingType = (RaPlayerList.PlayerSorting) result2;
            bool viewHiddenBadges = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenBadges);
            bool viewHiddenGlobalBadges = CommandProcessor.CheckPermissions(sender, PlayerPermissions.ViewHiddenGlobalBadges);
            if (sender is PlayerCommandSender playerCommandSender && playerCommandSender.ReferenceHub.authManager.NorthwoodStaff)
            {
                viewHiddenBadges = true;
                viewHiddenGlobalBadges = true;
            }
            
            StringBuilder stringBuilder = StringBuilderPool.Shared.Rent("\n");
            stringBuilder.Clear();

            if (CommandProcessor.CheckPermissions(sender, PlayerPermissions.AdminChat))
            {
                var plr = CedModPlayer.Get(sender.SenderId);
                if (!RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr) && !RemoteAdminModificationHandler.Singleton.Requesting.Contains(plr.UserId))
                {
                    RemoteAdminModificationHandler.Singleton.ResolvePreferences(plr, null);
                }

                if (RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr) && RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowReportsInRemoteAdmin)
                {
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
            }

            foreach (ReferenceHub hub in num != 0 ? __instance.SortPlayersDescending(sortingType) : __instance.SortPlayers(sortingType))
            {
                if (hub.Mode != ClientInstanceMode.DedicatedServer && hub.Mode != ClientInstanceMode.Unverified)
                {
                    var plr = CedModPlayer.Get(sender.SenderId);
                    if (!RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr) && !RemoteAdminModificationHandler.Singleton.Requesting.Contains(plr.UserId))
                    {
                        RemoteAdminModificationHandler.Singleton.ResolvePreferences(plr, null);
                    }
                    
                    
                    stringBuilder.Append(RaPlayerList.GetPrefix(hub, viewHiddenBadges, viewHiddenGlobalBadges));
                    stringBuilder.Append(hub.serverRoles.IsInOverwatch ? "<link=RA_OverwatchEnabled><color=white>[</color><color=#03f8fc>\uF06E</color><color=white>]</color></link> " : string.Empty);
                    stringBuilder.Append("<color={RA_ClassColor}>(").Append(hub.PlayerId).Append(") ");
                    if (RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(plr) && RemoteAdminModificationHandler.IngameUserPreferencesMap[plr].ShowWatchListUsersInRemoteAdmin)
                    {
                        if (RemoteAdminModificationHandler.GroupWatchlist.Any(s => s.UserIds.Contains(hub.authManager.UserId)))
                        {
                            if (RemoteAdminModificationHandler.GroupWatchlist.Count(s => s.UserIds.Contains(hub.authManager.UserId)) >= 2)
                            {
                                stringBuilder.Append($"<size=15><color=#00FFF6>[WMG{RemoteAdminModificationHandler.GroupWatchlist.Count(s => s.UserIds.Contains(hub.authManager.UserId))}]</color></size> ");
                            }
                            else
                            {
                                stringBuilder.Append($"<size=15><color=#00FFF6>[WG{RemoteAdminModificationHandler.GroupWatchlist.FirstOrDefault(s => s.UserIds.Contains(hub.authManager.UserId)).Id}]</color></size> ");
                            }
                        }
                        else if (RemoteAdminModificationHandler.Watchlist.Any(s => s.Userid == hub.authManager.UserId))
                        {
                            stringBuilder.Append($"<size=15><color=#00FFF6>[WL]</color></size> ");
                        }
                    }
                    stringBuilder.Append(hub.nicknameSync.CombinedName.Replace("\n", string.Empty).Replace("RA_", string.Empty)).Append("</color>");
                    stringBuilder.AppendLine();
                }
            }
            sender.RaReply(string.Format("${0} {1}", (object) __instance.DataId, (object) StringBuilderPool.Shared.ToStringReturn(stringBuilder)), true, !flag1, string.Empty);
        }
    }
}