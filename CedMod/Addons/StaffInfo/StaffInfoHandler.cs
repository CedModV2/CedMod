using System;
using System.Collections.Generic;
using System.Linq;
using CedMod.Addons.QuerySystem.WS;
using CedMod.Components;
using CentralAuth;
using PlayerRoles;
using PlayerRoles.Spectating;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using UnityEngine;
using Utils.NonAllocLINQ;

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
                CedModPlayer staffPlayer = CedModPlayer.Get(staffId);
                
                var playerId = cmd.Data["PlayerId"];
                CedModPlayer player = CedModPlayer.Get(playerId);
                
                if (staffPlayer == null || player == null)
                    return;

                StaffData[staffPlayer.UserId][player.UserId] = new Tuple<string, DateTime>(cmd.Data["Message"], DateTime.UtcNow);
                Requested[staffPlayer.UserId].Remove(player.UserId);
                
                string combined = cmd.Data["Message"];
                
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug($"received staffinfo {staffPlayer.Nickname} {player.Nickname}", CedModMain.Singleton.Config.QuerySystem.Debug);

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
                
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug($"recived staffinfo 2 {staffPlayer.Nickname} {player.Nickname}", CedModMain.Singleton.Config.QuerySystem.Debug);
                
                if (combined != "")
                    player.SendFakeCustomInfo(staffPlayer, combined);
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
                Log.Debug("Starting staffinfo", CedModMain.Singleton.Config.QuerySystem.Debug);
            
            foreach (var staff in CedModPlayer.GetPlayers<CedModPlayer>())
            {
                if (!staff.RemoteAdminAccess)
                    continue;
                
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug($"Staffinfo for {staff.Nickname}", CedModMain.Singleton.Config.QuerySystem.Debug);
                
                if (!StaffData.ContainsKey(staff.UserId))
                    StaffData.Add(staff.UserId, new Dictionary<string, Tuple<string, DateTime>>());
                
                if (!Requested.ContainsKey(staff.UserId))
                    Requested.Add(staff.UserId, new Dictionary<string, DateTime>());
                
                if (!RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(staff) && !RemoteAdminModificationHandler.Singleton.Requesting.Contains(staff.UserId))
                {
                    RemoteAdminModificationHandler.Singleton.ResolvePreferences(staff, null);
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug($"Staffinfo for {staff.Nickname} no prefs", CedModMain.Singleton.Config.QuerySystem.Debug);
                }
                
                if (!RemoteAdminModificationHandler.IngameUserPreferencesMap.ContainsKey(staff))
                    continue;

                foreach (var player in CedModPlayer.GetPlayers())
                {
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug($"Staffinfo for {staff.Nickname} getting {player.Nickname}", CedModMain.Singleton.Config.QuerySystem.Debug);
                    if (player.ReferenceHub.authManager.InstanceMode != ClientInstanceMode.ReadyClient)
                        continue;
                    
                    if (Requested[staff.UserId].ContainsKey(player.UserId) && Requested[staff.UserId][player.UserId] > DateTime.UtcNow.AddSeconds(-5))
                        continue;

                    Requested[staff.UserId].Remove(player.UserId);

                    if (StaffData[staff.UserId].ContainsKey(player.UserId) && StaffData[staff.UserId][player.UserId].Item2 > DateTime.UtcNow.AddMinutes(-3))
                        continue;
                    
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug($"Staffinfo for {staff.Nickname} getting final {player.Nickname}");

                    RequestInfo(staff, player);
                }

                var prefs = RemoteAdminModificationHandler.IngameUserPreferencesMap[staff];

                var currentlySpectating = CedModPlayer.GetPlayers().FirstOrDefault(s => s.ReferenceHub.IsSpectatedBy(staff.ReferenceHub));
                
                switch (staff.Role)
                {
                    case RoleTypeId.Overwatch when prefs.ShowModerationInfoOverwatch && currentlySpectating != null && StaffData[staff.UserId].ContainsKey(currentlySpectating.UserId) && StaffData[staff.UserId][currentlySpectating.UserId].Item1 != "":
                    case RoleTypeId.Spectator when prefs.ShowModerationInfoSpectator && currentlySpectating != null && StaffData[staff.UserId].ContainsKey(currentlySpectating.UserId) && StaffData[staff.UserId][currentlySpectating.UserId].Item1 != "":
                        staff.ReceiveHint(StaffData[staff.UserId][currentlySpectating.UserId].Item1, 1.2f);
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