using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AdminToys;
using AudioPooling;
using CedMod.Addons.QuerySystem.WS;
using CedMod.Addons.Sentinal.Patches;
using CedMod.Addons.Sentinal.Patches.Utilities;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;
using LabApi.Events.Arguments.Scp106Events;
using Newtonsoft.Json;
using PlayerRoles;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using UnityEngine;
using Utils.NonAllocLINQ;
using Logger = LabApi.Features.Console.Logger;

namespace CedMod.Addons.Sentinal
{
    public class SentinalBehaviour: MonoBehaviour
    {
        public static Dictionary<uint, (float, AudioClip clip, float maxDistance)> GunshotSource = new Dictionary<uint, (float, AudioClip clip, float maxDistance)>();
        public static Dictionary<uint, float> Stalking106 = new Dictionary<uint, float>();
        public float AuthTimer = 1;
        public Dictionary<int, Dictionary<uint, List<(int, float, float)>>> FrameCount = new Dictionary<int, Dictionary<uint, List<(int, float, float)>>>();
        Dictionary<uint, Dictionary<int, List<(int, float, float)>>> UserFrames = new Dictionary<uint, Dictionary<int, List<(int, float, float)>>>();
        public static ConcurrentQueue<(ReferenceHub hub, Vector2 eulerAngles, Vector3 position, ulong UFrames)> DirtyPositions = new ConcurrentQueue<(ReferenceHub hub, Vector2 eulerAngles, Vector3 position, ulong UFrames)>();
        public static int Frames = 0;
        public static ulong UFrames = 0;
        public static string RoundGuid = Guid.NewGuid().ToString();
        public static ulong UFrameLastPosSent = UFrames;
        private Dictionary<ReferenceHub, List<(string userid, string rotation, string position, ulong UFrames)>> _moveFrames = new Dictionary<ReferenceHub, List<(string userid, string rotation, string position, ulong UFrames)>>();
        private static List<uint> toRemoveUints = new List<uint>();

        public void Start()
        {
            AudioModule.OnSoundPlayed += OnSoundPlayed;
            LabApi.Events.Handlers.Scp106Events.ChangedStalkMode += StalkChanged;
            LabApi.Events.Handlers.ServerEvents.MapGenerated += RoomCache.MapGenerated;
            ItemBase.OnItemAdded += ItemCreated;
            ItemBase.OnItemRemoved += ItemRemoved;
        }

        private void ItemRemoved(ItemBase obj)
        {
            if (obj is Firearm firearm && firearm.TryGetModule(out RecoilPatternModule recoil))
            {
                if (recoil._counter != null)
                    recoil._counter.OnShotRecorded -= recoil.OnShot;
                recoil._counter?.Destruct();
                recoil._counter = null;
            }
        }

        private void ItemCreated(ItemBase obj)
        {
            if (obj is Firearm firearm && firearm.TryGetModule(out RecoilPatternModule recoil))
            {
                recoil._counter = new SubsequentShotsCounter(firearm);
                recoil._counter.OnShotRecorded += recoil.OnShot;
            }
        }

        private void OnSoundPlayed(ItemIdentifier arg1, PlayerRoleBase arg2, PooledAudioSource arg3)
        {
            if (arg2.TryGetOwner(out var owner))
            {
                GunshotSource[owner.netId] = (Time.time + 1.5f, arg3.Source.clip, arg3.Source.maxDistance);
            }
        }

        private void StalkChanged(Scp106ChangedStalkModeEventArgs ev)
        {
            Stalking106[ev.Player.NetworkId] = Time.time;
        }

        public void OnDestroy()
        {
            AudioModule.OnSoundPlayed -= OnSoundPlayed;
            LabApi.Events.Handlers.Scp106Events.ChangedStalkMode -= StalkChanged;
            LabApi.Events.Handlers.ServerEvents.MapGenerated -= RoomCache.MapGenerated;
            ItemBase.OnItemAdded -= ItemCreated;
            ItemBase.OnItemRemoved -= ItemRemoved;
        }

