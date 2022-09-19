﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CedMod.Addons.Events;
using CedMod.Addons.QuerySystem.WS;
using CedMod.ApiModals;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace CedMod.Addons.QuerySystem
{
    public class ThreadDispatcher : MonoBehaviour
    {
        public float timeLeftbeforeHeartBeat = 0;
        public float timeLeftbeforeWatchdog = 0;
        
        public void Start()
        {
            Log.Debug("ThreadDispatcher started", CedModMain.Singleton.Config.QuerySystem.Debug);
        }

        public static ConcurrentQueue<Action> ThreadDispatchQueue = new ConcurrentQueue<Action>();
        public void Update()
        {
            if (ThreadDispatchQueue.TryDequeue(out Action action))
            {
                try
                {
                    Log.Debug($"Invoking action", CedModMain.Singleton.Config.QuerySystem.Debug);
                    action.Invoke();
                    Log.Debug($"Action Invoked", CedModMain.Singleton.Config.QuerySystem.Debug);
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to invoke Dispatch\n{e}");
                }
            }

            timeLeftbeforeWatchdog -= Time.deltaTime;
            if (timeLeftbeforeWatchdog <= 0)
            {
                timeLeftbeforeWatchdog = 60f;
                if (!WebSocketSystem.Reconnect && !WebSocketSystem.SendThread.IsAlive)
                {
                    lock (WebSocketSystem.reconnectLock)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            WebSocketSystem.Reconnect = false;
                            Log.Error($"WS Watchdog: WS Sendthread not alive, restarting WS");
                            WebSocketSystem.Stop();
                            Thread.Sleep(1000);
                            WebSocketSystem.Start();
                        });
                    }
                }
                
                if (!WebSocketSystem.Reconnect && !WebSocketSystem.Socket.IsRunning && WebSocketSystem.LastConnection < DateTime.UtcNow.AddMinutes(-1))
                {
                    lock (WebSocketSystem.reconnectLock)
                    {
                        Task.Factory.StartNew(() =>
                        { 
                            WebSocketSystem.Reconnect = false;
                            Log.Error($"WS Watchdog: WS inactive out of reconnect mode without activity for 1 minutes, restarting WS");
                            WebSocketSystem.Stop();
                            Thread.Sleep(1000);
                            WebSocketSystem.Start();
                        });
                    }
                }
                
                if (WebSocketSystem.Reconnect && !WebSocketSystem.Socket.IsRunning && WebSocketSystem.LastConnection < DateTime.UtcNow.AddMinutes(-2))
                {
                    lock (WebSocketSystem.reconnectLock)
                    {
                        Task.Factory.StartNew(() =>
                        { 
                            WebSocketSystem.Reconnect = false;
                            Log.Error($"WS Watchdog: WS inactive in reconnect mode without activity for 2 minutes, restarting WS");
                            WebSocketSystem.Stop();
                            Thread.Sleep(1000);
                            WebSocketSystem.Start();
                        });
                    }
                }
                
                if (WebSocketSystem.LastConnection < DateTime.UtcNow.AddMinutes(-3))
                {
                    lock (WebSocketSystem.reconnectLock)
                    {
                        Task.Factory.StartNew(() =>
                        { 
                            WebSocketSystem.Reconnect = false;
                            Log.Error($"WS Watchdog: no activity for 3 minutes, restarting WS");
                            WebSocketSystem.Stop();
                            Thread.Sleep(1000);
                            WebSocketSystem.Start();
                        });
                    }
                }
            }

            if (WebSocketSystem.HelloMessage != null)
            {
                HandleQueryplayer.PlayerDummies.RemoveAll(s => s.GameObject == null);
                timeLeftbeforeHeartBeat -= Time.deltaTime;
                if (timeLeftbeforeHeartBeat <= 0)
                {
                    Log.Debug("Invoking heartbeat", CedModMain.Singleton.Config.QuerySystem.Debug);
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
                        Active = EventManager.currentEvent != null &&
                                 EventManager.currentEvent.EventPrefix == ev.EventPrefix,
                        Author = ev.EvenAuthor,
                        Description = ev.EventDescription,
                        Name = ev.EventName,
                        Prefix = ev.EventPrefix,
                        QueuePos = EventManager.nextEvent.Any(ev1 => ev1.EventName == ev.EventName)
                            ? EventManager.nextEvent.FindIndex(ev1 => ev1.EventName == ev.EventName) + 1
                            : -1
                    });
                }
            }

            if (WebSocketSystem.HelloMessage.SendStats)
            {
                foreach (var player in Player.List)
                {
                    players.Add(new PlayerObject()
                    {
                        DoNotTrack = player.DoNotTrack,
                        Name = player.Nickname,
                        Staff = PermissionsHandler.IsPermitted(player.ReferenceHub.serverRoles.Permissions, new PlayerPermissions[3] { PlayerPermissions.KickingAndShortTermBanning, PlayerPermissions.BanningUpToDay, PlayerPermissions.LongTermBanning}),
                        UserId = player.UserId,
                        PlayerId = player.Id,
                        RoleType = player.Role.Type
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
                            PluginVersion = CedModMain.Singleton.Version.ToString(),
                            UpdateStats = updateStats,
                            TrackingEnabled = LevelerStore.TrackingEnabled && EventManager.currentEvent == null
                        })
                    }
                }
            });
        }
    }
}
