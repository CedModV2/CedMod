using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CedMod.Addons.Audio;
using CedMod.Addons.Sentinal;
using CedMod.Addons.Sentinal.Patches;
using CedMod.Components;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using Mirror;

namespace CedMod.Handlers
{
    public class Server: CustomEventsHandler
    {
        public static Dictionary<ReferenceHub, ReferenceHub> reported = new Dictionary<ReferenceHub, ReferenceHub>();

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
                        ev.Prefix +=
                            $"<size=15><color=#00FFF6>[WMG{RemoteAdminModificationHandler.GroupWatchlist.Count(s => s.UserIds.Contains(ev.Player.UserId))}]</color></size> ";
                    }
                    else
                    {
                        ev.Prefix +=
                            $"<size=15><color=#00FFF6>[WG{RemoteAdminModificationHandler.GroupWatchlist.FirstOrDefault(s => s.UserIds.Contains(ev.Player.UserId)).Id}]</color></size> ";
                    }
                }
                else if (RemoteAdminModificationHandler.Watchlist.Any(s => s.Userid == ev.Player.UserId))
                {
                    ev.Prefix += $"<size=15><color=#00FFF6>[WL]</color></size> ";
                }
            }
            base.OnPlayerRaPlayerListAddingPlayer(ev);
        }
    }
}