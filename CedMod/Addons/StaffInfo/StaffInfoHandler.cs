using System;
using System.Collections.Generic;
using System.Linq;
using CedMod.Addons.QuerySystem.WS;
using CedMod.Components;
using CentralAuth;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.Spectating;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace CedMod.Addons.StaffInfo
{
    public class StaffInfoHandler: MonoBehaviour
    {
        public static Dictionary<string, Dictionary<string, Tuple<string, DateTime>>> StaffData = new Dictionary<string, Dictionary<string, Tuple<string, DateTime>>>();

        public static Dictionary<string, Dictionary<string, DateTime>> Requested = new Dictionary<string, Dictionary<string, DateTime>>();

        public static StaffInfoHandler Singleton;

        public void Start()
        {
            Singleton = this;
            WebSocketSystem.OnMessageReceived += QueryMessageReceived;
        }

        public void OnDestroy()
        {
            WebSocketSystem.OnMessageReceived -= QueryMessageReceived;
        }

        private void QueryMessageReceived(QueryCommand cmd)
        {
            if (cmd.Data["action"] == "StaffInfo")
            {
                var staffId = cmd.Data["StaffId"];
                Player staffPlayer = CedModPlayer.Get(staffId);
                
                var playerId = cmd.Data["PlayerId"];
                Player player = CedModPlayer.Get(playerId);
                
                if (staffPlayer == null || player == null)
                    return;

                StaffData[staffPlayer.UserId][player.UserId] = new Tuple<string, DateTime>(cmd.Data["Message"], DateTime.UtcNow);
                Requested[staffPlayer.UserId].Remove(player.UserId);
                
                string combined = cmd.Data["Message"];
                
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Logger.Debug($"received staffinfo {staffPlayer.Nickname} {player.Nickname}", CedModMain.Singleton.Config.QuerySystem.Debug);

                var watchlist = RemoteAdminModificationHandler.Watchlist.FirstOrDefault(s => s.Userid == player.UserId);
                var groupWatchlist = RemoteAdminModificationHandler.GroupWatchlist.Where(s => s.UserIds.Contains(player.UserId));

                if (watchlist != null)
                {
                    combined += $"<color=#00FFFF>Watchlist</color>\n{watchlist.Reason}\n";
                }

                foreach (var group in groupWatchlist)
                {
                    combined += $"<color=#00FFFF>Group watchlist WG{group.Id}</color>\n{watchlist.Reason}\n";
                }
                
                var prefs = RemoteAdminModificationHandler.IngameUserPreferencesMap[staffPlayer];
                StaffData[staffPlayer.UserId][player.UserId] = new Tuple<string, DateTime>(prefs.StreamerMode ? "" : combined, DateTime.UtcNow);
            }
        }

        public float Timer = 0;

        public void Update()
        {
            Timer -= Time.deltaTime;
            
            if (Timer >= 0)
                return;

            Timer = 1;
            
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Logger.Debug("Starting staffinfo", CedModMain.Singleton.Config.QuerySystem.Debug);
            
            foreach (var staff in Player.List)
            {
                if (!staff.RemoteAdminAccess)
                    continue;
                
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Logger.Debug($"Staffinfo for {staff.Nickname}", CedModMain.Singleton.Config.QuerySystem.Debug);
                
                if (!StaffData.ContainsKey(staff.UserId))
                    StaffData.Add(staff.UserId, new Dictionary<string, Tuple<string, DateTime>>());
                
                if (!Requested.ContainsKey(staff.UserId))
                    Requested.Add(staff.UserId, new Dictionary<string, DateTime>());
                
                if (!RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(staff) && !RemoteAdminModificationHandler.Singleton.Requesting.Contains(staff.UserId))
                {
                    RemoteAdminModificationHandler.Singleton.ResolvePreferences(staff, null);
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Logger.Debug($"Staffinfo for {staff.Nickname} no prefs", CedModMain.Singleton.Config.QuerySystem.Debug);
                }
                
                if (!RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(staff))
                    continue;
                
                var prefs = RemoteAdminModificationHandler.IngameUserPreferencesMap[staff];

                foreach (var player in Player.List)
                {
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Logger.Debug($"Staffinfo for {staff.Nickname} getting {player.Nickname}", CedModMain.Singleton.Config.QuerySystem.Debug);
                    
                    if (player.ReferenceHub.authManager.InstanceMode != ClientInstanceMode.ReadyClient)
                        continue;

                    if (staff.Role == RoleTypeId.Tutorial && !prefs.StreamerMode && StaffData[staff.UserId].ContainsKey(player.UserId))
                    {
                        player.SendFakeCustomInfo(staff, StaffInfoHandler.StaffData[staff.UserId][player.UserId].Item1);
                    }
                    else
                    {
                        player.SendFakeCustomInfo(staff, "");
                    }
                    
                    if (Requested[staff.UserId].ContainsKey(player.UserId) && Requested[staff.UserId][player.UserId] > DateTime.UtcNow.AddSeconds(-5))
                        continue;

                    Requested[staff.UserId].Remove(player.UserId);

                    if (StaffData[staff.UserId].ContainsKey(player.UserId) && StaffData[staff.UserId][player.UserId].Item2 > DateTime.UtcNow.AddMinutes(-3))
                        continue;
                    
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Logger.Debug($"Staffinfo for {staff.Nickname} getting final {player.Nickname}");

                    RequestInfo(staff, player);
                }
                
                if (prefs.StreamerMode)
                    continue;

                var currentlySpectating = Player.List.FirstOrDefault(s => s.ReferenceHub.IsSpectatedBy(staff.ReferenceHub));
                
                switch (staff.Role)
                {
                    case RoleTypeId.Overwatch when prefs.ShowModerationInfoOverwatch && currentlySpectating != null && StaffData[staff.UserId].ContainsKey(currentlySpectating.UserId) && StaffData[staff.UserId][currentlySpectating.UserId].Item1 != "":
                    case RoleTypeId.Spectator when prefs.ShowModerationInfoSpectator && currentlySpectating != null && StaffData[staff.UserId].ContainsKey(currentlySpectating.UserId) && StaffData[staff.UserId][currentlySpectating.UserId].Item1 != "":
                        staff.ReceiveHint("<align=right><size=25>" + StaffData[staff.UserId][currentlySpectating.UserId].Item1 + "</size>", 1.2f);
                        break;
                }
            }
        }

        public void RequestInfo(Player staff, Player player)
        {
            Requested[staff.UserId][player.UserId] = DateTime.UtcNow;
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    {"Message", "RequestData"},
                    {"UserId", staff.UserId},
                    {"TargetId", player.UserId},
                }
            });
        }
    }
}