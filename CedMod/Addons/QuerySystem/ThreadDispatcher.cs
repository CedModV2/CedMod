using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using CedMod.Addons.Audio;
using CedMod.Addons.Events;
using CedMod.Addons.QuerySystem.WS;
using CedMod.ApiModals;
using CentralAuth;
using Exiled.Loader;
using Newtonsoft.Json;
using PluginAPI.Core;
using SCPSLAudioApi.AudioCore;
using UnityEngine;
using Version = GameCore.Version;

namespace CedMod.Addons.QuerySystem
{
    public class ThreadDispatcher : MonoBehaviour
    {
        public float timeLeftbeforeAudioHeartBeat = 0;
        public float timeLeftbeforeHeartBeat = 0;
        public float timeLeftbeforeWatchdog = 0;
        
        public void Start()
        {
            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                Log.Debug("ThreadDispatcher started");
        }

        public static ConcurrentQueue<Action> ThreadDispatchQueue = new ConcurrentQueue<Action>();
        public void Update()
        {
            if (QuerySystem.UseWhitelist && !ServerConsole.WhiteListEnabled)
                ServerConsole.WhiteListEnabled = true;
            
            if (ThreadDispatchQueue.TryDequeue(out Action action))
            {
                try
                {
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug($"Invoking action");
                    action.Invoke();
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug($"Action Invoked");
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to invoke Dispatch\n{e}");
                }
            }

            timeLeftbeforeAudioHeartBeat -= Time.deltaTime;
            if (timeLeftbeforeAudioHeartBeat <= 0)
            {
                timeLeftbeforeAudioHeartBeat = AudioPlayerBase.AudioPlayers.Count >= 1 ? 1 : 20;

                List<CedModAudioPlayer> audioPlayers = new List<CedModAudioPlayer>();

                foreach (var player in AudioPlayerBase.AudioPlayers)
                {
                    audioPlayers.Add(new CedModAudioPlayer()
                    {
                        Continue = player.Value.Continue,
                        CurrentSpan = player.Value.VorbisReader == null ? TimeSpan.Zero : player.Value.VorbisReader.TimePosition,
                        LoopList = player.Value.Loop,
                        PlayerId = player.Key.PlayerId,
                        Queue = player.Value.AudioToPlay,
                        Shuffle = player.Value.Shuffle,
                        TotalTime = player.Value.VorbisReader == null ? TimeSpan.Zero : player.Value.VorbisReader.TotalTime,
                        Volume = player.Value.Volume,
                        IsCedModPlayer = player.Value is CustomAudioPlayer
                    });
                }
                
                WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
                {
                    Recipient = "PANEL",
                    Data = new Dictionary<string, string>()
                    {
                        { "Message", "AUDIOHEARTBEAT" },
                        {
                            "HeartbeatInfo", JsonConvert.SerializeObject(new AudioHeartbeatRequest()
                            {
                                Players = audioPlayers
                            })
                        }
                    }
                });
            }

            timeLeftbeforeWatchdog -= Time.deltaTime;
            if (timeLeftbeforeWatchdog <= 0)
            {
                timeLeftbeforeWatchdog = 60f;
                if (!WebSocketSystem.Reconnect && !WebSocketSystem.SendThread.IsAlive)
                {
                    lock (WebSocketSystem.reconnectLock)
                    {
                        Task.Run(async () =>
                        {
                            WebSocketSystem.Reconnect = false;
                            Log.Error($"WS Watchdog: WS Sendthread not alive, restarting WS");
                            WebSocketSystem.Stop();
                            await Task.Delay(1000, CedModMain.CancellationToken);
                            await WebSocketSystem.Start();
                        });
                    }
                }
                
                if (!WebSocketSystem.Reconnect && !WebSocketSystem.Socket.IsRunning && WebSocketSystem.LastConnection < DateTime.UtcNow.AddMinutes(-1))
                {
                    lock (WebSocketSystem.reconnectLock)
                    {
                        Task.Run(async () =>
                        { 
                            WebSocketSystem.Reconnect = false;
                            Log.Error($"WS Watchdog: WS inactive out of reconnect mode without activity for 1 minutes, restarting WS");
                            await WebSocketSystem.Stop();
                            await Task.Delay(1000, CedModMain.CancellationToken);
                            await WebSocketSystem.Start();
                        });
                    }
                }
                
                if (WebSocketSystem.Reconnect && !WebSocketSystem.Socket.IsRunning && WebSocketSystem.LastConnection < DateTime.UtcNow.AddMinutes(-2))
                {
                    lock (WebSocketSystem.reconnectLock)
                    {
                        Task.Run(async () =>
                        { 
                            WebSocketSystem.Reconnect = false;
                            Log.Error($"WS Watchdog: WS inactive in reconnect mode without activity for 2 minutes, restarting WS");
                            await WebSocketSystem.Stop();
                            await Task.Delay(1000, CedModMain.CancellationToken);
                            await WebSocketSystem.Start();
                        });
                    }
                }
                
                if (WebSocketSystem.LastConnection < DateTime.UtcNow.AddMinutes(-3))
                {
                    lock (WebSocketSystem.reconnectLock)
                    {
                        Task.Run(async () =>
                        { 
                            WebSocketSystem.Reconnect = false;
                            Log.Error($"WS Watchdog: no activity for 3 minutes, restarting WS");
                            await WebSocketSystem.Stop();
                            await Task.Delay(1000, CedModMain.CancellationToken);
                            await WebSocketSystem.Start();
                        });
                    }
                }
            }