        public void FixedUpdate()
        {
            
            if (UFrames == ulong.MaxValue) //precaution
                UFrames = 0;
            
            UFrames++;
            
            if (Frames == int.MaxValue) //precaution
                Frames = 0;
            
            Frames++;
            
            foreach (var pack in VoicePacketPacket.PacketsSent)
            {
                //Logger.Info($"{Frames} | {pack.Value}");
                bool offSize = false;
                foreach (var data in pack.Value)
                {
                    if (data.Item1 != 480 || data.Item2 <= -6 || data.Item3 >= 6)
                        offSize = true;
                }
                
                if (pack.Value.Count >= 30 || offSize) //keep track of all exceeding packets per frame
                {
                    FrameCount.TryAdd(Frames, new Dictionary<uint, List<(int, float, float)>>());
                    FrameCount[Frames].TryAdd(pack.Key, pack.Value);
                }
            }

            if (UFrames - UFrameLastPosSent >= 25)
            {
                _moveFrames.Clear();
                while (DirtyPositions.TryDequeue(out var pos))
                {
                    _moveFrames[pos.hub] = _moveFrames.ContainsKey(pos.hub) ? _moveFrames[pos.hub] : new List<(string userid, string rotation, string position, ulong UFrames)>();
                    if (pos.hub.authManager == null || string.IsNullOrEmpty(pos.hub.authManager.UserId))
                        continue;
                    
                    _moveFrames[pos.hub].Add((pos.hub.authManager.UserId, pos.eulerAngles.ToString(), pos.position.ToString(), pos.UFrames));
                }

                foreach (var frame in _moveFrames)
                {
                    WebSocketSystem.Enqueue(new QueryCommand()
                    {
                        Recipient = "PANEL",
                        Data = new Dictionary<string, string>()
                        {
                            {"Type", "OnPlayerMoveBatch"},
                            {"UserId", frame.Key.authManager.UserId},
                            {"Frames", JsonConvert.SerializeObject(frame.Value)}
                        }
                    });
                }

                UFrameLastPosSent = UFrames;
            }
            
            AuthTimer -= UnityEngine.Time.fixedDeltaTime;

            //process detections after the check
            if (AuthTimer <= 0)
            {
                toRemoveUints.Clear();
                foreach (var rad in VoicePacketPacket.Radio)
                {
                    if (rad.Value < Time.time)
                        toRemoveUints.Add(rad.Key);
                }

                foreach (var rem in toRemoveUints)
                {
                    VoicePacketPacket.Radio.Remove(rem);
                }
                toRemoveUints.Clear();
                
                foreach (var rad in VoicePacketPacket.Voice)
                {
                    if (rad.Value < Time.time)
                        toRemoveUints.Add(rad.Key);
                }

                foreach (var rem in toRemoveUints)
                {
                    VoicePacketPacket.Voice.Remove(rem);
                }
                toRemoveUints.Clear(); 
                
                foreach (var rad in FpcServerPositionDistributorPatch.LastAudioSent)
                {
                    if (rad.Value < Time.time)
                        toRemoveUints.Add(rad.Key);
                }

                foreach (var rem in toRemoveUints)
                {
                    FpcServerPositionDistributorPatch.LastAudioSent.Remove(rem);
                }
                toRemoveUints.Clear(); 
                
                VoicePacketPacket.Tracker.Clear();
                AuthTimer = 1;
                
                //process framedata into easily readable data for api
                foreach (var detectionPackets in FrameCount)
                {
                    foreach (var frameData in detectionPackets.Value)
                    {
                        UserFrames.TryAdd(frameData.Key, new Dictionary<int, List<(int, float, float)>>());
                        UserFrames[frameData.Key].TryAdd(detectionPackets.Key, frameData.Value);
                    }
                }

                foreach (var userFrames in UserFrames)
                {
                    var plr = ReferenceHub.AllHubs.FirstOrDefault(s => s.netId == userFrames.Key, null);
                    if (plr != null)
                    {
                        Task.Run(async () =>
                        {
                            using (HttpClient client = new HttpClient())
                            {
                                client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                                await VerificationChallenge.AwaitVerification();
                                try
                                {
                                    var response = await client.PostAsync($"http{(QuerySystem.QuerySystem.UseSSL ? "s" : "")}://{QuerySystem.QuerySystem.CurrentMaster}/Api/Sentinal/ReportV2?key={QuerySystem.QuerySystem.QuerySystemKey}&userid={plr.authManager.UserId}&type=VCExploit4&token={Uri.EscapeDataString(BanSystem.CedModAuthTokens.ContainsKey(plr) ? BanSystem.CedModAuthTokens[plr].Item1 : "unavailable")}&signature={Uri.EscapeDataString(BanSystem.CedModAuthTokens.ContainsKey(plr) ? BanSystem.CedModAuthTokens[plr].Item2 : "unavailable")}", new StringContent(JsonConvert.SerializeObject(userFrames.Value), Encoding.Default, "application/json"));
                                    if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                        Logger.Debug(await response.Content.ReadAsStringAsync());
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error(ex.ToString());
                                }
                            }
                        });
                    }
                }
                
                foreach (var lunge in Scp939LungePatch.LungeTime)
                {
                    var plr = ReferenceHub.AllHubs.FirstOrDefault(s => s.netId == lunge.Key, null);
                    if (plr == null)
                        continue;
                    
                    WebSocketSystem.Enqueue(new QueryCommand()
                    {
                        Recipient = "PANEL",
                        Data = new Dictionary<string, string>()
                        {
                            { "SentinalType", "SCP939LungeExploit" }, 
                            { "UserId", plr.authManager.UserId },
                            { "Data", JsonConvert.SerializeObject(lunge.Value) }
                        }
                    });
                }
                
                foreach (var lunge in FpcSyncDataPatch.MovementViolations)
                {
                    var plr = ReferenceHub.AllHubs.FirstOrDefault(s => s.netId == lunge.Key, null);
                    if (plr == null)
                        continue;
                    
                    WebSocketSystem.Enqueue(new QueryCommand()
                    {
                        Recipient = "PANEL",
                        Data = new Dictionary<string, string>()
                        {
                            { "SentinalType", "MovementViolation" }, 
                            { "UserId", plr.authManager.UserId },
                            { "Data", JsonConvert.SerializeObject(lunge.Value) },
                            { "Pos", plr.transform.position.ToString() }
                        }
                    });
                }
                
                FpcSyncDataPatch.MovementViolations.Clear();
                Scp939LungePatch.LungeTime.Clear();
                UserFrames.Clear();
                FrameCount.Clear(); //clear array as we have reported.
            }
            
            //clear the current list for the next frame
            VoicePacketPacket.PacketsSent.Clear();
        }
    }
}