            if (WebSocketSystem.HelloMessage != null)
            {
                timeLeftbeforeHeartBeat -= Time.deltaTime;
                if (timeLeftbeforeHeartBeat <= 0)
                {
                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                        Log.Debug("Invoking heartbeat");
                    timeLeftbeforeHeartBeat = 20;
                    SendHeartbeatMessage(true);
                }
            }
        }

        public static void SendHeartbeatMessage(bool updateStats)
        {
            if (WebSocketSystem.HelloMessage == null)
                return;
            List<EventModal> events = new List<EventModal>();
            List<PlayerObject> players = new List<PlayerObject>();
            if (WebSocketSystem.HelloMessage.SendEvents)
            {
                foreach (var ev in EventManager.AvailableEvents)
                {
                    events.Add(new EventModal()
                    {
                        Active = EventManager.CurrentEvent != null &&
                                 EventManager.CurrentEvent.EventPrefix == ev.EventPrefix,
                        Author = ev.EvenAuthor,
                        Description = ev.EventDescription,
                        Name = ev.EventName,
                        Prefix = ev.EventPrefix,
                        QueuePos = EventManager.EventQueue.Any(ev1 => ev1.EventName == ev.EventName)
                            ? EventManager.EventQueue.FindIndex(ev1 => ev1.EventName == ev.EventName) + 1
                            : -1
                    });
                }
            }

            if (WebSocketSystem.HelloMessage.SendStats)
            {
                foreach (ReferenceHub player in ReferenceHub.AllHubs)
                {
                    if (player.authManager.InstanceMode != ClientInstanceMode.ReadyClient)
                        continue;
                    
                    if (player.isLocalPlayer)
                        continue;
                    bool staff = false;

                    var data = ServerStatic.GetPermissionsHandler();
                    if (player.authManager.UserId != null && data._members.ContainsKey(player.authManager.UserId))
                    {
                        var group = data.GetGroup(data._members[player.authManager.UserId]);
                        if (group != null)
                        {
                            staff = PermissionsHandler.IsPermitted(group.Permissions, new PlayerPermissions[3] { PlayerPermissions.KickingAndShortTermBanning, PlayerPermissions.BanningUpToDay, PlayerPermissions.LongTermBanning });
                        }
                    }
                    
                    players.Add(new PlayerObject()
                    {
                        DoNotTrack = player.authManager.DoNotTrack,
                        Name = player.nicknameSync.Network_myNickSync,
                        Staff = staff,
                        UserId = player.authManager.UserId,
                        PlayerId = player.PlayerId,
                        RoleType = player.roleManager.CurrentRole.RoleTypeId,
                        HashedUserId = player.authManager.AuthenticationResponse.AuthToken.SyncHashed
                    });
                }
            }
            
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    { "Message", "HEARTBEAT" },
                    {
                        "HeartbeatInfo", JsonConvert.SerializeObject(new HeartbeatRequest()
                        {
                            Events = events,
                            Players = players,
                            PluginCommitHash = CedModMain.GitCommitHash,
                            PluginVersion = CedModMain.PluginVersion,
                            UpdateStats = updateStats,
                            TrackingEnabled = LevelerStore.TrackingEnabled && EventManager.CurrentEvent == null,
                            CedModVersionIdentifier = CedModMain.VersionIdentifier,
#if !EXILED
                            ExiledVersion = "NWAPI",
#else
                            ExiledVersion = Loader.Version.ToString(),
#endif
                            ScpSlVersion = $"{Version.Major}.{Version.Minor}.{Version.Revision}",
                            FileHash = CedModMain.FileHash,
                            KeyHash = CedModMain.GetHashCode(CedModMain.Singleton.Config.CedMod.CedModApiKey, new MD5CryptoServiceProvider()),
                            IsVerified = CustomNetworkManager.IsVerified
                        })
                    }
                }
            });
        }
    }
}
